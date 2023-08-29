
namespace DotNetPowerExtensions.RoslynExtensions.Tests;

public class PropertySymbolExtensions_Tests
{
    private static SemanticModel GetSemanticModel(params SyntaxTree[] trees) => CSharpCompilation.Create("Test",
                                syntaxTrees: trees, references: new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) })
                            .GetSemanticModel(trees.Last());

    [Test]
    public void Test_GetPropertyOverrideChain()
    {
        var source = """
        public class DeclareTypeBase
        {
            public virtual string TestProp { get; set; } = "T";
        }
        public class DeclareType : DeclareTypeBase
        {
            public override string TestProp { get; set; } = "T";
        }
        public class DeclareTypeSub : DeclareType
        {
            public override string TestProp { get; set; } = "T";
        }
        """;

        var tree = SyntaxFactory.ParseSyntaxTree(source);
        var subClass = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First(c => c.Identifier.ValueText == "DeclareTypeSub");

        var symbol = GetSemanticModel(tree).GetDeclaredSymbol(subClass);
        var prop = symbol!.GetMembers().OfType<IPropertySymbol>().First();

        var result = prop.GetPropertyOverrideChain().ToArray();

        result.Should().NotBeNullOrEmpty();
        result.Length.Should().Be(2);

        result!.First().Name.Should().Be("TestProp");
        result.First().ContainingType.Name.Should().Be("DeclareType");

        result.Last().Name.Should().Be("TestProp");
        result.Last().ContainingType.Name.Should().Be("DeclareTypeBase");
    }

    [Test]
    public void Test_GetBaseProperty()
    {
        var source = """
        public class DeclareTypeBase
        {
            public virtual string TestProp { get; set; } = "T";
        }
        public class DeclareType : DeclareTypeBase
        {
            public override string TestProp { get; set; } = "T";
        }
        public class DeclareTypeSub : DeclareType
        {
            public override string TestProp { get; set; } = "T";
        }
        """;

        var tree = SyntaxFactory.ParseSyntaxTree(source);
        var subClass = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First(c => c.Identifier.ValueText == "DeclareTypeSub");

        var symbol = GetSemanticModel(tree).GetDeclaredSymbol(subClass);
        var prop = symbol!.GetMembers().OfType<IPropertySymbol>().First();

        var result = prop.GetBaseProperty();

        result.Name.Should().Be("TestProp");
        result.ContainingType.Name.Should().Be("DeclareTypeBase");
    }

    [Test]
    public void Test_GetBaseProperty_RetrunsSelf_WhenNoBase()
    {
        var source = """
        public class DeclareType
        {
            public string TestProp { get; set; } = "T";
        }
        """;

        var tree = SyntaxFactory.ParseSyntaxTree(source);
        var subClass = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First(c => c.Identifier.ValueText == "DeclareType");

        var symbol = GetSemanticModel(tree).GetDeclaredSymbol(subClass);
        var prop = symbol!.GetMembers().OfType<IPropertySymbol>().First();

        var result = prop.GetBaseProperty();

        result.Name.Should().Be("TestProp");
        result.ContainingType.Name.Should().Be("DeclareType");
    }
}
