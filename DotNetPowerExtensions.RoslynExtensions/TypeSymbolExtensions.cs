using Microsoft.CodeAnalysis.Shared.Extensions;

namespace SequelPay.DotNetPowerExtensions.RoslynExtensions;

public static class TypeSymbolExtensions
{
    public static bool IsGenericEqualOrSubOf<T>(this T symbol, T baseType) where T : ITypeSymbol
    {
        if (symbol is null) throw new ArgumentNullException(nameof(symbol));
        if (baseType is null) throw new ArgumentNullException(nameof(baseType));

        // If the symbol is a constructed generic and the base is not a generic then the constructed is considered a child but not the open generic
        return symbol.IsGenericEqual(baseType) || symbol.InheritsFromOrEquals(baseType, true);
    }


    public static bool IsGenericEqualOrBaseOf<T>(this T? symbol, T? subType) where T : ITypeSymbol
    {
        if (symbol is null) throw new ArgumentNullException(nameof(symbol));
        if (subType is null) throw new ArgumentNullException(nameof(subType));

        return subType.IsGenericEqualOrSubOf(symbol);
    }

    public static bool ContainsSymbolOrSub<T>(this IEnumerable<T> symbols, T? baseType) where T : ITypeSymbol
    => baseType is not null && symbols.Any(s => s.InheritsFromOrEquals(baseType));

    public static bool ContainsSymbolOrBase<T>(this IEnumerable<T> symbols, T? subType) where T : ITypeSymbol
        => subType is not null && symbols.Any(s => subType.InheritsFromOrEquals(s));

    public static bool ContainsGenericOrSub<T>(this IEnumerable<T?> symbols, T? subType) where T : ITypeSymbol
        => subType is not null && symbols.Any(s => s?.IsGenericEqualOrBaseOf(subType) ?? false); // Remember that when other is base this is sub

    public static bool ContainsGenericOrBase<T>(this IEnumerable<T?> symbols, T? baseType) where T : ITypeSymbol
        => baseType is not null && symbols.Any(s => s?.IsGenericEqualOrSubOf(baseType) ?? false); // Remember that when other is sub this is base

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
        return ns + (ns.HasValue() ? "." : "") + name;
    }

    public static IEnumerable<ITypeSymbol> GetAllBaseTypes(this ITypeSymbol symbol)
    {
        for (var baseType = symbol.BaseType; baseType is not null; baseType = baseType.BaseType)
            yield return baseType;

        yield break;
    }

    public static IEnumerable<IMethodSymbol> GetConstructors(this ITypeSymbol typeSymbol, bool isStatic)
        => typeSymbol.GetMembers(".ctor").OfType<IMethodSymbol>().Where(m => m.IsStatic == isStatic);

    public static IMethodSymbol? GetDefaultConstructor(this ITypeSymbol typeSymbol)
        => typeSymbol.GetConstructors(false).FirstOrDefault(m => m.Parameters.Length == 0);
}
