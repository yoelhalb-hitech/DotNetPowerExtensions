﻿using DotNetPowerExtensions.Analyzers.MustInitialize.MightRequireAttribute;
using SequelPay.DotNetPowerExtensions;
using SequelPay.DotNetPowerExtensions.RoslynExtensions;
using System.Linq;

namespace DotNetPowerExtensions.Analyzers.MustInitialize;

internal class MustInitializeWorker : WorkerBase
{
    public MustInitializeWorker(SemanticModel semanticModel) : base(semanticModel)
    {
    }

    public MustInitializeWorker(Compilation compilation) : base(compilation)
    {
    }

    public MustInitializeWorker(Compilation compilation, SemanticModel semanticModel) : base(compilation, semanticModel)
    {
    }

    public static Type[] MustInitializeTypes =
    {
        typeof(SequelPay.DotNetPowerExtensions.MustInitializeAttribute),
    };
    public static Type InitializedType = typeof(SequelPay.DotNetPowerExtensions.InitializedAttribute);

    public INamedTypeSymbol[]? mustInitializeSymbols;
    public INamedTypeSymbol[] MustInitializeSymbols => mustInitializeSymbols ?? (mustInitializeSymbols = GetTypeSymbols(MustInitializeTypes));

    public INamedTypeSymbol[]? mightRequireSymbols;
    public INamedTypeSymbol[] MightRequireSymbols => mightRequireSymbols ?? (mightRequireSymbols = GetTypeSymbols(MightRequireUtils.Attributes));

    public INamedTypeSymbol? initializedSymbol;
    public INamedTypeSymbol InitializedSymbol => initializedSymbol ?? (initializedSymbol = GetTypeSymbol(InitializedType)!);

    public static string ShortName => nameof(SequelPay.DotNetPowerExtensions.MustInitializeAttribute).Replace(nameof(Attribute), "");
    public static AttributeListSyntax GetAttributeSyntax() => SyntaxFactoryExtensions.ParseAttribute("[" + ShortName + "]");

    public IEnumerable<Union<IPropertySymbol, IFieldSymbol>> GetClosestMembersWithAttribute(ITypeSymbol symbol, INamedTypeSymbol[] attributeSymbols)
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

    public IEnumerable<Union<IPropertySymbol, IFieldSymbol>> GetInitialized(IMethodSymbol method)
    {
        var chain = new[] { method }.Concat(method.GetConstructorChain(SemanticModel)).ToArray();

        var initializeAll = GetTypeSymbol(typeof(InitializesAllRequiredAttribute))!;
        var ctorsAll = chain.Where(c => c.HasAttribute(new[] { initializeAll })
                // Remember if the user doesn't have it in the framework he might implement it on his own so go by name
                || c.GetAttributes().Any(a => a.AttributeClass?.GetNamespace() == "System.Diagnostics.CodeAnalysis" && a.AttributeClass?.Name == "SetsRequiredMembersAttribute"));

        // Since InitalizesAll is based on the type we can for the type directly and avoid repeating it if there is a "this" chain
        var typesAll = ctorsAll.Select(c => c.ContainingType).Distinct(SymbolEqualityComparer.Default).OfType<INamedTypeSymbol>().ToArray();

        var membersAll = typesAll.SelectMany(t => GetClosestMembersWithAttribute(t, MustInitializeSymbols));

        var initializesSome = GetTypeSymbol(typeof(InitializesAttribute))!;

        var ctorsSome = chain
                .Where(c => c.HasAttribute(initializesSome))
                .SelectMany(c =>
                {
                    var names = c.GetAttribute(initializesSome)?.ConstructorArguments
                                    .SelectMany(a => a.Kind == TypedConstantKind.Array ? a.Values.ToArray() : new[] { a })
                                    .Select(a => a.Value)
                                    .OfType<string>()
                                    .ToArray();
                    return GetClosestMembersWithAttribute(c.ContainingType, MustInitializeSymbols).Where(m => names.Contains(m.As<ISymbol>()!.Name));
                });

        return membersAll.Concat(ctorsSome);
    }

