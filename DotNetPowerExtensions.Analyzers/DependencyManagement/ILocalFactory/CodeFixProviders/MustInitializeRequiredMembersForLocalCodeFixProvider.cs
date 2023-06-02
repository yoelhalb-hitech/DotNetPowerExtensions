using DotNetPowerExtensions.Analyzers.DependencyManagement.ILocalFactory.Analyzers;
using DotNetPowerExtensions.Analyzers.MustInitialize.CodeFixProviders;
using DotNetPowerExtensions.Analyzers.MustInitialize.MustInitializeAttribute.CodeFixProviders;
using DotNetPowerExtensions.Analyzers.MustInitialize.MightRequireAttribute;
using DotNetPowerExtensions.Analyzers.MustInitialize;
using Microsoft.CodeAnalysis;

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

        var semnaticModel = await document.GetSemanticModelAsync(c).ConfigureAwait(false);
        if (semnaticModel is null) return null;

        var worker = new MustInitializeWorker(semnaticModel);

        var innerClass = classType.TypeArguments.FirstOrDefault();
        if (innerClass is null) return null;

        var propsGroup = worker.GetRequiredToInitialize(innerClass, null, c).GroupBy(p => p.name).OrderBy(g => g.Key);
        if (!propsGroup.Any()) return null;

        if (declaration.ArgumentList.Arguments.FirstOrDefault()?.Expression is AnonymousObjectCreationExpressionSyntax creation)
        {
            var propsMissing = worker.GetNotInitializedNames(creation, innerClass, c).ToArray();
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

    private AnonymousObjectMemberDeclaratorSyntax GetPropertyAssignment((string name, ITypeSymbol type, ISymbol symbol) prop)
    {
        var defaultExpression = SyntaxFactory.DefaultExpression(prop.type.ToTypeSyntax());

        return SyntaxFactory.AnonymousObjectMemberDeclarator(SyntaxFactory.NameEquals(prop.name), defaultExpression);
    }
}