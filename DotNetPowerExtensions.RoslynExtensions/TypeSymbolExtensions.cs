using Microsoft.CodeAnalysis.Shared.Extensions;
using System.Linq;

namespace SequelPay.DotNetPowerExtensions.RoslynExtensions;

public static class TypeSymbolExtensions
{
    public static bool IsGenericEqualOrSubOf<T>(this T symbol, T baseType, bool includeInterfaces) where T : ITypeSymbol
    {
        if (symbol is null) throw new ArgumentNullException(nameof(symbol));
        if (baseType is null) throw new ArgumentNullException(nameof(baseType));

        // If the symbol is a constructed generic and the base is not a generic then the constructed is considered a child but not the open generic
        return symbol.IsGenericEqual(baseType) || symbol.InheritsFromGeneric(baseType, includeInterfaces);
    }


    public static bool IsGenericEqualOrBaseOf<T>(this T? symbol, T? subType, bool includeInterfaces) where T : ITypeSymbol
    {
        if (symbol is null) throw new ArgumentNullException(nameof(symbol));
        if (subType is null) throw new ArgumentNullException(nameof(subType));

        return subType.IsGenericEqualOrSubOf(symbol, includeInterfaces);
    }

    public static bool ContainsSymbolOrSub<T>(this IEnumerable<T> symbols, T? baseType, bool includeInterfaces) where T : ITypeSymbol
        => baseType is not null && symbols.Any(s => s.InheritsFromOrEquals(baseType, includeInterfaces));

    public static bool ContainsSymbolOrBase<T>(this IEnumerable<T> symbols, T? subType, bool includeInterfaces) where T : ITypeSymbol
        => subType is not null && symbols.Any(s => subType.InheritsFromOrEquals(s, includeInterfaces));

    public static bool ContainsGenericOrSub<T>(this IEnumerable<T?> symbols, T? subType, bool includeInterfaces) where T : ITypeSymbol
        => subType is not null && symbols.Any(s => s?.IsGenericEqualOrBaseOf(subType, includeInterfaces) ?? false); // Remember that when other is base this is sub

    public static bool ContainsGenericOrBase<T>(this IEnumerable<T?> symbols, T? baseType, bool includeInterfaces) where T : ITypeSymbol
        => baseType is not null && symbols.Any(s => s?.IsGenericEqualOrSubOf(baseType, includeInterfaces) ?? false); // Remember that when other is sub this is base

    public static string ToStringWithoutNamesapce(this ITypeSymbol symbol)
    {
        string str = symbol.ToString()!; // This will handle correctly keywords such as string and generics and tuples

        GetNamespaces(symbol).Distinct().ToList().ForEach(ns => str = str.Replace(ns + ".", "")); // But it also has namespaces whcih we have to remove

        return str;

        static IEnumerable<string> GetNamespaces(ITypeSymbol type)
        {
            if(type.ContainingNamespace is not null) yield return type.ContainingNamespace!.ToString()!;

            if (type is INamedTypeSymbol named && named.IsGenericType)
                foreach (var t in named.TypeArguments)
                    foreach (var @namespace in GetNamespaces(t)) yield return @namespace!;
        }
    }

    public static TypeSyntax ToTypeSyntax(this ITypeSymbol type)
    {
        if (type.SpecialType != SpecialType.None)
            return SyntaxFactory.PredefinedType(SyntaxFactory.ParseToken(type.ToString()!)); // We need to do it this way as the Test framwork expects a predefined type in this case

        var str = type.ToStringWithoutNamesapce(); // This will handle correctly keywords such as string and generics and tuples

        return SyntaxFactory.ParseName(str); // ParseName will handle correctly generic names
    }

    // TODO... so far this doesn't work on inner methods...
    public static string GetFullName(this ITypeSymbol typeSymbol)
    {
        string name = typeSymbol.MetadataName.ToString();

        for (var classDecl = typeSymbol.ContainingType; classDecl is not null; classDecl = classDecl.ContainingType)
            name = $"{classDecl.MetadataName}+{name}";

        var ns = typeSymbol.GetNamespace();
        return ns + (!string.IsNullOrWhiteSpace(ns) ? "." : "") + name;
    }

    public static bool InheritsFromOrEquals(this ITypeSymbol typeSymbol, ITypeSymbol baseType, bool includeInterfaces)
        => typeSymbol.IsEqualTo(baseType) || typeSymbol.InheritsFrom(baseType, includeInterfaces);

