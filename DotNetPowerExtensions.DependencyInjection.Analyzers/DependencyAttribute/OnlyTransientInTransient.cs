
namespace SequelPay.DotNetPowerExtensions.Analyzers.DependencyManagement.DependencyAttribute.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UseTransientOnlyInTransient : AnalyzerBase
{
    public const string DiagnosticId = "DNPE0223";
    protected const string Title = "UseTransientOnlyInTransient";
    protected const string Message = "A transient service should only be used in a class decorated with `Transient` or `Local`";

    protected DiagnosticDescriptor Diagnostic = new DiagnosticDescriptor(DiagnosticId, Title, Message, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Message + ".");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Diagnostic);

    protected override void Register(CompilationStartAnalysisContext compilationContext, MetadataUtil metadataUtil)
    {
        var localSymbols = metadataUtil.GetTypeSymbols(DependencyAnalyzerUtils.LocalAttributes);
        if (!localSymbols.Any()) return;

        var transientSymbols = metadataUtil.GetTypeSymbols(DependencyAnalyzerUtils.TransientAttributes);
        if (!transientSymbols.Any()) return;

        var symbols = metadataUtil.GetTypeSymbols(DependencyAnalyzerUtils.NonLocalAttributes)
                                    .Concat(transientSymbols)
                                    .ToArray();

        compilationContext
            .RegisterSyntaxNodeAction(c => AnalyzeConstructor(c, localSymbols, transientSymbols, symbols),
                                        SyntaxKind.ConstructorDeclaration);
    }

    private void AnalyzeConstructor(SyntaxNodeAnalysisContext context, INamedTypeSymbol[] localSymbols,
            INamedTypeSymbol[] transientSymbols, INamedTypeSymbol[] attributeSymbols)
    {
        try
        {
            var ctor = context.Node as ConstructorDeclarationSyntax;

            if (ctor is null || !ctor.ParameterList.Parameters.Any()
                || context.SemanticModel.GetDeclaredSymbol(ctor, context.CancellationToken) is not IMethodSymbol methodSymbol) return;

            // Check if this is a service
            if (methodSymbol.ContainingType.GetAttributes()
                            .Where(a => a.AttributeClass is not null)
                            .All(a => !attributeSymbols.ContainsGeneric(a.AttributeClass!))) return;

            if (methodSymbol.ContainingType.GetAttributes()
                .Where(a => a.AttributeClass is not null)
                .All(a => !attributeSymbols.ContainsGeneric(a.AttributeClass!))) return;

            var otherSymbols = attributeSymbols.Except(localSymbols).Except(transientSymbols).ToList();

            if (methodSymbol.ContainingType.GetAttributes()
                .Where(a => a.AttributeClass is not null)
                .All(a => !otherSymbols.ContainsGeneric(a.AttributeClass!))) return;

            foreach (var parameter in ctor.ParameterList.Parameters)
            {
                try
                {
                    var t = parameter.Type;
                    if (t is null) continue;

                    var symbol = context.SemanticModel.GetSymbolInfo(t, context.CancellationToken).Symbol;
                    if (symbol is null) continue;

                    if (symbol.HasAttribute(transientSymbols))
                    {
                        var diagnostic = Microsoft.CodeAnalysis.Diagnostic.Create(Diagnostic, parameter.GetLocation(), symbol.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
                catch { }
            }
        }
        catch { }
    }
}