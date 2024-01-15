using System.Diagnostics.CodeAnalysis;

namespace SequelPay.DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MustInitializeNotAllowedOnExplicitImplementation : MustInitializeAnalyzerBase
{
    public const string RuleId = "DNPE0108";
    protected const string Title = "MustInitializeNotAllowedOnExplicitImplementation";
    protected const string Message = "MustInitialize cannot be used on an explicit implementation";
    protected const string Description = Message + ".";

    protected override DiagnosticDescriptor DiagnosticDesc => Diagnostic;
    protected override bool IncludeInitializedAttribute => true;

    [SuppressMessage("Microsoft.Design", "CA1051: Do not declare visible instance fields", Justification = "The compiler only consideres fields when tracking analyzer releases")]
    protected DiagnosticDescriptor Diagnostic = new DiagnosticDescriptor(RuleId, Title, Message, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);


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
