
namespace DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MustInitializeNotAllowedOnExplicitImplementation : MustInitializeAnalyzerBase
{
    public override string RuleId => "DNPE0108";
    protected override string Title => "MustInitializeNotAllowedOnExplicitImplementation";
    protected override string Message => "MustInitialize cannot be used on an explicit implementation";

    public override void Register(CompilationStartAnalysisContext compilationContext, INamedTypeSymbol[] mustInitializeSymbols)
        => compilationContext.RegisterSymbolAction(c => AnalyzeSymbol(c, mustInitializeSymbols), SymbolKind.Property);

    private void AnalyzeSymbol(SymbolAnalysisContext context, INamedTypeSymbol[] mustInitializeSymbols)
    {
        try
        {
            var symbol = context.Symbol as IPropertySymbol;
            if (symbol is null || !symbol.ExplicitInterfaceImplementations.Any()) return;

            var attribute = symbol.GetAttribute(mustInitializeSymbols);
            if (attribute is not null) context.ReportDiagnostic(CreateDiagnostic(attribute));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}
