using DotNetPowerExtensions;
using DotNetPowerExtensions.Analyzers.DependencyManagement.LocalService.Analyzers;
using DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;
using DotNetPowerExtensions.Analyzers.MustInitialize.CodeFixProviders;
using DotNetPowerExtensions.Analyzers.MustInitialize.MustInitializeAttribute.CodeFixProviders;

namespace DotNetPowerExtensions.Analyzers.DependencyManagement.LocalService.CodeFixProviders;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MustInitializeRequiredMembersCodeFixProvider)), Shared]
public class MustInitializeRequiredMembersForLocalCodeFixProvider
                    : MustInitializeRequiredMembersCodeFixProviderBase<MustIinitializeRequiredMembersForLocalService, InvocationExpressionSyntax>
{
    protected override string DiagnosticId => MustIinitializeRequiredMembersForLocalService.DiagnosticId;

    protected override async Task<(SyntaxNode declToReplace, SyntaxNode newDecl)?> CreateChanges(Document document,
                                                                InvocationExpressionSyntax declaration, CancellationToken c)
    {
        if ((await document.GetSymbolInfo(declaration, c).ConfigureAwait(false))?.Symbol is not IMethodSymbol methodSymbol
                                                        || methodSymbol.ReceiverType is not INamedTypeSymbol classType) return null;

        var mustInitializeSymbols = await GetMustInitializedSymbols(document).ConfigureAwait(false);
        if (!mustInitializeSymbols.Any()) return null;

        var innerClass = classType.TypeArguments.FirstOrDefault();
        if (innerClass is null) return null;

        var props = MustInitializeRequiredMembersBase.GetMembersWithMustInitialize(innerClass, mustInitializeSymbols);
        if (!props.Any()) return null;

        if (declaration.ArgumentList.Arguments.FirstOrDefault()?.Expression is AnonymousObjectCreationExpressionSyntax creation)
        {
            var propsMissing = MustIinitializeRequiredMembersForLocalService.GetNotInitializedNames(creation, innerClass, mustInitializeSymbols).ToArray();
            creation = creation.WithInitializers(creation.Initializers.AddRange(props.Where(p => propsMissing.Contains(p.As<ISymbol>()!.Name)).Select(GetPropertyAssignment)));
        } 
        else
            creation = SyntaxFactory.AnonymousObjectCreationExpression(SyntaxFactory.SeparatedList(props.Select(GetPropertyAssignment)));

        var newArguments = SyntaxFactory.SeparatedList(new[] { SyntaxFactory.Argument(creation) });
        return (declaration, declaration.WithArgumentList(declaration.ArgumentList.WithArguments(newArguments)));
    }

    private AnonymousObjectMemberDeclaratorSyntax GetPropertyAssignment(Of<IPropertySymbol, IFieldSymbol> prop)
    {
        ITypeSymbol type = prop.First?.Type ?? prop.Second!.Type;

        // For an anonymous we have to specify the type of the property
        TypeSyntax typeSyntax;
        if(type.SpecialType != SpecialType.None)
            typeSyntax = SyntaxFactory.PredefinedType(SyntaxFactory.ParseToken(type.ToString()!)); // We need to do it this way as the Test framwork expects a predefined type in this case
        else
        {
            var str = type.ToStringWithoutNamesapce(); // This will handle correctly keywords such as string and generics and tuples
            
            typeSyntax = SyntaxFactory.ParseName(str); // ParseName will handle correctly generic names
        }

        var defaultExpression = SyntaxFactory.DefaultExpression(typeSyntax);

        return SyntaxFactory.AnonymousObjectMemberDeclarator(SyntaxFactory.NameEquals(prop.As<ISymbol>()!.Name), defaultExpression);
    }
}