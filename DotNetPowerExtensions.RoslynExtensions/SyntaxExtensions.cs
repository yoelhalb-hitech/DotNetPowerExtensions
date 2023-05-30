using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SequelPay.DotNetPowerExtensions.RoslynExtensions;

public static class SyntaxExtensions
{
    // From https://andrewlock.net/creating-a-source-generator-part-5-finding-a-type-declarations-namespace-and-type-hierarchy/#finding-the-namespace-for-a-class-syntax
    public static string GetNamespace(this BaseTypeDeclarationSyntax syntax)
    {
        string nameSpace = string.Empty;

#if NETSTANDARD2_0_OR_GREATER
        var namespaceParent = syntax.FirstAncestorOrSelf<BaseNamespaceDeclarationSyntax>();
#else
        var namespaceParent = syntax.FirstAncestorOrSelf<NamespaceDeclarationSyntax>();
#endif
        if (namespaceParent is null) return nameSpace;

        nameSpace = namespaceParent.Name.ToString();

#if NETSTANDARD2_0_OR_GREATER
        while ((namespaceParent = namespaceParent!.Parent?.FirstAncestorOrSelf<BaseNamespaceDeclarationSyntax>()) is not null)
            nameSpace = $"{namespaceParent.Name}.{nameSpace}";
#else
        while ((namespaceParent = namespaceParent!.FirstAncestorOrSelf<NamespaceDeclarationSyntax>()) is not null)
            nameSpace = $"{namespaceParent.Name}.{nameSpace}";
#endif

        return nameSpace;
    }

    // TODO... so far this doesn't work on inner methods...
    public static string GetContainerFullName(this MemberDeclarationSyntax syntax)
    {
        string name = string.Empty;

        var classDecl = syntax.FirstAncestorOrSelf<BaseTypeDeclarationSyntax>();
        if (classDecl is null) return name;

        name = classDecl.Identifier.Text.ToString();


        while (true)
        {
            var newDecl = classDecl!.Parent?.FirstAncestorOrSelf<BaseTypeDeclarationSyntax>();
            if (newDecl is null) break;

            classDecl = newDecl;
            name = $"{classDecl.Identifier.Text}+{name}";
        }

        return classDecl.GetNamespace() + "." + name;
    }

    public static string? GetUnqualifiedName(this NameSyntax nameSyntax) => nameSyntax.GetUnqualifiedNameToken()?.ValueText;

    public static SyntaxToken? GetUnqualifiedNameToken(this NameSyntax nameSyntax) => nameSyntax switch
    {
        IdentifierNameSyntax id => id.Identifier,
        QualifiedNameSyntax q => q.Right.Identifier,
        SimpleNameSyntax name => name.Identifier,
        AliasQualifiedNameSyntax alias => alias.Name.Identifier,
        _ => null,
    };

    public static SyntaxToken? GetNameToken(this AnonymousObjectMemberDeclaratorSyntax syntax)
    {
        if (syntax.NameEquals is not null) return syntax.NameEquals!.Name.Identifier;

        return GetUnqualifiedNameFromExpression(syntax.Expression);

        static SyntaxToken? GetUnqualifiedNameFromExpression(ExpressionSyntax expression) => expression switch
        {
            NameSyntax name => name.GetUnqualifiedNameToken(),
            MemberAccessExpressionSyntax member => member.Name.Identifier,
            MemberBindingExpressionSyntax binding => binding.Name.Identifier,
            ConditionalAccessExpressionSyntax cond => GetUnqualifiedNameFromExpression(cond.WhenNotNull),
            _ => null
        };
    }

    public static string? GetName(this AnonymousObjectMemberDeclaratorSyntax syntax) => syntax.GetNameToken()?.ValueText;
}
