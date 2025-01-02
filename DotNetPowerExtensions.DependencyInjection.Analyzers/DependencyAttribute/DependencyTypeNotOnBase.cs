using SequelPay.DotNetPowerExtensions.RoslynExtensions;
using Utils = SequelPay.DotNetPowerExtensions.Analyzers.DependencyManagement.DependencyAttribute.Analyzers.DependencyAnalyzerUtils;

namespace SequelPay.DotNetPowerExtensions.Analyzers.DependencyManagement.DependencyAttribute.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DependencyTypeNotOnBase : DiagnosticAnalyzer
{

    protected const string Category = "Language";
    public const string DiagnosticId = "DNPE0212";
    protected const string Title = "DependencyTypeNotOnBase";
    protected const string Message = "`{1}` not found on `{0}`, add `{1}` to the decleration of `{0}`";
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

                var symbols = Utils.AllDependencies.Select(t => metadata(t)).Where(x => x is not null).Select(x => x!).ToArray();

                var baseSymbols = Utils.BaseAttributes.Select(t => metadata(t)).Where(x => x is not null).Select(x => x!).ToArray();


                compilationContext
                    .RegisterSyntaxNodeAction(c => AnalyzeAttribute(c, symbols, baseSymbols), SyntaxKind.Attribute);
            });
        }
        catch { }
    }

    private void AnalyzeAttribute(SyntaxNodeAnalysisContext context, INamedTypeSymbol[] attributeSymbols, INamedTypeSymbol[] baseSymbols)
    {
        try
        {
            // Since a class decleration can be partial we will only report it on the attribute
            var result = Utils.GetAttributeWithTypes(context,
                                                            Utils.DependencyAttributeNames, attributeSymbols);
            if (result is null) return;
            var (attr, attrName, methodSymbol, forTypes) = result.Value;

            var baseAttribName = Utils.AttributeToBaseName(methodSymbol.ContainingType);
            var baseAttribType = baseSymbols.FirstOrDefault(s => s.Name == baseAttribName);
            if (baseAttribType is null) return;

            foreach (var forType in forTypes.Where(f => !f.HasAttribute(baseAttribType)))
            {
                var diagnostic = Microsoft.CodeAnalysis.Diagnostic.Create(Diagnostic, attr!.GetLocation(), forType.Name,
                                                                                                    baseAttribName, attrName);

                context.ReportDiagnostic(diagnostic);
            }
        }
        catch { }
    }
}
