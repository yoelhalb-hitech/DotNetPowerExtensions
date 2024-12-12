
using System.Diagnostics.CodeAnalysis;

namespace DotNetPowerExtensions.RoslynExtensions.Tests;

public class MethodSymbolExtensions_Tests
{
    private static string GetThis(string arg) => $" : this({arg})";
    private static string GetBase(string arg) => $" : base({arg})";

    private static string GetSourceBaseBase(bool withThis) => $$"""
    public class DeclareTypeBaseBase
    {
        public DeclareTypeBaseBase(){}
        public DeclareTypeBaseBase(string s) {{(withThis ? GetThis("") : "")}} {}
    }
    """;

    private static string GetSourceBase(bool withThis, bool withBase) => $$"""
    public class DeclareTypeBase : DeclareTypeBaseBase
    {
        public DeclareTypeBase() {{(withBase ? GetBase("\"testing\"") : "")}} {}
        public DeclareTypeBase(string s) {{(withThis ? GetThis("") : "")}} {}
    }
    """;

    private static string GetSource(bool withThis, bool withBase) => $$"""
    public class DeclareType: DeclareTypeBase
    {
        public DeclareType(string s) {{(withThis ? GetThis("10") : "")}} {}
        public DeclareType(int s) {{(withBase ? GetBase("\"test\"") : "")}} {}
    }
    """;

    [Test]
    public void Test_GetConstructorChain_WhenImplicitCtor_AndNonTrivial_AndSameAssembly()
    {
        var source = """
        public class DeclareTypeBaseBase { public DeclareTypeBaseBase(){} public DeclareTypeBaseBase(string s){} }
        public class DeclareTypeBase : DeclareTypeBaseBase {}
        public class DeclareType : DeclareTypeBase {}
        """;

        var (semanticModel, symbol) = TestUtils.GetModelAndTypeSymbol(source);
        var ctor = symbol!.GetConstructors(false).First();

        var result = ctor.GetConstructorChain(semanticModel).ToArray();

        result.Should().NotBeNullOrEmpty();
        result.Length.Should().Be(2);

        result!.First().Name.Should().Be(".ctor");
        result.First().ContainingType.Name.Should().Be("DeclareTypeBase");

        result.Last().Name.Should().Be(".ctor");
        result.Last().ContainingType.Name.Should().Be("DeclareTypeBaseBase");
    }

    [Test]
    [TestCase(false, false, false, false, 2)]
    [TestCase(true, false, false, false, 3)]
    [TestCase(true, false, true, false, 3)]
    [TestCase(false, false, true, false, 2)]
    [TestCase(false, true, true, false, 2)]
    [TestCase(true, true, false, true, 3)]
    [TestCase(true, true, true, false, 4)]
    [TestCase(true, true, true, true, 5)]
    public void Test_GetConstructorChain_WhenSingleFile(bool hasThis, bool hasBase, bool hasThisInBaseClass, bool hasBaseInBaseClass, int resultCount)
    {
        var source = GetSourceBaseBase(hasThisInBaseClass) + GetSourceBase(hasThisInBaseClass, hasBaseInBaseClass) + GetSource(hasThis, hasBase);

        var (semanticModel, symbol) = TestUtils.GetModelAndTypeSymbol(source);
        var ctor = symbol!.GetConstructors(false).First();

        var result = ctor.GetConstructorChain(semanticModel).ToArray();

        result.Should().NotBeNullOrEmpty();
        result.Length.Should().Be(resultCount);

        result!.First().Name.Should().Be(".ctor");
        result.First().ContainingType.Name.Should().Be(hasThis ? "DeclareType" : "DeclareTypeBase");

        if (hasThis)
        {
            result.Skip(1).First().Name.Should().Be(".ctor");
            result.Skip(1).First().ContainingType.Name.Should().Be("DeclareTypeBase");
        }

        result.Last().Name.Should().Be(".ctor");
        result.Last().Parameters.Should().BeEmpty();
        result.Last().ContainingType.Name.Should().Be("DeclareTypeBaseBase");
    }

