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

public abstract class RequiredWhenImplementingInterfaceBase : ByAttributeAnalyzerBase
{ 
    protected override string Title => DescriptiveName + "RequiredWhenImplementingInterface";
    protected override string Message => DescriptiveName + " is required when the interface property is " + DescriptiveName;

    public override void Register(CompilationStartAnalysisContext compilationContext, INamedTypeSymbol[] mustInitializeSymbols)
        => compilationContext.RegisterSymbolAction(c => AnalyzeSymbol(c, mustInitializeSymbols), SymbolKind.Property);

    private void AnalyzeSymbol(SymbolAnalysisContext context, INamedTypeSymbol[] mustInitializeSymbols)
    {
        try
        {
            var symbol = context.Symbol as IPropertySymbol;
            if (symbol is null || symbol.ContainingType.TypeKind == TypeKind.Interface) return;

            var hasAttribute = symbol.HasAttribute(mustInitializeSymbols);
            if (hasAttribute) return;

            var attribSymbols = GetAttributeSymbol(mustInitializeSymbols);
            if (!attribSymbols.Any()) return;

            var interfaceIsMustIntialize = symbol.ContainingType
                                        .AllInterfaces.Any(i => i.GetMembers(symbol.Name)
                                                        .OfType<IPropertySymbol>()
                                                        .Any(p => p.HasAttribute(attribSymbols)));

            if (interfaceIsMustIntialize) context.ReportDiagnostic(CreateDiagnostic(symbol));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}
