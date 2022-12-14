using DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;
using DotNetPowerExtensions.Analyzers.MustInitialize.MustInitializeAttribute.Analyzers;

namespace DotNetPowerExtensions.Analyzers.MustInitialize.CodeFixProviders;

public abstract class RequiredWhenOverridingCodeFixProviderBase<TAnalyzer>
    : ByAttributeCodeFixProviderBase<TAnalyzer, PropertyDeclarationSyntax>
                        where TAnalyzer : MustInitializeRequiredWhenOverriding, IMustInitializeAnalyzer
{
    protected virtual AttributeSyntax GetAttribute(IPropertySymbol prop, INamedTypeSymbol mustInitializeSymbol)
    {
        var baseTypes = prop.ContainingType.GetAllBaseTypes();
        var name = prop.Name;

        foreach (var baseType in baseTypes)
        {
            var member = baseType.GetMembers(name).FirstOrDefault();
            if (member is null) continue;

            return member.GetAttribute(mustInitializeSymbol)?.ApplicationSyntaxReference?.GetSyntax() as AttributeSyntax ?? throw new Exception("Base prop issue");
        }

        throw new Exception("Base prop not found");
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
