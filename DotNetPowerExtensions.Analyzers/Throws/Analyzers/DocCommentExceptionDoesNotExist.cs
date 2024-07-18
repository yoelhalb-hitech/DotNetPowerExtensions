using DotNetPowerExtensions.Extensions;

namespace DotNetPowerExtensions.Analyzers.Throws;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DocCommentExceptionDoesNotExist : DiagnosticAnalyzer
{
    protected const string Category = "Language";
    public const string DiagnosticId = "DNPE0504";
    protected const string Title = "DocCommentExceptionDoesNotExist";
    protected const string Message = "Exception `{0}` documented in DocComments does not exists in the current context";
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
                if (symbol is null) return;

                compilationContext.RegisterSymbolAction(s => AnalyzeSymbol(s, symbol), SymbolKind.Method);

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

            if (!symbolContext.Symbol.HasAttribute(attrSymbol) || !symbolContext.Symbol.GetDocumentationCommentXml().HasValue()) return;

            var exceptions = ThrowsUtils.GetDocCommentExceptions(symbol, symbolContext.Compilation);
            foreach (var line in exceptions.Where(e => e.Item2 is null))
            {
                var diagnostic = Microsoft.CodeAnalysis.Diagnostic.Create(Diagnostic, symbolContext.Symbol.Locations.FirstOrDefault(), line.Item1);
                symbolContext.ReportDiagnostic(diagnostic);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}
