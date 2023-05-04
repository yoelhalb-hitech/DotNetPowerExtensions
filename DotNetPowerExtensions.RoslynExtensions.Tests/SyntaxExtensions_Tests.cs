using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SequelPay.DotNetPowerExtensions.RoslynExtensions;

namespace DotNetPowerExtensions.RoslynExtensions.Tests;

public class SyntaxExtensions_Tests
{
    [Test]
    public void Test_GetNamespace()
    {
        var c = SyntaxFactory.ParseSyntaxTree("namespace A.B { namespace C { class T{}}}").GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

        var result = c.GetNamespace();

        result.Should().Be("A.B.C");
    }

    [Test]
    public void Test_GetContainerFullName()
    {
        var f = SyntaxFactory.ParseSyntaxTree("namespace A.B { namespace C { class T{ class T1 { public string TestField; } }}}")
                                                        .GetRoot().DescendantNodes().OfType<FieldDeclarationSyntax>().First();

        var result = f.GetContainerFullName();

        result.Should().Be("A.B.C.T+T1");
    }

    [Test]
    public void Test_GetName()
    {
        var source = """
        public class DeclareType
        {
            public static string TestProp { get; set; } = "T";
            public string NonStaticField = "X";
        }

        public class DeclareType<T>
        {
            public static string TestProp1 { get; set; } = "T";
            public static T TestField = default(T);
        }

        var x = 98;
        var y = new DeclareType();
        var a = new { TestInline = 10, DeclareType.TestProp, x, DeclareType<int>.TestProp1, DeclareType<int>.TestField, y.NonStaticField };
        """;

        var f = SyntaxFactory.ParseSyntaxTree(source).GetRoot().DescendantNodes().OfType<AnonymousObjectCreationExpressionSyntax>().First();
        var result = f!.DescendantNodes().OfType<AnonymousObjectMemberDeclaratorSyntax>().Select(m => m.GetName()).ToArray();

        result.Should().BeEquivalentTo(new[] { "TestInline", "TestProp", "x", "TestProp1", "TestField", "NonStaticField" } );
    }
}
