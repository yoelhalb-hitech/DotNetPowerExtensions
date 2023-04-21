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
#pragma warning disable CA2201 // Exception type System.Exception is not sufficiently specific
        return interfaceProp.GetAttribute(mustInitializeSymbol)?.ApplicationSyntaxReference?.GetSyntax() as AttributeSyntax ?? throw new Exception("Interface prop not found");
#pragma warning restore CA2201 // Exception type System.Exception is not sufficiently specific
    }

    protected override async Task<(SyntaxNode declToReplace, SyntaxNode newDecl)?> CreateChanges(Document document, PropertyDeclarationSyntax declaration, CancellationToken c)
    {
        var symbol = await document.GetDeclaredSymbol<IPropertySymbol>(declaration, c).ConfigureAwait(false);

        var mustInitializeClassMetadata = await document.GetTypeByMetadataName(AttributeType, c).ConfigureAwait(false);
        if (symbol is null || mustInitializeClassMetadata is null) return null;

        var attributeList = SyntaxFactory.AttributeList()
                                    .WithAttributes(SyntaxFactory.SeparatedList(new[] { GetAttribute(symbol, mustInitializeClassMetadata) }));

        return (declaration, declaration.AddAttributeLists(attributeList));
    }
}
