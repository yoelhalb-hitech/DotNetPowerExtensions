using DotNetPowerExtensions.Analyzers.MustInitialize;
using DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;
using DotNetPowerExtensions.Analyzers.MustInitialize.MightRequireAttribute;

namespace DotNetPowerExtensions.Analyzers.DependencyManagement.DependencyAttribute.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MustInitializeShouldAddMightRequire : MustInitializeRequiredMembersBase
{
    public const string DiagnosticId = "DNPE0215";
    protected const string Title = "MustInitializeShouldAddMightRequire";
    protected const string Message = "Add MightRequire on `{0}` for `{1}`";
    protected const string Description = Message + ".";
    protected override DiagnosticDescriptor DiagnosticDesc => Diagnostic;

    [SuppressMessage("Microsoft.Design", "CA1051: Do not declare visible instance fields", Justification = "The compiler only consideres fields when tracking analyzer releases")]
    protected DiagnosticDescriptor Diagnostic = new DiagnosticDescriptor(DiagnosticId, Title, Message, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

    public override void Register(CompilationStartAnalysisContext compilationContext, INamedTypeSymbol[] mustInitializeSymbols)
    {
        Func<Type, INamedTypeSymbol?> metadata = t => compilationContext.Compilation.GetTypeByMetadataName(t.FullName!);

        // TODO... maybe we only need it on the local (since for most initilize we anyway need local)
        var symbols = DependencyAnalyzerUtils.AllDependencies.Select(t => metadata(t)).Where(x => x is not null).Select(x => x!).ToArray();
        var mightRequireSymbols = MightRequireUtils.Attributes.Select(a => metadata(a)).OfType<INamedTypeSymbol>().ToArray();
        var initializedSymbol = metadata(typeof(InitializedAttribute));

        // TODO... maybe use an IOperation instead...
        compilationContext.RegisterSyntaxNodeAction(c => AnalyzeClass(c, mustInitializeSymbols, symbols, mightRequireSymbols, initializedSymbol), SyntaxKind.Attribute);
    }

    public static Dictionary<ITypeSymbol, List<Union<IPropertySymbol, IFieldSymbol>>>?
        GetMightRequireCandidates(AttributeSyntax attributeSyntax, ITypeSymbol[] types, SemanticModel semanticModel,
                    INamedTypeSymbol[] mustInitializeSymbols, INamedTypeSymbol[] mightRequireSymbols, INamedTypeSymbol? initializedSymbol, CancellationToken c)
    {
        var typeSyntax = attributeSyntax.FirstAncestorOrSelf<TypeDeclarationSyntax>();
        if (typeSyntax is null) return null;

        if (semanticModel.GetDeclaredSymbol(typeSyntax!, c) is not ITypeSymbol typeSymbol) return null;

        return GetMightRequireCandidates(typeSymbol, types, mustInitializeSymbols, mightRequireSymbols, initializedSymbol);
    }


    public static Dictionary<ITypeSymbol, List<Union<IPropertySymbol, IFieldSymbol>>>?
        GetMightRequireCandidates(ITypeSymbol typeSymbol, ITypeSymbol[] types,
                                        INamedTypeSymbol[] mustInitializeSymbols, INamedTypeSymbol[] mightRequireSymbols, INamedTypeSymbol? initializedSymbol)
    {
        var mustInitializeDict = types.ToDictionary(t => t,
                                    t => MustInitializeUtils.GetRequiredToInitialize(t, mustInitializeSymbols, mightRequireSymbols, initializedSymbol)
                                                                                                                .ToDictionary(s => s.name, s => s.type),
                                    SymbolEqualityComparer.Default);

        var dict = new Dictionary<ITypeSymbol, List<Union<IPropertySymbol, IFieldSymbol>>>(SymbolEqualityComparer.Default);
        types.ToList().ForEach(t => dict[t] = new List<Union<IPropertySymbol, IFieldSymbol>>());

        foreach (var member in MustInitializeUtils.GetClosestMembersWithAttribute(typeSymbol, mustInitializeSymbols))
        {
            foreach (var type in types)
            {
                var name = member.As<ISymbol>()!.Name;
                if (mustInitializeDict[type].ContainsKey(name)) continue;

                dict[type].Add(member);
            }
        }

        return dict;
    }

    private void AnalyzeClass(SyntaxNodeAnalysisContext context, INamedTypeSymbol[] mustInitializeSymbols,
                                            INamedTypeSymbol[] attributeSymbols, INamedTypeSymbol[] mightRequireSymbols, INamedTypeSymbol? initializedSymbol)
    {
        try
        {
            // Since a class decleration can be partial we will only report it on the attribute
            var result = DependencyAnalyzerUtils.GetAttributeWithTypes(context,
                                                            DependencyAnalyzerUtils.DependencyAttributeNames, attributeSymbols);
            if (result is null) return;
            var (attr, attrName, methodSymbol, types) = result.Value;

            var dict = GetMightRequireCandidates(attr, types, context.SemanticModel, mustInitializeSymbols,
                                                                mightRequireSymbols, initializedSymbol, context.CancellationToken);
            if(dict is null) return;

            foreach (var type in dict.Keys.Where(k => dict[k].Any())) // Doing for each type so that the code fix should be able to fix each one separately
            {
                var names = string.Join(",", dict[type].Select(e => e.As<ISymbol>()!.Name));

                var props = new Dictionary<string, string?>()
                {
                    ["Namespace"]= type.GetContainerFullName(),
                    ["Name"] = type.Name,
                };

                var diagnostic = Microsoft.CodeAnalysis.Diagnostic.Create(DiagnosticDesc, attr!.GetLocation(), props.ToImmutableDictionary(), type.Name, names);


                context.ReportDiagnostic(diagnostic);

            }


        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}
