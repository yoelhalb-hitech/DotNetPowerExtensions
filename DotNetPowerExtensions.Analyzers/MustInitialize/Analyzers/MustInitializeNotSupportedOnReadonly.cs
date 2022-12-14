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

namespace DotNetPowerExtensionsAnalyzer.MustInitialize.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MustInitializeNotSupportedOnReadonly : MustInitializeAnalyzerBase
{
    public override string RuleId => "DNPE0101";
    protected override string Title => "MustInitializeWritable";
    protected override string Message => "MustInitialize is not allowed on read only properties/fields.";

    public override void Register(CompilationStartAnalysisContext compilationContext, INamedTypeSymbol[] mustInitializeSymbols)
        => compilationContext.RegisterSymbolAction(c => AnalyzeSymbol(c, mustInitializeSymbols), SymbolKind.Property, SymbolKind.Field);

    private void AnalyzeSymbol(SymbolAnalysisContext context, INamedTypeSymbol[] mustInitializeSymbols)
    {
        try
        {
            var symbol = context.Symbol;

            var attribute = symbol.GetAttribute(mustInitializeSymbols);
            if (attribute is null) return;

            if (!(symbol is IPropertySymbol prop && prop.IsReadOnly) && !(symbol is IFieldSymbol field && field.IsReadOnly)) return;

            context.ReportDiagnostic(CreateDiagnostic(attribute));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}
