using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SequelPay.DotNetPowerExtensions.RoslynExtensions;

public static class DocumentExtensions
{
    public async static Task<TSymbol?> GetDeclaredSymbolAsync<TSymbol>(this Document document, SyntaxNode declarationSyntax, CancellationToken token = default)
                                                                    where TSymbol : class, ISymbol
        => (await document.GetSemanticModelAsync(token).ConfigureAwait(false))?.GetDeclaredSymbol(declarationSyntax, token) as TSymbol;

    public async static Task<INamedTypeSymbol?> GetTypeByMetadataNameAsync(this Document document, Type type, CancellationToken token = default)
        => (await document.GetSemanticModelAsync(token).ConfigureAwait(false))?.Compilation.GetTypeByMetadataName(type.FullName!);

    public async static Task<TypeInfo?> GetTypeInfoAsync(this Document document, ExpressionSyntax expr, CancellationToken token = default)
        => (await document.GetSemanticModelAsync(token).ConfigureAwait(false))?.GetTypeInfo(expr, token);

    public async static Task<SymbolInfo?> GetSymbolInfoAsync(this Document document, ExpressionSyntax expr, CancellationToken token = default)
        => (await document.GetSemanticModelAsync(token).ConfigureAwait(false))?.GetSymbolInfo(expr, token);

    public async static Task<INamedTypeSymbol[]> GetTypeSymbolsAsync(this Document document, Type[] types, CancellationToken c) =>
                    (await Task.WhenAll(
                            types.Select(async a => await document.GetTypeByMetadataNameAsync(a, c).ConfigureAwait(false))
                    ).ConfigureAwait(false))
                    .Where(s => s is not null).OfType<INamedTypeSymbol>().ToArray();
}
