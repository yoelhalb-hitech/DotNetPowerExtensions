﻿using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using System.Threading;

namespace DotNetPowerExtensions.Analyzers.Tests;

public abstract class AnalyzerVerifierBase<TAnlayzer> : AnalyzerVerifier<TAnlayzer, CSharpAnalyzerTest<TAnlayzer, NUnitVerifier>, NUnitVerifier>
            where TAnlayzer : DiagnosticAnalyzer, new()
{
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
}
