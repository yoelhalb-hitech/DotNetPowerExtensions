using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.CSharp.Testing.NUnit;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetPowerExtensionsAnalyzer.Test;

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

    public static Task VerifyCodeFixAsync(string source, DiagnosticResult[] expected, params string[] fixedSource)
    {
        var newFix = fixedSource.First();
        if (source.Contains("/:") && source.Contains(":/"))
        {
            newFix = source.Replace("[|", "").Replace("|]", "");
            var splitted = newFix.Split("/:")
                                        .Select((s, i) => i == 0 ? s : (fixedSource[i-1] + s.Substring(s.IndexOf(":/") + 2)));
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
