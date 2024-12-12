﻿using AutoMockFixture.NUnit3;
using Microsoft.CodeAnalysis.CSharp;
using SequelPay.DotNetPowerExtensions.Reflection;
using SyntaxGen = SequelPay.DotNetPowerExtensions.Roslyn.SyntaxGeneration;

namespace DotNetPowerExtensions.Roslyn.SyntaxGeneration.CSharp.Tests;

internal class SyntaxGeneration_OutParam_Tests
{
    [Test]
    [TestCase("int")]
    [TestCase("uint")]
    [TestCase("string")]
    [TestCase("void")]
    [TestCase("char")]
    [TestCase("object")]
    public void Test_OutParam_WithPredefinedTypeName(string typeName)
    {
        var result = SyntaxGen.OutParam(typeName, "Test");

        result.Type.Should().NotBeNull();
        result.Type!.Kind().Should().Be(SyntaxKind.PredefinedType);
        result.Type.ToString().Should().Be(typeName);

        result.Identifier.Value.Should().Be("Test");

        result.Modifiers.First().Text.Should().Be("out");
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
    public void Test_OutParam_WithPredefinedTypes<T>()
    {
        var result = SyntaxGen.OutParam<T>("Test");

        result.Type.Should().NotBeNull();
        result.Type!.Kind().Should().Be(SyntaxKind.PredefinedType);
        result.Type.ToString().Should().Be(typeof(T).ToCSharpTypeString(false, false, null));

        result.Identifier.Value.Should().Be("Test");

        result.Modifiers.First().Text.Should().Be("out");
    }

    [Test]
    [TestCase("Int32")]
    [TestCase("String")]
    [TestCase("List<string>")]
    public void Test_OutParam_WithNonPredefinedTypeName(string typeName)
    {
        var result = SyntaxGen.OutParam(typeName, "Test");

        result.Type.Should().NotBeNull();
        result.Type!.Kind().Should().Be(SyntaxKind.IdentifierName);
        result.Type.ToString().Should().Be(typeName);

        result.Identifier.Value.Should().Be("Test");

        result.Modifiers.First().Text.Should().Be("out");
    }
}