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

public abstract class RequiredWhenOverridingBase : ByAttributeAnalyzerBase
{    
    protected override string Title => DescriptiveName + "RequiredWhenOverriding";
    protected override string Message => DescriptiveName + " is required when oevrriding a property with " + DescriptiveName;

    public override void Register(CompilationStartAnalysisContext compilationContext, INamedTypeSymbol[] mustInitializeSymbols)
        => compilationContext.RegisterSymbolAction(c => AnalyzeSymbol(c, mustInitializeSymbols), SymbolKind.Property);

    private void AnalyzeSymbol(SymbolAnalysisContext context, INamedTypeSymbol[] mustInitializeSymbols)
    {
        try
        {
            var symbol = context.Symbol as IPropertySymbol;
            if (symbol is null || symbol.ContainingType.TypeKind == TypeKind.Interface || !symbol.IsOverride) return;

            var attribSymbols = GetAttributeSymbol(mustInitializeSymbols);
            if (!attribSymbols.Any()) return;

            var hasAttribute = symbol.HasAttribute(attribSymbols);
            if (hasAttribute) return;
            
            var baseHasAttribute = symbol.OverriddenProperty!.HasAttribute(attribSymbols);
            if (!baseHasAttribute) return;

            context.ReportDiagnostic(CreateDiagnostic(symbol));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}
