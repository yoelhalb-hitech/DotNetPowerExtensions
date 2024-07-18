
namespace DotNetPowerExtensions.Analyzers.DependencyManagement.DependencyAttribute.Analyzers;

internal static class DependencyAnalyzerUtils
{
    public static Type[] BaseAttributes =
    {
        typeof(LocalBaseAttribute),
        typeof(SingletonBaseAttribute),
        typeof(ScopedBaseAttribute),
        typeof(TransientBaseAttribute),
    };

    public static string[] NonLocalAttributeNames =
{
        nameof(SingletonAttribute),
        nameof(ScopedAttribute),
        nameof(TransientAttribute),
    };
    //CAUTION: The test framework for whatever reason has a compile issue if the following is before the decleration of the referenced props
    public static string[] DependencyAttributeNames = NonLocalAttributeNames.Concat(new[] { nameof(LocalAttribute) }).ToArray();

    public static Type[] LocalAttributes =
    {
        typeof(LocalAttribute),
        typeof(LocalAttribute<>),
        typeof(LocalAttribute<,>),
        typeof(LocalAttribute<,,>),
        typeof(LocalAttribute<,,,>),
        typeof(LocalAttribute<,,,,>),
        typeof(LocalAttribute<,,,,,>),
        typeof(LocalAttribute<,,,,,,>),
        typeof(LocalAttribute<,,,,,,,>),
    };

    public static Type[] NonLocalAttributes =
    {
        typeof(SingletonAttribute),
        typeof(SingletonAttribute<>),
        typeof(SingletonAttribute<,>),
        typeof(SingletonAttribute<,,>),
        typeof(SingletonAttribute<,,,>),
        typeof(SingletonAttribute<,,,,>),
        typeof(SingletonAttribute<,,,,,>),
        typeof(SingletonAttribute<,,,,,,>),
        typeof(SingletonAttribute<,,,,,,,>),
        typeof(ScopedAttribute),
        typeof(ScopedAttribute<>),
        typeof(ScopedAttribute<,>),
        typeof(ScopedAttribute<,,>),
        typeof(ScopedAttribute<,,,>),
        typeof(ScopedAttribute<,,,,>),
        typeof(ScopedAttribute<,,,,,>),
        typeof(ScopedAttribute<,,,,,,>),
        typeof(ScopedAttribute<,,,,,,,>),
        typeof(TransientAttribute),
        typeof(TransientAttribute<>),
        typeof(TransientAttribute<,>),
        typeof(TransientAttribute<,,>),
        typeof(TransientAttribute<,,,>),
        typeof(TransientAttribute<,,,,>),
        typeof(TransientAttribute<,,,,,>),
        typeof(TransientAttribute<,,,,,,>),
        typeof(TransientAttribute<,,,,,,,>),
    };
    //CAUTION: The test framework for whatever reason has a compile issue if the following is before the decleration of the referenced props
    public static Type[] AllDependencies = LocalAttributes.Concat(NonLocalAttributes).ToArray();

    public static (AttributeSyntax syntax, string name)? GetAttributeSyntaxInfo(AttributeSyntax? attr, string[] names)
    {
        var attrName = attr is null ? null : SequelPay.DotNetPowerExtensions.RoslynExtensions.SyntaxExtensions.GetUnqualifiedName(attr.Name)?.Replace(nameof(Attribute), "");
        if (attrName is null || !names.Contains(attrName + nameof(Attribute))) return null;
        return (attr!, attrName);
    }

    public static IMethodSymbol? GetSymbol(SyntaxNodeAnalysisContext context, AttributeSyntax? attr, INamedTypeSymbol[] attributeSymbols)
    {
        if (context.SemanticModel.GetSymbolInfo(attr!, context.CancellationToken).Symbol is not IMethodSymbol methodSymbol
                                    || !attributeSymbols.ContainsGeneric(methodSymbol.ContainingType)) return null;

        return methodSymbol;
    }

