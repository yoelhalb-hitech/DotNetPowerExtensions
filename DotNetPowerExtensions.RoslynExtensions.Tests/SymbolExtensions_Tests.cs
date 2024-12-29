
namespace SequelPay.DotNetPowerExtensions.RoslynExtensions.Tests;

public class SymbolExtensions_Tests
{
    private static SemanticModel GetSemanticModel(params SyntaxTree[] trees) => CSharpCompilation.Create("Test",
                                syntaxTrees: trees, references: new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) })
                            .GetSemanticModel(trees.Last());

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

        var symbol = GetSemanticModel(tree).Compilation.GetSymbolsWithName("TestField").First();
        var result = symbol!.GetContainerFullName();

        result.Should().Be("A.B.C.T+T1");
    }


    [Test]
    public void Test_IsGenericEqual()
    {
        var tree = SyntaxFactory.ParseSyntaxTree($$"""
            class Test
            {
                public List<int> ListInt { get; set; }
                public List<string> ListString { get; set; }
            }
        """);

        var semanticModel = GetSemanticModel(tree);

        var props = tree.GetRoot().DescendantNodes().OfType<PropertyDeclarationSyntax>();
        var propTypeSymbols = props.Select(prop => semanticModel.GetDeclaredSymbol(prop)!.Type).OfType<INamedTypeSymbol>().ToList();

        var result = propTypeSymbols.First()!.IsGenericEqual(propTypeSymbols.Last()!);

        result.Should().Be(true);
    }

    [Test]
    public void Test_IsGenericEqual_WithOpenAndConstructedAttribute()
    {
        var tree = SyntaxFactory.ParseSyntaxTree($$"""
            class TestAttribute<T1> : System.Attribute {}

            class Test
            {
                [TestAttribute<System.Type>]
                public void Testing(){}
            }
        """);
        var semanticModel = GetSemanticModel(tree);

        var attr = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        var attrSymbol = semanticModel.GetDeclaredSymbol(attr);

        var method = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First();
        var methodSymbol = semanticModel.GetDeclaredSymbol(method);

        var result = methodSymbol!.GetAttributes().First().AttributeClass!.IsGenericEqual(attrSymbol);

        result.Should().Be(true);
    }

    [Test]
    public void Test_IsGenericEqual_WithTwoConstructedAttributes()
    {
        var tree = SyntaxFactory.ParseSyntaxTree($$"""
            class TestAttribute<T1> : System.Attribute {}

            class Test
            {
                [TestAttribute<System.Type>]
                public void Testing1(){}

                [TestAttribute<string>]
                public void Testing2(){}
            }
        """);
        var semanticModel = GetSemanticModel(tree);

        var method1 = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First();
        var method1Symbol = semanticModel.GetDeclaredSymbol(method1);

        var method2 = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Last();
        var method2Symbol = semanticModel.GetDeclaredSymbol(method2);

        var result = method1Symbol!.GetAttributes().First().AttributeClass!.IsGenericEqual(method2Symbol!.GetAttributes().First().AttributeClass);

        result.Should().Be(true);
    }

    [Test]
    public void Test_IsGenericEqual_WithOpenAndConstructed()
    {
        var tree = SyntaxFactory.ParseSyntaxTree($$"""
            class Test<T>
            {
                public static Test<string> TestString { get; set; }
            }
        """);

        var semanticModel = GetSemanticModel(tree);

        var c = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        var symbol = semanticModel.GetDeclaredSymbol(c);

        var prop = tree.GetRoot().DescendantNodes().OfType<PropertyDeclarationSyntax>().First();
        var propTypeSymbol = (INamedTypeSymbol)semanticModel.GetDeclaredSymbol(prop)!.Type;

        var result = propTypeSymbol.IsGenericEqual(symbol);

        result.Should().Be(true);
    }

    [Test]
    [TestCase("class TestAttribute : System.Attribute {}", "[TestAttribute]", ExpectedResult = true, Description = "None")]
    [TestCase("class TestAttribute : System.Attribute {}", "", ExpectedResult = false, Description = "None")]
    [TestCase("class TestAttribute<T> : System.Attribute {}", "[TestAttribute<System.Type>]", ExpectedResult = true, Description = "Generic")]
    [TestCase("class TestAttribute<T> : System.Attribute {}", "", ExpectedResult = false, Description = "Generic")]
    [TestCase("class TestAttribute : System.Attribute {} class TestSubAttribute : TestAttribute {}", "[TestSubAttribute]", ExpectedResult = true, Description = "Sub")]
    [TestCase("class TestAttribute : System.Attribute {} class TestSubAttribute : TestAttribute {}", "", ExpectedResult = false, Description = "Sub")]
    [TestCase("class TestAttribute : System.Attribute {} class TestSubAttribute<T> : TestAttribute {}", "[TestSubAttribute<System.Type>]", ExpectedResult = true, Description = "Sub")]
    [TestCase("class TestAttribute : System.Attribute {} class TestSubAttribute<T> : TestAttribute {}", "", ExpectedResult = false, Description = "Sub")]
    [TestCase("class TestAttribute : System.Attribute {} class TestSubAttribute<T, T2> : TestAttribute {}", "[TestSubAttribute<System.Type, int>]", ExpectedResult = true, Description = "Sub2")]
    [TestCase("class TestAttribute : System.Attribute {} class TestSubAttribute<T, T2> : TestAttribute {}", "", ExpectedResult = false, Description = "Sub2")]
    public bool Test_HasAttribute(string attributeDecl, string attributeUse)
    {
        var tree = SyntaxFactory.ParseSyntaxTree($$"""
            {{attributeDecl}}

            class Test
            {
                {{attributeUse}}
                public void Testing(){}
            }
        """);
        var semanticModel = GetSemanticModel(tree);

        var attr = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        var attrSymbol = semanticModel.GetDeclaredSymbol(attr);

        var method = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First();
        var methodSymbol = semanticModel.GetDeclaredSymbol(method);

        var result = methodSymbol!.HasAttribute(attrSymbol);

        return result;
    }

    [Test]
    public void Test_HasSameBaseDecleration_WhenSameReturnsTrue()
    {
        var source = """
        public class DeclareTypeBase
        {
            public virtual int TestMethod(int i) => 10;
        }
        public class DeclareType : DeclareTypeBase
        {
            public override int TestMethod(int i) => 10;
        }
        public class DeclareTypeSub : DeclareType
        {
            public override int TestMethod(int i) => 10;
        }
        """;

        var tree = SyntaxFactory.ParseSyntaxTree(source);
        var subClass = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First(c => c.Identifier.ValueText == "DeclareTypeSub");

        var symbol = GetSemanticModel(tree).GetDeclaredSymbol(subClass);
        var methodWithParams = symbol!.GetMembers().OfType<IMethodSymbol>().First(m => m.Parameters.Any());

        var result = methodWithParams.HasSameBaseDecleration(methodWithParams.OverriddenMethod!);

        result.Should().BeTrue();
    }

    [Test]
    public void Test_HasSameBaseDecleration_WhenNotSameReturnsFalse()
    {
        var source = """
        public class DeclareTypeBase
        {
            public virtual int TestMethod() => 10;
            public virtual int TestMethod(int i) => 10;
        }
        public class DeclareType : DeclareTypeBase
        {
            public override int TestMethod() => 10;
            public override int TestMethod(int i) => 10;
        }
        public class DeclareTypeSub : DeclareType
        {
            public override int TestMethod() => 10;
            public override int TestMethod(int i) => 10;
        }
        """;

        var tree = SyntaxFactory.ParseSyntaxTree(source);
        var subClass = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First(c => c.Identifier.ValueText == "DeclareTypeSub");

        var symbol = GetSemanticModel(tree).GetDeclaredSymbol(subClass);
        var methodWithParams = symbol!.GetMembers().OfType<IMethodSymbol>().First(m => m.Parameters.Any());
        var methodWithNoParams = symbol!.GetMembers().OfType<IMethodSymbol>().First(m => !m.Parameters.Any());

        var result = methodWithParams.HasSameBaseDecleration(methodWithNoParams);

        result.Should().BeFalse();
    }

    [Test]
    public void Test_HasSameBaseDecleration_WhenShadowedReturnsFalse()
    {
        var source = """
        public class DeclareTypeBase
        {
            public virtual int TestProp { get; set };
        }
        public class DeclareType : DeclareTypeBase
        {
            public override int TestProp { get; set };
          }
        public class DeclareTypeSub : DeclareType
        {
            public new  int TestProp { get; set };
        }
        """;

        var tree = SyntaxFactory.ParseSyntaxTree(source);
        var subClass = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First(c => c.Identifier.ValueText == "DeclareTypeSub");

        var symbol = GetSemanticModel(tree).GetDeclaredSymbol(subClass);
        var subProp = symbol!.GetMembers().OfType<IPropertySymbol>().First();
        var baseProp = symbol!.BaseType!.GetMembers().OfType<IPropertySymbol>().First();

        var result = subProp.HasSameBaseDecleration(baseProp);

        result.Should().BeFalse();
    }
}
