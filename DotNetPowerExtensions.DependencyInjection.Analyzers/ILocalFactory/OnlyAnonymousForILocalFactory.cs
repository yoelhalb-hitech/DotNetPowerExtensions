
using SequelPay.DotNetPowerExtensions.Analyzers.DependencyManagement.DependencyAttribute.Analyzers;
using static DotNetPowerExtensions.DependencyInjection.Analyzers.AnalyzerBase;

namespace SequelPay.DotNetPowerExtensions.Analyzers.DependencyManagement.ILocalFactory.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class OnlyAnonymousForILocalFactory : AnalyzerBase
{
    public const string DiagnosticId = "DNPE0202";
    protected const string Title = "OnlyAnonymousForLocalFactory";
    protected const string Message = "Only an anonymous object is allowed for initializing with LocalService";

    protected DiagnosticDescriptor Diagnostic = new DiagnosticDescriptor(DiagnosticId, Title, Message, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Message + ".");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Diagnostic);

    protected override void Register(CompilationStartAnalysisContext compilationContext, MetadataUtil metadataUtil)
    {
        var typedSymbol = metadataUtil.GetTypeSymbol(typeof(ILocalFactory<>));
        if (typedSymbol is null) return;

        // TODO... maybe use an IOperation instead...
        compilationContext.RegisterSyntaxNodeAction(c => AnalyzeInvocation(c, typedSymbol), SyntaxKind.InvocationExpression);

    }

    private void AnalyzeInvocation(SyntaxNodeAnalysisContext context, INamedTypeSymbol serviceTypeSymbol)
    {
        try
        {
            var invocation = context.Node as InvocationExpressionSyntax;
            if(invocation is null
                || context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol is not IMethodSymbol methodSymbol
                || methodSymbol.ReceiverType is not INamedTypeSymbol classType
                || !classType.IsGenericType) return;

            if (methodSymbol.Name != nameof(ILocalFactory<object>.Create)) return;

            if (!classType.IsGenericEqual(serviceTypeSymbol)) return;

            var innerClass = classType.TypeArguments.First();

            if (invocation.ArgumentList.Arguments.FirstOrDefault()?.Expression is BaseObjectCreationExpressionSyntax expr)
            {
                var diagnostic = Microsoft.CodeAnalysis.Diagnostic.Create(Diagnostic, expr.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
        catch { }
    }
}
