using System.Diagnostics.CodeAnalysis;
using System.Text;
using Mono.Cecil;

namespace DotNetPowerExtensions.RoslynExtensions.Tests;

[SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035:Do not use APIs banned for analyzers", Justification = "We need File to load the module")]
public class CecilExtensions_Tests
{
    private static string GetCecilFullName(bool outerNS, bool innerNS,
                                            int outerGenericCount, bool hasInner, int innerGenericCount,
                                            bool hasInnerInner, int innerInnerGenericCount)
    {
        var sb = new StringBuilder();
        if (outerNS) { sb.Append("Outer."); }
        if (innerNS) { sb.Append("Inner."); }
        sb.Append("OuterType");
        if(outerGenericCount > 0) sb.Append("`" + outerGenericCount);
        if (hasInner) { sb.Append("/InnerType"); }
        if (innerGenericCount > 0) { sb.Append("`" + innerGenericCount); }
        if (hasInnerInner) { sb.Append("/InnerInnerType"); }
        if (innerInnerGenericCount > 0) { sb.Append("`" + innerInnerGenericCount); }

        return sb.ToString();
    }

    [Test]
    public void Test_GetCecilTypeName([Values(true, false)] bool outerNS, [Values(true, false)] bool innerNS,
                    [Values(true, false)] bool isStructOuter, [Values(0, 1,2,3)] int outerGenericCount,
                    [Values(true, false)] bool hasInner, [Values(true, false)] bool isStructInner, [Values(0, 1, 2, 3)] int innerGenericCount,
                    [Values(true, false)] bool hasInnerInner, [Values(true, false)] bool isStructInnerInner, [Values(0, 1, 2, 3)] int innerInnerGenericCount)
    {
        Assume.That(() => innerGenericCount > 0 || isStructInner || hasInnerInner ? hasInner : true);
        Assume.That(() => innerInnerGenericCount > 0 || isStructInnerInner ? hasInnerInner : true);

        var code = TestUtils.GetClassNamesCode(outerNS, innerNS, isStructOuter, outerGenericCount, hasInner, isStructInner, innerGenericCount,
                                                                            hasInnerInner, isStructInnerInner, innerInnerGenericCount);

        var (_, symbol) = TestUtils.GetModelAndTypeSymbol(code);
        var result = symbol?.GetCecilTypeName();

        var expectedName = GetCecilFullName(outerNS, innerNS, outerGenericCount, hasInner, innerGenericCount, hasInnerInner, innerInnerGenericCount);

        result.Should().Be(expectedName);
    }

    [Test]
    public void Test_GetTypeDefinition([Values(true, false)] bool outerNS, [Values(true, false)] bool innerNS,
                    [Values(true, false)] bool isStructOuter, [Values(0, 1, 2, 3)] int outerGenericCount,
                    [Values(true, false)] bool hasInner, [Values(true, false)] bool isStructInner, [Values(0, 1, 2, 3)] int innerGenericCount,
                    [Values(true, false)] bool hasInnerInner, [Values(true, false)] bool isStructInnerInner, [Values(0, 1, 2, 3)] int innerInnerGenericCount)
    {
        Assume.That(() => innerGenericCount > 0 || isStructInner || hasInnerInner ? hasInner : true);
        Assume.That(() => innerInnerGenericCount > 0 || isStructInnerInner ? hasInnerInner : true);

        var code = TestUtils.GetClassNamesCode(outerNS, innerNS, isStructOuter, outerGenericCount, hasInner, isStructInner, innerGenericCount,
                                                                            hasInnerInner, isStructInnerInner, innerInnerGenericCount);

        var dirPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dirPath);

        try
        {
            var (semanticModel, symbol) = TestUtils.GetModelAndTypeSymbol(code);

            var outputFile1 = Path.Combine(dirPath, semanticModel.Compilation.AssemblyName + ".dll");
            using (var stream1 = new FileStream(outputFile1, FileMode.OpenOrCreate)) { semanticModel.Compilation.Emit(stream1).Success.Should().BeTrue(); }

            var md = ModuleDefinition.ReadModule(outputFile1, new ReaderParameters() { InMemory = true }); // This way it is not blocking

            var td = md?.ToTypeDefinition(symbol);

            td.Should().NotBeNull();

            var expectedName = GetCecilFullName(outerNS, innerNS, outerGenericCount, hasInner, innerGenericCount, hasInnerInner, innerInnerGenericCount);

            td!.FullName.Should().Be(expectedName);
        }
        finally
        {
            Directory.Delete(dirPath, recursive: true);
        }
    }

    [Test]
    public void Test_ToTypeSymbol([Values(true, false)] bool outerNS, [Values(true, false)] bool innerNS,
                 [Values(true, false)] bool isStructOuter, [Values(0, 1, 2, 3)] int outerGenericCount,
                 [Values(true, false)] bool hasInner, [Values(true, false)] bool isStructInner, [Values(0, 1, 2, 3)] int innerGenericCount,
                 [Values(true, false)] bool hasInnerInner, [Values(true, false)] bool isStructInnerInner, [Values(0, 1, 2, 3)] int innerInnerGenericCount)
    {
        Assume.That(() => innerGenericCount > 0 || isStructInner || hasInnerInner ? hasInner : true);
        Assume.That(() => innerInnerGenericCount > 0 || isStructInnerInner ? hasInnerInner : true);

        var code = TestUtils.GetClassNamesCode(outerNS, innerNS, isStructOuter, outerGenericCount, hasInner, isStructInner, innerGenericCount,
                                                                            hasInnerInner, isStructInnerInner, innerInnerGenericCount);

        var dirPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dirPath);

        try
        {
            var (semanticModel, symbol) = TestUtils.GetModelAndTypeSymbol(code);

            var outputFile1 = Path.Combine(dirPath, semanticModel.Compilation.AssemblyName + ".dll");
            using (var stream1 = new FileStream(outputFile1, FileMode.OpenOrCreate)) { semanticModel.Compilation.Emit(stream1).Success.Should().BeTrue(); }

            var md = ModuleDefinition.ReadModule(outputFile1, new ReaderParameters() { InMemory = true }); // This way it is not blocking

            var td = md.ToTypeDefinition(symbol);

            td.Should().NotBeNull();

            var result = td!.ToTypeSymbol(semanticModel.Compilation);

            result.Should().Be(symbol);
        }
        finally
        {
            Directory.Delete(dirPath, recursive: true);
        }
    }

}
