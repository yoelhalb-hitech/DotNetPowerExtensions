
namespace DotNetPowerExtensions.Analyzers.Utils;

internal sealed class PropertyOverrideFactory
{
    public static PropertyDeclarationSyntax CreatePropertyOverride(PropertyDeclarationSyntax baseSyntax)
    {
        var isAbstract = baseSyntax!.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword));
        var name = baseSyntax.Identifier.Text;

        var accessorList = baseSyntax!.AccessorList ?? SyntaxFactory.AccessorList();
        var accessors = accessorList.Accessors.Select(a => CreateAccessor(a, isAbstract ? null : a.Kind() switch
        {
            SyntaxKind.GetAccessorDeclaration => "base." + name,
            SyntaxKind.SetAccessorDeclaration => "base." + name + " = value",
            _ => null,
        }));

        return SyntaxFactory.PropertyDeclaration(baseSyntax!.AttributeLists,
                GetModifiers(baseSyntax!.Modifiers, isAbstract),
                baseSyntax!.Type, null, baseSyntax.Identifier, accessorList.WithAccessors(SyntaxFactory.List(accessors)));
    }

    private static AccessorDeclarationSyntax CreateAccessor(AccessorDeclarationSyntax original, string? arrowBody)
    {
        var newAccessor = SyntaxFactory.AccessorDeclaration(original.Kind()).WithModifiers(original.Modifiers);

#if NETSTANDARD2_0_OR_GREATER
        if (!string.IsNullOrWhiteSpace(arrowBody))
        {
            var exprssionBody = SyntaxFactory.ArrowExpressionClause(SyntaxFactory.ParseExpression(arrowBody!));
            newAccessor = newAccessor.WithExpressionBody(exprssionBody);
        }
#else
        if (!string.IsNullOrWhiteSpace(arrowBody))
        {
            StatementSyntax statementBody =
                original.Kind() == SyntaxKind.GetAccessorDeclaration 
                    ? SyntaxFactory.ReturnStatement(SyntaxFactory.ParseExpression(arrowBody!)) 
                    : SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression(arrowBody!));

            newAccessor = newAccessor.WithBody(SyntaxFactory.Block(statementBody));
        }
#endif

        return newAccessor.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
    }

    private static SyntaxTokenList GetModifiers(SyntaxTokenList modifiers, bool isAbstract)
    {
        var overrideKeyword = SyntaxFactory.Token(SyntaxKind.OverrideKeyword);

        if (modifiers.Any(m => m.IsKind(SyntaxKind.VirtualKeyword)))
            modifiers = modifiers.Replace(modifiers.First(m => m.IsKind(SyntaxKind.VirtualKeyword)), overrideKeyword);

        if (isAbstract)
            modifiers = modifiers.Replace(modifiers.First(m => m.IsKind(SyntaxKind.AbstractKeyword)), overrideKeyword);

        if (!modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword)))
            modifiers = modifiers.Add(SyntaxFactory.Token(SyntaxKind.NewKeyword));

        return modifiers;
    }
}
