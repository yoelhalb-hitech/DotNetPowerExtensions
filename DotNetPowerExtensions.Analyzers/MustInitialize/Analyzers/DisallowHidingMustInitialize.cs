using System.Diagnostics.CodeAnalysis;

namespace DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DisallowHidingMustInitialize : MustInitializeAnalyzerBase
{
    public const string RuleId = "DNPE0111";
    protected const string Title = "DisallowHidingMustInitialize";
    protected const string Message = "Cannot hide a property with MustInitialize";
    protected const string Description = Message + ".";

    protected override bool IncludeInitializedAttribute => false; // If the base is initialized than it will allow hiding it
    protected override DiagnosticDescriptor DiagnosticDesc => Diagnostic;

    [SuppressMessage("Microsoft.Design", "CA1051: Do not declare visible instance fields", Justification = "The compiler only consideres fields when tracking analyzer releases")]
    protected DiagnosticDescriptor Diagnostic = new DiagnosticDescriptor(RuleId, Title, Message, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);


    public override void Register(CompilationStartAnalysisContext compilationContext, INamedTypeSymbol[] mustInitializeSymbols)
        => compilationContext.RegisterSymbolAction(c => AnalyzeSymbol(c, mustInitializeSymbols), SymbolKind.Property);

    private void AnalyzeSymbol(SymbolAnalysisContext context, INamedTypeSymbol[] mustInitializeSymbols)
    {
        try
        {
            var symbol = context.Symbol as IPropertySymbol;
            if (symbol is null || symbol.ContainingType.TypeKind == TypeKind.Interface || symbol.IsOverride) return;

            var baseTypes = symbol.ContainingType.GetAllBaseTypes(); // We assume that they are in order from the closest base type

            foreach (var baseType in baseTypes)
            {
                var baseMemeber = baseType.GetMembers(symbol.Name).FirstOrDefault(); // Remember properties can't have overloads
                if (baseMemeber is null) continue;

                var baseHasAttribute = baseMemeber.HasAttribute(mustInitializeSymbols);
                if (baseHasAttribute) context.ReportDiagnostic(CreateDiagnostic(symbol));

                // We assume that if the member is there it's obviously not a shadow and if it's an override it should have MustInitialize, so we return regardless
                return;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}
