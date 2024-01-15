
namespace SequelPay.DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;

public abstract class MustInitializeRequiredMembersBase : MustInitializeAnalyzerBase
{
    protected override bool IncludeInitializedAttribute => false;

    protected void ReportDiagnostics(SyntaxNodeAnalysisContext context, CSharpSyntaxNode node, IEnumerable<string> props)
    {
        if (!props.Any()) return;

        var combined = string.Join(", ", props);
        var diagnostic = Diagnostic.Create(DiagnosticDesc, node.GetLocation(), combined);

        context.ReportDiagnostic(diagnostic);
    }
}
