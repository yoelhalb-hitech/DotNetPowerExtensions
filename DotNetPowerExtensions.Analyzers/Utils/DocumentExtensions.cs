
namespace DotNetPowerExtensions.Analyzers.Utils;

internal static class DocumentExtensions
{
    public async static Task<TSymbol?> GetDeclaredSymbol<TSymbol>(this Document document, SyntaxNode declarationSyntax, CancellationToken token = default)
                                                                    where TSymbol : class, ISymbol
        => (await document.GetSemanticModelAsync(token).ConfigureAwait(false))?.GetDeclaredSymbol(declarationSyntax, token) as TSymbol;

    public async static Task<INamedTypeSymbol?> GetTypeByMetadataName(this Document document, Type type, CancellationToken token = default)
        => (await document.GetSemanticModelAsync(token).ConfigureAwait(false))?.Compilation.GetTypeByMetadataName(type.FullName!);

    public async static Task<TypeInfo?> GetTypeInfo(this Document document, ExpressionSyntax expr, CancellationToken token = default)
        => (await document.GetSemanticModelAsync(token).ConfigureAwait(false))?.GetTypeInfo(expr, token);

    public async static Task<SymbolInfo?> GetSymbolInfo(this Document document, ExpressionSyntax expr, CancellationToken token = default)
        => (await document.GetSemanticModelAsync(token).ConfigureAwait(false))?.GetSymbolInfo(expr, token);
}
