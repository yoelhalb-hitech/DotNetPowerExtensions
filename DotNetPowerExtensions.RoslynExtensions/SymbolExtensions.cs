using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Shared.Extensions;
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
    public static string GetContainerFullName(this ISymbol symbol)
    {
        string name = string.Empty;

        var classDecl = symbol.ContainingType;
        if (classDecl is null) return name;

        name = classDecl.Name.ToString();

        for (classDecl = classDecl.ContainingType; classDecl is not null; classDecl = classDecl.ContainingType)
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

    public static IEnumerable<ITypeSymbol> GetAllBaseTypes(this ITypeSymbol symbol)
    {
        for (var baseType = symbol.BaseType; baseType is not null; baseType = baseType.BaseType)
            yield return baseType;

        yield break;
    }

    public static AttributeData? GetAttribute(this ISymbol symbol, INamedTypeSymbol[] attributeSymbols)
        => symbol
            .GetAttributes()
            .FirstOrDefault(a => attributeSymbols.ContainsGenericOrSub(a.AttributeClass?.ConstructedFrom));

    public static bool HasAttribute(this ISymbol symbol, INamedTypeSymbol[] attributeSymbols)
        => symbol.GetAttribute(attributeSymbols) is not null;

    public static AttributeData? GetAttribute(this ISymbol symbol, INamedTypeSymbol attributeSymbol)
        => symbol.GetAttribute(new[] { attributeSymbol });

    public static bool HasAttribute(this ISymbol symbol, INamedTypeSymbol? attributeSymbol)
        => attributeSymbol is not null && symbol.HasAttribute(new[] { attributeSymbol });

    public static IEnumerable<TSyntax>? GetSyntax<TSyntax>(this ISymbol symbol) where TSyntax : SyntaxNode
        => symbol.DeclaringSyntaxReferences.Select(r => r.GetSyntax()).OfType<TSyntax>();

    public static IEnumerable<IMethodSymbol> GetConstructors(this ITypeSymbol typeSymbol, bool isStatic)
        => typeSymbol.GetMembers(".ctor").OfType<IMethodSymbol>().Where(m => m.IsStatic == isStatic);

    public static IMethodSymbol? GetDefaultConstructor(this ITypeSymbol typeSymbol)
        => typeSymbol.GetConstructors(false).FirstOrDefault(m => m.Parameters.Length == 0);

    /// <summary>
    /// Returns the entire constructor chain for a constructor (not including the passed constructor or the constructor of <see cref="object"/>)
    /// </summary>
    /// <param name="method">The constructor symbol for which we want to get the chain</param>
    /// <param name="semanticModel">The <see cref="SemanticModel"/> that was used to obtain the constructor symbol</param>
    /// <returns>A list of chained constructors symbols with the constructors closer to the passed constructor being returned first</returns>
    /// <exception cref="ArgumentException">The constructor is a static constructor</exception>
    /// <exception cref="ArgumentException">The symbol provided is not a constructor</exception>
    public static IEnumerable<IMethodSymbol> GetConstructorChain(this IMethodSymbol method, SemanticModel semanticModel)
    {
        if(method.IsStatic) throw new ArgumentException("Static constructor is not valid", nameof(method));
        if(method.Name != ".ctor") throw new ArgumentException("Not a constructor", nameof(method));

        var syntax = method.GetSyntax<ConstructorDeclarationSyntax>();
        var chained = syntax?.FirstOrDefault(s => s.Initializer is not null);

        IMethodSymbol? chainedSymbol = chained is null ? null : semanticModel.GetSymbolInfo(chained!.Initializer!).Symbol as IMethodSymbol;

        if(chainedSymbol is null) chainedSymbol = method.ContainingType.BaseType?.SpecialType == SpecialType.System_Object ? null :
                                                                                    method.ContainingType.BaseType?.GetDefaultConstructor();

        if (chainedSymbol is null) yield break;

        yield return chainedSymbol;
        foreach (var m in chainedSymbol.GetConstructorChain(semanticModel)) yield return m;
    }

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

    /// <summary>
    /// Returns the entire method override chain for a method
    /// </summary>
    /// <param name="method">The method symbol for which we want to get the chain</param>
    /// <returns>A list of method symbols with the method closer to the passed property being returned first</returns>
    public static IEnumerable<IMethodSymbol> GetMethodOverrideChain(this IMethodSymbol method)
    {
        var m = method;
        while (m.IsOverride && m.OverriddenMethod is not null)
        {
            m = m.OverriddenMethod;
            yield return m;
        }

        yield break;
    }

    /// <summary>
    /// Get the original method base decleration for a method (the one that does not have the <see langword="override" /> keyword)
    /// </summary>
    /// <param name="method">The method symbol for which we want to get the base</param>
    /// <returns>The base method</returns>
    public static IMethodSymbol GetBaseMethod(this IMethodSymbol method)
        => method.GetMethodOverrideChain().LastOrDefault() ?? method;

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
