
using Microsoft.CodeAnalysis.Elfie.Model;
using SequelPay.DotNetPowerExtensions;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace DotNetPowerExtensions.Analyzers.DependencyManagement.DependencyAttribute.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DependencyTypeDoesNotMatchBase : DiagnosticAnalyzer
{

    protected const string Category = "Language";
    public const string DiagnosticId = "DNPE0212";
    protected const string Title = "DependencyTypeDoesNotMatchBase";
    protected const string Message = "Use `{1}` for `{0}` and not `{2}`";
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
                    .RegisterSyntaxNodeAction(c => AnalyzeClass(c, symbols, baseSymbols), SyntaxKind.Attribute);
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }

    private void AnalyzeClass(SyntaxNodeAnalysisContext context, INamedTypeSymbol[] attributeSymbols, INamedTypeSymbol[] baseSymbols)
    {
        try
        {
            // Since a class decleration can be partial we will only report it on the attribute
            var attr = context.Node as AttributeSyntax;
            var attrName = attr?.Name.GetUnqualifiedName()?.Replace(nameof(Attribute), "");
            if (attrName is null || !DependencyAnalyzerUtils.DependencyAttributeNames.Contains(attrName + nameof(Attribute)))
                return;

            var args = attr!.ArgumentList?.Arguments.Where(a => a.NameEquals is null).ToArray();
            if (args?.Any() != true && attr.Name is not GenericNameSyntax
                                && (attr.Name is not QualifiedNameSyntax qualified || qualified.Right is not GenericNameSyntax))
                return;

            if (context.SemanticModel.GetSymbolInfo(attr!, context.CancellationToken).Symbol is not IMethodSymbol methodSymbol
                || !attributeSymbols.ContainsGeneric(methodSymbol.ContainingType)) return;

            var argTypes = args?.Select(a =>
            {
                var innerExpression = a.Expression;
                while (innerExpression is ParenthesizedExpressionSyntax paren && paren.Expression is not null) innerExpression = paren.Expression;
                return innerExpression;
            })
                .OfType<TypeOfExpressionSyntax>()
                .Select(e => context.SemanticModel.GetSymbolInfo(e.Type, context.CancellationToken).Symbol)
                .OfType<ITypeSymbol>();

            var types = (argTypes ?? new ITypeSymbol[] {}).Concat(methodSymbol.ContainingType.TypeArguments).ToArray();

            var parent = context.Node.Parent;
            while (parent is not null && !object.ReferenceEquals(parent, parent.Parent) && parent is not TypeDeclarationSyntax) parent = parent.Parent;

            if (parent is not TypeDeclarationSyntax) return;
            if (context.SemanticModel.GetDeclaredSymbol(parent!, context.CancellationToken) is not INamedTypeSymbol classSymbol) return;

            var bases = classSymbol.GetAllBaseTypes().Concat(classSymbol.AllInterfaces).ToArray();

            foreach (var type in types.Where(t => bases.Any(b => b.IsEqualTo(t) && b.HasAttribute(baseSymbols)
                                                            && !b.GetAttribute(baseSymbols)!.AttributeClass!.Name.StartsWith(attrName))))
            {
                var diagnostic = Microsoft.CodeAnalysis.Diagnostic.Create(Diagnostic, attr!.GetLocation(), type.Name,
                                            type.GetAttribute(baseSymbols)!.AttributeClass!.Name.Replace("Base" + nameof(Attribute),""), attrName);

                context.ReportDiagnostic(diagnostic);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}
