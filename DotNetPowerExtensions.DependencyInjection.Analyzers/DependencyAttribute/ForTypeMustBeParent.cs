using SequelPay.DotNetPowerExtensions.RoslynExtensions;

namespace SequelPay.DotNetPowerExtensions.Analyzers.DependencyManagement.DependencyAttribute.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ForTypeMustBeParent : DiagnosticAnalyzer
{

    protected const string Category = "Language";
    public const string DiagnosticId = "DNPE0210";
    protected const string Title = "ForTypeMustBeParent";
    protected const string Message = "{0} is not a base class or interface of {1}";
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

                var symbols = DependencyAnalyzerUtils.AllDependencies.Select(t => metadata(t)).Where(x => x is not null).Select(x => x!).ToArray();

                compilationContext
                    .RegisterSyntaxNodeAction(c => AnalyzeAttribute(c, symbols), SyntaxKind.Attribute);
            });
        }
        catch { }
    }

    private void AnalyzeAttribute(SyntaxNodeAnalysisContext context, INamedTypeSymbol[] attributeSymbols)
    {
        try
        {
            // Since a class decleration can be partial we will only report it on the attribute
            var result = DependencyAnalyzerUtils.GetAttributeWithTypes(context,
                                                            DependencyAnalyzerUtils.DependencyAttributeNames, attributeSymbols);
            if (result is null) return;
            var (attr, attrName, methodSymbol, types) = result.Value;

            var parent = context.Node.FirstAncestorOrSelf<TypeDeclarationSyntax>();
            if (parent is null) return;

            if (context.SemanticModel.GetDeclaredSymbol(parent!, context.CancellationToken) is not INamedTypeSymbol classSymbol) return;

            var bases = new[] { classSymbol }.Concat(classSymbol.GetAllBaseTypes().Concat(classSymbol.AllInterfaces)).ToArray();

            foreach (var type in types.Where(t => bases.All(b => !b.IsEqualTo(t))))
            {
                var diagnostic = Microsoft.CodeAnalysis.Diagnostic.Create(Diagnostic, attr!.GetLocation(), type.Name, classSymbol.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }
        catch { }
    }
}
