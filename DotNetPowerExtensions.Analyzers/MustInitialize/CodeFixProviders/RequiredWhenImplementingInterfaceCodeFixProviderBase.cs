using DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;

namespace DotNetPowerExtensions.Analyzers.MustInitialize.CodeFixProviders;

public abstract class RequiredWhenImplementingInterfaceCodeFixProviderBase<TAnalyzer>
                                    : ByAttributeCodeFixProviderBase<TAnalyzer, PropertyDeclarationSyntax>
                            where TAnalyzer : RequiredWhenImplementingInterfaceBase
{
    protected virtual AttributeSyntax GetAttribute(IPropertySymbol prop, INamedTypeSymbol mustInitializeSymbol)
    {
        var interfaces = prop.ContainingType.AllInterfaces;
        var name = prop.Name;

        var interfaceProp = interfaces.SelectMany(i => i.GetMembers(name)).First(m => m.HasAttribute(mustInitializeSymbol));
        return interfaceProp.GetAttribute(mustInitializeSymbol)?.ApplicationSyntaxReference?.GetSyntax() as AttributeSyntax ?? throw new Exception("Interface prop not found");
    }

    protected override async Task<(SyntaxNode declToReplace, SyntaxNode newDecl)?> CreateChanges(Document document, PropertyDeclarationSyntax propertyDecl, CancellationToken c)
    {
        var symbol = await document.GetDeclaredSymbol<IPropertySymbol>(propertyDecl, c).ConfigureAwait(false);

        var mustInitializeClassMetadata = await document.GetTypeByMetadataName(AttributeType, c).ConfigureAwait(false);
        if (symbol is null || mustInitializeClassMetadata is null) return null;

        var attributeList = SyntaxFactory.AttributeList()
                                    .WithAttributes(SyntaxFactory.SeparatedList(new[] { GetAttribute(symbol, mustInitializeClassMetadata) }));

        return (propertyDecl, propertyDecl.AddAttributeLists(attributeList));
    }
}
