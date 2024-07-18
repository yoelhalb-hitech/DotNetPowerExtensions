using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Operations;
using static DotNetPowerExtensions.Analyzers.Throws.AssignmentDataFlowOperationWalker;

namespace DotNetPowerExtensions.Analyzers.Tests.Throws.Utils;

internal class AssignmentDataFlowOperationWalker_Tests
{
    private static DataFlowContext GetContext(string methodBody, string parameters = "")
    {
        var text = $$"""
            using System;
            using System.Threading.Tasks;
            using System.Linq;
            using System.Collections.Generic;
            #nullable enable
            class Test
            {
                public Task TestMethod({{parameters}})
                {
                    {{methodBody}}
                }

                public static void Main(){}
        #pragma warning disable CS8618
                private string TestProp { get; set; }
            }
        """;
        var tree = SyntaxFactory.ParseSyntaxTree(text);

        return GetContextFromSyntaxTree(tree);
    }

    private static DataFlowContext GetContextFromSyntaxTree(SyntaxTree tree)
    {
        var semanticModel = ThrowsTestUtils.GetSemanticModel(tree);
        Assert.That(semanticModel.Compilation.GetDiagnostics().Count(d => d.WarningLevel == 0), Is.EqualTo(0));

        var method = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        return new DataFlowContext(semanticModel.Compilation)
        {
            StartOperation = semanticModel.GetOperation(method)!,
        };
    }

    private const string demo = $$"""
                    var c = (((Task?)null)!) ?? (((Task?)null)!);
                    var t = c;
                    var g = (true == true ? (t ?? throw new Exception()) : null);
                    return g;
        """;

    [Test]
    public void AnalazeReturn()
    {
        var context = GetContext(demo);
        context.StartingPointPredicate = o => (o is ILiteralOperation literalOperation
                                                    && literalOperation.ConstantValue.HasValue
                                                    && literalOperation.ConstantValue.Value is null,
                                        Array.Empty<ISymbol>());
        context.EndPointPredicate = o => o is IReturnOperation;

        var result = Analyze(context);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StartingOperations.Count, Is.EqualTo(3));
        Assert.That(result.EndingOperations.Count, Is.EqualTo(1));

        var flowResult = result.GetFlowedStartFromOperation(result.EndingOperations.First());

