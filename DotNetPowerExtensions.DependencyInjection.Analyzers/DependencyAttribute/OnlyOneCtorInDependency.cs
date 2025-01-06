using DotNetPowerExtensions.DependencyInjection.Analyzers;

namespace SequelPay.DotNetPowerExtensions.Analyzers.DependencyManagement.DependencyAttribute.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class OnlyOneCtorInDependency : AnalyzerBase
{
    public const string DiagnosticId = "DNPE0226";
    protected const string Title = "OnlyOneCtorInDependency";
    protected const string Message = "Only one public ctor is allowed in a class decorated with `Singleton/Scoped/Transient/Local` attribute";

    protected DiagnosticDescriptor Diagnostic = new(DiagnosticId, Title, Message, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Message + ".");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Diagnostic);

    protected override void Register(CompilationStartAnalysisContext compilationContext, MetadataUtil metadataUtil)
    {
        // We do not do it for the base attributes as they are not instantiated directly and can be used for a subclass
        var symbols = metadataUtil.GetTypeSymbols(DependencyAnalyzerUtils.AllDependencies);

        compilationContext
            .RegisterSyntaxNodeAction(c => AnalyzeConstructor(c, symbols),
                                        SyntaxKind.ClassDeclaration);
    }

    private void AnalyzeConstructor(SyntaxNodeAnalysisContext context, INamedTypeSymbol[] attributeSymbols)
    {
        try
        {
            var cl = context.Node as ClassDeclarationSyntax;

            if (cl is null
                || context.SemanticModel.GetDeclaredSymbol(cl, context.CancellationToken) is not ITypeSymbol typeSymbol) return;

            // Check if this is a service
            if (!typeSymbol.HasAttribute(attributeSymbols)) return;

            if(typeSymbol.GetConstructors(false).Where(c => !c.DeclaredAccessibility.HasFlag(Accessibility.Private)).Count() <= 1) return;

            var diagnostic = Microsoft.CodeAnalysis.Diagnostic.Create(Diagnostic, cl.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
        catch { }
    }
}