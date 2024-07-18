using DotNetPowerExtensions.Extensions;
using Microsoft.CodeAnalysis.Operations;

namespace DotNetPowerExtensions.Analyzers.Throws;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ReferencedDocCommentExceptionDoesNotExists : DiagnosticAnalyzer
{
    protected const string Category = "Language";
    public const string DiagnosticId = "DNPE0503";
    protected const string Title = "ReferencedDocCommentExceptionDoesNotExists";
    protected const string Message = "Exception `{0}` documented in DocComments on referenced/invoked member `{1}` does not exist in the current context";
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
                var symbolTypes = new[] { typeof(ThrowsByDocCommentAttribute), typeof(DoesNotThrowAttribute), typeof(ThrowsAttribute) };
                var symbols = symbolTypes.Select(t => metadata(t)).OfType<INamedTypeSymbol>().ToArray();

                compilationContext.RegisterSymbolStartAction(s => AnalyzeSymbolStart(s, symbols), SymbolKind.Method);

            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }

    private void AnalyzeSymbolStart(SymbolStartAnalysisContext symbolContext, INamedTypeSymbol[] attrSymbols)
    {
        try
        {
            var symbol = ThrowsUtils.GetCorrectSymbol(symbolContext);
            if (symbol is null || !symbol.HasAttribute(attrSymbols)) return;

            symbolContext.RegisterOperationAction(o => AnalyzeOperation(o), OperationKind.Invocation, OperationKind.PropertyReference, OperationKind.EventReference);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }

    private void AnalyzeOperation(OperationAnalysisContext operationContext)
    {
        try
        {
            var symbol = (operationContext.Operation as IMethodReferenceOperation)?.Method as ISymbol
                                ?? (operationContext.Operation as IPropertyReferenceOperation)?.Property as ISymbol
                                ?? (operationContext.Operation as IEventReferenceOperation)?.Event;
            if (symbol is null || !symbol.GetDocumentationCommentXml().HasValue()) return;

            var exceptions = ThrowsUtils.GetDocCommentExceptions(symbol, operationContext.Compilation);
            foreach (var line in exceptions.Where(e => e.Item2 is null))
            {
                var diagnostic = Microsoft.CodeAnalysis.Diagnostic.Create(Diagnostic, operationContext.Operation.Syntax.GetLocation(), line.Item1, symbol.Name);
                operationContext.ReportDiagnostic(diagnostic);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}
