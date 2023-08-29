using Microsoft.CodeAnalysis;

namespace DotNetPowerExtensions.RoslynExtensions.Tests;

public class EventSymbolExtensions_Tests
{
    private static SemanticModel GetSemanticModel(params SyntaxTree[] trees) => CSharpCompilation.Create("Test",
                                syntaxTrees: trees, references: new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) })
                            .GetSemanticModel(trees.Last());

    [Test]
    public void Test_GetEventOverrideChain()
    {
        var source = """
        public class DeclareTypeBase
        {
            public virtual event EventArgs? TestEvent;
        }
        public class DeclareType : DeclareTypeBase
        {
            public override event EventArgs? TestEvent;
        }
        public class DeclareTypeSub : DeclareType
        {
            public override event EventArgs? TestEvent;
        }
        """;

        var tree = SyntaxFactory.ParseSyntaxTree(source);
        var subClass = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First(c => c.Identifier.ValueText == "DeclareTypeSub");

        var symbol = GetSemanticModel(tree).GetDeclaredSymbol(subClass);
        var evt = symbol!.GetMembers().OfType<IEventSymbol>().First();

        var result = evt.GetEventOverrideChain().ToArray();

        result.Should().NotBeNullOrEmpty();
        result.Length.Should().Be(2);

        result!.First().Name.Should().Be("TestEvent");
        result.First().ContainingType.Name.Should().Be("DeclareType");

        result.Last().Name.Should().Be("TestEvent");
        result.Last().ContainingType.Name.Should().Be("DeclareTypeBase");
    }

    [Test]
    public void Test_GetBaseEvent()
    {
        var source = """
        public class DeclareTypeBase
        {
            public virtual event EventArgs? TestEvent;
        }
        public class DeclareType : DeclareTypeBase
        {
            public override event EventArgs? TestEvent;
        }
        public class DeclareTypeSub : DeclareType
        {
            public override event EventArgs? TestEvent;
        }
        """;

        var tree = SyntaxFactory.ParseSyntaxTree(source);
        var subClass = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First(c => c.Identifier.ValueText == "DeclareTypeSub");

        var symbol = GetSemanticModel(tree).GetDeclaredSymbol(subClass);
        var e = symbol!.GetMembers().OfType<IEventSymbol>().First();

        var result = e.GetBaseEvent();

        result.Name.Should().Be("TestEvent");
        result.ContainingType.Name.Should().Be("DeclareTypeBase");
    }

    [Test]
    public void Test_GetBaseEvent_RetrunsSelf_WhenNoBase()
    {
        var source = """
        public class DeclareType
        {
            public event EventArgs? TestEvent;
        }
        """;

        var tree = SyntaxFactory.ParseSyntaxTree(source);
        var subClass = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First(c => c.Identifier.ValueText == "DeclareType");

        var symbol = GetSemanticModel(tree).GetDeclaredSymbol(subClass);
        var e = symbol!.GetMembers().OfType<IEventSymbol>().First();

        var result = e.GetBaseEvent();

        result.Name.Should().Be("TestEvent");
        result.ContainingType.Name.Should().Be("DeclareType");
    }
}
