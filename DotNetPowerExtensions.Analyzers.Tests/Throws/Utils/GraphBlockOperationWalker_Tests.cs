using DotNetPowerExtensions.Analyzers.Throws.Utils;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System.Reflection.Metadata;
using static DotNetPowerExtensions.Analyzers.Throws.AssignmentDataFlowOperationWalker;

namespace DotNetPowerExtensions.Analyzers.Tests.Throws.Utils;

internal class GraphBlockOperationWalker_Tests
{
    private static GraphAnalyzer.DataFlowContext GetContext(string methodBody, string parameters = "")
    {
        var tree = SyntaxFactory.ParseSyntaxTree($$"""
            using System;
            using System.Threading.Tasks;
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
        """);

        return GetContextFromSyntaxTree(tree);
    }

    private static GraphAnalyzer.DataFlowContext GetContextFromSyntaxTree(SyntaxTree tree)
    {
        var semanticModel = ThrowsTestUtils.GetSemanticModel(tree);
        Assert.That(semanticModel.Compilation.GetDiagnostics().Count, Is.EqualTo(0));

        var method = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First();
        var operation = semanticModel.GetOperation(method) as IMethodBodyOperation;

        return new GraphAnalyzer.DataFlowContext
        {
            Compilation = semanticModel.Compilation,
            StartOperation = operation!,
            Graph = ControlFlowGraph.Create(operation!)
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

        var handler = new GraphAnalyzer(context);

        Assert.That(handler.ReturnInfos.Count, Is.EqualTo(1));

        var flowResult = handler.GetStartInfoForReturn().First();

        Assert.That(flowResult.Item2.Count, Is.EqualTo(3));
        Assert.That(flowResult.Item2.OfType<ILiteralOperation>().Count(), Is.EqualTo(3));
    }

    [Test]
    public void AnalazeThrow()
    {
        var context = GetContext(demo);
        context.StartingPointPredicate = o => (o is IObjectCreationOperation creationOperation
                                                    && creationOperation.Constructor?.ContainingType.Name == nameof(Exception),
                                        Array.Empty<ISymbol>());
        context.EndPointPredicate = o => o is IThrowOperation;

        var handler = new GraphAnalyzer(context);

        Assert.That(handler.ThrowsInfo.Count, Is.EqualTo(1));

        var flowResult = handler.GetStartInfoForThrows().First();

        Assert.That(flowResult.Item2.Count, Is.EqualTo(1));
        Assert.That(flowResult.Item2.OfType<IObjectCreationOperation>().Count(), Is.EqualTo(1));
    }

    [Test]
    public void AnalazeParameterReturn()
    {
        var context = GetContext(demo, "int i, string j = \"test\"");
        context.StartingPointPredicate = o => (o is IParameterReferenceOperation, Array.Empty<ISymbol>());
        context.EndPointPredicate = o => o is IReturnOperation;

        var handler = new GraphAnalyzer(context);

        Assert.That(handler.ReturnInfos.Count, Is.EqualTo(1));

        var flowResult = handler.GetStartInfoForReturn().First();
        Assert.That(flowResult.Item2, Is.Empty);
        Assert.That(flowResult.Item3, Is.Empty);
    }

    [Test]
    [TestCase(0, 0)]
    [TestCase(1, 1)]
    public void AnalazeInvocationOfInvocation_MultiLine(int skip, int expectedCount)
    {
        var context = GetContext("""
            Func<Action> f = () => () => {};
            var a = f();
            a();
            return Task.CompletedTask;
            """);
        context.StartingPointPredicate = o => (o is IInvocationOperation, Array.Empty<ISymbol>());
        context.EndPointPredicate = o => o is IInvocationOperation;

        var handler = new GraphAnalyzer(context);

        Assert.That(handler.ReadEndOperations.Count, Is.EqualTo(2));

        var flowResult = handler.GetStartInfo(handler.ReadEndOperations.Skip(skip).First().Key);

        Assert.That(flowResult.Item1.Count, Is.EqualTo(expectedCount));
        if(expectedCount > 0) Assert.That(flowResult.Item1.First(), Is.InstanceOf<IInvocationOperation>());
    }

    [Test]
    [TestCase(0, 0)]
    [TestCase(1, 1)]
    public void AnalazeInvocationOfInvocation_SingleLine(int skip, int expectedCount)
    {
        var context = GetContext("""
            Func<Action> f = () => () => {};
            f()();
            return Task.CompletedTask;
            """);
        context.StartingPointPredicate = o => (o is IInvocationOperation, Array.Empty<ISymbol>());
        context.EndPointPredicate = o => o is IInvocationOperation;

        var handler = new GraphAnalyzer(context);

        Assert.That(handler.ReadEndOperations.Count, Is.EqualTo(2));

        var flowResult = handler.GetStartInfo(handler.ReadEndOperations.Skip(skip).First().Key);

        Assert.That(flowResult.Item1.Count, Is.EqualTo(expectedCount));
        if (expectedCount > 0) Assert.That(flowResult.Item1.First(), Is.InstanceOf<IInvocationOperation>());
    }

    //[Test]
    //public void AnalazeAwait()
    //{
    //    var context = GetContext("""
    //        return Task.Run(async() => {
    //            Action a = () => {};
    //            a = () => {};
    //            var t = Task.Run(a);
    //            await t.ConfigureAwait(false);
    //        });
    //        """);
    //    context.StartingPointPredicate = o => (o is IAnonymousFunctionOperation, Array.Empty<ISymbol>());
    //    context.EndPointPredicate = o => o is IAwaitOperation;

    //    var result = Analaze(context);

    //    Assert.That(result, Is.Not.Null);
    //    Assert.That(result.StartingOperations.Count, Is.EqualTo(3));
    //    Assert.That(result.StartingSymbols.Count, Is.EqualTo(0));
    //    Assert.That(result.EndingOperations.Count, Is.EqualTo(1));

    //    var flowResult = result.GetFlowedStartFromOperation(result.EndingOperations.First());

    //    Assert.That(flowResult.Count, Is.EqualTo(2));
    //    Assert.That(flowResult.First(), Is.SameAs(result.StartingOperations.Skip(1).First()));
    //    Assert.That(flowResult.Last(), Is.SameAs(result.StartingOperations.Last()));
    //}

    //[Test]
    //public void AnalazeReadProperty()
    //{
    //    var context = GetContext("""
    //        return Task.Run(async() => {
    //            string b = "";
    //            Action a = () => { b = TestProp;  };
    //            a = () => {};
    //            var t = Task.Run(a);
    //            return await t.ConfigureAwait(false);
    //        });
    //        """);

    //    context.StartingPointPredicate = o => (o is IPropertyReferenceOperation, Array.Empty<ISymbol>());
    //    context.EndPointPredicate = o => o is IReturnOperation;

    //    var result = Analaze(context);

    //    Assert.That(result, Is.Not.Null);
    //    Assert.That(result.StartingOperations.Count, Is.EqualTo(1));
    //    Assert.That(result.StartingSymbols.Count, Is.EqualTo(0));
    //    Assert.That(result.EndingOperations.Count, Is.EqualTo(1));

    //    var flowResult = result.GetFlowedStartFromOperation(result.EndingOperations.First());

    //    Assert.That(flowResult.Count, Is.EqualTo(1));
    //    Assert.That(flowResult.First(), Is.SameAs(result.StartingOperations.First()));
    //}

    //[Test]
    //public void AnalazeWriteProperty()
    //{
    //    var context = GetContext("""
    //        return Task.Run(async() => {
    //            string b = "";
    //            Func<string> a = () => b = "Testing";
    //            a = () => b = "Test";
    //            var t = Task.Run(a);
    //            var result = await t.ConfigureAwait(false);
    //            TestProp = result = b;
    //        });
    //        """);

    //    context.StartingPointPredicate = o => (o is ILiteralOperation literal
    //                                            && literal.Type?.SpecialType == SpecialType.System_String, Array.Empty<ISymbol>());
    //    context.EndPointPredicate = o => o is IPropertyReferenceOperation;

    //    var result = Analaze(context);

    //    Assert.That(result, Is.Not.Null);
    //    Assert.That(result.StartingOperations.Count, Is.EqualTo(3));
    //    Assert.That(result.StartingSymbols.Count, Is.EqualTo(0));
    //    Assert.That(result.EndingOperations.Count, Is.EqualTo(1));

    //    var flowResult = result.GetFlowedStartFromOperation(result.EndingOperations.First());

    //    Assert.That(flowResult.Count, Is.EqualTo(3));
    //    Assert.That(flowResult.First(), Is.SameAs(result.StartingOperations.First()));
    //    Assert.That(flowResult.Skip(1).First(), Is.SameAs(result.StartingOperations.Skip(1).First()));
    //    Assert.That(flowResult.Last(), Is.SameAs(result.StartingOperations.Last()));
    //}

    //[Test]
    //public void AnalazePassingAsArgument()
    //{
    //    var context = GetContext("""
    //        return Task.Run(async() => {
    //            string b = "";
    //            Action a = () => { b = "Testing";  };
    //            a = () => {};
    //            if(b == "Test") b = "Test123";
    //            var t = Task.Run(a);
    //            var result = await t.ConfigureAwait(false);
    //            TestProp = result;
    //            _ = DateTime.Now.ToString(b);
    //        });
    //        """);

    //    context.StartingPointPredicate = o => (o is ILiteralOperation, Array.Empty<ISymbol>());
    //    context.EndPointPredicate = o => o is IArgumentOperation;

    //    var result = Analaze(context);

    //    Assert.That(result, Is.Not.Null);
    //    Assert.That(result.StartingOperations.Count, Is.EqualTo(4));
    //    Assert.That(result.StartingSymbols.Count, Is.EqualTo(0));
    //    Assert.That(result.EndingOperations.Count, Is.EqualTo(1));

    //    var flowResult = result.GetFlowedStartFromOperation(result.EndingOperations.First());

    //    Assert.That(flowResult.Count, Is.EqualTo(3));
    //    Assert.That(flowResult.First(), Is.SameAs(result.StartingOperations.First()));
    //    Assert.That(flowResult.Skip(1).First(), Is.SameAs(result.StartingOperations.Skip(1).First()));
    //    Assert.That(flowResult.Last(), Is.SameAs(result.StartingOperations.Skip(2).First()));
    //}


    //[Test]
    //public void AnalazeMultipleReturn()
    //{
    //    var context = GetContext($$"""
    //                var c = (((Task?)null)!) ?? (((Task?)null)!);
    //                var t = c;
    //                if(t is not null) return c;
    //                var g = (true == true ? (t ?? throw new Exception()) : null);
    //                t = g;
    //                return t;
    //        """);
    //    context.StartingPointPredicate = o => (o is ILiteralOperation literalOperation
    //                                                && literalOperation.ConstantValue.HasValue
    //                                                && literalOperation.ConstantValue.Value is null,
    //                                    Array.Empty<ISymbol>());
    //    context.EndPointPredicate = o => o is IReturnOperation;

    //    var result = Analaze(context);

    //    Assert.That(result, Is.Not.Null);
    //    Assert.That(result.StartingOperations.Count, Is.EqualTo(4)); // "is not null" is also a literal null
    //    Assert.That(result.EndingOperations.Count, Is.EqualTo(2));

    //    var firstFlowResult = result.GetFlowedStartFromOperation(result.EndingOperations.First());

    //    Assert.That(firstFlowResult.Count, Is.EqualTo(2));
    //    Assert.That(firstFlowResult.OfType<ILiteralOperation>().Count(), Is.EqualTo(2));

    //    var lastFlowResult = result.GetFlowedStartFromOperation(result.EndingOperations.Last());

    //    Assert.That(lastFlowResult.Count, Is.EqualTo(3));
    //    Assert.That(lastFlowResult.OfType<ILiteralOperation>().Count(), Is.EqualTo(3));
    //}

    [Test]
    public void AnalazeDefiniteAssignment()
    {
        var context = GetContext($$"""
                    var c = (((Task?)null)!) ?? (((Task?)null)!);
                    var t = c;
                    if(t is not null) return c;
                    var g = (true == true ? ((Task?)null ?? throw new Exception()) : (Task?)null);
                    t = g;
                    return t;
            """);
        context.StartingPointPredicate = o => (o is ILiteralOperation literalOperation
                                                    && literalOperation.ConstantValue.HasValue
                                                    && literalOperation.ConstantValue.Value is null,
                                        Array.Empty<ISymbol>());
        context.EndPointPredicate = o => o is IReturnOperation;

        var handler = new GraphAnalyzer(context);

        Assert.That(handler.ReturnInfos.Count, Is.EqualTo(2));

        var lastFlowResult = handler.GetStartInfoForReturn().Last();

        Assert.That(lastFlowResult.Item2.Count, Is.EqualTo(2));
        Assert.That(lastFlowResult.Item2.OfType<ILiteralOperation>().Count(), Is.EqualTo(2));
    }
}
