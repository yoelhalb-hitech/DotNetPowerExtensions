using System.Collections.Immutable;

namespace DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;

public abstract class MustInitializeAnalyzerBase : DiagnosticAnalyzer
{
    protected const string Category = "Language";

    protected abstract DiagnosticDescriptor DiagnosticDesc { get; }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DiagnosticDesc);

    protected virtual Diagnostic CreateDiagnostic(AttributeData attribute)
        => Diagnostic.Create(DiagnosticDesc, attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation());

    protected virtual Diagnostic CreateDiagnostic(IPropertySymbol symbol)
        => Diagnostic.Create(DiagnosticDesc, symbol.DeclaringSyntaxReferences.First().GetSyntax().GetLocation());

    public abstract void Register(CompilationStartAnalysisContext compilationContext, INamedTypeSymbol[] mustInitializeSymbols);

    public virtual string[] AttributeNames =>
    new[]
    {
        typeof(DotNetPowerExtensions.MustInitializeAttribute).FullName!,
    };

    public override void Initialize(AnalysisContext context)
    {
        try
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compilationContext =>
            {
                var symbols = AttributeNames.Select(n => compilationContext.Compilation.GetTypeByMetadataName(n));
                if (symbols.Any(s => s is null)) return;

                Register(compilationContext, symbols.OfType<INamedTypeSymbol>().ToArray());
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}
