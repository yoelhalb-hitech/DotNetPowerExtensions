using SequelPay.DotNetPowerExtensions.RoslynExtensions;

namespace SequelPay.DotNetPowerExtensions.Analyzers.DependencyManagement.DependencyAttribute.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UseServiceOnlyInDependency : DiagnosticAnalyzer
{
    protected const string Category = "Language";
    public const string DiagnosticId = "DNPE0221";
    protected const string Title = "UseServiceOnlyInDependency";
    protected const string Message = "Use a service only in a class decorated with `Singleton/Scoped/Transient/Local` attribute";
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

                var symbols = DependencyAnalyzerUtils.AllDependencies.Select(t => metadata(t)).OfType<INamedTypeSymbol>()
                                            .ToArray();

                compilationContext
                    .RegisterSyntaxNodeAction(c => AnalyzeConstructor(c, symbols),
                                                SyntaxKind.ConstructorDeclaration);
            });
        }
        catch { }
    }

    private void AnalyzeConstructor(SyntaxNodeAnalysisContext context, INamedTypeSymbol[] attributeSymbols)
    {
        try
        {
            var ctor = context.Node as ConstructorDeclarationSyntax;

            if (ctor is null || !ctor.ParameterList.Parameters.Any()
                || context.SemanticModel.GetDeclaredSymbol(ctor, context.CancellationToken) is not IMethodSymbol methodSymbol) return;

            if (methodSymbol.ContainingType.GetAttributes()
                            .Where(a => a.AttributeClass is not null)
                            .Any(a => attributeSymbols.ContainsGeneric(a.AttributeClass!))) return;

            foreach (var parameter in ctor.ParameterList.Parameters)
            {
                try
                {
                    var t = parameter.Type;
                    if (t is null) continue;

                    var symbol = context.SemanticModel.GetSymbolInfo(t, context.CancellationToken).Symbol;
                    if (symbol is null) continue;

                    if (symbol.HasAttribute(attributeSymbols))
                    {
                        var diagnostic = Microsoft.CodeAnalysis.Diagnostic.Create(Diagnostic, parameter.GetLocation(), symbol.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
                catch { }
            }
        }
        catch { }
    }
}