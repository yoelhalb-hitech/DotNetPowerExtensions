using DotNetPowerExtensions.DependencyInjection.Analyzers;

namespace SequelPay.DotNetPowerExtensions.Analyzers.DependencyManagement.DependencyAttribute.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class LocalServiceIsNotForLocal : AnalyzerBase
{
    public const string DiagnosticId = "DNPE0219";
    protected const string Title = "LocalServiceIsNotForLocal";
    protected const string Message = "`{0}` is not decorated with the `Local` attribute";

    protected DiagnosticDescriptor Diagnostic = new DiagnosticDescriptor(DiagnosticId, Title, Message, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Message + ".");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Diagnostic);

    protected override void Register(CompilationStartAnalysisContext compilationContext, MetadataUtil metadataUtil)
    {
        var localServiceSymbol = metadataUtil.GetTypeSymbol(typeof(ILocalFactory<>));
        if (localServiceSymbol is null) return;

        var localSymbols = metadataUtil.GetTypeSymbols(DependencyAnalyzerUtils.LocalAttributes);
        if (!localSymbols.Any()) return;

        var symbols = metadataUtil.GetTypeSymbols(DependencyAnalyzerUtils.NonLocalAttributes)
                                    .Concat(localSymbols)
                                    .ToArray();

        compilationContext
            .RegisterSyntaxNodeAction(c => AnalyzeConstructor(c, localSymbols, localServiceSymbol, symbols),
                                        SyntaxKind.ConstructorDeclaration);
    }

    private void AnalyzeConstructor(SyntaxNodeAnalysisContext context, INamedTypeSymbol[] localSymbols,
                                                                INamedTypeSymbol serviceTypeSymbol, INamedTypeSymbol[] attributeSymbols)
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

                    var symbol = context.SemanticModel.GetSymbolInfo(t, context.CancellationToken).Symbol as INamedTypeSymbol;
                    if (symbol is null) continue;

                    if(!symbol.IsGenericEqual(serviceTypeSymbol)) continue;

                    var innerSymbol = symbol.TypeArguments.FirstOrDefault();
                    if(innerSymbol is null) continue;

                    if (!innerSymbol.HasAttribute(localSymbols))
                    {
                        var diagnostic = Microsoft.CodeAnalysis.Diagnostic.Create(Diagnostic, parameter.GetLocation(), innerSymbol.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
                catch { }
            }
        }
        catch { }
    }
}