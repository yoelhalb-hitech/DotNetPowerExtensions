using DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;

namespace DotNetPowerExtensions.Analyzers.MustInitialize.CodeFixProviders;

public abstract class CannotUseBaseImplementationCodeFixProviderBase<TAnalyzer>
            : ByAttributeCodeFixProviderBase<TAnalyzer, TypeDeclarationSyntax> where TAnalyzer : CannotUseBaseImplementationBase
{
    protected override string Title => "Implement Required Properties";

    protected virtual AttributeSyntax GetAttribute(IPropertySymbol prop, INamedTypeSymbol mustInitializeSymbol)
    {
        var interfaces = prop.ContainingType.AllInterfaces.Except(prop.ContainingType.BaseType!.AllInterfaces);
        var name = prop.Name;

        var interfaceProp = interfaces.SelectMany(i => i.GetMembers(name)).First(m => m.HasAttribute(mustInitializeSymbol));

#pragma warning disable CA2201 // Exception type System.Exception is not sufficiently specific
        return interfaceProp.GetAttribute(mustInitializeSymbol)?.ApplicationSyntaxReference?.GetSyntax() as AttributeSyntax
                                                                                    ?? throw new Exception("Interface prop not found");
#pragma warning restore CA2201 // Exception type System.Exception is not sufficiently specific

    }

    protected virtual async Task<PropertyDeclarationSyntax?> GetPropertyDeclaration(IPropertySymbol prop, ITypeSymbol[] baseTypes,
                                                                                                        INamedTypeSymbol mustInitializeSymbol)
    {
        var baseProperty = baseTypes.First(t => t.GetMembers(prop.Name).Any()).GetMembers(prop.Name).OfType<IPropertySymbol>().First();
        var baseSyntax = await baseProperty.DeclaringSyntaxReferences.First().GetSyntaxAsync().ConfigureAwait(false) as PropertyDeclarationSyntax;
        if (baseSyntax is null)
        {
#pragma warning disable CA2201 // Exception type System.Exception is not sufficiently specific
            Logger.LogError(new Exception("BaseSyntax is null"));
#pragma warning restore CA2201 // Exception type System.Exception is not sufficiently specific
            return null;
        }

        var newDecl = PropertyOverrideFactory.CreatePropertyOverride(baseSyntax);

        var attribute = GetAttribute(prop, mustInitializeSymbol);
        var attributeList = SyntaxFactory.AttributeList().WithAttributes(SyntaxFactory.SeparatedList(new[] { attribute }));

        return newDecl.WithAttributeLists(newDecl.AttributeLists.Add(attributeList));
    }

    protected override async Task<(SyntaxNode, SyntaxNode)?> CreateChanges(Document document, TypeDeclarationSyntax declaration, CancellationToken c)
    {
        var originalTypeDecl = declaration;

        var symbol = await document.GetDeclaredSymbol<ITypeSymbol>(declaration, c).ConfigureAwait(false);

        var mustInitializeClassMetadata = await document.GetTypeByMetadataName(AttributeType, c).ConfigureAwait(false);
        if (symbol is null || mustInitializeClassMetadata is null) return null;

        var baseTypes = symbol.GetAllBaseTypes().ToArray(); // We assume it's ordered starting with the closest
        if (!baseTypes.Any()) return null;

        var requiredProperties = CannotUseBaseImplementationBase.GetRequiredProperties(symbol, new[] { mustInitializeClassMetadata });
        foreach (var prop in requiredProperties)
        {
            var expr = await GetPropertyDeclaration(prop, baseTypes, mustInitializeClassMetadata).ConfigureAwait(false);

#if NETSTANDARD2_0_OR_GREATER
            if (expr is not null) declaration = declaration.AddMembers(expr);
#else
            if (expr is not null) declaration =
                    SyntaxFactory.ClassDeclaration(declaration.AttributeLists, declaration.Modifiers, declaration.Identifier,
                    declaration.TypeParameterList, declaration.BaseList, declaration.ConstraintClauses, declaration.Members.Add(expr));
#endif
        }

        return (originalTypeDecl, declaration);
    }
}
