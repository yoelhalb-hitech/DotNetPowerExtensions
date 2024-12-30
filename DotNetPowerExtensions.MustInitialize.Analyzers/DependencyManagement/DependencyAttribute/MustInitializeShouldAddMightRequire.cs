using SequelPay.DotNetPowerExtensions;

namespace DotNetPowerExtensions.MustInitialize.Analyzers;

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
        // TODO... maybe use an IOperation instead...
        compilationContext.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.Attribute);
    }

    internal static Dictionary<ITypeSymbol, List<Union<IPropertySymbol, IFieldSymbol>>>?GetMightRequireCandidates(AttributeSyntax attributeSyntax,
                                                    ITypeSymbol[] types, SemanticModel semanticModel, MustInitializeWorker worker, CancellationToken c)
    {
        var typeSyntax = attributeSyntax.FirstAncestorOrSelf<TypeDeclarationSyntax>();
        if (typeSyntax is null) return null;

        if (semanticModel.GetDeclaredSymbol(typeSyntax!, c) is not ITypeSymbol typeSymbol) return null;

        return GetMightRequireCandidates(typeSymbol, types, worker, c);
    }


    internal static Dictionary<ITypeSymbol, List<Union<IPropertySymbol, IFieldSymbol>>>?
                GetMightRequireCandidates(ITypeSymbol typeSymbol, ITypeSymbol[] types, MustInitializeWorker worker, CancellationToken cancellationToken = default)
    {
        var mustInitializeDict = types.ToDictionary(t => t,
                                    t => worker.GetRequiredToInitialize(t, null, cancellationToken).ToDictionary(s => s.name, s => s.type),
                                    SymbolEqualityComparer.Default);

        var dict = new Dictionary<ITypeSymbol, List<Union<IPropertySymbol, IFieldSymbol>>>(SymbolEqualityComparer.Default);
        types.ToList().ForEach(t => dict[t] = new List<Union<IPropertySymbol, IFieldSymbol>>());

        foreach (var member in worker.GetClosestMembersWithAttribute(typeSymbol, worker.MustInitializeSymbols))
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

    private void AnalyzeClass(SyntaxNodeAnalysisContext context)
    {
        try
        {
            var worker = new MustInitializeWorker(context.Compilation, context.SemanticModel);
            // Since a class decleration can be partial we will only report it on the attribute
            var result = DependencyAnalyzerUtils.GetAttributeWithTypes(context,
                                                            DependencyAnalyzerUtils.DependencyAttributeNames,
                                                            worker.GetTypeSymbols(DependencyAnalyzerUtils.AllDependencies));
            if (result is null) return;
            var (attr, attrName, methodSymbol, types) = result.Value;

            var dict = GetMightRequireCandidates(attr, types, context.SemanticModel, worker, context.CancellationToken);
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
