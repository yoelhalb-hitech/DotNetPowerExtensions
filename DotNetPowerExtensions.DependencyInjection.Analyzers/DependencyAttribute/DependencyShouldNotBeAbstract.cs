using DotNetPowerExtensions.DependencyInjection.Analyzers;

namespace SequelPay.DotNetPowerExtensions.Analyzers.DependencyManagement.DependencyAttribute.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DependencyShouldNotBeAbstract : AnalyzerBase
{
    public const string DiagnosticId = "DNPE0209";
    protected const string Title = "DependencyShouldNotBeAbstract";
    protected const string Message = "Use `{0}Base` instead of `{0}` when class is abstract";

    protected DiagnosticDescriptor Diagnostic = new DiagnosticDescriptor(DiagnosticId, Title, Message, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Message + ".");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Diagnostic);

    protected override void Register(CompilationStartAnalysisContext compilationContext, MetadataUtil metadataUtil)
    {
        var symbols = metadataUtil.GetTypeSymbols(DependencyAnalyzerUtils.AllDependencies);

        compilationContext
            .RegisterSyntaxNodeAction(c => AnalyzeClass(c, symbols), SyntaxKind.Attribute);
    }

    private void AnalyzeClass(SyntaxNodeAnalysisContext context, INamedTypeSymbol[] attributeSymbols)
    {
        try
        {
            // Since a class decleration can be partial we will only report it on the attribute
            var result = DependencyAnalyzerUtils.GetAttributeInfo(context,
                                                            DependencyAnalyzerUtils.DependencyAttributeNames, attributeSymbols);
            if (result is null) return;

            var (attr, attrName, methodSymbol) = result.Value!;

            var parent = context.Node.FirstAncestorOrSelf<TypeDeclarationSyntax>();
            if (parent is null) return;

            var classSymbol = context.SemanticModel.GetDeclaredSymbol(parent, context.CancellationToken);
            if (classSymbol is null || !classSymbol.IsAbstract) return;

            var diagnostic = Microsoft.CodeAnalysis.Diagnostic.Create(Diagnostic, attr!.GetLocation(), attrName);
            context.ReportDiagnostic(diagnostic);
        }
        catch { }
    }
}
