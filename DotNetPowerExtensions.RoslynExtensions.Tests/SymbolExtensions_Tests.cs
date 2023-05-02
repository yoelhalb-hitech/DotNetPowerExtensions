using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SequelPay.DotNetPowerExtensions.RoslynExtensions;

namespace DotNetPowerExtensions.RoslynExtensions.Tests;

internal class SymbolExtensions_Tests
{
    private static SemanticModel GetSemanticModel(SyntaxTree tree) => CSharpCompilation.Create("Test",
                                syntaxTrees: new[] { tree }, references: new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) })
                            .GetSemanticModel(tree);

    [Test]
    public void Test_GetNamespace()
    {
        var tree = SyntaxFactory.ParseSyntaxTree("namespace A.B { namespace C { class T{}}}");
        var c = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

        var symbol = GetSemanticModel(tree).GetDeclaredSymbol(c);

        var result = symbol!.GetNamespace();

        result.Should().Be("A.B.C");
    }

    [Test]
    public void Test_GetContainerFullName()
    {
        var tree = SyntaxFactory.ParseSyntaxTree("namespace A.B { namespace C { class T{ class T1 { public string TestField; } }}}");
        //var f = tree.GetRoot().DescendantNodes().OfType<FieldDeclarationSyntax>().First();

        var symbol = GetSemanticModel(tree).Compilation.GetSymbolsWithName("TestField").First();// .GetDeclaredSymbol(f) doesn't work for whater reason;
        var result = symbol!.GetContainerFullName();

        result.Should().Be("A.B.C.T+T1");
    }

    [Test]
    public void Test_ToStringWithoutNamesapce()
    {
        var tree = SyntaxFactory.ParseSyntaxTree("namespace A.B { namespace C { class T{class T1 {}}}}");
        var c = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Last();

        var symbol = GetSemanticModel(tree).GetDeclaredSymbol(c);

        var result = symbol!.ToStringWithoutNamesapce();

        result.Should().Be("T.T1");
    }

    [Test]
    public void Test_ToTypeSyntax()
    {
        var tree = SyntaxFactory.ParseSyntaxTree("namespace A.B { namespace C { class T { class T1 {}}}}");
        var c = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Last();

        var symbol = GetSemanticModel(tree).GetDeclaredSymbol(c);

        var result = symbol!.ToTypeSyntax();

        result.ToFullString().Should().Be("T.T1");
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