    public Dictionary<string, Union<IPropertySymbol, IFieldSymbol>[]> GetInitialized(ITypeSymbol type)
    {
        if (type.TypeKind != TypeKind.Class && type.TypeKind != TypeKind.Struct) return new Dictionary<string, Union<IPropertySymbol, IFieldSymbol>[]>();

        var ctors = type.GetConstructors(false);

        var working = ToNamedDict(GetInitialized(ctors.First()));

        // If no ctor specified then we can only consider ot be intialized if all ctors are intiialiazing it in their chain
        foreach (var ctor in ctors.Skip(1)) //TODO... Maybe we can optimize if a ctor is in the others chain
        {
            var current = ToNamedDict(GetInitialized(ctor));
            var intersectKeys = current.Keys.Intersect(working.Keys).ToArray();

            working = working
                    .Where(w => intersectKeys.Contains(w.Key))
                    .ToDictionary(w => w.Key, w => w.Value
                                                        .Where(v => current[w.Key].Any(c => c.As<ISymbol>()!.HasSameBaseDecleration(v.As<ISymbol>()!)))
                                                        .ToArray());
        }

        return working;
    }

    private Dictionary<string, Union<IPropertySymbol, IFieldSymbol>[]> ToNamedDict(IEnumerable<Union<IPropertySymbol, IFieldSymbol>> unions)
        => unions.GroupBy(i => i.As<ISymbol>()!.Name).ToDictionary(g => g.Key, g => g.ToArray());

    public Dictionary<string, Union<IPropertySymbol, IFieldSymbol>[]> GetInitialized(ITypeSymbol type, IMethodSymbol? method)
        => method is null ? GetInitialized(type) : ToNamedDict(GetInitialized(method));

    public IEnumerable<Union<IPropertySymbol, IFieldSymbol>> FilteredMustInitialize(IEnumerable<Union<IPropertySymbol, IFieldSymbol>> mustInitializeMemebrs,
                                                                                        Dictionary<string, Union<IPropertySymbol, IFieldSymbol>[]> initialized)
    {
        var mustInitializeDict = ToNamedDict(mustInitializeMemebrs);
        foreach(var key in mustInitializeDict.Keys.Except(initialized.Keys))
        {
            foreach (var entry in mustInitializeDict[key]) yield return entry;
        }

        foreach (var key in mustInitializeDict.Keys.Intersect(initialized.Keys))
        {
            foreach (var entry in mustInitializeDict[key]
                    .Where(v => !initialized[key].Any(i => i.As<ISymbol>()!.HasSameBaseDecleration(v.As<ISymbol>()!))))
                yield return entry;
        }
    }

    private IEnumerable<Union<IPropertySymbol, IFieldSymbol>> GetMustInitialize(ITypeSymbol symbol, IMethodSymbol? method,
                        out Dictionary<string, Union<IPropertySymbol, IFieldSymbol>[]> initialized)
    {
        var closest = GetClosestMembersWithAttribute(symbol, MustInitializeSymbols);

        initialized = GetInitialized(symbol, method);

        return FilteredMustInitialize(closest, initialized);
    }

    // NOTE: This one is for initializing the class itself so we don't care on MightRequire
    public IEnumerable<string> GetNotInitializedNames(ObjectCreationExpressionSyntax typeDecl, ITypeSymbol symbol, IMethodSymbol? ctor)
    {
        var props = GetMustInitialize(symbol, ctor, out _).Select(m => m.As<ISymbol>()!.Name);

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

    // NOTE: This one is for initializing for DI which might be typed as the base so we care on MightRequire
    public IEnumerable<string> GetNotInitializedNames(AnonymousObjectCreationExpressionSyntax typeDecl, ITypeSymbol symbol)
    {
        var props = GetRequiredToInitialize(symbol, null).Select(m => m.name);

        var initialized = typeDecl.Initializers.Select(i => i.GetName()).Where(x => x is not null).Select(x => x!);

        return props.Except(initialized).Distinct();
    }

    public IEnumerable<(string name, ITypeSymbol type)> GetRequiredToInitialize(ITypeSymbol symbol, IMethodSymbol? ctor)
    {
        var props = GetMustInitialize(symbol, ctor, out var initialized);
        foreach (var prop in props)
        {
            yield return (prop.As<ISymbol>()!.Name, prop.First?.Type ?? prop.Second!.Type);
        }

        var initializedByMember = ToNamedDict(GetClosestMembersWithAttribute(symbol, new[] { InitializedSymbol }));

        foreach (var info in MightRequireUtils.GetMightRequiredInfos(symbol, MightRequireSymbols))
        {
            // Assuming that we can't have members with the same name and different types
            if (initialized.ContainsKey(info.Name) || initializedByMember.ContainsKey(info.Name)) continue;

            yield return (info.Name, info.Type);
        }
    }
}