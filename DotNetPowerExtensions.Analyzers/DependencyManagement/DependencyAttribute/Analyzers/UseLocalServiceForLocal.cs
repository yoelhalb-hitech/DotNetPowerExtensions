
namespace DotNetPowerExtensions.Analyzers.DependencyManagement.DependencyAttribute.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UseLocalServiceForLocal : DiagnosticAnalyzer
{
    protected const string Category = "Language";
    public const string DiagnosticId = "DNPE0204";
    protected const string Title = "UseLocalServiceForLocal";
    protected const string Message = "Use LocalService for a service decorated with Local attribute";
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
                Func<Type, INamedTypeSymbol?> metadata = t => compilationContext.Compilation.GetTypeByMetadataName(t.FullName!);

                var localServiceSymbol = metadata(typeof(ILocalFactory<>));
                if (localServiceSymbol is null) return;

                var localSymbols = DependencyAnalyzerUtils.LocalAttributes.Select(t => metadata(t)).OfType<INamedTypeSymbol>().ToArray();
                if (!localSymbols.Any()) return;

                var symbols = DependencyAnalyzerUtils.NonLocalAttributes.Select(t => metadata(t)).OfType<INamedTypeSymbol>()
                                            .Concat(localSymbols)
                                            .ToArray();

                compilationContext
                    .RegisterSyntaxNodeAction(c => AnalyzeConstructor(c, localSymbols, localServiceSymbol, symbols),
                                                SyntaxKind.ConstructorDeclaration);
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }

    private void AnalyzeConstructor(SyntaxNodeAnalysisContext context, INamedTypeSymbol[] localSymbols,
                                                                INamedTypeSymbol serviceTypeSymbol, INamedTypeSymbol[] attributeSymbols)
    {
        try
        {
            var ctor = context.Node as ConstructorDeclarationSyntax;

            if (ctor is null || !ctor.ParameterList.Parameters.Any()
                || context.SemanticModel.GetDeclaredSymbol(ctor, context.CancellationToken) is not IMethodSymbol methodSymbol) return;

            // Check if this is a service
            if (methodSymbol.ContainingType.GetAttributes()
                            .Where(a => a.AttributeClass is not null)
                            .All(a => !attributeSymbols.ContainsGeneric(a.AttributeClass!))) return;

            foreach (var parameter in ctor.ParameterList.Parameters)
            {
                try
                {
                    var t = parameter.Type;
                    if (t is null) continue;

                    var symbol = context.SemanticModel.GetSymbolInfo(t).Symbol;
                    if (symbol is null) continue;

                    if (symbol.HasAttribute(localSymbols))
                    {
                        var diagnostic = Microsoft.CodeAnalysis.Diagnostic.Create(Diagnostic, parameter.GetLocation());
                        context.ReportDiagnostic(diagnostic);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}