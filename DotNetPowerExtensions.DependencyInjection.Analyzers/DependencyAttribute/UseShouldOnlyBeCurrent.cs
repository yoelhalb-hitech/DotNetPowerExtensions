using SequelPay.DotNetPowerExtensions.RoslynExtensions;
using SequelPay.DotNetPowerExtensions;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using static DotNetPowerExtensions.DependencyInjection.Analyzers.AnalyzerBase;

namespace SequelPay.DotNetPowerExtensions.Analyzers.DependencyManagement.DependencyAttribute.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UseShouldOnlyBeCurrent : AnalyzerBase
{
    public const string DiagnosticId = "DNPE0207";
    protected const string Title = "UseShouldBeCurrent";
    protected const string Message = "The `Use` attribute should only be the current generic type";

    protected DiagnosticDescriptor Diagnostic = new DiagnosticDescriptor(DiagnosticId, Title, Message, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Message + ".");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Diagnostic);

    protected override void Register(CompilationStartAnalysisContext compilationContext, MetadataUtil metadataUtil)
    {
        // We do not do it for the base attributes as they are not instantiated directly and can be used for a subclass
        var symbols = metadataUtil.GetTypeSymbols(DependencyAnalyzerUtils.AllDependencies);

        compilationContext
            .RegisterSyntaxNodeAction(c => AnalyzeClass(c, symbols),
                                        SyntaxKind.Attribute);
    }

    private void AnalyzeClass(SyntaxNodeAnalysisContext context, INamedTypeSymbol[] attributeSymbols)
    {
        try
        {
            // Since a class decleration can be partial we will only report it on the attribute
            var result = DependencyAnalyzerUtils.GetAttributeInfo(context,
                                                            DependencyAnalyzerUtils.DependencyAttributeNames, attributeSymbols);
            if (result is null) return;
            var (attr, attrName, methodSymbol) = result.Value;

            var (useExpression, innerExpression) = DependencyAnalyzerUtils.GetUse(attr);
            if (innerExpression is not TypeOfExpressionSyntax typeExpression) return;

            var parent = context.Node.FirstAncestorOrSelf<TypeDeclarationSyntax>();
            if (parent is null) return;

            if (context.SemanticModel.GetDeclaredSymbol(parent!, context.CancellationToken) is not INamedTypeSymbol classSymbol) return;
            if (!classSymbol.IsGenericType) return;

            if (context.SemanticModel.GetSymbolInfo(typeExpression.Type!, context.CancellationToken).Symbol is not INamedTypeSymbol typeSymbol) return;
            if (typeSymbol.IsGenericType && classSymbol.ConstructUnboundGenericType().IsEqualTo(typeSymbol.ConstructUnboundGenericType())) return;

            var diagnostic = Microsoft.CodeAnalysis.Diagnostic.Create(Diagnostic, useExpression!.GetLocation());

            context.ReportDiagnostic(diagnostic);
        }
        catch { }
    }
}