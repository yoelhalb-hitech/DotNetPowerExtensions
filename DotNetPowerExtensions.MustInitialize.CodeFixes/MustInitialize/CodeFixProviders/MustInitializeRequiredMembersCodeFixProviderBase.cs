using Microsoft.CodeAnalysis.Operations;

namespace SequelPay.DotNetPowerExtensions.Analyzers.MustInitialize.CodeFixProviders;

public abstract class MustInitializeRequiredMembersCodeFixProviderBase<TAnalyzer, TNode> : MustInitializeCodeFixProviderBase<TAnalyzer, TNode>
                                                                                    where TAnalyzer : MustInitializeRequiredMembersBase
                                                                                    where TNode : CSharpSyntaxNode
{
    protected override string Title => "Initialize Required Properties";

    private Type[] Attributes =
    {
        typeof(SequelPay.DotNetPowerExtensions.MustInitializeAttribute),
    };

    protected virtual async Task<INamedTypeSymbol[]> GetMustInitializedSymbols(Document document, CancellationToken c) =>
                                            await document.GetTypeSymbolsAsync(Attributes, c).ConfigureAwait(false);

    protected virtual async Task<(SyntaxNode, SyntaxNode)?> GetInitializerChanges(Document document, BaseObjectCreationExpressionSyntax typeDecl,
                                                                                                                    CancellationToken cancellationToken)
    {
        var symbol = (await document.GetTypeInfoAsync(typeDecl, cancellationToken).ConfigureAwait(false))?.Type;
        if (symbol is null) return null;

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if(semanticModel is null) return null;

        var worker = new MustInitializeWorker(semanticModel);

        var ctor = (semanticModel.GetOperation(typeDecl, cancellationToken) as IObjectCreationOperation)?.Constructor;
        var props = worker.GetNotInitializedNames(typeDecl, symbol, ctor, cancellationToken);

        var initalizer = typeDecl.Initializer
                        ?? SyntaxFactory.InitializerExpression(SyntaxKind.ObjectInitializerExpression);
        foreach (var prop in props ?? Array.Empty<string>())
        {
            var expr = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                                        SyntaxFactory.IdentifierName(prop),
#if NETSTANDARD2_0_OR_GREATER
                                                        SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression));
#else
                                                        SyntaxFactory.LiteralExpression(SyntaxKind.DefaultKeyword));
#endif
            initalizer = initalizer.AddExpressions(expr);
        }

        // Remmeber that everything is immutable
        if (typeDecl.Initializer is null)
        {
            return (typeDecl, typeDecl.WithInitializer(initalizer));
        }

        return (typeDecl.Initializer, initalizer);
    }
}