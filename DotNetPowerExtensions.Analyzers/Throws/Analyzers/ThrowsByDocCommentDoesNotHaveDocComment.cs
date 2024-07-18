using DotNetPowerExtensions.Extensions;

namespace DotNetPowerExtensions.Analyzers.Throws;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ThrowsByDocCommentDoesNotHaveDocComment : DiagnosticAnalyzer
{
    protected const string Category = "Language";
    public const string DiagnosticId = "DNPE0501";
    protected const string Title = "ThrowsByDocCommentDoesNotHaveDocComment";
    protected const string Message = "When `ThrowsByDocComment` then Doc comment is required";
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
                Func<Type, INamedTypeSymbol?> metadata = t => compilationContext.Compilation.GetTypeByMetadataName(t.FullName!);
                var symbol = metadata(typeof(ThrowsByDocCommentAttribute));

                compilationContext
                    .RegisterOperationAction(c => AnalyzeOperation(c),
                                OperationKind.Invocation);
                compilationContext
                    .RegisterOperationBlockStartAction(c => AnalyzeOperationBlock(c));
                compilationContext
                    .RegisterCodeBlockAction(c => AnalyzeCodeBlock(c));
                compilationContext.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.MethodDeclaration);
                compilationContext.RegisterSymbolAction(s => AnalyzeSymbol(s, symbol), SymbolKind.Method, SymbolKind.Property);

            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }

    private void AnalyzeSymbol(SymbolAnalysisContext symbolContext, INamedTypeSymbol attrSymbol)
    {
        try
        {
            var symbol = ThrowsUtils.GetCorrectSymbol(symbolContext);

            if (symbol is null || !symbol.HasAttribute(attrSymbol))
                return;

            var docs = symbol!.GetDocumentationCommentXml();
            if(!docs.HasValue())
            {
                var diagnostic = Microsoft.CodeAnalysis.Diagnostic.Create(Diagnostic, symbol.Locations.FirstOrDefault());
                symbolContext.ReportDiagnostic(diagnostic);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }

    private void AnalyzeInvocation(SyntaxNodeAnalysisContext obj)
    {
        //throw new NotImplementedException();
    }

    private void AnalyzeCodeBlock(CodeBlockAnalysisContext c)
    {
        //throw new NotImplementedException();
    }

    private void AnalyzeOperationBlock(OperationBlockStartAnalysisContext c)
    {
        //throw new NotImplementedException();
    }

    private void AnalyzeOperation(OperationAnalysisContext c)
    {
        //throw new NotImplementedException();
    }
}
