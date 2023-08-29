using DotNetPowerExtensions.RoslynExtensions;
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
    protected abstract bool IncludeInitializedAttribute { get; }

    public virtual Type[] AttributeTypes =>
    new[]
    {
        typeof(SequelPay.DotNetPowerExtensions.MustInitializeAttribute),
    };

    public override void Initialize(AnalysisContext context)
    {
        try
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compilationContext =>
            {
                var symbols = AttributeTypes.Select(n => compilationContext.Compilation.GetTypeSymbol(n));
                if (symbols.Any(s => s is null)) return;

                if(IncludeInitializedAttribute)
                {
                    var symbol = compilationContext.Compilation.GetTypeSymbol(typeof(SequelPay.DotNetPowerExtensions.InitializedAttribute));
                    if (symbol is not null) symbols = symbols.Union<INamedTypeSymbol?>(new [] { symbol }, SymbolEqualityComparer.Default);
                }

                Register(compilationContext, symbols.OfType<INamedTypeSymbol>().ToArray());
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}
