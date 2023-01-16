
using System.Diagnostics.CodeAnalysis;

namespace DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MustInitializeNotSupportedOnStatic : MustInitializeAnalyzerBase
{
    public const string RuleId = "DNPE0106";
    protected const string Title = "MustInitializeInstance";
    protected const string Message = "MustInitialize is not allowed on static properties/fields";
    protected const string Description = Message + ".";

    protected override DiagnosticDescriptor DiagnosticDesc => Diagnostic;

    [SuppressMessage("Microsoft.Design", "CA1051: Do not declare visible instance fields", Justification = "The compiler only consideres fields when tracking analyzer releases")]
    protected DiagnosticDescriptor Diagnostic = new DiagnosticDescriptor(RuleId, Title, Message, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);


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
