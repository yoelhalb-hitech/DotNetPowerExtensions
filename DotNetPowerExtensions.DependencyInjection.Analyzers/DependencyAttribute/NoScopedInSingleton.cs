using SequelPay.DotNetPowerExtensions.RoslynExtensions;

namespace SequelPay.DotNetPowerExtensions.Analyzers.DependencyManagement.DependencyAttribute.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class NoScopedInSingleton : DiagnosticAnalyzer
{
    protected const string Category = "Language";
    public const string DiagnosticId = "DNPE0224";
    protected const string Title = "NoScopedInSingleton";
    protected const string Message = "Do not use scoped service in a class decorated with `Singleton`";
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


                var singletonSymbols = DependencyAnalyzerUtils.SingletonAttributes.Select(t => metadata(t)).OfType<INamedTypeSymbol>().ToArray();
                if (!singletonSymbols.Any()) return;

                var scopedSymbols = DependencyAnalyzerUtils.ScopedAttributes.Select(t => metadata(t)).OfType<INamedTypeSymbol>().ToArray();
                if (!scopedSymbols.Any()) return;

                compilationContext
                    .RegisterSyntaxNodeAction(c => AnalyzeConstructor(c, singletonSymbols, scopedSymbols),
                                                SyntaxKind.ConstructorDeclaration);
            });
        }
        catch { }
    }

    private void AnalyzeConstructor(SyntaxNodeAnalysisContext context, INamedTypeSymbol[] singletonSymbols,
                                                                                INamedTypeSymbol[] scopedSymbols)
    {
        try
        {
            var ctor = context.Node as ConstructorDeclarationSyntax;

            if (ctor is null || !ctor.ParameterList.Parameters.Any()
                || context.SemanticModel.GetDeclaredSymbol(ctor, context.CancellationToken) is not IMethodSymbol methodSymbol) return;

            // Check if this is a singleton
            if (methodSymbol.ContainingType.GetAttributes()
                            .Where(a => a.AttributeClass is not null)
                            .All(a => !singletonSymbols.ContainsGeneric(a.AttributeClass!))) return;

            foreach (var parameter in ctor.ParameterList.Parameters)
            {
                try
                {
                    var t = parameter.Type;
                    if (t is null) continue;

                    var symbol = context.SemanticModel.GetSymbolInfo(t, context.CancellationToken).Symbol;
                    if (symbol is null) continue;

                    if (symbol.HasAttribute(scopedSymbols))
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