
using System.Diagnostics.CodeAnalysis;

namespace DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MustInitializeNotSupportedOnReadonly : MustInitializeAnalyzerBase
{
    public const string RuleId = "DNPE0101";
    protected const string Title = "MustInitializeWritable";
    protected const string Message = "MustInitialize is not allowed on read only properties/fields.";

    protected override DiagnosticDescriptor DiagnosticDesc => Diagnostic;

    [SuppressMessage("Microsoft.Design", "CA1051: Do not declare visible instance fields", Justification = "The compiler only consideres fields when tracking analyzer releases")]
    protected DiagnosticDescriptor Diagnostic = new DiagnosticDescriptor(RuleId, Title, Title, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Message);


    public override void Register(CompilationStartAnalysisContext compilationContext, INamedTypeSymbol[] mustInitializeSymbols)
        => compilationContext.RegisterSymbolAction(c => AnalyzeSymbol(c, mustInitializeSymbols), SymbolKind.Property, SymbolKind.Field);

    private void AnalyzeSymbol(SymbolAnalysisContext context, INamedTypeSymbol[] mustInitializeSymbols)
    {
        try
        {
            var symbol = context.Symbol;

            var attribute = symbol.GetAttribute(mustInitializeSymbols);
            if (attribute is null) return;

            if (!(symbol is IPropertySymbol prop && prop.IsReadOnly) && !(symbol is IFieldSymbol field && field.IsReadOnly)) return;

            context.ReportDiagnostic(CreateDiagnostic(attribute));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}
