
namespace DotNetPowerExtensions.MustInitialize.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MightRequireShouldBeLocal : DiagnosticAnalyzer
{
    protected const string Category = "Language";
    public const string DiagnosticId = "DNPE0213";
    protected const string Title = "MightRequireShouldBeLocal";
    protected const string Message = "Use `Local` for a class that is decorated with MightRequire";
    protected const string Description = Message + ".";
    [SuppressMessage("Microsoft.Design", "CA1051: Do not declare visible instance fields", Justification = "The compiler only consideres fields when tracking analyzer releases")]
    protected DiagnosticDescriptor Diagnostic = new DiagnosticDescriptor(DiagnosticId, Title, Message, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Diagnostic);

    public override void Initialize(AnalysisContext context)
    {
        try
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compilationContext =>
            {
                Func<Type, INamedTypeSymbol?> metadata = t => compilationContext.Compilation.GetTypeSymbol(t);

                var symbols = DependencyAnalyzerUtils.NonLocalAttributes.Select(t => metadata(t)).Where(x => x is not null).Select(x => x!).ToArray();
                var mightRequireSymbols = MightRequireUtils.Attributes.Select(a => metadata(a)).OfType<INamedTypeSymbol>().ToArray();

                compilationContext
                    .RegisterSyntaxNodeAction(c => AnalyzeClass(c, symbols, mightRequireSymbols), SyntaxKind.Attribute);
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }

    private void AnalyzeClass(SyntaxNodeAnalysisContext context, INamedTypeSymbol[] attributeSymbols, INamedTypeSymbol[] mightRequireSymbols)
    {
        try
        {
            // Since a class decleration can be partial we will only report it on the attribute
            var result = DependencyAnalyzerUtils.GetAttributeWithTypes(context,
                                                           DependencyAnalyzerUtils.NonLocalAttributeNames, attributeSymbols);
            if (result is null) return;
            var (attr, attrName, methodSymbol, types) = result.Value;

            var parent = context.Node.FirstAncestorOrSelf<TypeDeclarationSyntax>();
            if (parent is null) return;

            if(types.Any(t => MightRequireUtils.GetMightRequiredInfos(t, mightRequireSymbols).Any()))
            {
                var diagnostic = Microsoft.CodeAnalysis.Diagnostic.Create(Diagnostic, attr!.GetLocation());

                context.ReportDiagnostic(diagnostic);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}
