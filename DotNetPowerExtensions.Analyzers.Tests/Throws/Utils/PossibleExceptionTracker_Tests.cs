
using DotNetPowerExtensions.Analyzers.Throws;
using Microsoft.CodeAnalysis.CSharp;

namespace DotNetPowerExtensions.Analyzers.Tests.Throws.Utils;

internal class PossibleExceptionTracker_Tests
{
    [Test]
    public void Test_GetSymbolExceptions()
    {
        var tree = SyntaxFactory.ParseSyntaxTree($$"""
            using System;
            class Test
            {
                /// <exception cref="NotSupportedException"></exception>
                /// <exception cref="NullReferenceException"></exception>
                /// <exception cref="InvalidOperationException"></exception>
                [SequelPay.DotNetPowerExtensions.ThrowsAttribute<System.ArgumentNullException, System.InvalidOperationException>]
                public void TestMethod(){}
            }
        """);

        var semanticModel = ThrowsTestUtils.GetSemanticModel(tree);

        var method = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First();
        var symbol = semanticModel.GetDeclaredSymbol(method);

        var result = new PossibleExceptionTracker(semanticModel.Compilation).GetSymbolExceptions(symbol!);

        Assert.That(result.Count(), Is.AtMost(4));

        Assert.That(result.Select(r => r.Name),
            Is.EquivalentTo(new[] { "NotSupportedException", "NullReferenceException", "InvalidOperationException", "ArgumentNullException" }));
    }
}
