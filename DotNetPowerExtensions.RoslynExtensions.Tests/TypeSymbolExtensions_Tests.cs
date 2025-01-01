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

    #region GetAllFields

    [Test]
    public void Test_GetAllFields_NoBase()
    {
        var source = """
        public class DeclareType
        {
            public string field1;
            public int field2;
            protected List<string> protectedField1;
            protected int[] protectedField2;
            private Type privateField1;
            private Assembly privateField2;
        }
        """;

        var tree = SyntaxFactory.ParseSyntaxTree(source);
        var c = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

        var symbol = GetSemanticModel(tree).GetDeclaredSymbol(c);
        var result = symbol!.GetAllFields().ToList();

        result.Should().NotBeNullOrEmpty();
        result.Count().Should().Be(6);

        result.Select(r => r.Name).Should().BeEquivalentTo(new[] { "field1", "field2", "protectedField1", "protectedField2", "privateField1", "privateField2" } );
    }

    [Test]
    public void Test_GetAllFields_DoesNotIncludeCompilerGeneratedFields()
    {
        var source = """
        public class DeclareType
        {
            public string prop1 { get; set; }
        }
        """;

        var tree = SyntaxFactory.ParseSyntaxTree(source);
        var c = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

        var symbol = GetSemanticModel(tree).GetDeclaredSymbol(c);
        var result = symbol!.GetAllFields().ToList();

        result.Should().BeEmpty();
    }

    [Test]
    public void Test_GetAllFields_WithBase()
    {
        var source = """
        public class DeclareTypeBase
        {
            public string field1;
            public int field2;
            protected List<string> protectedField1;
            protected int[] protectedField2;
            private Type privateField1;
            private Assembly privateField2;
        }
        public class DeclareType : DeclareTypeBase
        {
            public string newField1;
            public int newField2;
            protected List<string> newProtectedField1;
            protected int[] newProtectedField2;
            private Type newPrivateField1;
            private Assembly newPrivateField2;
        }
        """;

        var tree = SyntaxFactory.ParseSyntaxTree(source);
        var c = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Last();

        var symbol = GetSemanticModel(tree).GetDeclaredSymbol(c);
        var result = symbol!.GetAllFields().ToList();

        result.Should().NotBeNullOrEmpty();
        result.Count().Should().Be(10);

        result.Select(r => r.Name).Should().BeEquivalentTo(new[]
        {
            "field1", "field2", "protectedField1", "protectedField2",
            "newField1", "newField2", "newProtectedField1", "newProtectedField2", "newPrivateField1", "newPrivateField2"
        });
    }

    [Test]
    public void Test_GetAllFields_IncludeShadow()
    {
        var source = """
        public class DeclareTypeBase
        {
            public string field1;
            public int field2;
            protected List<string> protectedField1;
            protected int[] protectedField2;
            private Type privateField1;
            private Assembly privateField2;
        }
        public class DeclareType : DeclareTypeBase
        {
            public string field1;
            public int field2;
            protected List<string> protectedField1;
            protected int[] protectedField2;
            private Type privateField1;
            private Assembly privateField2;
        }
        """;

        var tree = SyntaxFactory.ParseSyntaxTree(source);
        var c = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Last();

        var symbol = GetSemanticModel(tree).GetDeclaredSymbol(c);
        var result = symbol!.GetAllFields(true).ToList();

        result.Should().NotBeNullOrEmpty();
        result.Count().Should().Be(10);

        result.Select(r => r.Name).Should().BeEquivalentTo(new[]
        {
            "field1", "field2", "protectedField1", "protectedField2",
            "field1", "field2", "protectedField1", "protectedField2", "privateField1", "privateField2"
        });
    }

    [Test]
    public void Test_GetAllFields_NoIncludeShadow()
    {
        var source = """
        public class DeclareTypeBase
        {
            public string field1;
            public int field2;
            protected List<string> protectedField1;
            protected int[] protectedField2;
            private Type privateField1;
            private Assembly privateField2;
        }
        public class DeclareType : DeclareTypeBase
        {
            public string field1;
            public int field2;
            protected List<string> protectedField1;
            protected int[] protectedField2;
            private Type privateField1;
            private Assembly privateField2;
        }
        """;

        var tree = SyntaxFactory.ParseSyntaxTree(source);
        var c = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Last();

        var symbol = GetSemanticModel(tree).GetDeclaredSymbol(c);
        var result = symbol!.GetAllFields(false).ToList();

        result.Should().NotBeNullOrEmpty();
        result.Count().Should().Be(6);

        result.Select(r => r.Name).Should().BeEquivalentTo(new[]
        {
            "field1", "field2", "protectedField1", "protectedField2", "privateField1", "privateField2"
        });
    }

    #endregion

    #region GetAllProperties

    [Test]
    public void Test_GetAllProperties_NoBase()
    {
        var source = """
        public class DeclareType
        {
            public string Prop1 { get; set; }
            public int Prop2 { get; }
            protected List<string> protectedProp1 { set; }
            protected int[] protectedProp2 { get; set; }
            private Type privateProp1 { get; set; }
            private Assembly privateProp2 { set; }
        }
        """;

        var tree = SyntaxFactory.ParseSyntaxTree(source);
        var c = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

        var symbol = GetSemanticModel(tree).GetDeclaredSymbol(c);
        var result = symbol!.GetAllProperties().ToList();

        result.Should().NotBeNullOrEmpty();
        result.Count().Should().Be(6);

        result.Select(r => r.Name).Should().BeEquivalentTo(new[] { "Prop1", "Prop2", "protectedProp1", "protectedProp2", "privateProp1", "privateProp2" });
    }

    [Test]
    public void Test_GetAllProperties_WithBase()
    {
        var source = """
        public class DeclareTypeBase
        {
            public string Prop1 { get; set; }
            public int Prop2 { get; }
            protected List<string> protectedProp1 { set; }
            protected int[] protectedProp2 { get; set; }
            private Type privateProp1 { get; set; }
            private Assembly privateProp2 { set; }
        }
        public class DeclareType : DeclareTypeBase
        {
            public string newProp1 { get; set; }
            public int newProp2 { get; }
            protected List<string> newProtectedProp1 { set; }
            protected int[] newProtectedProp2 { get; set; }
            private Type newPrivateProp1 { get; set; }
            private Assembly newPrivateProp2 { set; }
        }
        """;

        var tree = SyntaxFactory.ParseSyntaxTree(source);
        var c = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Last();

        var symbol = GetSemanticModel(tree).GetDeclaredSymbol(c);
        var result = symbol!.GetAllProperties().ToList();

        result.Should().NotBeNullOrEmpty();
        result.Count().Should().Be(10);

        result.Select(r => r.Name).Should().BeEquivalentTo(new[]
        {
            "Prop1", "Prop2", "protectedProp1", "protectedProp2",
            "newProp1", "newProp2", "newProtectedProp1", "newProtectedProp2", "newPrivateProp1", "newPrivateProp2",
        });
    }

    [Test]
    public void Test_GetAllProperties_IncludeShadow()
    {
        var source = """
        public class DeclareTypeBase
        {
            public string Prop1 { get; set; }
            public int Prop2 { get; }
            protected List<string> protectedProp1 { set; }
            protected int[] protectedProp2 { get; set; }
            private Type privateProp1 { get; set; }
            private Assembly privateProp2 { set; }
        }
        public class DeclareType : DeclareTypeBase
        {
            public string Prop1 { get; set; }
            public int Prop2 { get; }
            protected List<string> protectedProp1 { set; }
            protected int[] protectedProp2 { get; set; }
            private Type privateProp1 { get; set; }
            private Assembly privateProp2 { set; }
        }
        """;

        var tree = SyntaxFactory.ParseSyntaxTree(source);
        var c = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Last();

        var symbol = GetSemanticModel(tree).GetDeclaredSymbol(c);
        var result = symbol!.GetAllProperties(true).ToList();

        result.Should().NotBeNullOrEmpty();
        result.Count().Should().Be(10);

        result.Select(r => r.Name).Should().BeEquivalentTo(new[]
        {
            "Prop1", "Prop2", "protectedProp1", "protectedProp2",
            "Prop1", "Prop2", "protectedProp1", "protectedProp2", "privateProp1", "privateProp2",
        });
    }

    [Test]
    public void Test_GetAllProperties_NoIncludeShadow()
    {
        var source = """
        public class DeclareTypeBase
        {
            public string Prop1 { get; set; }
            public int Prop2 { get; }
            protected List<string> protectedProp1 { set; }
            protected int[] protectedProp2 { get; set; }
            private Type privateProp1 { get; set; }
            private Assembly privateProp2 { set; }
        }
        public class DeclareType : DeclareTypeBase
        {
            public string Prop1 { get; set; }
            public int Prop2 { get; }
            protected List<string> protectedProp1 { set; }
            protected int[] protectedProp2 { get; set; }
            private Type privateProp1 { get; set; }
            private Assembly privateProp2 { set; }
        }
        """;

        var tree = SyntaxFactory.ParseSyntaxTree(source);
        var c = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Last();

        var symbol = GetSemanticModel(tree).GetDeclaredSymbol(c);
        var result = symbol!.GetAllProperties(false).ToList();

        result.Should().NotBeNullOrEmpty();
        result.Count().Should().Be(6);

        result.Select(r => r.Name).Should().BeEquivalentTo(new[]
        {
            "Prop1", "Prop2", "protectedProp1", "protectedProp2", "privateProp1", "privateProp2",
        });
    }

    #endregion
}
