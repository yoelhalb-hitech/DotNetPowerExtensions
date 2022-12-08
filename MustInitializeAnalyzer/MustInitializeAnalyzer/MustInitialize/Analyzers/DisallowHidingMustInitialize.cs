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
public class DisallowHidingMustInitialize : MustInitializeAnalyzerBase
{
    public override string RuleId => "DNPE0111";
    protected override string Title => "DisallowHidingMustInitialize";
    protected override string Message => "Cannot hide a property with MustInitialize";

    public override void Register(CompilationStartAnalysisContext compilationContext, INamedTypeSymbol[] mustInitializeSymbols)
        => compilationContext.RegisterSymbolAction(c => AnalyzeSymbol(c, mustInitializeSymbols), SymbolKind.Property);

    private void AnalyzeSymbol(SymbolAnalysisContext context, INamedTypeSymbol[] mustInitializeSymbols)
    {
        try
        {
            var symbol = context.Symbol as IPropertySymbol;
            if (symbol is null || symbol.ContainingType.TypeKind == TypeKind.Interface || symbol.IsOverride) return;

            var baseTypes = symbol.ContainingType.GetAllBaseTypes(); // We assume that they are in order from the closest base type

            foreach (var baseType in baseTypes)
            {
                var baseMemeber = baseType.GetMembers(symbol.Name).FirstOrDefault(); // Remember properties can't have overloads
                if (baseMemeber is null) continue;

                var baseHasAttribute = baseMemeber.HasAttribute(mustInitializeSymbols);
                if (baseHasAttribute) context.ReportDiagnostic(CreateDiagnostic(symbol));

                // We assume that if the member is there it's obviously not a shadow and if it's an override it should have MustInitialize, so we return regardless
                return;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}
