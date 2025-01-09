


using Microsoft.CodeAnalysis.Operations;

namespace DotNetPowerExtensions.MustInitialize.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MustInitializeNotAllowedGenericWithNew : MustInitializeAnalyzerBase
{
    public const string DiagnosticId = "DNPE0219";
    protected const string Title = "MustInitializeNotInGenericWithNew";
    protected const string Message = "Cannot use `{0}` as type argument for {1} because {1} requires `new()` but {0} has `MustInitialize`";
    protected const string Description = Message + ".";

    protected override DiagnosticDescriptor DiagnosticDesc => Diagnostic;

    protected override bool IncludeInitializedAttribute => false;

    [SuppressMessage("Microsoft.Design", "CA1051: Do not declare visible instance fields", Justification = "The compiler only consideres fields when tracking analyzer releases")]
    protected DiagnosticDescriptor Diagnostic = new DiagnosticDescriptor(DiagnosticId, Title, Message, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

    public override void Register(CompilationStartAnalysisContext compilationContext, INamedTypeSymbol[] mustInitializeSymbols)
    {
        compilationContext.RegisterSyntaxNodeAction(c => AnalyzeCreation(c, mustInitializeSymbols),
                SyntaxKind.GenericName);
    }

    private void AnalyzeCreation(SyntaxNodeAnalysisContext context, INamedTypeSymbol[] mustInitializeSymbols)
    {
        try
        {
            var expr = context.Node as GenericNameSyntax;
            if (expr is null) return;

            var symbol = context.SemanticModel.GetSymbolInfo(expr, context.CancellationToken).Symbol as INamedTypeSymbol;
            if (symbol is null || !symbol.IsGenericType) return;

            var worker = new MustInitializeWorker(context.Compilation, context.SemanticModel);
            for (int i = 0; i < symbol.TypeArguments.Length; i++)
            {
                var param = symbol.TypeParameters[i];
                if (!param.HasConstructorConstraint) continue;

                var arg = symbol.TypeArguments[i];
                if (arg.IsEqualTo(param)) continue;

                //if(arg.iseerortype)
                var ctor = (arg as INamedTypeSymbol)?.Constructors.FirstOrDefault(c => c.Parameters.Length == 0);
                if(ctor is null) continue; // Technically shouln't happen in valid code

                if (worker.GetMustInitialize(arg, ctor, out _).Any())
                {
                    var diagnostic = Microsoft.CodeAnalysis.Diagnostic.Create(DiagnosticDesc, expr.GetLocation(), arg.Name, symbol.OriginalDefinition.Name);

                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
        catch { }
    }
}
