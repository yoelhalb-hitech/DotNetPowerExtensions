using DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;
using SequelPay.DotNetPowerExtensions;
using System.Diagnostics.CodeAnalysis;

namespace DotNetPowerExtensions.Analyzers.DependencyManagement.ILocalFactory.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class OnlyAnonymousForRequiredMembersForILocalFactory : MustInitializeAnalyzerBase
{
    public const string DiagnosticId = "DNPE0202";
    protected const string Title = "OnlyAnonymousForRequiredMembers";
    protected const string Message = "Only an anonymous object is allowed for initializing with LocalService";
    protected const string Description = "Only an anonymous object is allowed for initializing with LocalService.";
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
            if(invocation is null
                || context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol is not IMethodSymbol methodSymbol
                || methodSymbol.ReceiverType is not INamedTypeSymbol classType
                || !classType.IsGenericType) return;

            if (methodSymbol.Name != nameof(ILocalFactory<object>.Create)) return;

            if (!classType.IsGenericEqual(serviceTypeSymbol)) return;

            var innerClass = classType.TypeArguments.First();
            
            if (invocation.ArgumentList.Arguments.FirstOrDefault()?.Expression is ObjectCreationExpressionSyntax expr)
            {
                var diagnostic = Microsoft.CodeAnalysis.Diagnostic.Create(DiagnosticDesc, expr.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}
