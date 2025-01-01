
using SequelPay.DotNetPowerExtensions;

namespace DotNetPowerExtensions.MustInitialize.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MustInitializeRequiredMembersForILocalFactory : MustInitializeRequiredMembersBase
{
    public const string DiagnosticId = "DNPE0201";
    protected const string Title = "MustInitializeRequiredMembers";
    protected const string Message = "Must initialize '{0}'";
    protected const string Description = "Must initialize members decorated with the MustInitialize attribute.";
    protected override DiagnosticDescriptor DiagnosticDesc => Diagnostic;

    [SuppressMessage("Microsoft.Design", "CA1051: Do not declare visible instance fields", Justification = "The compiler only consideres fields when tracking analyzer releases")]
    protected DiagnosticDescriptor Diagnostic = new DiagnosticDescriptor(DiagnosticId, Title, Message, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);


    public override void Register(CompilationStartAnalysisContext compilationContext, INamedTypeSymbol[] mustInitializeSymbols)
    {
        // TODO... maybe use an IOperation instead...
        compilationContext.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        try
        {
            var invocation = context.Node as InvocationExpressionSyntax;
            if (invocation is null
                || context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol is not IMethodSymbol methodSymbol
                || methodSymbol.ReceiverType is not INamedTypeSymbol classType
                || !classType.IsGenericType
                || methodSymbol.Name != nameof(ILocalFactory<object>.Create)) return;

            var worker = new MustInitializeWorker(context.Compilation, context.SemanticModel);

            if (!classType.IsGenericEqual(worker.GetTypeSymbol(typeof(ILocalFactory<>)))) return;

            var innerClass = classType.TypeArguments.First();

            var argExpression = invocation.ArgumentList.Arguments.FirstOrDefault()?.Expression;
            if (argExpression is BaseObjectCreationExpressionSyntax) return; // Will be handled by `OnlyAnonymousForRequiredMembersForLocalService` analyzer

            IEnumerable <string> props;
            if (argExpression is AnonymousObjectCreationExpressionSyntax creation)
                props = worker.GetNotInitializedNames(creation, innerClass, context.CancellationToken);
            else
                props = worker.GetRequiredToInitialize(innerClass, null, context.CancellationToken)
                                                    .Select(m => m.name)
                                                    .Distinct();

            ReportDiagnostics(context, argExpression as CSharpSyntaxNode ?? invocation.ArgumentList, props.OrderBy(p => p));
        }
        catch { }
    }
}