    public static bool InheritsFrom(this ITypeSymbol typeSymbol, ITypeSymbol baseType, bool includeInterfaces)
        => typeSymbol.GetAllBaseTypes().Concat(includeInterfaces ? typeSymbol.AllInterfaces : new ITypeSymbol[0]).Any(t => t.IsEqualTo(baseType));

    public static bool InheritsFromGeneric(this ITypeSymbol typeSymbol, ITypeSymbol baseType, bool includeInterfaces)
        => typeSymbol.GetAllBaseTypes().Concat(includeInterfaces ? typeSymbol.AllInterfaces : new ITypeSymbol[0]).Any(t => t.IsGenericEqual(baseType));

    public static IEnumerable<ITypeSymbol> GetAllBaseTypes(this ITypeSymbol typeSymbol)
    {
        for (var baseType = typeSymbol.BaseType; baseType is not null; baseType = baseType.BaseType)
            yield return baseType;

        yield break;
    }

    public static IEnumerable<ITypeSymbol> GetAllBaseTypesAndThis(this ITypeSymbol typeSymbol)
    {
        yield return typeSymbol;

        foreach (var t in typeSymbol.GetAllBaseTypes()) yield return t;
    }

    public static IEnumerable<IMethodSymbol> GetConstructors(this ITypeSymbol typeSymbol, bool isStatic)
        => typeSymbol.GetMembers(".ctor").OfType<IMethodSymbol>().Where(m => m.IsStatic == isStatic);

    public static IMethodSymbol? GetDefaultConstructor(this ITypeSymbol typeSymbol)
        => typeSymbol.GetConstructors(false).FirstOrDefault(m => m.Parameters.Length == 0);

    /// <summary>
    /// Return all fields, including inherited/shadowed ones (unless it is base private)
    /// </summary>
    /// <param name="typeSymbol">The <see cref="ITypeSymbol"/> containing/inheriting the fields</param>
    /// <param name="includeShadowed">If shadowed fields should be included in the result</param>
    public static IEnumerable<IFieldSymbol> GetAllFields(this ITypeSymbol typeSymbol, bool includeShadowed = false)
    {
        var currentFields = typeSymbol.GetMembers().OfType<IFieldSymbol>().Where(f => f.CanBeReferencedByName).ToArray();

        if (typeSymbol.BaseType is null) return currentFields;

        var baseFields = typeSymbol.BaseType.GetAllFields(includeShadowed)
                                .Where(f => f.DeclaredAccessibility != Accessibility.Private);

        if (includeShadowed) return baseFields.Concat(currentFields);

        var currentNames = currentFields.Select(p => p.Name).ToList();

        // TODO... We need to check what is going on with explicit implementations
        return baseFields.Where(p => !currentNames.Contains(p.Name)).Concat(currentFields);
    }

    /// <summary>
    /// Return all properties, including inherited (unless it is base private), but not overriden properties
    /// </summary>
    /// <param name="typeSymbol">The <see cref="ITypeSymbol"/> containing/inheriting the fields</param>
    /// <param name="includeShadowed">If shadowed properties should be included in the result</param>
    /// <remarks>Does not include interfaces currently, not even default implemented, so be carefull</remarks>
    public static IEnumerable<IPropertySymbol> GetAllProperties(this ITypeSymbol typeSymbol, bool includeShadowed = false)
    {
        var currentPropeties = typeSymbol.GetMembers().OfType<IPropertySymbol>().ToArray();

        if (typeSymbol.BaseType is null) return currentPropeties;

        var baseProperties = typeSymbol.BaseType.GetAllProperties(includeShadowed)
                                .Where(p => p.DeclaredAccessibility != Accessibility.Private);

        if(includeShadowed)
        {
            var overridenProperties = currentPropeties.Where(p => p.IsOverride).Select(p => p.OverriddenProperty).OfType<IPropertySymbol>();

            return baseProperties.Except(overridenProperties).Concat(currentPropeties);
        }

        var currentNames = currentPropeties.Select(p => p.Name).ToList();

        // TODO... We need to check what is going on with explicit implementations
        return baseProperties.Where(p => !currentNames.Contains(p.Name)).Concat(currentPropeties);
    }
}
