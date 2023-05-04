using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using SequelPay.DotNetPowerExtensions.RoslynExtensions;

namespace DotNetPowerExtensions.RoslynExtensions.Tests;

public class SyntaxFactoryExtensions_Tests
{
    [Test]
    public void Test_ParseAttribute()
    {
        var expr = """[TestAttribute<Arg>("Str", typeof(Test))]""";

        var result = SyntaxFactoryExtensions.ParseAttribute(expr);

        result.ToFullString().Should().Be(expr);
    }

    [Test]
    [TestCase("public virtual string TestProp { get; set; }", ExpectedResult = "public override string TestProp { get => base.TestProp; set => base.TestProp = value; }")]
    [TestCase("public abstract string TestProp { get; set; }", ExpectedResult = "public override string TestProp { get; set; }")]
    [TestCase("public override string TestProp { get; set; }", ExpectedResult = "public override string TestProp { get => base.TestProp; set => base.TestProp = value; }")]
    [TestCase("public string TestProp { get; set; }", ExpectedResult = "public new string TestProp { get => base.TestProp; set => base.TestProp = value; }")]
    [TestCase("protected string TestProp { get; set; }", ExpectedResult = "protected new string TestProp { get => base.TestProp; set => base.TestProp = value; }")]
    [TestCase("protected string TestProp { get; }", ExpectedResult = "protected new string TestProp { get => base.TestProp; }")]
    [TestCase("protected string TestProp { set; private get; }", ExpectedResult = "protected new string TestProp { set => base.TestProp = value; }")]
    [TestCase("protected virtual string TestProp { set; private get; }", ExpectedResult = "protected override string TestProp { set => base.TestProp = value; }")]
    [TestCase("protected string TestProp { get; private set; }", ExpectedResult = "protected new string TestProp { get => base.TestProp; }")]
    [TestCase("protected virtual string TestProp { get; private set; }", ExpectedResult = "protected override string TestProp { get => base.TestProp; }")]
    public string Test_CreatePropertyOverride(string propExpr)
    {
        var prop = SyntaxFactory.ParseSyntaxTree(propExpr)
                                                        .GetRoot().DescendantNodes().OfType<PropertyDeclarationSyntax>().First();

        var result = SyntaxFactoryExtensions.CreatePropertyOverride(prop);

        return result.ToFullString();
    }
}