        Assert.That(flowResult.Count, Is.EqualTo(3));
        Assert.That(flowResult.OfType<ILiteralOperation>().Count(), Is.EqualTo(3));
    }

    [Test]
    public void AnalazeThrow()
    {
        var context = GetContext(demo);
        context.StartingPointPredicate = o => (o is IObjectCreationOperation creationOperation
                                                    && creationOperation.Constructor?.ContainingType.Name == nameof(Exception),
                                        Array.Empty<ISymbol>());
        context.EndPointPredicate = o => o is IThrowOperation;

        var result = Analyze(context);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StartingOperations.Count, Is.EqualTo(1));
        Assert.That(result.EndingOperations.Count, Is.EqualTo(1));

        var flowResult = result.GetFlowedStartFromOperation(result.EndingOperations.First());

        Assert.That(flowResult.Count, Is.EqualTo(1));
        Assert.That(flowResult.OfType<IObjectCreationOperation>().Count(), Is.EqualTo(1));
    }

    [Test]
    public void AnalazeParameterReturn()
    {
        var context = GetContext(demo, "int i, string j = \"test\"");
        context.StartingPointPredicate = o => (o is IMethodBodyOperation,
                                        ((o as IMethodBodyOperation)?.Syntax as MethodDeclarationSyntax)?.ParameterList.Parameters
                                                .Select(p => context.Compilation.GetSemanticModel(context.Compilation.SyntaxTrees.First()).GetDeclaredSymbol(p))
                                                .OfType<ISymbol>()
                                                .ToArray()
                                             ?? Array.Empty<ISymbol>());
        context.EndPointPredicate = o => o is IReturnOperation;

        var result = Analyze(context);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StartingOperations.Count, Is.EqualTo(1));
        Assert.That(result.StartingSymbols.Count, Is.EqualTo(2));
        Assert.That(result.EndingOperations.Count, Is.EqualTo(1));

        var flowResult = result.GetFlowedStartFromOperation(result.EndingOperations.First());

        Assert.That(flowResult, Is.Empty);
    }

    [Test]
    public void AnalazeInvocationOfInvocation()
    {
        var context = GetContext("""
            Func<Action> f = () => () => {};
            var a = f();
            a();
            return Task.CompletedTask;
            """);
        context.StartingPointPredicate = o => (o is IInvocationOperation, Array.Empty<ISymbol>());
        context.EndPointPredicate = o => o is IInvocationOperation;

        var result = Analyze(context);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StartingOperations.Count, Is.EqualTo(2));
        Assert.That(result.StartingSymbols.Count, Is.EqualTo(0));
        Assert.That(result.EndingOperations.Count, Is.EqualTo(2));

        var flowResult = result.GetFlowedStartFromOperation(result.EndingOperations.Last());

        Assert.That(flowResult.Count, Is.EqualTo(1));
        Assert.That(flowResult.First(), Is.SameAs(result.StartingOperations.First()));
    }
    [Test]
    public void AnalazeAwait()
    {
        var context = GetContext("""
            return Task.Run(async() => {
                Action a = () => {};
                a = () => {};
                var t = Task.Run(a);
                await t.ConfigureAwait(false);
            });
            """);
        context.StartingPointPredicate = o => (o is IAnonymousFunctionOperation, Array.Empty<ISymbol>());
        context.EndPointPredicate = o => o is IAwaitOperation;

        var result = Analyze(context);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StartingOperations.Count, Is.EqualTo(3));
        Assert.That(result.StartingSymbols.Count, Is.EqualTo(0));
        Assert.That(result.EndingOperations.Count, Is.EqualTo(1));

        var flowResult = result.GetFlowedStartFromOperation(result.EndingOperations.First());

        Assert.That(flowResult.Count, Is.EqualTo(2));
        Assert.That(flowResult.First(), Is.SameAs(result.StartingOperations.Skip(1).First()));
        Assert.That(flowResult.Last(), Is.SameAs(result.StartingOperations.Last()));
    }

    [Test]
    public void AnalazeReadProperty()
    {
        var context = GetContext("""
            return Task.Run(async() => {
                string b = "";
                Action a = () => { b = TestProp;  };
                a = () => {};
                var t = Task.Run(a);
                return await t.ConfigureAwait(false);
            });
            """);

        context.StartingPointPredicate = o => (o is IPropertyReferenceOperation, Array.Empty<ISymbol>());
        context.EndPointPredicate = o => o is IReturnOperation;

        var result = Analyze(context);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StartingOperations.Count, Is.EqualTo(1));
        Assert.That(result.StartingSymbols.Count, Is.EqualTo(0));
        Assert.That(result.EndingOperations.Count, Is.EqualTo(1));

        var flowResult = result.GetFlowedStartFromOperation(result.EndingOperations.First());

        Assert.That(flowResult.Count, Is.EqualTo(1));
        Assert.That(flowResult.First(), Is.SameAs(result.StartingOperations.First()));
    }

    [Test]
    public void AnalazeWriteProperty()
    {
        var context = GetContext("""
            return Task.Run(async() => {
                string b = "";
                Func<string> a = () => b = "Testing";
                a = () => b = "Test";
                var t = Task.Run(a);
                var result = await t.ConfigureAwait(false);
                TestProp = result = b;
            });
            """);

        context.StartingPointPredicate = o => (o is ILiteralOperation literal
                                                && literal.Type?.SpecialType == SpecialType.System_String, Array.Empty<ISymbol>());
        context.EndPointPredicate = o => o is IPropertyReferenceOperation;

        var result = Analyze(context);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StartingOperations.Count, Is.EqualTo(3));
        Assert.That(result.StartingSymbols.Count, Is.EqualTo(0));
        Assert.That(result.EndingOperations.Count, Is.EqualTo(1));

        var flowResult = result.GetFlowedStartFromOperation(result.EndingOperations.First());

        Assert.That(flowResult.Count, Is.EqualTo(3));
        Assert.That(flowResult.First(), Is.SameAs(result.StartingOperations.First()));
        Assert.That(flowResult.Skip(1).First(), Is.SameAs(result.StartingOperations.Skip(1).First()));
        Assert.That(flowResult.Last(), Is.SameAs(result.StartingOperations.Last()));
    }

    [Test]
    public void AnalazePassingAsArgument()
    {
        var context = GetContext("""
            return Task.Run(async() => {
                string b = "";
                Action a = () => { b = "Testing";  };
                a = () => {};
                if(b == "Test") b = "Test123";
                var t = Task.Run(a);
                var result = await t.ConfigureAwait(false);
                TestProp = result;
                _ = DateTime.Now.ToString(b);
            });
            """);

        context.StartingPointPredicate = o => (o is ILiteralOperation, Array.Empty<ISymbol>());
        context.EndPointPredicate = o => o is IArgumentOperation;

        var result = Analyze(context);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StartingOperations.Count, Is.EqualTo(4));
        Assert.That(result.StartingSymbols.Count, Is.EqualTo(0));
        Assert.That(result.EndingOperations.Count, Is.EqualTo(1));

        var flowResult = result.GetFlowedStartFromOperation(result.EndingOperations.First());

        Assert.That(flowResult.Count, Is.EqualTo(3));
        Assert.That(flowResult.First(), Is.SameAs(result.StartingOperations.First()));
        Assert.That(flowResult.Skip(1).First(), Is.SameAs(result.StartingOperations.Skip(1).First()));
        Assert.That(flowResult.Last(), Is.SameAs(result.StartingOperations.Skip(2).First()));
    }

    [Test]
    public void AnalazeCollectionReadWrite([Values(true, false)] bool singleLine, [Values(true, false)] bool useList)
    {
        // NOTE: Array works differently than other collections so we test it separately
        var type = useList ? "List<Action>" : "[]";
        var inner = singleLine ? $$"""return new {{type}}{a}[0];""" : $$"""
            var a1 = new {{type}}{a};
            var b1 = new {{(useList ? type + "(1)" : "Action[1]")}};
            b1[0] = a1[0];
            var b = b1[0];
            return b;
        """;
        HandleCollection(inner);
    }

    private static void HandleCollection(string inner)
    {
        var context = GetContext($$"""
            return Task.Run(() => {
                {{inner}}
            });
            """, "Action a");

        context.StartingPointPredicate = o => (o is IMethodBodyOperation, ((o as IMethodBodyOperation)?.Syntax as MethodDeclarationSyntax)?.ParameterList.Parameters
                                                .Select(p => context.Compilation.GetSemanticModel(context.Compilation.SyntaxTrees.First()).GetDeclaredSymbol(p))
                                                .OfType<ISymbol>()
                                                .ToArray()
                                             ?? Array.Empty<ISymbol>());
        context.EndPointPredicate = o => o is IReturnOperation;

        var result = Analyze(context);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StartingOperations.Count, Is.EqualTo(1));
        Assert.That(result.StartingSymbols.Count, Is.EqualTo(1));
        Assert.That(result.EndingOperations.Count, Is.EqualTo(2)); // The `Task.Run` return is also a return

        var flowResult = result.GetFlowedStartSymbolFromOperation(result.EndingOperations.Last());

        Assert.That(flowResult.Count, Is.EqualTo(1));
        Assert.That(flowResult.First(), Is.SameAs(result.StartingSymbols.First()));
    }

    [Test]
    public void AnalazeMultiDimensionalArrayReadWrite([Values(true, false)] bool singleLine)
    {
        var inner = singleLine ? $$"""return ((new [,]{ new []{a} } as object[,])[0][0] as Action)!;""" : $$"""
            var a1 = new []{a};
            object[,] b1 = new Action[1,1];
            (b1 as Action[,])[0][0] = a1[0][0];
            var b = (b1[0][0] as Action);
            return b;
        """;
        HandleCollection(inner);
    }
    [Test]
    public void AnalazeMoreComplicatedCollectionReadWrite([Values(true, false)] bool singleLine, [Values(true, false)] bool useList)
    {
        // NOTE: Array works differently than other collections so we test it separately
        var type = useList ? "List<Action>" : "[]";
        var complicatedType = useList ? "List<SList<object>>" : "object[][]";
        var inner = singleLine ? $$"""return (new {{complicatedType}}{ new {{type}}{a} }[0][0] as Action)!;""" : $$"""
            var a1 = new {{type}}{a};
            IEnumerable<IEnumerable<object>> b1 = new {{(useList ? "List<List<Action>>{new List<Action>(1)}" : "Action[][1]{new Action[1]}")}};
            (b1 as {{complicatedType}})[0][0] = a1[0];
            var b = (b1[0][0] as Action);
            return b;
        """;

        HandleCollection(inner);
    }


    [Test]
    public void AnalazeCollectionWithFunctions([Values(true, false)] bool singleLine, [Values(true, false)] bool useList)
    {
        var type = useList ? "List<Action>" : "[]";
        var toType = useList ? "ToList" : "ToArray";
        var inner = singleLine ? $$"""return new {{type}}{a}.Union(new {{type}}{arg2}).Select(x => x).{{toType}}().First();""" : $$"""
            var a1 = new {{type}}{a};
            var b1 = a1.Union(new {{type}}{arg2}).Select(x => x).{{toType}}();
            var b = b1.First();
            return b;
        """;
        var context = GetContext($$"""
            return Task.Run(() => {
                {{inner}}
            });
            """, "Action a, Action arg2, Action arg3");

        context.StartingPointPredicate = o => (o is IMethodBodyOperation, ((o as IMethodBodyOperation)?.Syntax as MethodDeclarationSyntax)?.ParameterList.Parameters
                                                .Select(p => context.Compilation.GetSemanticModel(context.Compilation.SyntaxTrees.First()).GetDeclaredSymbol(p))
                                                .OfType<ISymbol>()
                                                .ToArray()
                                             ?? Array.Empty<ISymbol>());
        context.EndPointPredicate = o => o is IReturnOperation;

        var result = Analyze(context);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StartingOperations.Count, Is.EqualTo(1));
        Assert.That(result.StartingSymbols.Count, Is.EqualTo(3));
        Assert.That(result.EndingOperations.Count, Is.EqualTo(3)); // The `Task.Run` return is also a return

        var flowResult = result.GetFlowedStartSymbolFromOperation(result.EndingOperations.Last(o => o.Syntax is ReturnStatementSyntax)); // The x => x can be at different positions based on singleLine or not

        Assert.That(flowResult.Count, Is.EqualTo(3));
        Assert.That(flowResult.First(), Is.SameAs(result.StartingSymbols.First()));
        Assert.That(flowResult.Skip(1).First(), Is.SameAs(result.StartingSymbols.Skip(1).First()));
        Assert.That(flowResult.Last(), Is.SameAs(result.StartingSymbols.Last()));
    }

    [Test]
    public void IgnoresNotReleventCollectionFunctions([Values(true, false)] bool singleLine, [Values(true, false)] bool useList)
    {
        // In order to test it correctly we need to have it of the same type
        var type = useList ? "List<int>" : "[]";
        var inner = singleLine ? $$"""return new {{type}}{a}.Count();""" : $$"""
            var a1 = new {{type}}{a};
            var b = a1.Count();
            return b;
        """;
        var context = GetContext($$"""
            return Task.Run(() => {
                {{inner}}
            });
            """, "int a");

        context.StartingPointPredicate = o => (o is IMethodBodyOperation, ((o as IMethodBodyOperation)?.Syntax as MethodDeclarationSyntax)?.ParameterList.Parameters
                                                .Select(p => context.Compilation.GetSemanticModel(context.Compilation.SyntaxTrees.First()).GetDeclaredSymbol(p))
                                                .OfType<ISymbol>()
                                                .ToArray()
                                             ?? Array.Empty<ISymbol>());
        context.EndPointPredicate = o => o is IReturnOperation;

        var result = Analyze(context);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StartingOperations.Count, Is.EqualTo(1));
        Assert.That(result.StartingSymbols.Count, Is.EqualTo(1));
        Assert.That(result.EndingOperations.Count, Is.EqualTo(2)); // The `Task.Run` return is also a return

        var flowResult = result.GetFlowedStartSymbolFromOperation(result.EndingOperations.Last());

        Assert.That(flowResult.Count, Is.EqualTo(0));
    }

    [Test]
    public void AnalazeListAddAndAddRange()
    {
        var inner = $$"""
            var a1 = new System.Collections.Generic.List<Action>();
            a1.Add(a);
            a1.AddRange(new []{b, c});
            var b = b1.First();
            return b;
        """;
        var context = GetContext($$"""
            return Task.Run(() => {
                {{inner}}
            });
            """, "Action a, Action b, Action c");

        context.StartingPointPredicate = o => (o is IMethodBodyOperation, ((o as IMethodBodyOperation)?.Syntax as MethodDeclarationSyntax)?.ParameterList.Parameters
                                                .Select(p => context.Compilation.GetSemanticModel(context.Compilation.SyntaxTrees.First()).GetDeclaredSymbol(p))
                                                .OfType<ISymbol>()
                                                .ToArray()
                                             ?? Array.Empty<ISymbol>());
        context.EndPointPredicate = o => o is IReturnOperation;

        var result = Analyze(context);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StartingOperations.Count, Is.EqualTo(1));
        Assert.That(result.StartingSymbols.Count, Is.EqualTo(3));
        Assert.That(result.EndingOperations.Count, Is.EqualTo(2)); // The `Task.Run` return is also a return

        var flowResult = result.GetFlowedStartSymbolFromOperation(result.EndingOperations.Last());

        Assert.That(flowResult.Count, Is.EqualTo(3));
        Assert.That(flowResult.First(), Is.SameAs(result.StartingSymbols.First()));
        Assert.That(flowResult.Skip(1).First(), Is.SameAs(result.StartingSymbols.Skip(1).First()));
        Assert.That(flowResult.Last(), Is.SameAs(result.StartingSymbols.Last()));
    }


    [Test]
    public void AnalazeOperators()
    {
        var context = GetContext($$"""
            return Task.Run(() => {
                var a = (true && (true || !true));
                return a;
            });
            """);

        context.StartingPointPredicate = o => (o is ILiteralOperation, Array.Empty<ISymbol>());
        context.EndPointPredicate = o => o is IReturnOperation;

        var result = Analyze(context);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StartingOperations.Count, Is.EqualTo(3));
        Assert.That(result.StartingSymbols.Count, Is.EqualTo(1));
        Assert.That(result.EndingOperations.Count, Is.EqualTo(2)); // The `Task.Run` return is also a return

        var flowResult = result.GetFlowedStartSymbolFromOperation(result.EndingOperations.Last());

        Assert.That(flowResult.Count, Is.EqualTo(3));
        Assert.That(flowResult.First(), Is.SameAs(result.StartingSymbols.First()));
        Assert.That(flowResult.Skip(1).First(), Is.SameAs(result.StartingSymbols.Skip(1).First()));
        Assert.That(flowResult.Last(), Is.SameAs(result.StartingSymbols.Last()));
    }

    [Test]
    public void AnalazeAnonymous()
    {
        var context = GetContext($$"""
            return Task.Run(() => {
                var a = new { Test = true };
                return a.Test;
            });
            """);

        context.StartingPointPredicate = o => (o is ILiteralOperation, Array.Empty<ISymbol>());
        context.EndPointPredicate = o => o is IReturnOperation;

        var result = Analyze(context);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StartingOperations.Count, Is.EqualTo(3));
        Assert.That(result.StartingSymbols.Count, Is.EqualTo(1));
        Assert.That(result.EndingOperations.Count, Is.EqualTo(2)); // The `Task.Run` return is also a return

        var flowResult = result.GetFlowedStartSymbolFromOperation(result.EndingOperations.Last());

        Assert.That(flowResult.Count, Is.EqualTo(3));
        Assert.That(flowResult.First(), Is.SameAs(result.StartingSymbols.First()));
        Assert.That(flowResult.Skip(1).First(), Is.SameAs(result.StartingSymbols.Skip(1).First()));
        Assert.That(flowResult.Last(), Is.SameAs(result.StartingSymbols.Last()));
    }

    [Test]
    public void AnalazeTuple()
    {
        var context = GetContext($$"""
            return Task.Run(() => {
                var a = (true, true);
                return a.Item1;
            });
            """);

        context.StartingPointPredicate = o => (o is ILiteralOperation, Array.Empty<ISymbol>());
        context.EndPointPredicate = o => o is IReturnOperation;

        var result = Analyze(context);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StartingOperations.Count, Is.EqualTo(2));
        Assert.That(result.StartingSymbols.Count, Is.EqualTo(1));
        Assert.That(result.EndingOperations.Count, Is.EqualTo(2)); // The `Task.Run` return is also a return

        var flowResult = result.GetFlowedStartSymbolFromOperation(result.EndingOperations.Last());

        Assert.That(flowResult.Count, Is.EqualTo(1));
        Assert.That(flowResult.First(), Is.SameAs(result.StartingSymbols.First()));
    }

    [Test]
    public void AnalazeDestructuredTuple()
    {
        var context = GetContext($$"""
            return Task.Run(() => {
                var (i1, i2) = (true, true);
                return i2;
            });
            """);

        context.StartingPointPredicate = o => (o is ILiteralOperation, Array.Empty<ISymbol>());
        context.EndPointPredicate = o => o is IReturnOperation;

        var result = Analyze(context);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StartingOperations.Count, Is.EqualTo(2));
        Assert.That(result.StartingSymbols.Count, Is.EqualTo(1));
        Assert.That(result.EndingOperations.Count, Is.EqualTo(2)); // The `Task.Run` return is also a return

        var flowResult = result.GetFlowedStartSymbolFromOperation(result.EndingOperations.Last());

        Assert.That(flowResult.Count, Is.EqualTo(1));
        Assert.That(flowResult.First(), Is.SameAs(result.StartingSymbols.First()));
    }

    [Test]
    public void AnalazeForEach([Values(true, false)] bool useList)
    {
        var inner = useList ? @"
                                var b = new []{ a };
                                Action? c = null;
                                b.ToList().ForEach(x => c = x);
                                return c;"
                            : @"
                                var b = new []{ a };
                                foreach(var c in b) return c;";


        HandleCollection(inner);
    }

    [Test]
    public void AnalazeMultipleReturn()
    {
        var context = GetContext($$"""
                    var c = (((Task?)null)!) ?? (((Task?)null)!);
                    var t = c;
                    if(t is not null) return c;
                    var g = (true == true ? (t ?? throw new Exception()) : null);
                    t = g;
                    return t;
            """);
        context.StartingPointPredicate = o => (o is ILiteralOperation literalOperation
                                                    && literalOperation.ConstantValue.HasValue
                                                    && literalOperation.ConstantValue.Value is null,
                                        Array.Empty<ISymbol>());
        context.EndPointPredicate = o => o is IReturnOperation;

        var result = Analyze(context);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StartingOperations.Count, Is.EqualTo(4)); // "is not null" is also a literal null
        Assert.That(result.EndingOperations.Count, Is.EqualTo(2));

        var firstFlowResult = result.GetFlowedStartFromOperation(result.EndingOperations.First());

        Assert.That(firstFlowResult.Count, Is.EqualTo(2));
        Assert.That(firstFlowResult.OfType<ILiteralOperation>().Count(), Is.EqualTo(2));

        var lastFlowResult = result.GetFlowedStartFromOperation(result.EndingOperations.Last());

        Assert.That(lastFlowResult.Count, Is.EqualTo(3));
        Assert.That(lastFlowResult.OfType<ILiteralOperation>().Count(), Is.EqualTo(3));
    }

    [Test]
    [Ignore("Too hard to implement for now")]
    public void AnalazeDefiniteAssignment()
    {
        var context = GetContext($$"""
                    var c = (((Task?)null)!) ?? (((Task?)null)!);
                    var t = c;
                    if(t is not null) return c;
                    var g = (true == true ? ((int?)10 ?? throw new Exception()) : null);
                    t = g;
                    return t;
            """);
        context.StartingPointPredicate = o => (o is ILiteralOperation literalOperation
                                                    && literalOperation.ConstantValue.HasValue
                                                    && literalOperation.ConstantValue.Value is null,
                                        Array.Empty<ISymbol>());
        context.EndPointPredicate = o => o is IReturnOperation;

        var result = Analyze(context);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StartingOperations.Count, Is.EqualTo(4)); // "is not null" is also a literal null
        Assert.That(result.EndingOperations.Count, Is.EqualTo(2));

        var lastFlowResult = result.GetFlowedStartFromOperation(result.EndingOperations.Last());

        Assert.That(lastFlowResult.Count, Is.EqualTo(1));
        Assert.That(lastFlowResult.OfType<ILiteralOperation>().Count(), Is.EqualTo(1));
    }
}
