using DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;
using DotNetPowerExtensions;
using System.Diagnostics.CodeAnalysis;

namespace DotNetPowerExtensions.Analyzers.DependencyManagement.LocalService.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MustIinitializeRequiredMembersForLocalService : MustInitializeRequiredMembersBase
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
        var typeName = typeof(LocalService<>).FullName;
        var typedSymbol = compilationContext.Compilation.GetTypeByMetadataName(typeName!);
        if (typedSymbol is null) return;

        // TODO... maybe use an IOperation instead...
        compilationContext.RegisterSyntaxNodeAction(c => AnalyzeInvocation(c, mustInitializeSymbols, typedSymbol), SyntaxKind.InvocationExpression);
    }

    public static IEnumerable<string> GetNotInitializedNames(AnonymousObjectCreationExpressionSyntax typeDecl, ITypeSymbol symbol, INamedTypeSymbol[] mustInitializeSymbols)
    {
        var props = GetMembersWithMustInitialize(symbol, mustInitializeSymbols).Select(m => m.As<ISymbol>()!.Name);

        var initialized = typeDecl.Initializers.Select(i => i.GetName()).Where(x => x is not null).Select(x => x!);

        return props.Except(initialized).Distinct();
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
                || methodSymbol.Name != nameof(LocalService<object>.Get)) return;

            if (!classType.IsGenericEqual(serviceTypeSymbol)) return;

            var innerClass = classType.TypeArguments.First();

            var argExpression = invocation.ArgumentList.Arguments.FirstOrDefault()?.Expression;
            if (argExpression is ObjectCreationExpressionSyntax) return; // Will be handled by `OnlyAnonymousForRequiredMembersForLocalService` analyzer

            IEnumerable <string> props;
            if (argExpression is AnonymousObjectCreationExpressionSyntax creation)
                props = GetNotInitializedNames(creation, innerClass, mustInitializeSymbols);
            else 
                props = GetMembersWithMustInitialize(innerClass, mustInitializeSymbols).Select(m => m.As<ISymbol>()!.Name).Distinct();

            ReportDiagnostics(context, argExpression as CSharpSyntaxNode ?? invocation.ArgumentList, props);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}