    public static (AttributeSyntax syntax, string name, IMethodSymbol methodSymbol)?
                                        GetAttributeInfo(SyntaxNodeAnalysisContext context, string[] names, INamedTypeSymbol[] attributeSymbols)
    {
        var attr = context.Node as AttributeSyntax;
        if (attr is null) return null;

        var result = GetAttributeSyntaxInfo(attr, names);
        if(result is null) return null;

        var symbol = GetSymbol(context, attr, attributeSymbols);
        if(symbol is null) return null;

        return (attr, result.Value.name, symbol);
    }

    public static ExpressionSyntax GetInner(ExpressionSyntax expression)
    {
        var innerExpression = expression;
        while (innerExpression is ParenthesizedExpressionSyntax paren && paren.Expression is not null) innerExpression = paren.Expression;
        return innerExpression;
    }

    /// <summary>
    /// Get the types passed for the <see cref="SequelPay.DotNetPowerExtensions.DependencyAttribute.Use"/> property
    /// </summary>
    /// <param name="attr"></param>
    /// <returns></returns>

    public static (AttributeArgumentSyntax? useExpression, ExpressionSyntax? innerExpression) GetUse(AttributeSyntax attr)
    {
        var useExpression = attr.ArgumentList?.Arguments.FirstOrDefault(a => a.NameEquals?.Name is IdentifierNameSyntax name
            && name.Identifier.Text == nameof(SequelPay.DotNetPowerExtensions.DependencyAttribute.Use));
        if (useExpression is null) return (null, null);

        return (useExpression, GetInner(useExpression.Expression));
    }

    public static (AttributeSyntax syntax, string name, IMethodSymbol methodSymbol, ITypeSymbol[] )?
                                    GetAttributeWithTypes(SyntaxNodeAnalysisContext context, string[] names, INamedTypeSymbol[] attributeSymbols)
    {
        var info = GetAttributeInfo(context, names, attributeSymbols);
        if (info is null) return null;

        var (attr, name, symbol) = info.Value;

        var args = attr!.ArgumentList?.Arguments.Where(a => a.NameEquals is null).ToArray();
        if (args?.Any() != true && attr.Name is not GenericNameSyntax
                            && (attr.Name is not QualifiedNameSyntax qualified || qualified.Right is not GenericNameSyntax))
            return null;

        var argTypes = args?.Select(a => GetInner(a.Expression))
            .OfType<TypeOfExpressionSyntax>()
            .Select(e => context.SemanticModel.GetSymbolInfo(e.Type, context.CancellationToken).Symbol)
                .OfType<ITypeSymbol>();

        var types = GetForTypes(attr, symbol, context.SemanticModel, context.CancellationToken);
        if (types is null) return null; // Should not happen

        return (attr, name, symbol, types);
    }

    public static ITypeSymbol[]? GetForTypes(AttributeSyntax attr, IMethodSymbol symbol, SemanticModel semanticModel, CancellationToken c)
    {
        var args = attr!.ArgumentList?.Arguments.Where(a => a.NameEquals is null).ToArray();
        if (args?.Any() != true && attr.Name is not GenericNameSyntax
                            && (attr.Name is not QualifiedNameSyntax qualified || qualified.Right is not GenericNameSyntax))
            return null;

        var argTypes = args?.Select(a => GetInner(a.Expression))
            .OfType<TypeOfExpressionSyntax>()
            .Select(e => semanticModel.GetSymbolInfo(e.Type, c).Symbol)
        .OfType<ITypeSymbol>();

        return (argTypes ?? new ITypeSymbol[] { }).Concat(symbol!.ContainingType.TypeArguments).ToArray();
    }

    /// <summary>
    /// Get the <see cref="SequelPay.DotNetPowerExtensions.DependencyAttribute.For" /> types (or the generics for C#11 and up)
    /// </summary>
    /// <param name="attribute"></param>
    /// <returns></returns>
    public static ITypeSymbol[] GetForTypes(AttributeData attribute)
        => attribute.ConstructorArguments
                        .Where(a => !a.IsNull)
                        .SelectMany(a => a.Kind == TypedConstantKind.Array ?
                                a.Values.Where(a1 => !a1.IsNull).Select(a => a.Value) :
                                new[] { a.Value })
                        .OfType<ITypeSymbol>()
                    .Concat(attribute.AttributeClass!.TypeArguments)
                    .ToArray();
}
