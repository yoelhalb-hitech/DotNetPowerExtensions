
namespace DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;

public abstract class RequiredWhenOverridingBase : ByAttributeAnalyzerBase
{
    protected override bool IncludeInitializedAttribute => true;
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
