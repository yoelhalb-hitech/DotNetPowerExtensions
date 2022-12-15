﻿using DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;
using Microsoft.CodeAnalysis.Testing;

namespace DotNetPowerExtensions.Analyzers.Tests.MustInitialize;

internal class MustInitializeAnalyzerVerifierBase<TAnalyzer>
        : AnalyzerVerifierBase<TAnalyzer> where TAnalyzer : MustInitializeAnalyzerBase, new()
{
    const string NamespaceString = $"{nameof(DotNetPowerExtensions)}.{nameof(DotNetPowerExtensions.MustInitialize)}";
    public static string[] Suffixes = { "", nameof(Attribute), "()", $"{nameof(Attribute)}()" };
    public static string[] Prefixes = {"", NamespaceString + ".",
                                                                    $"global::{NamespaceString}." };

    public static string NamespacePart = $"using {NamespaceString};" + Environment.NewLine;

    public static new Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
        => AnalyzerVerifierBase<TAnalyzer>.VerifyAnalyzerAsync(NamespacePart + source, expected);
}