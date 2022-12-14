﻿
namespace DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MustInitializeNotAllowedOnDefaultInterfaceImplementation : MustInitializeAnalyzerBase
{
    public override string RuleId => "DNPE0109";
    protected override string Title => "MustInitializeNotAllowedOnDefaultInterfaceImplementation";
    protected override string Message => "MustInitialize cannot be used on an default interface implementation";

    public override void Register(CompilationStartAnalysisContext compilationContext, INamedTypeSymbol[] mustInitializeSymbols)
        => compilationContext.RegisterSymbolAction(c => AnalyzeSymbol(c, mustInitializeSymbols), SymbolKind.Property);

    private void AnalyzeSymbol(SymbolAnalysisContext context, INamedTypeSymbol[] mustInitializeSymbols)
    {
        try
        {
            var symbol = context.Symbol as IPropertySymbol;
            // Non implemented interface properties are considered abstract
            if (symbol is null || symbol.ContainingType.TypeKind != TypeKind.Interface || symbol.IsAbstract) return;

            var attribute = symbol.GetAttribute(mustInitializeSymbols);
            if (attribute is not null) context.ReportDiagnostic(CreateDiagnostic(attribute));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}
