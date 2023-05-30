extern alias Workspaces;

using Microsoft.CodeAnalysis.CSharp.Extensions;
using Workspaces::Microsoft.CodeAnalysis.Shared.Extensions;

namespace DotNetPowerExtensions.Analyzers.DependencyManagement.ILocalFactory.CompletionProviders;

internal class FeatureUtils
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
