using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.ComponentModel;
using System.Security.AccessControl;

namespace SequelPay.DotNetPowerExtensions.RoslynExtensions;

public static class SymbolExtensions
{
    public static string GetNamespace(this ISymbol symbol)
    {
        string nameSpace = string.Empty;

        var namespaceParent = symbol.ContainingNamespace;
        if (namespaceParent is null) return nameSpace;

        nameSpace = namespaceParent.Name.ToString();

        for (namespaceParent = namespaceParent.ContainingNamespace; namespaceParent?.IsGlobalNamespace == false; namespaceParent = namespaceParent.ContainingNamespace)
            nameSpace = $"{namespaceParent.Name}.{nameSpace}";

        return nameSpace;
    }

    // TODO... so far this doesn't work on inner methods...
    public static string GetContainerFullName(this ISymbol symbol)
    {
        var classDecl = symbol.ContainingType;
        if (classDecl is null) return string.Empty;

        return classDecl.GetFullName();
    }

    public static bool IsEqualTo<T>(this T? symbol, T? other) where T : ISymbol =>
#if NETSTANDARD2_0_OR_GREATER
        SymbolEqualityComparer.Default.Equals(symbol, other);
#else
        symbol is not null && other is not null && EqualityComparer<T>.Default.Equals(symbol, other);
#endif

    public static bool IsGenericEqual<T>(this T? symbol, T? other) where T : INamedTypeSymbol
        => symbol is not null && other is not null && other?.IsGenericType == symbol!.IsGenericType
                && (!symbol!.IsGenericType ? symbol.IsEqualTo(other) : symbol.ConstructUnboundGenericType().IsEqualTo(other.ConstructUnboundGenericType()));

    public static bool IsGenericEqualOrSubOf<T>(this T symbol, T baseType) where T : INamedTypeSymbol
    {
        if (symbol is null) throw new ArgumentNullException(nameof(symbol));
        if (baseType is null) throw new ArgumentNullException(nameof(baseType));

        // If the symbol is a constructed generic and the base is not a generic then the constructed is considered a child but not the open generic
        return symbol.IsGenericEqual(baseType) || symbol.InheritsFromOrEquals(baseType, true);
    }


    public static bool IsGenericEqualOrBaseOf<T>(this T? symbol, T? subType) where T : INamedTypeSymbol
    {
        if (symbol is null) throw new ArgumentNullException(nameof(symbol));
        if (subType is null) throw new ArgumentNullException(nameof(subType));

        return subType.IsGenericEqualOrSubOf(symbol);
    }



    public static bool ContainsSymbol<T>(this IEnumerable<T> symbols, T? other) where T : ISymbol
        => other is not null && symbols.Any(s => s.IsEqualTo(other));

    public static bool ContainsSymbolOrSub<T>(this IEnumerable<T> symbols, T? baseType) where T : ITypeSymbol
        => baseType is not null && symbols.Any(s => s.InheritsFromOrEquals(baseType));

    public static bool ContainsSymbolOrBase<T>(this IEnumerable<T> symbols, T? subType) where T : ITypeSymbol
        => subType is not null && symbols.Any(s => subType.InheritsFromOrEquals(s));

    public static bool ContainsGeneric<T>(this IEnumerable<T?> symbols, T? other) where T : INamedTypeSymbol
        => other is not null && symbols.Any(s => s?.IsGenericEqual(other) ?? false);

    public static bool ContainsGenericOrSub<T>(this IEnumerable<T?> symbols, T? subType) where T : INamedTypeSymbol
        => subType is not null && symbols.Any(s => s?.IsGenericEqualOrBaseOf(subType) ?? false); // Remember that when other is base this is sub

    public static bool ContainsGenericOrBase<T>(this IEnumerable<T?> symbols, T? baseType) where T : INamedTypeSymbol
        => baseType is not null && symbols.Any(s => s?.IsGenericEqualOrSubOf(baseType) ?? false); // Remember that when other is sub this is base

    public static AttributeData? GetAttribute(this ISymbol symbol, INamedTypeSymbol[] attributeSymbols)
        => symbol
            .GetAttributes()
            .FirstOrDefault(a => attributeSymbols.ContainsGenericOrSub(a.AttributeClass?.OriginalDefinition));

