using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetPowerExtensionsAnalyzer.Utils;

internal class PropertyOverrideFactory
{
    public PropertyDeclarationSyntax CreatePropertyOverride(PropertyDeclarationSyntax baseSyntax)
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

    protected virtual AccessorDeclarationSyntax CreateAccessor(AccessorDeclarationSyntax original, string? arrowBody)
    {
        var newAccessor = SyntaxFactory.AccessorDeclaration(original.Kind()).WithModifiers(original.Modifiers);
        
        if (!string.IsNullOrWhiteSpace(arrowBody))
        {
            var exprssionBody = SyntaxFactory.ArrowExpressionClause(SyntaxFactory.ParseExpression(arrowBody));
            newAccessor = newAccessor.WithExpressionBody(exprssionBody);
        }

        return newAccessor.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
    }

    protected virtual SyntaxTokenList GetModifiers(SyntaxTokenList modifiers, bool isAbstract)
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
