using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SequelPay.DotNetPowerExtensions.RoslynExtensions.Utils;

internal sealed class PropertyOverrideFactory
{
    public static PropertyDeclarationSyntax CreatePropertyOverride(PropertyDeclarationSyntax baseSyntax)
    {
        var isAbstract = baseSyntax!.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword));
        var name = baseSyntax.Identifier.Text;

        var accessorList = baseSyntax!.AccessorList ?? SyntaxFactory.AccessorList();
        var accessors = accessorList.Accessors
                            .Where(a => !a.Modifiers.Any(m => m.IsKind(SyntaxKind.PrivateKeyword)))
                            .Select(a => CreateAccessor(a, isAbstract ? null : a.Kind() switch
        {
            SyntaxKind.GetAccessorDeclaration => "base." + name,
            SyntaxKind.SetAccessorDeclaration => "base." + name + " = value",
            _ => null,
        }));

        return PropertyDeclaration(baseSyntax!.AttributeLists,
                GetModifiers(baseSyntax!.Modifiers, isAbstract),
                baseSyntax!.Type, null, baseSyntax.Identifier, accessorList.WithAccessors(List(accessors)));
    }

    private static AccessorDeclarationSyntax CreateAccessor(AccessorDeclarationSyntax original, string? arrowBody)
    {
        var newAccessor = AccessorDeclaration(original.Kind()).WithModifiers(original.Modifiers);

#if NETSTANDARD2_0_OR_GREATER
        if (!string.IsNullOrWhiteSpace(arrowBody))
        {
            var exprssionBody = ArrowExpressionClause(ParseExpression(arrowBody!).WithLeadingTrivia(ParseLeadingTrivia(" ")))
                                                                                            .WithLeadingTrivia(ParseLeadingTrivia(" "));
            newAccessor = newAccessor.WithExpressionBody(exprssionBody);
        }
#else
        if (!string.IsNullOrWhiteSpace(arrowBody))
        {
            StatementSyntax statementBody =
                original.Kind() == SyntaxKind.GetAccessorDeclaration
                    ? ReturnStatement(ParseExpression(arrowBody!))
                    : ExpressionStatement(ParseExpression(arrowBody!));

            newAccessor = newAccessor.WithBody(Block(statementBody));
        }
#endif

        return newAccessor.WithSemicolonToken(Token(SyntaxKind.SemicolonToken)).WithTrailingTrivia(ParseTrailingTrivia(" "));
    }

    private static SyntaxTokenList GetModifiers(SyntaxTokenList modifiers, bool isAbstract)
    {
        var overrideKeyword = Token(SyntaxKind.OverrideKeyword).WithTrailingTrivia(ParseTrailingTrivia(" "));

        if (modifiers.Any(m => m.IsKind(SyntaxKind.VirtualKeyword)))
            modifiers = modifiers.Replace(modifiers.First(m => m.IsKind(SyntaxKind.VirtualKeyword)), overrideKeyword);

        if (isAbstract)
            modifiers = modifiers.Replace(modifiers.First(m => m.IsKind(SyntaxKind.AbstractKeyword)), overrideKeyword);

        if (!modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword)))
            modifiers = modifiers.Add(Token(SyntaxKind.NewKeyword).WithTrailingTrivia(ParseTrailingTrivia(" ")));

        return modifiers;
    }
}
