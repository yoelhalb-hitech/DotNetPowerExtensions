using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace DotNetPowerExtensions.Analyzers.Tests;

public class NullableCSharpAnalyzerTest<TAnlayzer, TVerifier> : CSharpAnalyzerTest<TAnlayzer, TVerifier>
    where TAnlayzer : DiagnosticAnalyzer, new()
    where TVerifier : IVerifier, new()
{
    protected override bool IsCompilerDiagnosticIncluded(Diagnostic diagnostic, CompilerDiagnostics compilerDiagnostics)
    {
        if (compilerDiagnostics == CompilerDiagnostics.Errors && diagnostic.Severity == DiagnosticSeverity.Warning
                            && diagnostic.Id.StartsWith("CS") && int.TryParse(diagnostic.Id.Substring(2), out var code))
        {
            if (code >= 8600 && code <= 8900) return true;
            return false;
        }

        return base.IsCompilerDiagnosticIncluded(diagnostic, compilerDiagnostics);
    }
}
