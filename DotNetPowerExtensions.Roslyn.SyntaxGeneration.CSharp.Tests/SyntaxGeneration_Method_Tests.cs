using AutoMockFixture.NUnit3;
using Microsoft.CodeAnalysis.CSharp;
using SequelPay.DotNetPowerExtensions.Reflection;
using SyntaxGen = SequelPay.DotNetPowerExtensions.Roslyn.SyntaxGeneration;

namespace DotNetPowerExtensions.Roslyn.SyntaxGeneration.CSharp.Tests;

internal class SyntaxGeneration_Method_Tests
{
    [Test]
    [TestCase("int")]
    [TestCase("uint")]
    [TestCase("string")]
    [TestCase("void")]
    [TestCase("char")]
    [TestCase("object")]
    public void Test_Method_WithPredefinedTypeName(string typeName)
    {
        var result = SyntaxGen.Method(typeName, "Test");

        result.ReturnType.Kind().Should().Be(SyntaxKind.PredefinedType);
        result.ReturnType.ToString().Should().Be(typeName);

        result.Identifier.Value.Should().Be("Test");

        result.ParameterList.Parameters.Should().BeEmpty();
    }

    [Test]
    [TestCase<int>]
    [TestCase<uint>]
    [TestCase<string>]
    [TestCase<char>]
    [TestCase<object>]
    [TestCase<Int32>]
    [TestCase<UInt32>]
    [TestCase<String>]
    [TestCase<Char>]
    [TestCase<Object>]
    public void Test_Method_WithPredefinedTypes<T>()
    {
        var result = SyntaxGen.Method<T>("Test");

        result.ReturnType.Kind().Should().Be(SyntaxKind.PredefinedType);
        result.ReturnType.ToString().Should().Be(typeof(T).ToCSharpTypeString(false, false, null));

        result.Identifier.Value.Should().Be("Test");

        result.ParameterList.Parameters.Should().BeEmpty();
    }

    [Test]
    [TestCase("Int32")]
    [TestCase("String")]
    [TestCase("List<string>")]
    public void Test_Method_WithNonPredefinedTypeName(string typeName)
    {
        var result = SyntaxGen.Method(typeName, "Test");

        result.ReturnType.Kind().Should().Be(SyntaxKind.IdentifierName);
        result.ReturnType.ToString().Should().Be(typeName);

        result.Identifier.Value.Should().Be("Test");

        result.ParameterList.Parameters.Should().BeEmpty();
    }

    [Test]
    public void Test_VoidMethod()
    {
        var result = SyntaxGen.VoidMethod("Test");

        result.ReturnType.Kind().Should().Be(SyntaxKind.PredefinedType);
        result.ReturnType.ToString().Should().Be("void");

        result.Identifier.Value.Should().Be("Test");

        result.ParameterList.Parameters.Should().BeEmpty();
    }

    [Test]
    public void Test_Method_WithParameters()
    {
        var parameters = new[] {
             SyntaxGen.Param<int>("param1"),
             SyntaxGen.Param<List<string>>("param2"),
             SyntaxGen.OutParam<IEnumerable<char>>("param3"),
             SyntaxGen.RefParam<IEnumerable<Type>>("param4"),
        };
        var result = SyntaxGen.Method<string>("Test", parameters);

        result.ReturnType.Kind().Should().Be(SyntaxKind.PredefinedType);
        result.ReturnType.ToString().Should().Be("string");

        result.Identifier.Value.Should().Be("Test");

        result.ParameterList.Parameters.Should().NotBeEmpty();
#pragma warning disable NullConditionalAssertion // Justification: null operator is inside lambda
        result.ParameterList.Parameters.Select(p => p.Type?.ToString())
                    .Should().BeEquivalentTo(parameters.Select(p => p.Type!.ToString()));
        result.ParameterList.Parameters.Select(p => p.Type?.Kind())
            .Should().BeEquivalentTo(parameters.Select(p => p.Type!.Kind()));
#pragma warning restore NullConditionalAssertion
        result.ParameterList.Parameters.Select(p => p.Identifier.Text).Should().BeEquivalentTo(parameters.Select(p => p.Identifier.Text));
    }
}
