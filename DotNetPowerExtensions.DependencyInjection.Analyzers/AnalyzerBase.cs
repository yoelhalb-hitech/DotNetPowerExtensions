
namespace DotNetPowerExtensions.DependencyInjection.Analyzers;

public abstract class AnalyzerBase : DiagnosticAnalyzer
{
    protected const string Category = "Language";

    protected internal class MetadataUtil(CompilationStartAnalysisContext compilationContext)
    {
        public INamedTypeSymbol? GetTypeSymbol(Type t) => compilationContext.Compilation.GetTypeSymbol(t);
        public INamedTypeSymbol[] GetTypeSymbols(IEnumerable<Type> types) => types.Select(GetTypeSymbol).OfType<INamedTypeSymbol>().ToArray();

    }

    protected abstract void Register(CompilationStartAnalysisContext compilationContext, MetadataUtil metadataUtil);

    public override void Initialize(AnalysisContext context)
    {
        try
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compilationContext =>
            {
                Register(compilationContext, new MetadataUtil(compilationContext));
            });
        }
        catch { }
    }
}
