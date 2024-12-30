using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using System.Reflection;
using System.Threading;

namespace DotNetPowerExtensions.Analyzers.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "This is how Microsoft does it")]
public abstract class AnalyzerVerifierBase<TAnlayzer> : AnalyzerVerifier<TAnlayzer, CSharpAnalyzerTest<TAnlayzer, NUnitVerifier>, NUnitVerifier>
            where TAnlayzer : DiagnosticAnalyzer, new()
{
    const string NamespaceString = $"SequelPay.{nameof(DotNetPowerExtensions)}";

#pragma warning disable CA2211 // Non-constant fields should not be visible
    public static string[] Suffixes = ["", nameof(Attribute), "()", $"{nameof(Attribute)}()"];
    public static string[] Prefixes = ["", NamespaceString + ".",
                                                                    $"global::{NamespaceString}." ];

#pragma warning disable RS1035 // Do not use APIs banned for analyzers
    public static string NamespacePart = $"using {NamespaceString};" + Environment.NewLine;
#pragma warning restore RS1035 // Do not use APIs banned for analyzers
#pragma warning restore CA2211 // Non-constant fields should not be visible


    public static new Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<TAnlayzer, NUnitVerifier>
        {
            TestCode = NamespacePart + source,
        };

        return VerifyAnalyzerAsync(test, expected);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035:Do not use APIs banned for analyzers", Justification = "Just a test, and we depend on it")]
    internal static Task VerifyAnalyzerAsync(AnalyzerTest<NUnitVerifier> test, DiagnosticResult[] expected)
    {
        var assemblies = typeof(TAnlayzer).Assembly.GetReferencedAssemblies()
                .Where(a => a.FullName?.StartsWith(NamespaceString, StringComparison.Ordinal) == true);
        foreach ( var assembly in assemblies)//.Where(a => !string.IsNullOrWhiteSpace(a.Location)))
        {
            var asm = Assembly.Load(assembly.FullName);
            if(!string.IsNullOrWhiteSpace(asm.Location))
            {
                test.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(asm.Location));
            }
        }

        test.ExpectedDiagnostics.AddRange(expected);

        return test.RunAsync(CancellationToken.None);
    }
}
