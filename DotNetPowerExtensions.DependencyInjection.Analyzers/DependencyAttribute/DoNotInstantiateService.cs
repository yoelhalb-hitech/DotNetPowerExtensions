using DotNetPowerExtensions.DependencyInjection.Analyzers;
using SequelPay.DotNetPowerExtensions.Analyzers.DependencyManagement.DependencyAttribute.Analyzers;

namespace SequelPay.DotNetPowerExtensions.Analyzers.DependencyManagement.ILocalFactory.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DoNotInstantiateService : AnalyzerBase
{
    public const string DiagnosticId = "DNPE0225";
    protected const string Title = "DoNotInstantiateService";
    protected const string Message = "Do not instantiate a service manually, use DI instead";

    protected DiagnosticDescriptor Diagnostic = new DiagnosticDescriptor(DiagnosticId, Title, Message, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Message + ".");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Diagnostic);

    protected override void Register(CompilationStartAnalysisContext compilationContext, MetadataUtil metadataUtil)
    {
        var symbols = metadataUtil.GetTypeSymbols(DependencyAnalyzerUtils.AllDependencies.Concat(DependencyAnalyzerUtils.BaseAttributes));

        // TODO... maybe use an IOperation instead...
        compilationContext.RegisterSyntaxNodeAction(c => AnalyzeInvocation(c, symbols),
                            SyntaxKind.ObjectCreationExpression, SyntaxKind.ImplicitObjectCreationExpression);
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