    public static bool HasAttribute(this ISymbol symbol, INamedTypeSymbol[] attributeSymbols)
        => symbol.GetAttribute(attributeSymbols) is not null;

    public static AttributeData? GetAttribute(this ISymbol symbol, INamedTypeSymbol attributeSymbol)
        => symbol.GetAttribute(new[] { attributeSymbol });

    public static bool HasAttribute(this ISymbol symbol, INamedTypeSymbol? attributeSymbol)
        => attributeSymbol is not null && symbol.HasAttribute(new[] { attributeSymbol });

    public static IEnumerable<TSyntax>? GetSyntax<TSyntax>(this ISymbol symbol, CancellationToken token = default) where TSyntax : SyntaxNode
        => symbol.DeclaringSyntaxReferences.Select(r => r.GetSyntax(token)).OfType<TSyntax>();

    /// <summary>
    /// Returns the entire property override chain for a property
    /// </summary>
    /// <param name="property">The property symbol for which we want to get the chain</param>
    /// <returns>A list of property symbols with the property closer to the passed property being returned first</returns>
    public static IEnumerable<IPropertySymbol> GetPropertyOverrideChain(this IPropertySymbol property)
    {
        var prop = property;
        while(prop.IsOverride && prop.OverriddenProperty is not null)
        {
            prop = prop.OverriddenProperty;
            yield return prop;
        }

        yield break;
    }

    /// <summary>
    /// Get the original property base decleration for a property (the one that does not have the <see langword="override" /> keyword)
    /// </summary>
    /// <param name="property">The property symbol for which we want to get the base</param>
    /// <returns>The base property</returns>
    public static IPropertySymbol GetBaseProperty(this IPropertySymbol property)
        => property.GetPropertyOverrideChain().LastOrDefault() ?? property;

    /// <summary>
    /// Returns the entire event override chain for an event
    /// </summary>
    /// <param name="evt">The event symbol for which we want to get the chain</param>
    /// <returns>A list of property symbols with the property closer to the passed property being returned first</returns>
    public static IEnumerable<IEventSymbol> GetEventOverrideChain(this IEventSymbol evt)
    {
        var e = evt;
        while (e.IsOverride && e.OverriddenEvent is not null)
        {
            e = e.OverriddenEvent;
            yield return e;
        }

        yield break;
    }

    /// <summary>
    /// Get the original event base decleration for a event (the one that does not have the <see langword="override" /> keyword)
    /// </summary>
    /// <param name="evt">The event symbol for which we want to get the base</param>
    /// <returns>The base event</returns>
    public static IEventSymbol GetBaseEvent(this IEventSymbol evt)
        => evt.GetEventOverrideChain().LastOrDefault() ?? evt;

    public static bool HasSameBaseDecleration(this ISymbol symbol1, ISymbol symbol2)
    {
        if(symbol1.IsEqualTo(symbol2)) return true;

        if(symbol1.Name != symbol2.Name) return false;

        return IsSameBase<IPropertySymbol>(p => p.Type, p => p.OverriddenProperty, p => p.GetBaseProperty())
            || IsSameBase<IEventSymbol>(e => e.Type, e => e.OverriddenEvent, e => e.GetBaseEvent())
            || IsSameBase<IMethodSymbol>(m => m.ReturnType, m => m.OverriddenMethod, m => m.GetBaseMethod());

        bool IsSameBase<T>(Func<T,ITypeSymbol> typeFunc, Func<T,T?> overrideFunc, Func<T, T> baseFunc) where T : ISymbol
        {
            if(symbol1 is not T t1 || symbol2 is not T t2) return false;

            if (!typeFunc(t1).IsEqualTo(typeFunc(t2))) return false;

            return overrideFunc(t1)?.IsEqualTo(t2) == true // Optimization to avoid going through the property chain for the trivial case
                || t1.IsEqualTo(overrideFunc(t2)) == true
                || overrideFunc(t1)?.IsEqualTo(overrideFunc(t2)) == true
                || baseFunc(t1).IsEqualTo(baseFunc(t2));
        }
    }
}
