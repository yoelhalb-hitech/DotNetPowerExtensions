using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetPowerExtensionsAnalyzer.Utils;

internal static class DocumentExtensions
{
    public async static Task<TSymbol?> GetDeclaredSymbol<TSymbol>(this Document document, SyntaxNode declarationSyntax, CancellationToken token = default)
                                                                    where TSymbol : class, ISymbol
        => (await document.GetSemanticModelAsync(token).ConfigureAwait(false))?.GetDeclaredSymbol(declarationSyntax, token) as TSymbol;

    public async static Task<INamedTypeSymbol?> GetTypeByMetadataName(this Document document, Type type, CancellationToken token = default)
        => (await document.GetSemanticModelAsync(token).ConfigureAwait(false))?.Compilation.GetTypeByMetadataName(type.FullName!);

    public async static Task<TypeInfo?> GetTypeInfo(this Document document, ExpressionSyntax expr, CancellationToken token = default)
        => (await document.GetSemanticModelAsync(token).ConfigureAwait(false))?.GetTypeInfo(expr);
}
