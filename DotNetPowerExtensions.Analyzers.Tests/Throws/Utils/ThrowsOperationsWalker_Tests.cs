using DotNetPowerExtensions.Analyzers.Throws;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Operations;
using SequelPay.DotNetPowerExtensions;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DotNetPowerExtensions.Analyzers.Tests.Throws.Utils;

internal class ThrowsOperationsWalker_Tests
{
    private static ITypeSymbol[] GetExceptions(string code)
    {
        var tree = SyntaxFactory.ParseSyntaxTree(code);

        var semanticModel = ThrowsTestUtils.GetSemanticModel(tree);

        var method = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First();
        var operation = semanticModel.GetOperation(method)!;

        return new ThrowsOperationWalker(semanticModel.Compilation).GetExceptionsForOperation(operation);
    }

    [Test]
    [TestCase("", ExpectedResult = new string[] { }, Description = "When None")]
    [TestCase("throw new ArgumentNullException()", ExpectedResult = new string[] { nameof(ArgumentNullException) }, Description = "When throws")]
    [TestCase("try { throw new ArgumentNullException(); } catch{}", ExpectedResult = new string[] { }, Description = "When throws and catches all")]
    [TestCase("try { throw new ArgumentNullException(); } catch(ArgumentNullException){}", ExpectedResult = new string[] { }, Description = "When throws and catches by type")]
    [TestCase("try { throw new ArgumentNullException(); throw new NullReferenceException(); } catch(ArgumentNullException){}", ExpectedResult = new string[] { nameof(NullReferenceException) }, Description = "When throws two and catches one")]
    [TestCase("try { throw new ArgumentNullException(); } catch(ArgumentNullException ex){}", ExpectedResult = new string[] { }, Description = "When throws and catches with variable")]
    [TestCase("try { throw new ArgumentNullException(); } catch(Exception){}", ExpectedResult = new string[] { }, Description = "When throws and catches by base type")]
    [TestCase("try { throw new ArgumentNullException(); } catch(NullReferenceException){}", ExpectedResult = new string[] { nameof(ArgumentNullException) }, Description = "When throws and catches by another type")]
    [TestCase("try { throw new ArgumentNullException(); } catch(ArgumentNullException ex) when (true == false){}", ExpectedResult = new string[] { nameof(ArgumentNullException) }, Description = "When filter")]
    [TestCase("try { throw new ArgumentNullException(); } catch(ArgumentNullException ex) { throw new ArgumentNullException(); }", ExpectedResult = new string[] { nameof(ArgumentNullException) }, Description = "When throws same in catch")]
    [TestCase("try { throw new ArgumentNullException(); } catch(ArgumentNullException ex) { throw new NullReferenceException(); }", ExpectedResult = new string[] { nameof(NullReferenceException) }, Description = "When throws another in catch")]
    [TestCase("try { throw new ArgumentNullException(); } catch(ArgumentNullException ex) { throw; }", ExpectedResult = new string[] { nameof(ArgumentNullException) }, Description = "When rethrows")]
    [TestCase("try { throw new ArgumentNullException(); } catch(ArgumentNullException ex) { throw ex; }", ExpectedResult = new string[] { nameof(ArgumentNullException) }, Description = "When rethrows with variable")]
    [TestCase("try { throw new ArgumentNullException(); } catch(ArgumentNullException ex) {} finally{ throw new ArgumentNullException(); }", ExpectedResult = new string[] { nameof(ArgumentNullException) }, Description = "When throws in finally")]
    [TestCase("try { throw new Exception(); } catch(NullReferenceException){}", ExpectedResult = new string[] { nameof(Exception) }, Description = "When throws and catches by sub type")]
    [TestCase("try { throw new Exception(); } catch(NullReferenceException){} catch{}", ExpectedResult = new string[] { }, Description = "When throws and catches by both")]
    public string[] Test_GetExceptionsForOperation(string body)
    {
        var code = $$"""
            using System;
            class Test
            {
                public void TestMethod(){ {{body}}; }
            }
        """;

        var result = GetExceptions(code);

        return result.Select(r => r.Name).ToArray();
    }

