using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using System.Threading;

namespace DotNetPowerExtensions.Analyzers.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "This is how Microsoft does it")]
public abstract class NullableAnalyzerVerifierBase<TAnalyzer> : AnalyzerVerifierBase<TAnalyzer>
            where TAnalyzer : DiagnosticAnalyzer, new()
{
    [Obsolete($"Use {nameof(NullableVerifyAnalyzerAsync)} instead")]
    public static new Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
    {
        throw new NotSupportedException("Use NullableVerifyAnalyzerAsync instead"); // This way we make sure that it's not easy to confuse
    }
    public static Task NullableVerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
    {
#pragma warning disable RS1035 // Do not use APIs banned for analyzers
        var test = new NullableCSharpAnalyzerTest<TAnalyzer, NUnitVerifier>
        {
            TestCode = "#nullable enable" + Environment.NewLine + AnalyzerVerifierBase<TAnalyzer>.NamespacePart + source,
        };
#pragma warning restore RS1035 // Do not use APIs banned for analyzers

        return VerifyAnalyzerAsync(test, expected);
    }
}
