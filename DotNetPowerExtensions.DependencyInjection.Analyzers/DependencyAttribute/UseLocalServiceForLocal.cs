namespace SequelPay.DotNetPowerExtensions.Analyzers.DependencyManagement.DependencyAttribute.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UseLocalServiceForLocal : AnalyzerBase
{
    public const string DiagnosticId = "DNPE0204";
    protected const string Title = "UseLocalServiceForLocal";
    protected const string Message = "Use `ILocalFactory<{0}>` because `{0}` decorated with the `Local` attribute";

    protected DiagnosticDescriptor Diagnostic = new DiagnosticDescriptor(DiagnosticId, Title, Message, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Message + ".");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Diagnostic);

    protected override void Register(CompilationStartAnalysisContext compilationContext, MetadataUtil metadataUtil)
    {
        // We do not do it for the base attributes as they are not instantiated directly and can be used for a subclass
        var localSymbols = metadataUtil.GetTypeSymbols(DependencyAnalyzerUtils.LocalAttributes);
        if (!localSymbols.Any()) return;

        var symbols = metadataUtil.GetTypeSymbols(DependencyAnalyzerUtils.NonLocalAttributes)
                                    .Concat(localSymbols)
                                    .ToArray();

        compilationContext
            .RegisterSyntaxNodeAction(c => AnalyzeConstructor(c, localSymbols, symbols),
                                        SyntaxKind.ConstructorDeclaration);
    }

    private void AnalyzeConstructor(SyntaxNodeAnalysisContext context, INamedTypeSymbol[] localSymbols,
                                                                INamedTypeSymbol[] attributeSymbols)
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

            var nonLocalSymbols = attributeSymbols.Except(localSymbols).ToArray();

            foreach (var parameter in ctor.ParameterList.Parameters)
            {
                try
                {
                    var t = parameter.Type;
                    if (t is null) continue;

                    var symbol = context.SemanticModel.GetSymbolInfo(t, context.CancellationToken).Symbol;
                    if (symbol is null) continue;

                    if (symbol.HasAttribute(localSymbols) && !symbol.HasAttribute(nonLocalSymbols))
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