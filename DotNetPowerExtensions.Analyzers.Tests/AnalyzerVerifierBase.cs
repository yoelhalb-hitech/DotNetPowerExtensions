using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using SequelPay.DotNetPowerExtensions;
using System.Threading;

namespace DotNetPowerExtensions.Analyzers.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "This is how Microsoft does it")]
internal abstract class AnalyzerVerifierBase<TAnlayzer> : AnalyzerVerifier<TAnlayzer, CSharpAnalyzerTest<TAnlayzer, NUnitVerifier>, NUnitVerifier>
            where TAnlayzer : DiagnosticAnalyzer, new()
{
    const string NamespaceString = $"{nameof(SequelPay)}.{nameof(DotNetPowerExtensions)}";
    public static string[] Suffixes = { "", nameof(Attribute), "()", $"{nameof(Attribute)}()" };
    public static string[] Prefixes = {"", NamespaceString + ".",
                                                                    $"global::{NamespaceString}." };

    public static string NamespacePart = $"using {NamespaceString};" + Environment.NewLine;

    public static new Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<TAnlayzer, NUnitVerifier>
        {
            TestCode = NamespacePart + source,
        };

        test.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(
                                                typeof(MustInitializeAttribute).Assembly.Location));
        test.ExpectedDiagnostics.AddRange(expected);

        return test.RunAsync(CancellationToken.None);
    }
}
