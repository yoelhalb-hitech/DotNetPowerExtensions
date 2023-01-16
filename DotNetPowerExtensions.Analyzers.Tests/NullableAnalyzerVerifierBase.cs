using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using SequelPay.DotNetPowerExtensions;
using System.Threading;

namespace DotNetPowerExtensions.Analyzers.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "This is how Microsoft does it")]
internal abstract class NullableAnalyzerVerifierBase<TAnalyzer> : AnalyzerVerifier<TAnalyzer, CSharpAnalyzerTest<TAnalyzer, NUnitVerifier>, NUnitVerifier>
            where TAnalyzer : DiagnosticAnalyzer, new()
{
    public static string[] Suffixes = AnalyzerVerifierBase<TAnalyzer>.Suffixes;
    public static string[] Prefixes = AnalyzerVerifierBase<TAnalyzer>.Prefixes;

    [Obsolete("Use NullableVerifyAnalyzerAsync instead")]
    public static new Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
    {
        throw new NotSupportedException("Use NullableVerifyAnalyzerAsync instead"); // This way we make sure that it's not easy to confuse
    }
    public static Task NullableVerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
    {
        var test = new NullableCSharpAnalyzerTest<TAnalyzer, NUnitVerifier>
        {
            TestCode = "#nullable enable" + Environment.NewLine + AnalyzerVerifierBase<TAnalyzer>.NamespacePart + source,
        };       

        test.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(
                                                typeof(MustInitializeAttribute).Assembly.Location));
        test.ExpectedDiagnostics.AddRange(expected);

        return test.RunAsync(CancellationToken.None);
    }
}
