using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Data;
using Microsoft.CodeAnalysis.CSharp;
using DotNetPowerExtensions.MustInitialize;
using System.Linq;
using DotNetPowerExtensionsAnalyzer.Utils;
using DotNetPowerExtensionsAnalyzer.MustInitialize.Analyzers;

namespace DotNetPowerExtensionsAnalyzer.MustInitialize.MustInitializeAttribute.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CannotUseBaseImplementationForMustInitialize : CannotUseBaseImplementationBase, IMustInitializeAnalyzer
{
    public static string DiagnosticId => "DNPE0105";

    public override string RuleId => DiagnosticId;   

    protected override Type AttributeType => typeof(DotNetPowerExtensions.MustInitialize.MustInitializeAttribute);
}
