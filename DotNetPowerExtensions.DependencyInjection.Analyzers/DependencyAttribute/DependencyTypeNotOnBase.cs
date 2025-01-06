using DotNetPowerExtensions.DependencyInjection.Analyzers;
using Utils = SequelPay.DotNetPowerExtensions.Analyzers.DependencyManagement.DependencyAttribute.Analyzers.DependencyAnalyzerUtils;

namespace SequelPay.DotNetPowerExtensions.Analyzers.DependencyManagement.DependencyAttribute.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DependencyTypeNotOnBase : AnalyzerBase
{
    public const string DiagnosticId = "DNPE0212";
    protected const string Title = "DependencyTypeNotOnBase";
    protected const string Message = "`{1}` not found on `{0}`, add `{1}` to the decleration of `{0}`";

    protected DiagnosticDescriptor Diagnostic = new DiagnosticDescriptor(DiagnosticId, Title, Message, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Message + ".");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Diagnostic);

    protected override void Register(CompilationStartAnalysisContext compilationContext, MetadataUtil metadataUtil)
    {
        var symbols = metadataUtil.GetTypeSymbols(DependencyAnalyzerUtils.AllDependencies);
        var baseSymbols = metadataUtil.GetTypeSymbols(DependencyAnalyzerUtils.BaseAttributes);

        compilationContext
            .RegisterSyntaxNodeAction(c => AnalyzeAttribute(c, symbols, baseSymbols), SyntaxKind.Attribute);
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
