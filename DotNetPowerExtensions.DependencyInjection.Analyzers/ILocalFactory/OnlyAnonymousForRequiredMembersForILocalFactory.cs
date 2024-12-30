
namespace SequelPay.DotNetPowerExtensions.Analyzers.DependencyManagement.ILocalFactory.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class OnlyAnonymousForRequiredMembersForILocalFactory : DiagnosticAnalyzer
{
    protected const string Category = "Language";
    public const string DiagnosticId = "DNPE0202";
    protected const string Title = "OnlyAnonymousForRequiredMembers";
    protected const string Message = "Only an anonymous object is allowed for initializing with LocalService";
    protected const string Description = "Only an anonymous object is allowed for initializing with LocalService.";

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
                var typedSymbol = compilationContext.Compilation.GetTypeSymbol(typeof(ILocalFactory<>));
                if (typedSymbol is null) return;

                // TODO... maybe use an IOperation instead...
                compilationContext.RegisterSyntaxNodeAction(c => AnalyzeInvocation(c, typedSymbol), SyntaxKind.InvocationExpression);

            });
        }
        catch { }
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
