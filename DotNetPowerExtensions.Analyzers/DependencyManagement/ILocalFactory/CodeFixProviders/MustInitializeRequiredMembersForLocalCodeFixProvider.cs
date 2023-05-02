using DotNetPowerExtensions.Analyzers.DependencyManagement.ILocalFactory.Analyzers;
using DotNetPowerExtensions.Analyzers.MustInitialize.CodeFixProviders;
using DotNetPowerExtensions.Analyzers.MustInitialize.MustInitializeAttribute.CodeFixProviders;
using DotNetPowerExtensions.Analyzers.MustInitialize.MightRequireAttribute;
using DotNetPowerExtensions.Analyzers.MustInitialize;

namespace DotNetPowerExtensions.Analyzers.DependencyManagement.ILocalFactory.CodeFixProviders;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MustInitializeRequiredMembersCodeFixProvider)), Shared]
public class MustInitializeRequiredMembersForLocalCodeFixProvider
                    : MustInitializeRequiredMembersCodeFixProviderBase<MustInitializeRequiredMembersForILocalFactory, InvocationExpressionSyntax>
{
    protected override string DiagnosticId => MustInitializeRequiredMembersForILocalFactory.DiagnosticId;

    protected override async Task<(SyntaxNode declToReplace, SyntaxNode newDecl)?> CreateChanges(Document document,
                                                                InvocationExpressionSyntax declaration, CancellationToken c)
    {
        if ((await document.GetSymbolInfoAsync(declaration, c).ConfigureAwait(false))?.Symbol is not IMethodSymbol methodSymbol
                                                        || methodSymbol.ReceiverType is not INamedTypeSymbol classType) return null;

        var mustInitializeSymbols = await GetMustInitializedSymbols(document, c).ConfigureAwait(false);
        var mightRequireSymbols = await MightRequireUtils.GetMightRequireSymbols(document).ConfigureAwait(false);
        if (!mustInitializeSymbols.Any() && !mustInitializeSymbols.Any()) return null;

        var intializedSymbol = await document.GetTypeByMetadataNameAsync(typeof(InitializedAttribute), c).ConfigureAwait(false);

        var innerClass = classType.TypeArguments.FirstOrDefault();
        if (innerClass is null) return null;

        var propsGroup = MustInitializeUtils.GetRequiredToInitialize(innerClass, mustInitializeSymbols, mightRequireSymbols, intializedSymbol)
                        .GroupBy(p => p.name)
                        .OrderBy(g => g.Key);
        if (!propsGroup.Any()) return null;

        if (declaration.ArgumentList.Arguments.FirstOrDefault()?.Expression is AnonymousObjectCreationExpressionSyntax creation)
        {
            var propsMissing = MustInitializeUtils
                    .GetNotInitializedNames(creation, innerClass, mustInitializeSymbols, mightRequireSymbols, intializedSymbol)
                    .ToArray();
            creation = creation.WithInitializers(
                    creation.Initializers.AddRange(propsGroup.Where(g => propsMissing.Contains(g.Key)).Select(g => GetPropertyAssignment(g.First()))));
        }
        else
        {
            creation = SyntaxFactory.AnonymousObjectCreationExpression(SyntaxFactory.SeparatedList(propsGroup.Select(g => GetPropertyAssignment(g.First()))));

        }

        var newArguments = SyntaxFactory.SeparatedList(new[] { SyntaxFactory.Argument(creation) });
        return (declaration, declaration.WithArgumentList(declaration.ArgumentList.WithArguments(newArguments)));
    }

    private AnonymousObjectMemberDeclaratorSyntax GetPropertyAssignment((string name, ITypeSymbol type) prop)
    {
        var defaultExpression = SyntaxFactory.DefaultExpression(prop.type.ToTypeSyntax());

        return SyntaxFactory.AnonymousObjectMemberDeclarator(SyntaxFactory.NameEquals(prop.name), defaultExpression);
    }
}