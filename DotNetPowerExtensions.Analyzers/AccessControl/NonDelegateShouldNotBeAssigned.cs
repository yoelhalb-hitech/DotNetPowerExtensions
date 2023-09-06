using SequelPay.DotNetPowerExtensions.RoslynExtensions;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace DotNetPowerExtensions.Analyzers.AccessControl;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class NonDelegateShouldNotBeAssigned : DiagnosticAnalyzer
{
    // TODO... Add warning when using Refelction to access method
    // TODO... Add requirement to set the attribute when implementing or overriding (shadowing?)
    protected const string Category = "Language";
    public const string DiagnosticId = "DNPE0301";
    protected const string Title = "NonDelegateShouldNotBeAssigned";
    protected const string Message = "A NonDelegate method should not be assigned to a variable/member/parameter";
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
                var symbol = compilationContext.Compilation.GetTypeSymbol(typeof(SequelPay.DotNetPowerExtensions.NonDelegateAttribute));
                if (symbol is null) return;

                compilationContext.RegisterSyntaxNodeAction(c => AnalyzeIdentifier(c, symbol), SyntaxKind.IdentifierName);
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }

    private void AnalyzeIdentifier(SyntaxNodeAnalysisContext context, INamedTypeSymbol symbol)
    {
        try
        {
            var identifier = context.Node as IdentifierNameSyntax;
            if (identifier is null
                || context.SemanticModel.GetSymbolInfo(identifier, context.CancellationToken).Symbol is not IMethodSymbol methodSymbol
                || !methodSymbol.HasAttribute(symbol)) return;

            var parent = identifier.Parent;
            while(parent is not null && parent is MemberAccessExpressionSyntax) parent = parent.Parent;

            if(parent is not InvocationExpressionSyntax)
            {
                var diag = Microsoft.CodeAnalysis.Diagnostic.Create(Diagnostic, identifier.GetLocation());
                context.ReportDiagnostic(diag);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}
