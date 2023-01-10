using DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;
using DotNetPowerExtensions.Analyzers.MustInitialize.MustInitializeAttribute.Analyzers;

namespace DotNetPowerExtensions.Analyzers.MustInitialize.CodeFixProviders;

public abstract class RequiredWhenOverridingCodeFixProviderBase<TAnalyzer>
    : ByAttributeCodeFixProviderBase<TAnalyzer, PropertyDeclarationSyntax>
                        where TAnalyzer : MustInitializeRequiredWhenOverriding
{
    protected virtual AttributeSyntax GetAttribute(IPropertySymbol prop, INamedTypeSymbol mustInitializeSymbol)
    {
        var baseTypes = prop.ContainingType.GetAllBaseTypes();
        var name = prop.Name;

        foreach (var baseType in baseTypes)
        {
            var member = baseType.GetMembers(name).FirstOrDefault();
            if (member is null) continue;

#pragma warning disable CA2201 // Exception type System.Exception is not sufficiently specific
            return member.GetAttribute(mustInitializeSymbol)?.ApplicationSyntaxReference?.GetSyntax() as AttributeSyntax ?? throw new Exception("Base prop issue");
        }

        throw new Exception("Base prop not found");
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
