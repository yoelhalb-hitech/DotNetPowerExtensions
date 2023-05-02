using DotNetPowerExtensions.Analyzers.MustInitialize;
using DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;

namespace DotNetPowerExtensions.Analyzers.DependencyManagement.ILocalFactory.Analyzers;

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
        var typeName = typeof(ILocalFactory<>).FullName;
        var typedSymbol = compilationContext.Compilation.GetTypeByMetadataName(typeName!);
        if (typedSymbol is null) return;

        // TODO... maybe use an IOperation instead...
        compilationContext.RegisterSyntaxNodeAction(c => AnalyzeInvocation(c, mustInitializeSymbols, typedSymbol), SyntaxKind.InvocationExpression);
    }

    private void AnalyzeInvocation(SyntaxNodeAnalysisContext context, INamedTypeSymbol[] mustInitializeSymbols, INamedTypeSymbol serviceTypeSymbol)
    {
        try
        {
            var invocation = context.Node as InvocationExpressionSyntax;
            if (invocation is null
                || context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol is not IMethodSymbol methodSymbol
                || methodSymbol.ReceiverType is not INamedTypeSymbol classType
                || !classType.IsGenericType
                || methodSymbol.Name != nameof(ILocalFactory<object>.Create)) return;

            if (!classType.IsGenericEqual(serviceTypeSymbol)) return;

            var innerClass = classType.TypeArguments.First();

            var argExpression = invocation.ArgumentList.Arguments.FirstOrDefault()?.Expression;
            if (argExpression is ObjectCreationExpressionSyntax) return; // Will be handled by `OnlyAnonymousForRequiredMembersForLocalService` analyzer

            IEnumerable <string> props;
            if (argExpression is AnonymousObjectCreationExpressionSyntax creation)
                props = MustInitializeUtils.GetNotInitializedNames(creation, innerClass, mustInitializeSymbols);
            else
                props = MustInitializeUtils.GetRequiredToInitialize(innerClass, mustInitializeSymbols)
                                                    .Select(m => m.name)
                                                    .Distinct();

            ReportDiagnostics(context, argExpression as CSharpSyntaxNode ?? invocation.ArgumentList, props.OrderBy(p => p));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}
