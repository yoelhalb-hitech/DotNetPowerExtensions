﻿using DotNetPowerExtensionsAnalyzer.MustInitialize.Analyzers;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetPowerExtensionsAnalyzer.Test.MustInitialize;

internal class NullableMustInitializeAnalyzerVerifierBase<TAnalyzer> 
        : NullableAnalyzerVerifierBase<TAnalyzer> where TAnalyzer : DiagnosticAnalyzer, new()
{
    public static string[] Suffixes = MustInitializeAnalyzerVerifierBase<DisallowHidingMustInitialize>.Suffixes; // Random analyzer
    public static string[] Prefixes = MustInitializeAnalyzerVerifierBase<DisallowHidingMustInitialize>.Prefixes;    

    public static string NamespacePart = MustInitializeAnalyzerVerifierBase<DisallowHidingMustInitialize>.NamespacePart;

    public static new Task NullableVerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
        => NullableAnalyzerVerifierBase<TAnalyzer>.NullableVerifyAnalyzerAsync(NamespacePart + source, expected);
}
