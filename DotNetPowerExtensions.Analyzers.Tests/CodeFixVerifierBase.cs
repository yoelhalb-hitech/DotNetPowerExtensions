using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using System.Threading;
using System;

namespace DotNetPowerExtensions.Analyzers.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "This is how Microsoft does it")]
public abstract class CodeFixVerifierBase<TAnlayzer, TCodeFix> : 
                        CodeFixVerifier<TAnlayzer, TCodeFix, CSharpCodeFixTest<TAnlayzer, TCodeFix, NUnitVerifier>, NUnitVerifier>
            where TAnlayzer : DiagnosticAnalyzer, new()
            where TCodeFix : CodeFixProvider, new()
{
    public static Task VerifyCodeFixAsync(string source, params string[] fixedSource)
        => VerifyCodeFixAsync(source, DiagnosticResult.EmptyDiagnosticResults, fixedSource);

    public static new Task VerifyCodeFixAsync(string source, string fixedSource)
        => VerifyCodeFixAsync(source, DiagnosticResult.EmptyDiagnosticResults, fixedSource);

    public static Task VerifyCodeFixAsync(string source, DiagnosticResult expected, params string[] fixedSource)
        => VerifyCodeFixAsync(source, new[] { expected }, fixedSource);

    public static new Task VerifyCodeFixAsync(string source, DiagnosticResult expected, string fixedSource)
        => VerifyCodeFixAsync(source, new[] { expected }, fixedSource);

    public static new Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<TAnlayzer, NUnitVerifier>
        {
            TestCode = source,
        };

        test.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(
                                                typeof(DotNetPowerExtensions.MustInitialize.MustInitializeAttribute).Assembly.Location));
        test.ExpectedDiagnostics.AddRange(expected);

        return test.RunAsync(CancellationToken.None);
    }

    public static new Task VerifyCodeFixAsync(string source, DiagnosticResult[] expected, string fixedSource)
        => VerifyCodeFixAsync(source, expected, new[] { fixedSource });

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1307:Specify StringComparison for clarity", Justification = "Not Alphabeth")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1310:Specify StringComparison for correctness", Justification = "Not Alphabeth")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2201:Do not raise reserved exception types")]    
    public static Task VerifyCodeFixAsync(string source, DiagnosticResult[] expected, params string[] fixedSource)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNullOrEmpty(source);
        ArgumentNullException.ThrowIfNull(fixedSource);
        ArgumentNullException.ThrowIfNullOrEmpty(fixedSource.FirstOrDefault(), nameof(fixedSource));
#else
        if(string.IsNullOrWhiteSpace(source)) throw new ArgumentNullException(nameof(source));
        if(string.IsNullOrWhiteSpace(fixedSource?.FirstOrDefault())) throw new ArgumentNullException(nameof(fixedSource));
#endif
        var newFix = fixedSource.First();
        if (source.Contains("/:") && source.Contains(":/"))
        {
            newFix = source.Replace("[|", "").Replace("|]", "");
            var splitted = newFix.Split("/:")
                                        .Select((s, i) => i == 0 ? s : (string.Concat(fixedSource[i-1], s.AsSpan(s.IndexOf(":/") + 2))));
            newFix = string.Join("", splitted);
            source = source.Replace("/:", "").Replace(":/", "");
        }
        else if (fixedSource.Length > 1)
            throw new System.Exception("Cannot have multiple fix strings if the source doesn't have the /::/ placeholders");

        var test = new CSharpCodeFixTest<TAnlayzer, TCodeFix, NUnitVerifier>
        {
            TestCode = source,
            FixedCode = newFix,
        };

        test.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(
                                                typeof(DotNetPowerExtensions.MustInitialize.MustInitializeAttribute).Assembly.Location));
        test.ExpectedDiagnostics.AddRange(expected);

        return test.RunAsync(CancellationToken.None);
    }
}
