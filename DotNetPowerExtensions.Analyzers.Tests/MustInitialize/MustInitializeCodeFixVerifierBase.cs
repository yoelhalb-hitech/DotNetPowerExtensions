using DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;
using DotNetPowerExtensions.Analyzers.MustInitialize.CodeFixProviders;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;

namespace DotNetPowerExtensions.Analyzers.Tests.MustInitialize;

internal class MustInitializeCodeFixVerifierBase<TAnalyzer, TCodeFix, TNode>
        : CodeFixVerifierBase<TAnalyzer, TCodeFix>
    where TAnalyzer : MustInitializeAnalyzerBase, IMustInitializeAnalyzer, new()
    where TCodeFix : MustInitializeCodeFixProviderBase<TAnalyzer, TNode>, new()
    where TNode : CSharpSyntaxNode
{
    public static string[] Suffixes = MustInitializeAnalyzerVerifierBase<TAnalyzer>.Suffixes;
    public static string[] Prefixes = MustInitializeAnalyzerVerifierBase<TAnalyzer>.Prefixes;

    public static new Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
        => CodeFixVerifierBase<TAnalyzer, TCodeFix>.VerifyAnalyzerAsync(MustInitializeAnalyzerVerifierBase<TAnalyzer>.NamespacePart + source, expected);
    public static new Task VerifyCodeFixAsync(string source, params string[] fixedSource)
        => VerifyCodeFixAsync(source, DiagnosticResult.EmptyDiagnosticResults, fixedSource);

    public static new Task VerifyCodeFixAsync(string source, string fixedSource)
        => VerifyCodeFixAsync(source, DiagnosticResult.EmptyDiagnosticResults, fixedSource);

    public static new Task VerifyCodeFixAsync(string source, DiagnosticResult expected, params string[] fixedSource)
        => VerifyCodeFixAsync(source, new[] { expected }, fixedSource);

    public static new Task VerifyCodeFixAsync(string source, DiagnosticResult expected, string fixedSource)
        => VerifyCodeFixAsync(source, new[] { expected }, fixedSource);
    public static new Task VerifyCodeFixAsync(string source, DiagnosticResult[] expected, string fixedSource)
        => VerifyCodeFixAsync(source, expected, new[] { fixedSource });

    public static new Task VerifyCodeFixAsync(string source, DiagnosticResult[] expected, params string[] fixedSource)
        => CodeFixVerifierBase<TAnalyzer, TCodeFix>
                .VerifyCodeFixAsync(MustInitializeAnalyzerVerifierBase<TAnalyzer>.NamespacePart + source, expected, fixedSource);
}
