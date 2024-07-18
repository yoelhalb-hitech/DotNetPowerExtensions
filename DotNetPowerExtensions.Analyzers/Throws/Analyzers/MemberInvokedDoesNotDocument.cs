using DotNetPowerExtensions.Extensions;
using Microsoft.CodeAnalysis.Operations;
using SequelPay.DotNetPowerExtensions.RoslynExtensions;
using System;

namespace DotNetPowerExtensions.Analyzers.Throws;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MemberInvokedDoesNotDocument : DiagnosticAnalyzer
{
    protected const string Category = "Language";
    public const string DiagnosticId = "DNPE0502";
    protected const string Title = "MemberInvokedDoesNotDocument";
    protected const string Message = "Member `{0}` referenced/invoked does not have exception documentation";
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

                compilationContext.RegisterSymbolStartAction(s => AnalyzeStartSymbol(s, symbols), SymbolKind.Method); // Property directly doesn't work since we need the get/set to work
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }

    private void AnalyzeStartSymbol(SymbolStartAnalysisContext symbolContext, INamedTypeSymbol[] attrSymbols)
    {
        try
        {
            var symbol = ThrowsUtils.GetCorrectSymbol(symbolContext);

            if (symbol is null || !symbol.HasAttribute(attrSymbols))
                return;

            symbolContext.RegisterOperationAction(o => AnalyzeOperation(o, attrSymbols), OperationKind.Invocation, OperationKind.PropertyReference, OperationKind.EventReference);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }

    private void AnalyzeOperation(OperationAnalysisContext operationContext, INamedTypeSymbol[] attrSymbols)
    {
        try
        {
            var symbol = (operationContext.Operation as IMethodReferenceOperation)?.Method as ISymbol
                                ?? (operationContext.Operation as IPropertyReferenceOperation)?.Property as ISymbol
                                ?? (operationContext.Operation as IObjectCreationOperation)?.Constructor as ISymbol
                                ?? (operationContext.Operation as IEventReferenceOperation)?.Event;
            if (symbol is null) return;

            if (!symbol.HasAttribute(attrSymbols) && !symbol.GetDocumentationCommentXml().HasValue())
            {
                var diagnostic = Microsoft.CodeAnalysis.Diagnostic.Create(Diagnostic, operationContext.Operation.Syntax.GetLocation(), symbol.Name);
                operationContext.ReportDiagnostic(diagnostic);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}

