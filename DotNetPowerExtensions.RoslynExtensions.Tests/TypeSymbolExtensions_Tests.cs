using System.Text;

namespace DotNetPowerExtensions.RoslynExtensions.Tests;

public class TypeSymbolExtensions_Tests
{
    private static SemanticModel GetSemanticModel(params SyntaxTree[] trees) => CSharpCompilation.Create("Test",
                                syntaxTrees: trees, references: new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) })
                            .GetSemanticModel(trees.Last());

    [Test]
    public void Test_IsGenericEqualOrSubOf()
    {
        var tree = SyntaxFactory.ParseSyntaxTree($$"""
            class Test
            {
                class TestBase{}
                class TestSub<T> : TestBase{}
                public TestSub<string> TestSubString { get; set; }
                public TestBase TestBase { get; set; }
            }
        """);

        var semanticModel = GetSemanticModel(tree);

        var props = tree.GetRoot().DescendantNodes().OfType<PropertyDeclarationSyntax>();
        var propTypeSymbols = props.Select(prop => semanticModel.GetDeclaredSymbol(prop)!.Type).OfType<INamedTypeSymbol>().ToList();

        var result = propTypeSymbols.First()!.IsGenericEqualOrSubOf(propTypeSymbols.Last()!, true);

        result.Should().Be(true);
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
    public void Test_GetConstructors()
    {
        var source = """
        public class DeclareType
        {
            public DeclareType(){}
            public DeclareType(string s){}
        }
        """;

        var tree = SyntaxFactory.ParseSyntaxTree(source);
        var c = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

        var symbol = GetSemanticModel(tree).GetDeclaredSymbol(c);
        var result = symbol!.GetConstructors(false).ToList();

        result.Should().NotBeNullOrEmpty();
        result.Count().Should().Be(2);

        result.First().Name.Should().Be(".ctor");
        result.First().Parameters.Should().BeEmpty();

        result.Last().Name.Should().Be(".ctor");
        result.Last().Parameters.Should().NotBeEmpty();
    }

    [Test]
    public void Test_GetDefaultConstructor()
    {
        var source = """
        public class DeclareType
        {
            public DeclareType(){}
            public DeclareType(string s){}
        }
        """;

        var tree = SyntaxFactory.ParseSyntaxTree(source);
        var c = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

        var symbol = GetSemanticModel(tree).GetDeclaredSymbol(c);
        var result = symbol!.GetDefaultConstructor();

        result.Should().NotBeNull();
        result!.Name.Should().Be(".ctor");
        result.Parameters.Should().BeEmpty();
    }

    private static string GetExpectedFullName(bool outerNS, bool innerNS,
                                        int outerGenericCount, bool hasInner, int innerGenericCount,
                                        bool hasInnerInner, int innerInnerGenericCount)
    {
        var sb = new StringBuilder();
        if (outerNS) { sb.Append("Outer."); }
        if (innerNS) { sb.Append("Inner."); }
        sb.Append("OuterType");
        if (outerGenericCount > 0) sb.Append("`" + outerGenericCount);
        if (hasInner) { sb.Append("+InnerType"); }
        if (innerGenericCount > 0) { sb.Append("`" + innerGenericCount); }
        if (hasInnerInner) { sb.Append("+InnerInnerType"); }
        if (innerInnerGenericCount > 0) { sb.Append("`" + innerInnerGenericCount); }

        return sb.ToString();
    }


    [Test]
    public void Test_GetFullName([Values(true, false)] bool outerNS, [Values(true, false)] bool innerNS,
                    [Values(true, false)] bool isStructOuter, [Values(0, 1, 2, 3)] int outerGenericCount,
                    [Values(true, false)] bool hasInner, [Values(true, false)] bool isStructInner, [Values(0, 1, 2, 3)] int innerGenericCount,
                    [Values(true, false)] bool hasInnerInner, [Values(true, false)] bool isStructInnerInner, [Values(0, 1, 2, 3)] int innerInnerGenericCount)
    {
        Assume.That(() => innerGenericCount > 0 || isStructInner || hasInnerInner ? hasInner : true);
        Assume.That(() => innerInnerGenericCount > 0 || isStructInnerInner ? hasInnerInner : true);

        var code = TestUtils.GetClassNamesCode(outerNS, innerNS, isStructOuter, outerGenericCount, hasInner, isStructInner, innerGenericCount,
                                                                            hasInnerInner, isStructInnerInner, innerInnerGenericCount);
        var (_, symbol) = TestUtils.GetModelAndTypeSymbol(code);
        var result = symbol.GetFullName();

        var expectedName = GetExpectedFullName(outerNS, innerNS, outerGenericCount, hasInner, innerGenericCount, hasInnerInner, innerInnerGenericCount);

        result.Should().Be(expectedName);
    }
}
