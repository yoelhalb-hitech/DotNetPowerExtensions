
using System.Diagnostics.CodeAnalysis;

namespace DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ExplicitImplementationNotAllowed : MustInitializeAnalyzerBase
{
    public const string RuleId = "DNPE0107";
    protected const string Title = "ExplicitImplementationNotAllowed";
    protected const string Message = "Not allowed to implement explictly a property with MustInitialize.";

    protected override DiagnosticDescriptor DiagnosticDesc => Diagnostic;

    [SuppressMessage("Microsoft.Design", "CA1051: Do not declare visible instance fields", Justification = "The compiler only consideres fields when tracking analyzer releases")]
    protected DiagnosticDescriptor Diagnostic = new DiagnosticDescriptor(RuleId, Title, Title, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Message);


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