    [Test]
    [TestCase(false, false, false, false, 2)]
    [TestCase(true, false, false, false, 3)]
    [TestCase(true, false, true, false, 3)]
    [TestCase(false, false, true, false, 2)]
    [TestCase(false, true, true, false, 2)]
    [TestCase(true, true, false, true, 3)]
    [TestCase(true, true, true, false, 4)]
    [TestCase(true, true, true, true, 5)]
    public void Test_GetConstructorChain_WhenMultipleSourceFiles(bool hasThis, bool hasBase, bool hasThisInBaseClass, bool hasBaseInBaseClass, int resultCount)
    {
        var sources = new[] { GetSourceBaseBase(hasThisInBaseClass), GetSourceBase(hasThisInBaseClass, hasBaseInBaseClass), GetSource(hasThis, hasBase) };

        var (semanticModel, symbol) = TestUtils.GetModelAndTypeSymbol(sources);
        var ctor = symbol!.GetConstructors(false).First();

        var result = ctor.GetConstructorChain(semanticModel).ToArray();

        result.Should().NotBeNullOrEmpty();
        result.Length.Should().Be(resultCount);

        result!.First().Name.Should().Be(".ctor");
        result.First().ContainingType.Name.Should().Be(hasThis ? "DeclareType" : "DeclareTypeBase");

        if(hasThis)
        {
            result.Skip(1).First().Name.Should().Be(".ctor");
            result.Skip(1).First().ContainingType.Name.Should().Be("DeclareTypeBase");
        }

        result.Last().Name.Should().Be(".ctor");
        result.Last().Parameters.Should().BeEmpty();
        result.Last().ContainingType.Name.Should().Be("DeclareTypeBaseBase");
    }

    [Test]
    [TestCase(false, false, false, false, 2)]
    [TestCase(true, false, false, false, 3)]
    [TestCase(true, false, true, false, 3)]
    [TestCase(false, false, true, false, 2)]
    [TestCase(false, true, true, false, 2)]
    [TestCase(true, true, false, true, 3)]
    [TestCase(true, true, true, false, 4)]
    [TestCase(true, true, true, true, 5)]
    [SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035:Do not use APIs banned for analyzers", Justification = "We need File to load the module")]
    public void Test_GetConstructorChain_WithThisAndBase_WhenMultipleProjects(bool hasThis, bool hasBase, bool hasThisInBaseClass, bool hasBaseInBaseClass, int resultCount)
    {
        var dirPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dirPath);

        try
        {
            var compilation1 = TestUtils.GetCompilation("Test", new[] { SyntaxFactory.ParseSyntaxTree(GetSourceBaseBase(hasThisInBaseClass)) }, Array.Empty<string>());
            var outputFile1 = Path.Combine(dirPath, compilation1.AssemblyName + ".dll");
            using (var stream1 = new FileStream(outputFile1, FileMode.OpenOrCreate)) { compilation1.Emit(stream1).Success.Should().BeTrue(); }

            var compilation2 = TestUtils.GetCompilation("Test1", new[] { SyntaxFactory.ParseSyntaxTree(GetSourceBase(hasThisInBaseClass, hasBaseInBaseClass)) },
                                                                                                                                        new []{ outputFile1 });
            var outputFile2 = Path.Combine(dirPath, compilation2.AssemblyName + ".dll");
            using (var stream2 = new FileStream(outputFile2, FileMode.OpenOrCreate)) { compilation2.Emit(stream2).Success.Should().BeTrue(); }

            var tree = SyntaxFactory.ParseSyntaxTree(GetSource(hasThis, hasBase));
            var subClass = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First(c => c.Identifier.ValueText == "DeclareType");

            var semanticModel = TestUtils.GetCompilation("Test2", new[] { tree }, new[] { outputFile1, outputFile2 })
                                                                                            .GetSemanticModel(tree);


            var symbol = semanticModel.GetDeclaredSymbol(subClass);
            var ctor = symbol!.GetConstructors(false).First(c => c.Parameters.Any(p => p.Type.SpecialType == SpecialType.System_String));

            var result = ctor.GetConstructorChain(semanticModel).ToArray();

            result.Should().NotBeNullOrEmpty();
            result.Length.Should().Be(resultCount);

            result!.First().Name.Should().Be(".ctor");
            result.First().ContainingType.Name.Should().Be(hasThis ? "DeclareType" : "DeclareTypeBase");

            if (hasThis)
            {
                result.Skip(1).First().Name.Should().Be(".ctor");
                result.Skip(1).First().ContainingType.Name.Should().Be("DeclareTypeBase");
            }

            result.Last().Name.Should().Be(".ctor");
            result.Last().Parameters.Should().BeEmpty();
            result.Last().ContainingType.Name.Should().Be("DeclareTypeBaseBase");
        }
        finally
        {
            Directory.Delete(dirPath, recursive: true);
        }
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

        var (_, symbol) = TestUtils.GetModelAndTypeSymbol(source);
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

        var (_, symbol) = TestUtils.GetModelAndTypeSymbol(source);
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

        var (_, symbol) = TestUtils.GetModelAndTypeSymbol(source);
        var method = symbol!.GetMembers().OfType<IMethodSymbol>().First(m => !m.Parameters.Any());

        var result = method.GetBaseMethod();

        result.Name.Should().Be("TestMethod");
        result.Parameters.Length.Should().Be(0);
        result.ContainingType.Name.Should().Be("DeclareType");
    }
}