    [Test]
    [TestCase("new ArgumentNullException()", true, ExpectedResult = new string[] { nameof(ArgumentNullException) })]
    [TestCase("new ArgumentNullException()", false, ExpectedResult = new string[] { nameof(ArgumentNullException) })]
    [TestCase("(new ArgumentNullException() as Exception)", true, ExpectedResult = new string[] { nameof(Exception), nameof(ArgumentNullException) })]
    [TestCase("(new ArgumentNullException() as Exception)", false, ExpectedResult = new string[] { nameof(Exception), nameof(ArgumentNullException) })]
    [TestCase("null", true, ExpectedResult = new string[] { nameof(NullReferenceException) })]
    [TestCase("null", false, ExpectedResult = new string[] { nameof(NullReferenceException) })]
    [TestCase("(null)", true, ExpectedResult = new string[] { nameof(NullReferenceException) })]
    [TestCase("(null)", false, ExpectedResult = new string[] { nameof(NullReferenceException) })]
    [TestCase("new Exception()", true, ExpectedResult = new string[] { nameof(Exception) })]
    [TestCase("new Exception()", false, ExpectedResult = new string[] { nameof(Exception) })]
    [TestCase("(true == true ? new Exception() : new Exception())", true, ExpectedResult = new string[] { nameof(Exception) })]
    [TestCase("true == true ? new Exception() : new Exception()", false, ExpectedResult = new string[] { nameof(Exception) })]
    [TestCase("(true == true ? new Exception() : new Exception())", false, ExpectedResult = new string[] { nameof(Exception) })]
    [TestCase("(new Exception() ?? new Exception())", true, ExpectedResult = new string[] { nameof(Exception) })]
    [TestCase("new Exception() ?? new Exception()", false, ExpectedResult = new string[] { nameof(Exception) })]
    [TestCase("(new Exception() ?? new Exception())", false, ExpectedResult = new string[] { nameof(Exception) })]
    [TestCase("(true == true ? new ArgumentNullException() : new ArgumentNullException())", true, ExpectedResult = new string[] { nameof(ArgumentNullException) })]
    [TestCase("(true == true ? new ArgumentNullException() : new ArgumentNullException())", false, ExpectedResult = new string[] { nameof(ArgumentNullException) })]
    [TestCase("true == true ? new ArgumentNullException() as Exception : new NullReferenceException()", false, ExpectedResult = new string[] { nameof(Exception), nameof(ArgumentNullException), nameof(NullReferenceException) })]
    [TestCase("(true == true ? new ArgumentNullException() as Exception : new NullReferenceException())", true, ExpectedResult = new string[] { nameof(Exception), nameof(ArgumentNullException), nameof(NullReferenceException) })]
    [TestCase("true == true ? new ArgumentNullException() as Exception : new NullReferenceException()", false, ExpectedResult = new string[] { nameof(Exception), nameof(ArgumentNullException), nameof(NullReferenceException) })]
    [TestCase("(true == true ? new ArgumentNullException() as Exception : new NullReferenceException())", false, ExpectedResult = new string[] { nameof(Exception), nameof(ArgumentNullException), nameof(NullReferenceException) })]
    [TestCase("(new ArgumentNullException() as Exception ?? new NullReferenceException())", true, ExpectedResult = new string[] { nameof(Exception), nameof(ArgumentNullException), nameof(NullReferenceException) })]
    [TestCase("new ArgumentNullException() as Exception ?? new NullReferenceException()", false, ExpectedResult = new string[] { nameof(Exception), nameof(ArgumentNullException), nameof(NullReferenceException) })]
    [TestCase("(new ArgumentNullException() as Exception ?? new NullReferenceException())", false, ExpectedResult = new string[] { nameof(Exception), nameof(ArgumentNullException), nameof(NullReferenceException) })]
    public string[] Test_GetExceptionsForThrowOperation(string body, bool useExpression) // Note that for useExpression we always need parens when more complicated expression otherwise it gets messed up
    {
        var tree = SyntaxFactory.ParseSyntaxTree($$"""
            using System;
            class Test
            {
                public void TestMethod(){ {{(useExpression ? " _ = null ?? " : "")}} throw {{body}}; }
            }
        """);
        var semanticModel = ThrowsTestUtils.GetSemanticModel(tree);

        var nodes = tree.GetRoot().DescendantNodes();
        var matching = useExpression ? (IEnumerable<SyntaxNode>)nodes.OfType<ThrowExpressionSyntax>() : nodes.OfType<ThrowStatementSyntax>();
        var throws = matching.First();
        var operation = semanticModel.GetOperation(throws) as IThrowOperation;

        var result = new ThrowsOperationWalker(semanticModel.Compilation).GetExceptionsForOperation(operation!);

        return result.Select(r => r.Name).ToArray();
    }

    [Test]
    public void Test_GetExceptionsForOperation_IgnoresInnerMethod()
    {
        var code = $$"""
            using System;
            class Test
            {
                public void TestMethod(){
                    void Inner() { throw new Exception(); }
                }
            }
        """;

        var result = GetExceptions(code);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Test_GetExceptionsForOperation_IgnoresNotInvokedLambda()
    {
        var code = $$"""
            using System;
            class Test
            {
                public void TestMethod(){
                    Action a = () => throw new Exception();
                    Expression<Action> e = () => throw new Exception();
                    EventHandler h = (o, h) => throw new Exception();
                }
            }
        """;

        var result = GetExceptions(code);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Test_GetExceptionsForOperation_DoesNotIgnoreInvokedLambda()
    {
        var code = $$"""
            using System;
            using System.Linq.Expressions;
            class Test
            {
                public void TestMethod(int i = 10){
                    Action a = () => throw new Exception();
                    Expression<Action> e = () => throw new NullReferenceException();
                    EventHandler h = (o, h) => throw new ArgumentNullException();
        a = () => throw new Exception();
        Action b = a = (true == true ? (Action)(() => throw new NullReferenceException()) : (Action)(() => throw new Exception()));
                    a();
                    e.Compile().Invoke();

                    ((Action)(() => throw new Exception()))();
                    (true == true ? (Action)(() => throw new NullReferenceException()) : (Action)(() => throw new Exception()))();
                    var r = a.BeginInvoke((t) => { }, null);
                    a.EndInvoke(r);

                    int.TryParse("10", out var result);


                    void Inner(){}
                    var inne = Inner;
                }
            }
        """;

        var result = GetExceptions(code);

        Assert.That(result, Is.Not.Empty);
        Assert.That(result.Length, Is.EqualTo(3));
        Assert.That(result.Select(r => r.Name).Count(), Is.EquivalentTo(new[] { nameof(Exception), nameof(NullReferenceException), nameof(ArgumentNullException) }));
    }
}
