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

    public static bool IsGenericEqual(this ISymbol? symbol, ISymbol? other)
        => symbol?.OriginalDefinition.IsEqualTo(other?.OriginalDefinition) == true;

    public static bool ContainsGeneric<T>(this IEnumerable<T?> symbols, T? other) where T : ISymbol
        => other is not null && symbols.Any(s => s?.IsGenericEqual(other) ?? false);

    public static bool ContainsSymbol<T>(this IEnumerable<T> symbols, T? other) where T : ISymbol
        => other is not null && symbols.Any(s => s.IsEqualTo(other));

    public static IEnumerable<AttributeData> GetAttributes(this ISymbol symbol, INamedTypeSymbol[] attributeSymbols)
     => symbol
         .GetAttributes()
         .Where(a => attributeSymbols.ContainsGenericOrSub(a.AttributeClass?.OriginalDefinition, false));


    public static AttributeData? GetAttribute(this ISymbol symbol, INamedTypeSymbol[] attributeSymbols)
        => symbol
            .GetAttributes(attributeSymbols)
            .FirstOrDefault();

    public static bool HasAttribute(this ISymbol symbol, INamedTypeSymbol[] attributeSymbols)
        => symbol.GetAttribute(attributeSymbols) is not null;

    public static IEnumerable<AttributeData> GetAttributes(this ISymbol symbol, INamedTypeSymbol attributeSymbol)
        => symbol.GetAttributes(new[] { attributeSymbol });

    public static AttributeData? GetAttribute(this ISymbol symbol, INamedTypeSymbol attributeSymbol)
        => symbol.GetAttribute(new[] { attributeSymbol });

    public static bool HasAttribute(this ISymbol symbol, INamedTypeSymbol? attributeSymbol)
        => attributeSymbol is not null && symbol.HasAttribute(new[] { attributeSymbol });

    public static IEnumerable<TSyntax>? GetSyntax<TSyntax>(this ISymbol symbol, CancellationToken token = default) where TSyntax : SyntaxNode
        => symbol.DeclaringSyntaxReferences.Select(r => r.GetSyntax(token)).OfType<TSyntax>();

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
