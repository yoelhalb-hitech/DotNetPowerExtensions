using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace DotNetPowerExtensions.Analyzers.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "This is how Microsoft does it")]
internal sealed class NullableCSharpAnalyzerTest<TAnalyzer, TVerifier> : CSharpAnalyzerTest<TAnalyzer, TVerifier>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TVerifier : IVerifier, new()
{
    public static string[] Suffixes = AnalyzerVerifierBase<TAnalyzer>.Suffixes;
    public static string[] Prefixes = AnalyzerVerifierBase<TAnalyzer>.Prefixes;

    protected override bool IsCompilerDiagnosticIncluded(Diagnostic diagnostic, CompilerDiagnostics compilerDiagnostics)
    {
        if (compilerDiagnostics == CompilerDiagnostics.Errors && diagnostic.Severity == DiagnosticSeverity.Warning
                            && diagnostic.Id.StartsWith("CS", StringComparison.OrdinalIgnoreCase) && int.TryParse(diagnostic.Id.AsSpan(2), out var code))
        {
            if (code >= 8600 && code <= 8900) return true;
            return false;
        }

        return base.IsCompilerDiagnosticIncluded(diagnostic, compilerDiagnostics);
    }
}
