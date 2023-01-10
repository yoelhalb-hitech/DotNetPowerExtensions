
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
    

    [System.Diagnostics.CodeAnalysis.SuppressMessage("MicrosoftCodeAnalysisCorrectness",
                "RS1024:Symbols should be compared for equality", Justification = "Comparing to null")]
    public static IEnumerable<ITypeSymbol> GetAllBaseTypes(this ITypeSymbol symbol)
    {
        for (var baseType = symbol.BaseType; baseType is not null; baseType = baseType.BaseType)
            yield return baseType;

        yield break;
    }

    public static AttributeData? GetAttribute(this ISymbol symbol, ITypeSymbol[] attributeSymbols)
        => symbol
            .GetAttributes()
            .FirstOrDefault(a => attributeSymbols.Any(s => s.IsEqualTo(a.AttributeClass?.ConstructedFrom)));

    public static bool HasAttribute(this ISymbol symbol, ITypeSymbol[] attributeSymbols)
        => symbol.GetAttribute(attributeSymbols) is not null;

    public static AttributeData? GetAttribute(this ISymbol symbol, ITypeSymbol attributeSymbols)
        => symbol.GetAttribute(new[] { attributeSymbols });

    public static bool HasAttribute(this ISymbol symbol, ITypeSymbol attributeSymbols)
        => symbol.HasAttribute(new[] { attributeSymbols });
}
