
namespace DotNetPowerExtensions.Analyzers.DependencyManagement.DependencyAttribute.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DependencyRequiredWhenBase : DiagnosticAnalyzer
{
    protected const string Category = "Language";
    public const string DiagnosticId = "DNPE0211";
    protected const string Title = "DependencyRequiredWhenBase";
    protected const string Message = "`{0}` for {1} is required";
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

                var symbols = DependencyAnalyzerUtils.AllDependencies.Select(t => metadata(t)).Where(x => x is not null).Select(x => x!).ToArray();

                var baseSymbols = DependencyAnalyzerUtils.BaseAttributes.Select(t => metadata(t)).Where(x => x is not null).Select(x => x!).ToArray();

                compilationContext
                    .RegisterSyntaxNodeAction(c => AnalyzeClass(c, symbols, baseSymbols),
                                SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration, SyntaxKind.RecordDeclaration, SyntaxKind.RecordStructDeclaration);
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }

    private void AnalyzeClass(SyntaxNodeAnalysisContext context, INamedTypeSymbol[] attributeSymbols, INamedTypeSymbol[] baseAttributeSymbols)
    {
        try
        {
            // Since a class decleration can be partial we have to go by the symbol
            var decl = context.Node as TypeDeclarationSyntax;
            if (decl is null) return;

            var symbol = context.SemanticModel.GetDeclaredSymbol(decl, context.CancellationToken);
            if (symbol is null || (symbol.BaseType is null && !symbol.Interfaces.Any())) return;

            var bases = symbol.GetAllBaseTypes()
                            .Concat(symbol.AllInterfaces)
                            .Where(t => t.HasAttribute(baseAttributeSymbols))
                            .ToArray();
            if (!bases.Any()) return;

            var types = symbol.GetAttributes()
                                .Where(a => attributeSymbols.ContainsGeneric(a.AttributeClass))
                                .SelectMany(a => DependencyAnalyzerUtils.GetForTypes(a))
                                .ToArray();

            var basesMissing = bases.Where(b => types.All(t => !t.IsEqualTo(b)));

            foreach (var baseType in basesMissing)
            {
                var attrName = baseType.GetAttribute(baseAttributeSymbols)!.AttributeClass!.Name.Replace("Base", "");
                var diagnostic = Microsoft.CodeAnalysis.Diagnostic.Create(Diagnostic, decl!.GetLocation(), attrName, baseType.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}
