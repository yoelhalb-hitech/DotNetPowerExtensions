using SequelPay.DotNetPowerExtensions.RoslynExtensions;

namespace SequelPay.DotNetPowerExtensions.Analyzers.DependencyManagement.DependencyAttribute.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UseTransientOnlyInTransient : DiagnosticAnalyzer
{
    protected const string Category = "Language";
    public const string DiagnosticId = "DNPE0223";
    protected const string Title = "UseTransientOnlyInTransient";
    protected const string Message = "A transient service should only be used in a class decorated with `Transient` or `Local`";
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
                Func<Type, INamedTypeSymbol?> metadata = t => compilationContext.Compilation.GetTypeSymbol(t);

                var localSymbols = DependencyAnalyzerUtils.LocalAttributes.Select(t => metadata(t)).OfType<INamedTypeSymbol>().ToArray();
                if (!localSymbols.Any()) return;

                var transientSymbols = DependencyAnalyzerUtils.TransientAttributes.Select(t => metadata(t)).OfType<INamedTypeSymbol>().ToArray();
                if (!transientSymbols.Any()) return;

                var symbols = DependencyAnalyzerUtils.NonLocalAttributes.Select(t => metadata(t)).OfType<INamedTypeSymbol>()
                                            .Concat(transientSymbols)
                                            .ToArray();

                compilationContext
                    .RegisterSyntaxNodeAction(c => AnalyzeConstructor(c, localSymbols, transientSymbols, symbols),
                                                SyntaxKind.ConstructorDeclaration);
            });
        }
        catch { }
    }

    private void AnalyzeConstructor(SyntaxNodeAnalysisContext context, INamedTypeSymbol[] localSymbols,
            INamedTypeSymbol[] transientSymbols, INamedTypeSymbol[] attributeSymbols)
    {
        try
        {
            var ctor = context.Node as ConstructorDeclarationSyntax;

            if (ctor is null || !ctor.ParameterList.Parameters.Any()
                || context.SemanticModel.GetDeclaredSymbol(ctor, context.CancellationToken) is not IMethodSymbol methodSymbol) return;

            // Check if this is a service
            if (methodSymbol.ContainingType.GetAttributes()
                            .Where(a => a.AttributeClass is not null)
                            .All(a => !attributeSymbols.ContainsGeneric(a.AttributeClass!))) return;

            if (methodSymbol.ContainingType.GetAttributes()
                .Where(a => a.AttributeClass is not null)
                .All(a => !attributeSymbols.ContainsGeneric(a.AttributeClass!))) return;

            var otherSymbols = attributeSymbols.Except(localSymbols).Except(transientSymbols).ToList();

            if (methodSymbol.ContainingType.GetAttributes()
                .Where(a => a.AttributeClass is not null)
                .All(a => !otherSymbols.ContainsGeneric(a.AttributeClass!))) return;

            foreach (var parameter in ctor.ParameterList.Parameters)
            {
                try
                {
                    var t = parameter.Type;
                    if (t is null) continue;

                    var symbol = context.SemanticModel.GetSymbolInfo(t, context.CancellationToken).Symbol;
                    if (symbol is null) continue;

                    if (symbol.HasAttribute(transientSymbols))
                    {
                        var diagnostic = Microsoft.CodeAnalysis.Diagnostic.Create(Diagnostic, parameter.GetLocation(), symbol.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
                catch { }
            }
        }
        catch { }
    }
}