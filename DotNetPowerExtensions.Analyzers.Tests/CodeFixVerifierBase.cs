using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace DotNetPowerExtensions.Analyzers.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "This is how Microsoft does it")]
internal abstract class CodeFixVerifierBase<TAnalyzer, TCodeFix> :
                        CodeFixVerifier<TAnalyzer, TCodeFix, CSharpCodeFixTest<TAnalyzer, TCodeFix, NUnitVerifier>, NUnitVerifier>
            where TAnalyzer : DiagnosticAnalyzer, new()
            where TCodeFix : CodeFixProvider, new()
{
    public static string[] Suffixes = AnalyzerVerifierBase<TAnalyzer>.Suffixes;
    public static string[] Prefixes = AnalyzerVerifierBase<TAnalyzer>.Prefixes;

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
        var test = new CSharpAnalyzerTest<TAnalyzer, NUnitVerifier>
        {
            TestCode = AnalyzerVerifierBase<TAnalyzer>.NamespacePart + source,
        };

        test.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(
                                                typeof(SequelPay.DotNetPowerExtensions.MustInitializeAttribute).Assembly.Location));
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
#if NET7_0_OR_GREATER
        ArgumentNullException.ThrowIfNullOrEmpty(source);
        ArgumentNullException.ThrowIfNull(fixedSource);
        ArgumentNullException.ThrowIfNullOrEmpty(fixedSource.FirstOrDefault(), nameof(fixedSource));
#else
        if(string.IsNullOrWhiteSpace(source)) throw new ArgumentNullException(nameof(source));
        if(string.IsNullOrWhiteSpace(fixedSource?.FirstOrDefault())) throw new ArgumentNullException(nameof(fixedSource));
#endif
        source = AnalyzerVerifierBase<TAnalyzer>.NamespacePart + source;

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

        var test = new CSharpCodeFixTest<TAnalyzer, TCodeFix, NUnitVerifier>
        {
            TestCode = source,
            FixedCode = newFix,
        };

        test.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(
                                                typeof(SequelPay.DotNetPowerExtensions.MustInitializeAttribute).Assembly.Location));
        test.ExpectedDiagnostics.AddRange(expected);

        return test.RunAsync(CancellationToken.None);
    }
}
