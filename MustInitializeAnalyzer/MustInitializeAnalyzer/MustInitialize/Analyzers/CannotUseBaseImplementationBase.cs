﻿using Microsoft.CodeAnalysis.Diagnostics;
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

public abstract class CannotUseBaseImplementationBase : ByAttributeAnalyzerBase
{
    protected override string Title => "CannotUseBaseImplementation";
    protected override string Message => "Cannot use base implementation of {0} because it lacks " + DescriptiveName;

    public override void Register(CompilationStartAnalysisContext compilationContext, INamedTypeSymbol[] mustInitializeSymbols)
        => compilationContext.RegisterSymbolAction(c => AnalyzeSymbol(c, mustInitializeSymbols), SymbolKind.NamedType);


    public static IPropertySymbol[] GetRequiredProperties(ITypeSymbol symbol, INamedTypeSymbol[] mustInitializeSymbols)
    {
        if (symbol.BaseType is null) return new IPropertySymbol[] { };

        var implementedProps = symbol.GetMembers().OfType<IPropertySymbol>().Select(p => p.Name).ToArray();

        var differentInterfaces = symbol.AllInterfaces.Except(symbol.BaseType.AllInterfaces);
        var baseProperties = symbol
                                .GetAllBaseTypes()
                                .SelectMany(t => t.GetMembers()
                                                    .OfType<IPropertySymbol>()
                                                    // We include abstract in case this class will not implement it only a subclass
                                                    .Where(p => !p.DeclaredAccessibility.HasFlag(Accessibility.Private))
                                                    .Select(p => p.Name))
                                .ToArray();

        return differentInterfaces.SelectMany(i => i.GetMembers().OfType<IPropertySymbol>()
                                                                .Where(p => baseProperties.Contains(p.Name) && !implementedProps.Contains(p.Name))
                                                                .Where(p => p.HasAttribute(mustInitializeSymbols)))
                                                            .ToArray();
    }

    private void AnalyzeSymbol(SymbolAnalysisContext context, INamedTypeSymbol[] mustInitializeSymbols)
    {
        try
        {
            var symbol = context.Symbol as ITypeSymbol;
            if (symbol is null || symbol.TypeKind == TypeKind.Interface || !symbol.Interfaces.Any()
                                            || symbol.BaseType is null || symbol.BaseType.Name == nameof(Object)) return;

            // If the declared interfaces are the same then we assume the base has done everything correctly
            var interfacesAreDiffering = symbol.Interfaces.Except(symbol.BaseType.Interfaces).Any();
            if (!interfacesAreDiffering) return;

            var attribSymbols = GetAttributeSymbol(mustInitializeSymbols);
            if (!attribSymbols.Any()) return;

            var propertiesWithAttributes = GetRequiredProperties(symbol, attribSymbols);
            foreach (var property in propertiesWithAttributes)
            {
                foreach (var location in symbol.Locations)
                {
                    var diagnostic = Diagnostic.Create(DiagnosticDesc, location, property.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}
