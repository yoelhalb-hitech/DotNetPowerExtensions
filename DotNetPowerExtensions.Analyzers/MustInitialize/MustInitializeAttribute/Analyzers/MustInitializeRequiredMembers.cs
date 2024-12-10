using SequelPay.DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;
using Microsoft.CodeAnalysis.Operations;

namespace SequelPay.DotNetPowerExtensions.Analyzers.MustInitialize.MustInitializeAttribute.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MustInitializeRequiredMembers : MustInitializeRequiredMembersBase
{
    public const string DiagnosticId = "DNPE0103";
    protected const string Title = "MustInitializeRequiredMembers";
    protected const string Message = "Must initialize '{0}'";
    protected const string Description = "Must initialize members decorated with the MustInitialize attribute.";
    protected override DiagnosticDescriptor DiagnosticDesc => Diagnostic;

    [SuppressMessage("Microsoft.Design", "CA1051: Do not declare visible instance fields", Justification = "The compiler only consideres fields when tracking analyzer releases")]
    protected DiagnosticDescriptor Diagnostic = new DiagnosticDescriptor(DiagnosticId, Title, Message, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

    public override void Register(CompilationStartAnalysisContext compilationContext, INamedTypeSymbol[] mustInitializeSymbols)
    {
        // TODO... maybe use an IOperation instead...
        compilationContext.RegisterSyntaxNodeAction(c => AnalyzeCreation(c, mustInitializeSymbols),
                SyntaxKind.ObjectCreationExpression, SyntaxKind.ImplicitObjectCreationExpression);
    }

    private void AnalyzeCreation(SyntaxNodeAnalysisContext context, INamedTypeSymbol[] mustInitializeSymbols)
    {
        try
        {
            var expr = context.Node as BaseObjectCreationExpressionSyntax;
            if (expr is null) return;

            var symbol = context.SemanticModel.GetTypeInfo(expr, context.CancellationToken).Type as ITypeSymbol;
            if (symbol is null) return;

            var ctor = (context.SemanticModel.GetOperation(expr, context.CancellationToken) as IObjectCreationOperation)?.Constructor;

            var worker = new MustInitializeWorker(context.Compilation, context.SemanticModel);
            var props = worker.GetNotInitializedNames(expr, symbol, ctor, context.CancellationToken);

            ReportDiagnostics(context, expr, props);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}
