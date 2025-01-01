using System.Text.RegularExpressions;

namespace DotNetPowerExtensions.MustInitialize.Analyzers;

#if !NET45 && !NET46

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class SuppressOriginalNotExisting : DiagnosticSuppressor
{
    private static readonly SuppressionDescriptor MightRequireRule = new SuppressionDescriptor(
        id: "YH10002",
        suppressedDiagnosticId: "DNPE0217",
        justification: "It has MightRequire");

    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions => ImmutableArray.Create(MightRequireRule);

    public override void ReportSuppressions(SuppressionAnalysisContext context)
    {
        try
        {
            foreach (var diagnostic in context.ReportedDiagnostics)
            {
                AnalyzeDiagnostic(diagnostic, context);
            }
        }
        catch { }
    }

    private static void AnalyzeDiagnostic(Diagnostic diagnostic, SuppressionAnalysisContext context)
    {
        try
        {
            var node = diagnostic.Location.SourceTree?.GetRoot(context.CancellationToken).FindNode(diagnostic.Location.SourceSpan);
            if (node is not SimpleNameSyntax identifier) return;

            var semanticModel = context.GetSemanticModel(diagnostic.Location.SourceTree!);
            var invocation = node.FirstAncestorOrSelf<InvocationExpressionSyntax>();

            if (invocation is null || semanticModel is null
                || semanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol is not IMethodSymbol methodSymbol
                || methodSymbol.ReceiverType is not INamedTypeSymbol classType
                || !classType.IsGenericType) return;

            var innerClass = classType.TypeArguments.FirstOrDefault();
            if (innerClass is null) return;

            var worker = new MustInitializeWorker(context.Compilation, semanticModel);

            var mightRequires = MightRequireUtils.GetMightRequiredInfos(innerClass, worker.MightRequireSymbols)
                                                .Select(m => m.Name).ToList();

            if(mightRequires.Contains(identifier.Identifier.ValueText))
            {
                context.ReportSuppression(Suppression.Create(MightRequireRule, diagnostic));
            }
        }
        catch { }
    }
}

#endif
