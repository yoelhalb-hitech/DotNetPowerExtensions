extern alias Workspaces;
using Workspaces::Microsoft.CodeAnalysis.Shared.Extensions;

using DotNetPowerExtensions.Extensions;

using Microsoft.CodeAnalysis;
using System.Threading;

namespace DotNetPowerExtensions.Analyzers.Throws;

public class ThrowsUtils
{
    public static IEnumerable<(string, INamedTypeSymbol?)> GetDocCommentExceptions(ISymbol symbol, Compilation compilation)
    {
        var exceptionTypes = symbol.GetDocumentationComment(compilation, expandIncludes: true, expandInheritdoc: true).ExceptionTypes;

        return exceptionTypes.Select(e => (TrimCrefPrefix(e),
        DocumentationCommentId.GetFirstSymbolForDeclarationId(e, compilation) as INamedTypeSymbol
            ?? compilation.GetTypeByMetadataName(TrimCrefPrefix(e))
            ?? SymbolKey.ResolveString(TrimCrefPrefix(e), compilation).GetAnySymbol() as INamedTypeSymbol));

        static string TrimCrefPrefix(string value) => value.SubstringFrom(':')!; // Since this is probably a CREF
    }

    public static IEnumerable<ITypeSymbol> GetThrowsExceptions(ISymbol symbol, Compilation compilation)
    {
        var attrSymbol = compilation.GetTypeByMetadataName(typeof(ThrowsAttribute).FullName)!;
        var methodAttr = symbol.GetAttribute(attrSymbol);
        if (methodAttr is null) return new INamedTypeSymbol[] { };

        return methodAttr.ConstructorArguments
                        .Where(a => !a.IsNull)
                        .SelectMany(a => a.Kind == TypedConstantKind.Array ?
                                a.Values.Where(a1 => !a1.IsNull).Select(a => a.Value) :
                                new[] { a.Value })
                        .OfType<ITypeSymbol>()
                    .Concat(methodAttr.AttributeClass!.TypeArguments)
                    .ToArray();
    }

    public static ISymbol? GetCorrectSymbol(SymbolAnalysisContext symbolContext)
    {
        var methodSymbol = symbolContext.Symbol as IMethodSymbol;

        return GetCorrectSymbol(methodSymbol);
    }

    public static ISymbol? GetCorrectSymbol(SymbolStartAnalysisContext symbolContext)
    {
        var methodSymbol = symbolContext.Symbol as IMethodSymbol;

        return GetCorrectSymbol(methodSymbol);
    }

    private static ISymbol? GetCorrectSymbol(IMethodSymbol? methodSymbol)
    {
        var propSymbol = methodSymbol?.AssociatedSymbol as IPropertySymbol;// Property directly doesn't work since it only works on the get/set
        var eventSymbol = methodSymbol?.AssociatedSymbol as IEventSymbol; // Same

        return (ISymbol?)propSymbol ?? (ISymbol?)eventSymbol ?? methodSymbol;
    }
}
