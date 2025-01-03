﻿
namespace DotNetPowerExtensions.MustInitialize.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MustInitializeNotSupportedOnReadonly : MustInitializeAnalyzerBase
{
    public const string RuleId = "DNPE0101";
    protected const string Title = "MustInitializeWritable";
    protected const string Message = "MustInitialize is not allowed on read only properties/fields";
    protected const string Description = Message + ".";

    protected override DiagnosticDescriptor DiagnosticDesc => Diagnostic;
    protected override bool IncludeInitializedAttribute => false; // Initialized could technically be used in a subclass where the base decleration has a private set (should that be possible)

    [SuppressMessage("Microsoft.Design", "CA1051: Do not declare visible instance fields", Justification = "The compiler only consideres fields when tracking analyzer releases")]
    protected DiagnosticDescriptor Diagnostic = new DiagnosticDescriptor(RuleId, Title, Message, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);


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
        catch { }
    }
}
