using DotNetPowerExtensions.Analyzers.MustInitialize;
using DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;
using DotNetPowerExtensions.Analyzers.MustInitialize.MightRequireAttribute;

namespace DotNetPowerExtensions.Analyzers.DependencyManagement.DependencyAttribute.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MustInitializeConflictsWithMightRequire : MustInitializeRequiredMembersBase
{
    public const string DiagnosticId = "DNPE0216";
    protected const string Title = "MustInitializeConflictsWithMightRequire";
    protected const string Message = "The type of member `{0}` with `MustInitialize` conflicts with `MightRequire` on `{1}`";
    protected const string Description = Message + ".";
    protected override DiagnosticDescriptor DiagnosticDesc => Diagnostic;

    [SuppressMessage("Microsoft.Design", "CA1051: Do not declare visible instance fields", Justification = "The compiler only consideres fields when tracking analyzer releases")]
    protected DiagnosticDescriptor Diagnostic = new DiagnosticDescriptor(DiagnosticId, Title, Message, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

    public override void Register(CompilationStartAnalysisContext compilationContext, INamedTypeSymbol[] mustInitializeSymbols)
    {
        Func<Type, INamedTypeSymbol?> metadata = t => compilationContext.Compilation.GetTypeByMetadataName(t.FullName!);

        var symbols = DependencyAnalyzerUtils.AllDependencies.Select(t => metadata(t)).Where(x => x is not null).Select(x => x!).ToArray();
        var mightRequireSymbols = MightRequireUtils.Attributes.Select(a => metadata(a)).OfType<INamedTypeSymbol>().ToArray();

        // TODO... maybe use an IOperation instead...
        compilationContext.RegisterSyntaxNodeAction(c => AnalyzeClass(c, mustInitializeSymbols, symbols, mightRequireSymbols), SyntaxKind.Attribute);
    }

    private void AnalyzeClass(SyntaxNodeAnalysisContext context, INamedTypeSymbol[] mustInitializeSymbols,
                                                                INamedTypeSymbol[] attributeSymbols, INamedTypeSymbol[] mightRequireSymbols)
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

            var baseDict = types.ToDictionary(t => t, t => MightRequireUtils.GetMightRequiredInfos(t, mightRequireSymbols), SymbolEqualityComparer.Default);

            foreach (var member in MustInitializeUtils.GetClosestMembersWithAttribute(classSymbol, mustInitializeSymbols))
            {
                foreach (var type in types)
                {
                    if (baseDict[type].All(b => b.Name != member.As<ISymbol>()!.Name || b.Type.IsEqualTo(member.First?.Type ?? member.Second!.Type))) continue;

                    var diagnostic = Microsoft.CodeAnalysis.Diagnostic.Create(DiagnosticDesc, attr!.GetLocation(), member.As<ISymbol>()!.Name, type.Name);

                    context.ReportDiagnostic(diagnostic);
                }

            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}
