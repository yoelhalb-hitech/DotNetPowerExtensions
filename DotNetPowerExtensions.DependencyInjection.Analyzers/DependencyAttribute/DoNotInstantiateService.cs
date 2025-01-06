
using SequelPay.DotNetPowerExtensions.Analyzers.DependencyManagement.DependencyAttribute.Analyzers;

namespace SequelPay.DotNetPowerExtensions.Analyzers.DependencyManagement.ILocalFactory.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DoNotInstantiateService : DiagnosticAnalyzer
{
    protected const string Category = "Language";
    public const string DiagnosticId = "DNPE0225";
    protected const string Title = "DoNotInstantiateService";
    protected const string Message = "Do not instantiate a service manually, use DI instead";
    protected const string Description = Message + ".";

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

                var symbols = DependencyAnalyzerUtils.AllDependencies
                                .Concat(DependencyAnalyzerUtils.BaseAttributes)
                                .Select(t => metadata(t)).OfType<INamedTypeSymbol>().ToArray();

                // TODO... maybe use an IOperation instead...
                compilationContext.RegisterSyntaxNodeAction(c => AnalyzeInvocation(c, symbols),
                                    SyntaxKind.ObjectCreationExpression, SyntaxKind.ImplicitObjectCreationExpression);

            });
        }
        catch { }
    }

    private void AnalyzeInvocation(SyntaxNodeAnalysisContext context, INamedTypeSymbol[] symbols)
    {
        try
        {
            var invocation = context.Node as BaseObjectCreationExpressionSyntax;
            if(invocation is null
                || context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol is not IMethodSymbol methodSymbol)
                return;

            if (methodSymbol.ContainingType.HasAttribute(symbols))
            {
                var diagnostic = Microsoft.CodeAnalysis.Diagnostic.Create(Diagnostic, invocation.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
        catch { }
    }
}
