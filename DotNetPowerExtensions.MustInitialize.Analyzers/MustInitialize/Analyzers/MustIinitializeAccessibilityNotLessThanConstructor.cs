
namespace DotNetPowerExtensions.MustInitialize.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MustIinitializeAccessibilityNotLessThanConstructor : MustInitializeAnalyzerBase
{
    public const string RuleId = "DNPE0102";
    protected const string Title = "MustInitializeShouldBeAccessible";
    protected const string Message = "MustInitialize accessibility cannot be less than the constuctur";
    protected const string Description = Message + ".";

    protected override DiagnosticDescriptor DiagnosticDesc => Diagnostic;
    protected override bool IncludeInitializedAttribute => false;

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

            var accesibility = symbol.DeclaredAccessibility;
            if (symbol is IPropertySymbol prop && prop.SetMethod is not null)
                accesibility = prop.SetMethod.DeclaredAccessibility;

            // In general the >= prerforms well
            Func<Accessibility, Accessibility, bool> predicate = (propAccess, outerAccess) => (propAccess, outerAccess) switch
            {
                (Accessibility.Internal, Accessibility.Protected) => false,
                _ => propAccess >= outerAccess,
            };

            // When the containing type (or container of container) is the same as the property
            //              we know that nobody can create the object outside the scope even if the ctor would allow outside
            if (predicate(accesibility, symbol.ContainingType.DeclaredAccessibility)) return;
            for (var containingType = symbol.ContainingType.ContainingType;
                containingType?.ContainingType is not null && containingType?.IsEqualTo(containingType.ContainingType) != true;
                containingType = containingType!.ContainingType)
            {
                if (predicate(accesibility, symbol.ContainingType.DeclaredAccessibility)) return;
            }

            var hasAllCtorAccessibility = symbol.ContainingType.Constructors.All(c => predicate(accesibility, c.DeclaredAccessibility));

            // We have a ctor that is in a scope that the must initialize property is not accessed
            if (!hasAllCtorAccessibility) context.ReportDiagnostic(CreateDiagnostic(attribute));
        }
        catch { }
    }
}
