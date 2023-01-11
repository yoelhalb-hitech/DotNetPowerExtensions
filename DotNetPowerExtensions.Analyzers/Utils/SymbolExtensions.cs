﻿
namespace DotNetPowerExtensions.Analyzers.Utils;

internal static class SymbolExtensions
{
    public static string GetNamespace(this ISymbol symbol)
    {
        string nameSpace = string.Empty;

        var namespaceParent = symbol.ContainingNamespace;
        if (namespaceParent is null) return nameSpace;

        nameSpace = namespaceParent.Name.ToString();

        for (; namespaceParent.ContainingNamespace is not null; namespaceParent = namespaceParent.ContainingNamespace)
            nameSpace = $"{namespaceParent.Name}.{nameSpace}";

        return nameSpace;
    }

    public static string ToStringWithoutNamesapce(this ITypeSymbol symbol)
    {
        string str = symbol.ToString()!; // This will handle correctly keywords such as string and generics and tuples

        GetNamespaces(symbol).Distinct().ToList().ForEach(ns => str = str.Replace(ns + ".", "")); // But it also has namespaces whcih we have to remove
        
        return str;

        IEnumerable<string> GetNamespaces(ITypeSymbol type)
        {
            if(type.ContainingNamespace is not null) yield return type.ContainingNamespace.ToString();

            if (type is INamedTypeSymbol named && named.IsGenericType)
                foreach (var t in named.TypeArguments)
                    foreach (var @namespace in GetNamespaces(t)) yield return @namespace;
        }
    }

    // TODO... so far this doesn't work on inner methods...
    public static string GetContainerFullName(this ISymbol symbol)
    {
        string name = string.Empty;

        var classDecl = symbol.ContainingType;
        if (classDecl is null) return name;

        name = classDecl.Name.ToString();

        for (; classDecl.ContainingType is not null; classDecl = classDecl.ContainingType)
            name = $"{classDecl.Name}+{name}";

        return symbol.GetNamespace() + "." + name;
    }

    public static bool IsEqualTo<T>(this T? symbol, T? other) where T : ISymbol =>
#if NETSTANDARD2_0_OR_GREATER
        SymbolEqualityComparer.Default.Equals(symbol, other);
#else
        symbol is not null && other is not null && EqualityComparer<T>.Default.Equals(symbol, other);
#endif

    public static bool IsGenericEqual<T>(this T? symbol, T? other) where T : INamedTypeSymbol
        => symbol is not null && other?.IsGenericType == symbol!.IsGenericType 
            && (!symbol!.IsGenericType ? symbol.IsEqualTo(other) : symbol.ConstructUnboundGenericType().IsEqualTo(other.ConstructUnboundGenericType()));

    public static bool ContainsSymbol<T>(this IEnumerable<T> symbols, T? other) where T : ISymbol
        => other is not null && symbols.Any(s => s.IsEqualTo(other));

    public static bool ContainsGeneric<T>(this IEnumerable<T?> symbols, T? other) where T : INamedTypeSymbol
        => other is not null && symbols.Any(s => s?.IsGenericEqual(other) ?? false);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("MicrosoftCodeAnalysisCorrectness",
                "RS1024:Symbols should be compared for equality", Justification = "Comparing to null")]
    public static IEnumerable<ITypeSymbol> GetAllBaseTypes(this ITypeSymbol symbol)
    {
        for (var baseType = symbol.BaseType; baseType is not null; baseType = baseType.BaseType)
            yield return baseType;

        yield break;
    }

    public static AttributeData? GetAttribute(this ISymbol symbol, INamedTypeSymbol[] attributeSymbols)
        => symbol
            .GetAttributes()
            .FirstOrDefault(a => attributeSymbols.ContainsGeneric(a.AttributeClass?.ConstructedFrom));

    public static bool HasAttribute(this ISymbol symbol, INamedTypeSymbol[] attributeSymbols)
        => symbol.GetAttribute(attributeSymbols) is not null;

    public static AttributeData? GetAttribute(this ISymbol symbol, INamedTypeSymbol attributeSymbol)
        => symbol.GetAttribute(new[] { attributeSymbol });

    public static bool HasAttribute(this ISymbol symbol, INamedTypeSymbol? attributeSymbol)
        => attributeSymbol is null ? false : symbol.HasAttribute(new[] { attributeSymbol });
}
