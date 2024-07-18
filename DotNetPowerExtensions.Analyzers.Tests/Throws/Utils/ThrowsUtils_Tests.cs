using DotNetPowerExtensions.Analyzers.Throws;
using Microsoft.CodeAnalysis.CSharp;
using SequelPay.DotNetPowerExtensions;
using System.Reflection;

namespace DotNetPowerExtensions.Analyzers.Tests.Throws.Utils;

internal class ThrowsUtils_Tests
{
    [Test]
    public void Test_GetDocCommentExceptions()
    {
        var tree = SyntaxFactory.ParseSyntaxTree($$"""
            class Test
            {
                /// <exception cref="System.NotSupportedException"></exception>
                /// <exception cref="NullReferenceException"></exception>
                public void TestMethod(){}
            }
        """);

        var semanticModel = ThrowsTestUtils.GetSemanticModel(tree);
        var symbol = ThrowsTestUtils.GetFirstMethodSymbol(semanticModel);

        var result = ThrowsUtils.GetDocCommentExceptions(symbol!, semanticModel.Compilation);

        Assert.That(result.First().Item1, Is.EquivalentTo("System.NotSupportedException"));
        Assert.That(result.First().Item2, Is.Not.Null);
        Assert.That(result.First().Item2.Name, Is.EqualTo("NotSupportedException"));

        Assert.That(result.Last().Item1, Is.EquivalentTo("NullReferenceException"));
        Assert.That(result.Last().Item2, Is.Null);
    }

    [Test]
    public void Test_GetThrowsExceptions_NonGeneric()
    {
        var tree = SyntaxFactory.ParseSyntaxTree($$"""
            class Test
            {
                [SequelPay.DotNetPowerExtensions.ThrowsAttribute(typeof(System.ArgumentNullException), typeof(System.InvalidOperationException))]
                public void TestMethod(){}
            }
        """);

        var semanticModel = ThrowsTestUtils.GetSemanticModel(tree);

        var symbol = ThrowsTestUtils.GetFirstMethodSymbol(semanticModel);

        var result = ThrowsUtils.GetThrowsExceptions(symbol!, semanticModel.Compilation);

        Assert.That(result.Count(), Is.EqualTo(2));
        Assert.That(result.First().Name, Is.EqualTo("ArgumentNullException"));
        Assert.That(result.Last().Name, Is.EqualTo("InvalidOperationException"));
    }

    [Test]
    public void Test_GetThrowsExceptions_Generic()
    {
        var tree = SyntaxFactory.ParseSyntaxTree($$"""
            class Test
            {
                [SequelPay.DotNetPowerExtensions.ThrowsAttribute<System.ArgumentNullException, System.InvalidOperationException>]
                public void TestMethod(){}
            }
        """);

        var semanticModel = ThrowsTestUtils.GetSemanticModel(tree);
        var symbol = ThrowsTestUtils.GetFirstMethodSymbol(semanticModel);

        var result = ThrowsUtils.GetThrowsExceptions(symbol!, semanticModel.Compilation);

        Assert.That(result.Count(), Is.EqualTo(2));
        Assert.That(result.First().Name, Is.EqualTo("ArgumentNullException"));
        Assert.That(result.Last().Name, Is.EqualTo("InvalidOperationException"));
    }
}
