using DotNetPowerExtensions.Analyzers.MustInitialize.MightRequireAttribute;
using SequelPay.DotNetPowerExtensions;
using SequelPay.DotNetPowerExtensions.RoslynExtensions;
using System.Linq;

namespace DotNetPowerExtensions.Analyzers.MustInitialize;

internal class MustInitializeUtils
{
    public static string ShortName => nameof(SequelPay.DotNetPowerExtensions.MustInitializeAttribute).Replace(nameof(Attribute), "");
    public static AttributeListSyntax GetAttributeSyntax() => SyntaxFactoryExtensions.ParseAttribute("[" + MustInitializeUtils.ShortName + "]");

    public static IEnumerable<Union<IPropertySymbol, IFieldSymbol>> GetClosestMembersWithAttribute(ITypeSymbol symbol, INamedTypeSymbol[] attributeSymbols)
    {
        // We don't need the interfaces, since we require to specify it directly on the implementation, and c# 8 default interfaces are not allowed
        var bases = symbol.GetAllBaseTypes().ToList();
        var symbols = new[] { symbol }.Concat(bases);
        var allMembers = symbols.SelectMany(s => s.GetMembers()
                                                    .OfType<IPropertySymbol>()
                                                    .Select(p => new Union<IPropertySymbol, IFieldSymbol>(p))
                                                    .Concat(
                                                            s.GetMembers()
                                                            .OfType<IFieldSymbol>()
                                                            .Select(p => new Union<IPropertySymbol, IFieldSymbol>(p))));

        var byNames = allMembers.GroupBy(r => r.As<ISymbol>()!.Name);

        // We take the closest base, this way if it has been marked with `Initialized` instead we will be fine
        // Remember that each override must be marked and hiding is not allowed (unless `Initialized`)
        return byNames
                .Select(n => n.OrderBy(x => bases.IndexOf(x.As<ISymbol>()!.ContainingType)).First())
                .Where(n => n.As<ISymbol>()!.HasAttribute(attributeSymbols));
    }

    public static IEnumerable<string> GetNotInitializedNames(ObjectCreationExpressionSyntax typeDecl, ITypeSymbol symbol, INamedTypeSymbol[] mustInitializeSymbols)
    {
        var props = GetClosestMembersWithAttribute(symbol, mustInitializeSymbols).Select(m => m.As<ISymbol>()!.Name);

        if (typeDecl.Initializer is not null)
        {
            var childs = typeDecl.Initializer.ChildNodes();
            var propsInitialized = childs.OfType<IdentifierNameSyntax>()
                    .Union(childs.OfType<AssignmentExpressionSyntax>().Select(c => c.Left).OfType<IdentifierNameSyntax>())
                .Select(c => c.Identifier.Text);

            props = props.Except(propsInitialized);
        }

        return props;
    }

    public static IEnumerable<string> GetNotInitializedNames(AnonymousObjectCreationExpressionSyntax typeDecl, ITypeSymbol symbol,
                                        INamedTypeSymbol[] mustInitializeSymbols, INamedTypeSymbol[] mightRequireSymbols, INamedTypeSymbol? initializedSymbol)
    {
        var props = GetRequiredToInitialize(symbol, mustInitializeSymbols, mightRequireSymbols, initializedSymbol).Select(m => m.name);

        var initialized = typeDecl.Initializers.Select(i => i.GetName()).Where(x => x is not null).Select(x => x!);

        return props.Except(initialized).Distinct();
    }

    public static IEnumerable<(string name, ITypeSymbol type)> GetRequiredToInitialize(ITypeSymbol symbol, INamedTypeSymbol[] mustInitializeSymbols,
                                                                        INamedTypeSymbol[] mightRequireSymbols, INamedTypeSymbol? initializedSymbol)
    {
        var props = GetClosestMembersWithAttribute(symbol, mustInitializeSymbols);
        foreach (var prop in props)
        {
            yield return (prop.As<ISymbol>()!.Name, prop.First?.Type ?? prop.Second!.Type);
        }

        var initialized = initializedSymbol is null ? null :
                                    GetClosestMembersWithAttribute(symbol, new[] { initializedSymbol })
                                        .ToDictionary(m => m.As<ISymbol>()!.Name, m => m.First?.ContainingType ?? m.Second!.ContainingType);

        var baseTypes = symbol.GetAllBaseTypes().Concat(symbol.AllInterfaces).ToArray();
        var baseDict = baseTypes.ToDictionary(t => t, t => (ITypeSymbol[]?)null, SymbolEqualityComparer.Default);
        baseDict.Add(symbol, baseTypes);

        foreach (var info in MightRequireUtils.GetMightRequiredInfos(symbol, mightRequireSymbols))
        {
            if(initialized?.TryGetValue(info.Name, out var init) == true)
            {
                if (baseDict[init] is null) baseDict[init] = init.GetAllBaseTypes().Concat(init.AllInterfaces).ToArray();

                // We only ignore initialized properties if they are declared in a sub of the MightRequire
                // Note that if the current type has both initilized and MightRequire we assume that maybe he used to MightRequire because a subclass will override it and require to initilaize it
                if (baseDict[init]!.ContainsSymbol(info.ContainingSymbol)) continue;
            }

             yield return (info.Name, info.Type);
        }
    }
}
