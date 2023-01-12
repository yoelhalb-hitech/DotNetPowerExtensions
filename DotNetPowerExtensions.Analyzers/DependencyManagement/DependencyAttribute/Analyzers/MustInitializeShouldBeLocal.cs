
using DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;
using DotNetPowerExtensions;
using System.Diagnostics.CodeAnalysis;

namespace DotNetPowerExtensions.Analyzers.DependencyManagement.DependencyAttribute.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MustInitializeShouldBeLocal : MustInitializeRequiredMembersBase
{
    public const string DiagnosticId = "DNPE0205";
    protected const string Title = "UseLocalWhenMustInitialize";
    protected const string Message = "Use `Local` for a class that contains members with Mustinitialize";
    protected const string Description = Message + ".";
    protected override DiagnosticDescriptor DiagnosticDesc => Diagnostic;

    [SuppressMessage("Microsoft.Design", "CA1051: Do not declare visible instance fields", Justification = "The compiler only consideres fields when tracking analyzer releases")]
    protected DiagnosticDescriptor Diagnostic = new DiagnosticDescriptor(DiagnosticId, Title, Message, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

    public override void Register(CompilationStartAnalysisContext compilationContext, INamedTypeSymbol[] mustInitializeSymbols)
    {
        Func<Type, INamedTypeSymbol?> metadata = t => compilationContext.Compilation.GetTypeByMetadataName(t.FullName!);

        var allAttributeTypes = new[]
        {
            typeof(SingletonAttribute),
            typeof(SingletonAttribute<>),
            typeof(ScopedAttribute),
            typeof(ScopedAttribute<>),
            typeof(TransientAttribute),
            typeof(TransientAttribute<>),
        };
        var symbols = allAttributeTypes.Select(t => metadata(t)).Where(x => x is not null).Select(x => x!);

        // TODO... maybe use an IOperation instead...
        compilationContext.RegisterSyntaxNodeAction(c => AnalyzeClass(c, mustInitializeSymbols, symbols), SyntaxKind.Attribute);
    }

    private string[] DependencyAttributeNames =
    {
        nameof(SingletonAttribute), nameof(ScopedAttribute), nameof(TransientAttribute),
    };

    private void AnalyzeClass(SyntaxNodeAnalysisContext context, INamedTypeSymbol[] mustInitializeSymbols, IEnumerable<INamedTypeSymbol> attributeSymbols)
    {
        try
        {
            // Since a class decleration can be partial we will only report it on the attribute
            var attr = context.Node as AttributeSyntax;
            var attrName = attr?.Name.GetUnqualifiedName()?.Replace(nameof(Attribute), "");
            if (attrName is null || !DependencyAttributeNames.Contains(attrName + nameof(Attribute))) return;

            if (context.SemanticModel.GetSymbolInfo(attr!, context.CancellationToken).Symbol is not IMethodSymbol methodSymbol
                || !attributeSymbols.ContainsGeneric(methodSymbol.ContainingType)) return;

            var parent = context.Node.Parent;
            while (parent is not null && !object.ReferenceEquals(parent, parent.Parent) && parent is not ClassDeclarationSyntax) parent = parent.Parent;

            if(parent is not ClassDeclarationSyntax) return;

            if (context.SemanticModel.GetDeclaredSymbol(parent!, context.CancellationToken) is not INamedTypeSymbol classSymbol) return;

            if(classSymbol.GetMembers().Any(m => m.HasAttribute(mustInitializeSymbols)))
            {
                var diagnostic = Microsoft.CodeAnalysis.Diagnostic.Create(DiagnosticDesc, attr!.GetLocation());

                context.ReportDiagnostic(diagnostic);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}
