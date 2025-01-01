using Microsoft.CodeAnalysis.CSharp.Extensions;

namespace SequelPay.DotNetPowerExtensions.Analyzers.DependencyManagement.ILocalFactory.Features;

internal class FeatureUtils
{
    public static ISymbol? BindTokenToDeclaringSymbol(SemanticModel semanticModel, SyntaxToken token, CancellationToken cancellationToken = default)
    {
        // We only care if it is a simple identifier name
        if (token.Kind() is not SyntaxKind.IdentifierToken || token.Parent is not IdentifierNameSyntax identifier) return null;

        return BindIDentifierToDeclaringSymbol(semanticModel, identifier, cancellationToken);
    }

    public static ISymbol? BindIDentifierToDeclaringSymbol(SemanticModel semanticModel, IdentifierNameSyntax identifier, CancellationToken cancellationToken = default)
    {
        // We only care if it is a simple identifier name
        if (identifier.Parent is not NameEqualsSyntax nameEquals
            || nameEquals.Parent is not AnonymousObjectMemberDeclaratorSyntax declerator
            || declerator.Parent is not AnonymousObjectCreationExpressionSyntax creation) return null;

        var type = FeatureUtils.GetInitializedType(semanticModel, creation, cancellationToken);
        if (type is null) return null;

        var members = type.GetMembers(identifier.Identifier.Text);
        if (members.OfType<IPropertySymbol>().Any()) return members.First(); // Can only be one
        if (members.OfType<IFieldSymbol>().Any()) return members.First(); // Can only be one

        var mightRequire = MightRequireUtils.GetMightRequiredInfos(type, new MustInitializeWorker(semanticModel).MightRequireSymbols)
                                .FirstOrDefault(m => m.Name == identifier.Identifier.Text)?
                                .Attribute.AttributeConstructor;
        return mightRequire;
    }

    public static SyntaxToken? GetToken(SemanticModel semanticModel, int position, CancellationToken cancellationToken)
    {
        var tree = semanticModel.SyntaxTree;

        if (tree.IsInNonUserCode(position, cancellationToken)) return null;

        var token = tree.FindTokenOnLeftOfPosition(position, cancellationToken);
        return token.GetPreviousTokenIfTouchingWord(position);
    }

    public static ITypeSymbol? GetInitializedType(SemanticModel semanticModel, AnonymousObjectCreationExpressionSyntax creationSyntax,
                                                                                                                CancellationToken cancellationToken)
    {
        if (creationSyntax.Parent?.Parent?.Parent is not InvocationExpressionSyntax invocation
            || semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol is not IMethodSymbol methodSymbol
            || methodSymbol.ReceiverType is not INamedTypeSymbol classType
            || !classType.IsGenericType
            || methodSymbol.Name != nameof(ILocalFactory<object>.Create))
            return null;

        var innerClass = classType.TypeArguments.First();

        return innerClass;
    }
}
