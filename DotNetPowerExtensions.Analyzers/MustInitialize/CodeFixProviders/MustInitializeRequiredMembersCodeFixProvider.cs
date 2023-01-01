using DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;

namespace DotNetPowerExtensions.Analyzers.MustInitialize.CodeFixProviders;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MustInitializeRequiredMembersCodeFixProvider)), Shared]
public class MustInitializeRequiredMembersCodeFixProvider : MustInitializeCodeFixProviderBase<MustInitializeRequiredMembers, ObjectCreationExpressionSyntax>
{
    protected override string Title => "Initialize Required Properties";

    protected override string DiagnosticId => MustInitializeRequiredMembers.DiagnosticId;

    protected Type[] Attributes =
    {
        typeof(DotNetPowerExtensions.MustInitialize.MustInitializeAttribute),
    };

    protected override async Task<(SyntaxNode, SyntaxNode)?> CreateChanges(Document document, ObjectCreationExpressionSyntax typeDecl, CancellationToken cancellationToken)
    {
        var symbol = (await document.GetTypeInfo(typeDecl, cancellationToken).ConfigureAwait(false))?.Type;

        var mustInitializeSymbols = await Task.WhenAll(
                Attributes.Select(async a => await document.GetTypeByMetadataName(a).ConfigureAwait(false))
        ).ConfigureAwait(false);
        if (symbol is null || mustInitializeSymbols.Any(s => s is null)) return null;

        var props = MustInitializeRequiredMembers.GetNotInitializedNames(typeDecl, symbol, mustInitializeSymbols.OfType<INamedTypeSymbol>().ToArray());

        var initalizer = typeDecl.Initializer
                        ?? SyntaxFactory.InitializerExpression(SyntaxKind.ObjectInitializerExpression);
        foreach (var prop in props ?? new string[] { })
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