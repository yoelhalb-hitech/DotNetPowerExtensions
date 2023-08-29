using System.Reflection;

namespace DotNetPowerExtensions.RoslynExtensions.Tests;

public class CompilationExtensions_Tests
{
    [Test]
    public void Test_GetTypeSymbol_FromReflectionType_WithSimilarNames()
    {
        var dirPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dirPath);

        var source = """ public class DeclareType{} """;

        try
        {
            var compilation1 = TestUtils.GetCompilation("Test", new[] { SyntaxFactory.ParseSyntaxTree(source) }, Array.Empty<string>());
            var outputFile1 = Path.Combine(dirPath, compilation1.AssemblyName + ".dll");
            using (var stream1 = new FileStream(outputFile1, FileMode.OpenOrCreate)) { compilation1.Emit(stream1).Success.Should().BeTrue(); }

            var compilation2 = TestUtils.GetCompilation("Test1", new[] { SyntaxFactory.ParseSyntaxTree(source) }, new[] { outputFile1 });
            var outputFile2 = Path.Combine(dirPath, compilation2.AssemblyName + ".dll");
            using (var stream2 = new FileStream(outputFile2, FileMode.OpenOrCreate)) { compilation2.Emit(stream2).Success.Should().BeTrue(); }

            var tree = SyntaxFactory.ParseSyntaxTree("");
            var compilation = TestUtils.GetCompilation("Test2", new[] { tree }, new[] { outputFile1, outputFile2 });

            var asm = Assembly.LoadFrom(outputFile1);
            var t = asm.GetType("DeclareType");
            var result = compilation.GetTypeSymbol(t!);
            result.Should().NotBeNull();

            result!.ContainingAssembly.Should().NotBeNull();
            result.ContainingAssembly.Name.Should().Be("Test");
        }
        finally
        {
#pragma warning disable CA1031 // Do not catch general exception types
            try { Directory.Delete(dirPath, recursive: true); } catch { }
#pragma warning restore CA1031 // Do not catch general exception types
        }
    }

    [Test]
    public void Test_GetTypeSymbol_FromString_WithSimilarNames()
    {
        var dirPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dirPath);

        var source = """ public class DeclareType{} """;

        try
        {
            var compilation1 = TestUtils.GetCompilation("Test", new[] { SyntaxFactory.ParseSyntaxTree(source) }, Array.Empty<string>());
            var outputFile1 = Path.Combine(dirPath, compilation1.AssemblyName + ".dll");
            using (var stream1 = new FileStream(outputFile1, FileMode.OpenOrCreate)) { compilation1.Emit(stream1).Success.Should().BeTrue(); }

            var compilation2 = TestUtils.GetCompilation("Test1", new[] { SyntaxFactory.ParseSyntaxTree(source) },new[] { outputFile1 });
            var outputFile2 = Path.Combine(dirPath, compilation2.AssemblyName + ".dll");
            using (var stream2 = new FileStream(outputFile2, FileMode.OpenOrCreate)) { compilation2.Emit(stream2).Success.Should().BeTrue(); }

            var tree = SyntaxFactory.ParseSyntaxTree("");
            var compilation = TestUtils.GetCompilation("Test2", new[] { tree }, new[] { outputFile1, outputFile2 });

            var result = compilation.GetTypeSymbol("DeclareType", compilation1.Assembly.Identity.ToString());
            result.Should().NotBeNull();

            result!.ContainingAssembly.Should().NotBeNull();
            result.ContainingAssembly.Name.Should().Be("Test");
        }
        finally
        {
            Directory.Delete(dirPath, recursive: true);
        }
    }
}
