
namespace DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ExplicitImplementationNotAllowed : MustInitializeAnalyzerBase
{
    public override string RuleId => "DNPE0107";
    protected override string Title => "ExplicitImplementationNotAllowed";
    protected override string Message => "Not allowed to implement explictly a property with MustInitialize";

    public override void Register(CompilationStartAnalysisContext compilationContext, INamedTypeSymbol[] mustInitializeSymbols)
        => compilationContext.RegisterSymbolAction(c => AnalyzeSymbol(c, mustInitializeSymbols), SymbolKind.Property);

    private void AnalyzeSymbol(SymbolAnalysisContext context, INamedTypeSymbol[] mustInitializeSymbols)
    {
        try
        {
            var symbol = context.Symbol as IPropertySymbol;
            if (symbol is null || !symbol.ExplicitInterfaceImplementations.Any()) return;

            var hasAttribute = symbol.ExplicitInterfaceImplementations.Any(p => p.HasAttribute(mustInitializeSymbols));
            if (hasAttribute) context.ReportDiagnostic(CreateDiagnostic(symbol));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}
