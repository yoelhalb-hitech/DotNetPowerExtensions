using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SequelPay.DotNetPowerExtensions.RoslynExtensions;

namespace DotNetPowerExtensions.RoslynExtensions.Tests;

public class SymbolExtensions_Tests
{
    private static SemanticModel GetSemanticModel(params SyntaxTree[] trees) => CSharpCompilation.Create("Test",
                                syntaxTrees: trees, references: new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) })
                            .GetSemanticModel(trees.Last());

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
        var propTypeSymbols = props.Select(prop => semanticModel.GetDeclaredSymbol(prop)!.Type).OfType<INamedTypeSymbol>();

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
        var propTypeSymbols = props.Select(prop => semanticModel.GetDeclaredSymbol(prop)!.Type).OfType<INamedTypeSymbol>();

        var result = propTypeSymbols.First()!.IsGenericEqualOrSubOf(propTypeSymbols.Last()!);

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
        var attr = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        var attrSymbol = GetSemanticModel(tree).GetDeclaredSymbol(attr);

        var method = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First();
        var methodSymbol = GetSemanticModel(tree).GetDeclaredSymbol(method);

        var result = methodSymbol!.HasAttribute(attrSymbol);

        return result;
    }

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
        var result = symbol!.GetConstructors(false);

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

    [Test]
    public void Test_GetConstructorChain_WithDefaultBase()
    {
        var source = """
        public class DeclareTypeBaseBase
        {
            public DeclareTypeBaseBase(){}
            public DeclareTypeBaseBase(string s){}
        }
        public class DeclareTypeBase : DeclareTypeBaseBase
        {
            public DeclareTypeBase(){}
            public DeclareTypeBase(string s){}
        }
        public class DeclareType: DeclareTypeBase
        {
            public DeclareType(string s){}
        }
        """;

        var tree = SyntaxFactory.ParseSyntaxTree(source);
        var subClass = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First(c => c.Identifier.ValueText == "DeclareType");

        var symbol = GetSemanticModel(tree).GetDeclaredSymbol(subClass);
        var ctor = symbol!.GetConstructors(false).First();

        var result = ctor.GetConstructorChain(GetSemanticModel(tree)).ToArray();

        result.Should().NotBeNullOrEmpty();
        result.Length.Should().Be(2);

        result!.First().Name.Should().Be(".ctor");
        result.First().Parameters.Should().BeEmpty();
        result.First().ContainingType.Name.Should().Be("DeclareTypeBase");

        result.Last().Name.Should().Be(".ctor");
        result.Last().Parameters.Should().BeEmpty();
        result.Last().ContainingType.Name.Should().Be("DeclareTypeBaseBase");
    }

    [Test]
    public void Test_GetConstructorChain_WithDefaultBase_WhenMultipleSourceFiles()
    {
        var source1 = """
        public class DeclareTypeBaseBase
        {
            public DeclareTypeBaseBase(){}
            public DeclareTypeBaseBase(string s){}
        }
        """;
        var source2 = """
        public class DeclareTypeBase : DeclareTypeBaseBase
        {
            public DeclareTypeBase(){}
            public DeclareTypeBase(string s){}
        }
        """;
        var source3 = """
        public class DeclareType: DeclareTypeBase
        {
            public DeclareType(string s){}
        }
        """;

        var tree = SyntaxFactory.ParseSyntaxTree(source3);
        var subClass = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First(c => c.Identifier.ValueText == "DeclareType");

        var semanticModel = GetSemanticModel(SyntaxFactory.ParseSyntaxTree(source1), SyntaxFactory.ParseSyntaxTree(source2), tree);
        var symbol = semanticModel.GetDeclaredSymbol(subClass);
        var ctor = symbol!.GetConstructors(false).First();

        var result = ctor.GetConstructorChain(semanticModel).ToArray();

        result.Should().NotBeNullOrEmpty();
        result.Length.Should().Be(2);

        result!.First().Name.Should().Be(".ctor");
        result.First().Parameters.Should().BeEmpty();
        result.First().ContainingType.Name.Should().Be("DeclareTypeBase");

        result.Last().Name.Should().Be(".ctor");
        result.Last().Parameters.Should().BeEmpty();
        result.Last().ContainingType.Name.Should().Be("DeclareTypeBaseBase");
    }

    [Test]
    public void Test_GetConstructorChain_WithThisAndBase()
    {
        var source = """
        public class DeclareTypeBaseBase
        {
            public DeclareTypeBaseBase(){}
            public DeclareTypeBaseBase(string s){}
        }
        public class DeclareTypeBase : DeclareTypeBaseBase
        {
            public DeclareTypeBase(){}
            public DeclareTypeBase(string s){}
        }
        public class DeclareType: DeclareTypeBase
        {
            public DeclareType(string s) : this(10){}
            public DeclareType(int s) : base("test"){}
        }
        """;

        var tree = SyntaxFactory.ParseSyntaxTree(source);
        var subClass = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First(c => c.Identifier.ValueText == "DeclareType");

        var symbol = GetSemanticModel(tree).GetDeclaredSymbol(subClass);
        var ctor = symbol!.GetConstructors(false).First(c => c.Parameters.Any(p => p.Type.SpecialType == SpecialType.System_String));

        var result = ctor.GetConstructorChain(GetSemanticModel(tree)).ToArray();

        result.Should().NotBeNullOrEmpty();
        result.Length.Should().Be(3);

        result!.First().Name.Should().Be(".ctor");
        result.First().Parameters.Should().NotBeEmpty();
        result.First().Parameters.First().Type.SpecialType.Should().Be(SpecialType.System_Int32);
        result.First().ContainingType.Name.Should().Be("DeclareType");

        result.Skip(1).First().Name.Should().Be(".ctor");
        result.Skip(1).First().Parameters.Should().NotBeEmpty();
        result.Skip(1).First().Parameters.First().Type.SpecialType.Should().Be(SpecialType.System_String);
        result.Skip(1).First().ContainingType.Name.Should().Be("DeclareTypeBase");

        result.Last().Name.Should().Be(".ctor");
        result.Last().Parameters.Should().BeEmpty();
        result.Last().ContainingType.Name.Should().Be("DeclareTypeBaseBase");
    }

    [Test]
    public void Test_GetConstructorChain_WithThisAndBase_WhenMultipleSourceFiles()
    {
        var source1 = """
        public class DeclareTypeBaseBase
        {
            public DeclareTypeBaseBase(){}
            public DeclareTypeBaseBase(string s):this(){}
        }
        """;
        var source2 = """
        public class DeclareTypeBase : DeclareTypeBaseBase
        {
            public DeclareTypeBase():base("testing"){}
            public DeclareTypeBase(string s):this(){}
        }
        """;
        var source3 = """
        public class DeclareType: DeclareTypeBase
        {
            public DeclareType(string s) : this(10){}
            public DeclareType(int s) : base("test"){}
        }
        """;

        var tree = SyntaxFactory.ParseSyntaxTree(source3);
        var subClass = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First(c => c.Identifier.ValueText == "DeclareType");

        var semanticModel = GetSemanticModel(SyntaxFactory.ParseSyntaxTree(source1), SyntaxFactory.ParseSyntaxTree(source2), tree);
        var symbol = semanticModel.GetDeclaredSymbol(subClass);
        var ctor = symbol!.GetConstructors(false).First(c => c.Parameters.Any(p => p.Type.SpecialType == SpecialType.System_String));

        var result = ctor.GetConstructorChain(semanticModel).ToArray();

        result.Should().NotBeNullOrEmpty();
        result.Length.Should().Be(5);

        result!.First().Name.Should().Be(".ctor");
        result.First().Parameters.Should().NotBeEmpty();
        result.First().Parameters.First().Type.SpecialType.Should().Be(SpecialType.System_Int32);
        result.First().ContainingType.Name.Should().Be("DeclareType");

        result.Skip(1).First().Name.Should().Be(".ctor");
        result.Skip(1).First().Parameters.Should().NotBeEmpty();
        result.Skip(1).First().Parameters.First().Type.SpecialType.Should().Be(SpecialType.System_String);
        result.Skip(1).First().ContainingType.Name.Should().Be("DeclareTypeBase");

        result.Last().Name.Should().Be(".ctor");
        result.Last().Parameters.Should().BeEmpty();
        result.Last().ContainingType.Name.Should().Be("DeclareTypeBaseBase");
    }

    [Test]
    public void Test_GetConstructorChain_WithThisAndBase_AndDefaultBase_WhenMultipleSourceFiles()
    {
        var source1 = """
        public class DeclareTypeBaseBase
        {
            public DeclareTypeBaseBase(){}
            public DeclareTypeBaseBase(string s):this(){}
        }
        """;
        var source2 = """
        public class DeclareTypeBase : DeclareTypeBaseBase
        {
            public DeclareTypeBase(){}
            public DeclareTypeBase(string s):this(){}
        }
        """;
        var source3 = """
        public class DeclareType: DeclareTypeBase
        {
            public DeclareType(string s) : this(10){}
            public DeclareType(int s) : base("test"){}
        }
        """;

        var tree = SyntaxFactory.ParseSyntaxTree(source3);
        var subClass = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First(c => c.Identifier.ValueText == "DeclareType");

        var semanticModel = GetSemanticModel(SyntaxFactory.ParseSyntaxTree(source1), SyntaxFactory.ParseSyntaxTree(source2), tree);
        var symbol = semanticModel.GetDeclaredSymbol(subClass);
        var ctor = symbol!.GetConstructors(false).First(c => c.Parameters.Any(p => p.Type.SpecialType == SpecialType.System_String));

        var result = ctor.GetConstructorChain(semanticModel).ToArray();

        result.Should().NotBeNullOrEmpty();
        result.Length.Should().Be(4);

        result!.First().Name.Should().Be(".ctor");
        result.First().Parameters.Should().NotBeEmpty();
        result.First().Parameters.First().Type.SpecialType.Should().Be(SpecialType.System_Int32);
        result.First().ContainingType.Name.Should().Be("DeclareType");

        result.Skip(1).First().Name.Should().Be(".ctor");
        result.Skip(1).First().Parameters.Should().NotBeEmpty();
        result.Skip(1).First().Parameters.First().Type.SpecialType.Should().Be(SpecialType.System_String);
        result.Skip(1).First().ContainingType.Name.Should().Be("DeclareTypeBase");

        result.Last().Name.Should().Be(".ctor");
        result.Last().Parameters.Should().BeEmpty();
        result.Last().ContainingType.Name.Should().Be("DeclareTypeBaseBase");
    }


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

    [Test]
    public void Test_GetMethodOverrideChain()
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
        var method = symbol!.GetMembers().OfType<IMethodSymbol>().First(m => m.Parameters.Any());

        var result = method.GetMethodOverrideChain().ToArray();

        result.Should().NotBeNullOrEmpty();
        result.Length.Should().Be(2);

        result!.First().Name.Should().Be("TestMethod");
        result.First().Parameters.Length.Should().Be(1);
        result.First().ContainingType.Name.Should().Be("DeclareType");

        result.Last().Name.Should().Be("TestMethod");
        result.Last().Parameters.Length.Should().Be(1);
        result.Last().ContainingType.Name.Should().Be("DeclareTypeBase");
    }

    [Test]
    public void Test_GetBaseMethod()
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
        var method = symbol!.GetMembers().OfType<IMethodSymbol>().First(m => !m.Parameters.Any());

        var result = method.GetBaseMethod();

        result.Name.Should().Be("TestMethod");
        result.Parameters.Length.Should().Be(0);
        result.ContainingType.Name.Should().Be("DeclareTypeBase");
    }

    [Test]
    public void Test_GetBaseMethod_RetrunsSelf_WhenNoBase()
    {
        var source = """
        public class DeclareTypeBase
        {
            public virtual int TestMethod() => 10;
            public virtual int TestMethod(int i) => 10;
        }
        public class DeclareType
        {
            public new int TestMethod() => 10;
        }
        """;

        var tree = SyntaxFactory.ParseSyntaxTree(source);
        var subClass = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First(c => c.Identifier.ValueText == "DeclareType");

        var symbol = GetSemanticModel(tree).GetDeclaredSymbol(subClass);
        var method = symbol!.GetMembers().OfType<IMethodSymbol>().First(m => !m.Parameters.Any());

        var result = method.GetBaseMethod();

        result.Name.Should().Be("TestMethod");
        result.Parameters.Length.Should().Be(0);
        result.ContainingType.Name.Should().Be("DeclareType");
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
    public void Test_HaveSameBaseDecleration_WhenShadowedReturnsFalse()
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
