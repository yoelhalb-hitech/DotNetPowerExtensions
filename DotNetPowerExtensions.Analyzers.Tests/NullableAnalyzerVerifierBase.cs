using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using System.Threading;

namespace DotNetPowerExtensions.Analyzers.Tests;

public abstract class NullableAnalyzerVerifierBase<TAnlayzer> : AnalyzerVerifier<TAnlayzer, CSharpAnalyzerTest<TAnlayzer, NUnitVerifier>, NUnitVerifier>
            where TAnlayzer : DiagnosticAnalyzer, new()
{
    [Obsolete]
    public static new Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
    {
        throw new NotSupportedException("Use NullableVerifyAnalyzerAsync instead"); // This way we make sure that it's not easy to confuse
    }
    public static Task NullableVerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
    {
        var test = new NullableCSharpAnalyzerTest<TAnlayzer, NUnitVerifier>
        {
            TestCode = "#nullable enable" + Environment.NewLine + source,
        };       

        test.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(
                                                typeof(DotNetPowerExtensions.MustInitialize.MustInitializeAttribute).Assembly.Location));
        test.ExpectedDiagnostics.AddRange(expected);

        return test.RunAsync(CancellationToken.None);
    }
}
