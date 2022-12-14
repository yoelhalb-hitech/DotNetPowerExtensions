
namespace DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MustInitializeNotSupportedOnStatic : MustInitializeAnalyzerBase
{
    public override string RuleId => "DNPE0106";
    protected override string Title => "MustInitializeInstance";
    protected override string Message => "MustInitialize is not allowed on static properties/fields.";

    public override void Register(CompilationStartAnalysisContext compilationContext, INamedTypeSymbol[] mustInitializeSymbols)
        => compilationContext.RegisterSymbolAction(c => AnalyzeSymbol(c, mustInitializeSymbols), SymbolKind.Property, SymbolKind.Field);

    private void AnalyzeSymbol(SymbolAnalysisContext context, INamedTypeSymbol[] mustInitializeSymbols)
    {
        try
        {
            var symbol = context.Symbol;
            if (!(symbol is IPropertySymbol prop && prop.IsStatic) && !(symbol is IFieldSymbol field && field.IsStatic)) return;

            var attribute = symbol.GetAttribute(mustInitializeSymbols);
            if (attribute is not null) context.ReportDiagnostic(CreateDiagnostic(attribute));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}
