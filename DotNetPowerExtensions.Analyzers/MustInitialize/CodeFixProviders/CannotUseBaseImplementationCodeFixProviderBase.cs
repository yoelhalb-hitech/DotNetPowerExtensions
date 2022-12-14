using DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;

namespace DotNetPowerExtensions.Analyzers.MustInitialize.CodeFixProviders;

public abstract class CannotUseBaseImplementationCodeFixProviderBase<TAnalyzer>
            : ByAttributeCodeFixProviderBase<TAnalyzer, TypeDeclarationSyntax> where TAnalyzer : CannotUseBaseImplementationBase, IMustInitializeAnalyzer
{
    protected override string Title => "Implement Required Properties";

    protected virtual AttributeSyntax GetAttribute(IPropertySymbol prop, INamedTypeSymbol mustInitializeSymbol)
    {
        var interfaces = prop.ContainingType.AllInterfaces.Except(prop.ContainingType.BaseType!.AllInterfaces);
        var name = prop.Name;

        var interfaceProp = interfaces.SelectMany(i => i.GetMembers(name)).First(m => m.HasAttribute(mustInitializeSymbol));

        return interfaceProp.GetAttribute(mustInitializeSymbol)?.ApplicationSyntaxReference?.GetSyntax() as AttributeSyntax
                                                                                    ?? throw new Exception("Interface prop not found");
    }

    protected virtual async Task<PropertyDeclarationSyntax?> GetPropertyDeclaration(IPropertySymbol prop, ITypeSymbol[] baseTypes,
                                                                                                        INamedTypeSymbol mustInitializeSymbol)
    {
        var baseProperty = baseTypes.First(t => t.GetMembers(prop.Name).Any()).GetMembers(prop.Name).OfType<IPropertySymbol>().First();
        var baseSyntax = await baseProperty.DeclaringSyntaxReferences.First().GetSyntaxAsync().ConfigureAwait(false) as PropertyDeclarationSyntax;
        if (baseSyntax is null)
        {
            Logger.LogError(new Exception("BaseSyntax is null"));
            return null;
        }

        var newDecl = new PropertyOverrideFactory().CreatePropertyOverride(baseSyntax);

        var attribute = GetAttribute(prop, mustInitializeSymbol);
        var attributeList = SyntaxFactory.AttributeList().WithAttributes(SyntaxFactory.SeparatedList(new[] { attribute }));

        return newDecl.WithAttributeLists(newDecl.AttributeLists.Add(attributeList));
    }

    protected override async Task<(SyntaxNode, SyntaxNode)?> CreateChanges(Document document, TypeDeclarationSyntax typeDecl, CancellationToken c)
    {
        var originalTypeDecl = typeDecl;

        var symbol = await document.GetDeclaredSymbol<ITypeSymbol>(typeDecl, c).ConfigureAwait(false);

        var mustInitializeClassMetadata = await document.GetTypeByMetadataName(AttributeType, c).ConfigureAwait(false);
        if (symbol is null || mustInitializeClassMetadata is null) return null;

        var baseTypes = symbol.GetAllBaseTypes().ToArray(); // We assume it's ordered starting with the closest
        if (!baseTypes.Any()) return null;

        var requiredProperties = CannotUseBaseImplementationBase.GetRequiredProperties(symbol, new[] { mustInitializeClassMetadata });
        foreach (var prop in requiredProperties)
        {
            var expr = await GetPropertyDeclaration(prop, baseTypes, mustInitializeClassMetadata).ConfigureAwait(false);

            if (expr is not null) typeDecl = typeDecl.AddMembers(expr);
        }

        return (originalTypeDecl, typeDecl);
    }
}
