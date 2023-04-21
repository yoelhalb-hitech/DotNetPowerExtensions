
namespace DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;

public abstract class RequiredWhenImplementingInterfaceBase : ByAttributeAnalyzerBase
{
    protected override bool IncludeInitializedAttribute => true;
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
