using DotNetPowerExtensions.Analyzers.DependencyManagement.ILocalFactory.Analyzers;
using DotNetPowerExtensions.Analyzers.MustInitialize.CodeFixProviders;
using DotNetPowerExtensions.Analyzers.MustInitialize.MustInitializeAttribute.CodeFixProviders;
using DotNetPowerExtensions.Analyzers.MustInitialize.MightRequireAttribute;
using DotNetPowerExtensions.Analyzers.MustInitialize;
using DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;

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

        var mustInitializeSymbols = await GetMustInitializedSymbols(document).ConfigureAwait(false);
        if (!mustInitializeSymbols.Any()) return null;

        var innerClass = classType.TypeArguments.FirstOrDefault();
        if (innerClass is null) return null;

        var props = MustInitializeUtils.GetRequiredToInitialize(innerClass, mustInitializeSymbols);
        if (!props.Any()) return null;

        if (declaration.ArgumentList.Arguments.FirstOrDefault()?.Expression is AnonymousObjectCreationExpressionSyntax creation)
        {
            var propsMissing = MustInitializeUtils.GetNotInitializedNames(creation, innerClass, mustInitializeSymbols).ToArray();
            creation = creation.WithInitializers(creation.Initializers.AddRange(props.Where(p => propsMissing.Contains(p.As<ISymbol>()!.Name)).Select(GetPropertyAssignment)));
        }
        else
            creation = SyntaxFactory.AnonymousObjectCreationExpression(SyntaxFactory.SeparatedList(props.Select(GetPropertyAssignment)));

        var newArguments = SyntaxFactory.SeparatedList(new[] { SyntaxFactory.Argument(creation) });
        return (declaration, declaration.WithArgumentList(declaration.ArgumentList.WithArguments(newArguments)));
    }

    private AnonymousObjectMemberDeclaratorSyntax GetPropertyAssignment((string name, ITypeSymbol type) prop)
    {
        var defaultExpression = SyntaxFactory.DefaultExpression(prop.type.ToTypeSyntax());

        return SyntaxFactory.AnonymousObjectMemberDeclarator(SyntaxFactory.NameEquals(prop.name), defaultExpression);
    }
}