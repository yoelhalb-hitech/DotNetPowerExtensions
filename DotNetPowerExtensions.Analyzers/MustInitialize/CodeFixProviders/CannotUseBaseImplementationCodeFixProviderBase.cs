using DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;

namespace DotNetPowerExtensions.Analyzers.MustInitialize.CodeFixProviders;

public abstract class CannotUseBaseImplementationCodeFixProviderBase<TAnalyzer>
            : ByAttributeCodeFixProviderBase<TAnalyzer, TypeDeclarationSyntax> where TAnalyzer : CannotUseBaseImplementationBase
{
    protected override string Title => "Implement Required Properties";

    protected virtual PropertyDeclarationSyntax? GetPropertyDeclaration(IPropertySymbol prop, ITypeSymbol[] baseTypes)
    {
        var baseProperty = baseTypes.First(t => t.GetMembers(prop.Name).Any()).GetMembers(prop.Name).OfType<IPropertySymbol>().FirstOrDefault();

        var baseSyntax = baseProperty?.GetSyntax<PropertyDeclarationSyntax>().FirstOrDefault();
        if (baseSyntax is null) return null;

        return SyntaxFactoryExtensions.CreatePropertyOverride(baseSyntax)
                            .AddAttributeLists(MustInitializeWorker.GetAttributeSyntax());
    }

    protected override async Task<(SyntaxNode, SyntaxNode)?> CreateChanges(Document document, TypeDeclarationSyntax declaration, CancellationToken c)
    {
        var originalTypeDecl = declaration;

        var symbol = await document.GetDeclaredSymbolAsync<ITypeSymbol>(declaration, c).ConfigureAwait(false);

        var mustInitializeClassMetadata = await document.GetTypeSymbolsAsync(new[] { AttributeType }, c).ConfigureAwait(false);
        if (symbol is null || mustInitializeClassMetadata is null) return null;

        var baseTypes = symbol.GetAllBaseTypes().ToArray(); // We assume it's ordered starting with the closest
        if (!baseTypes.Any()) return null;

        var requiredProperties = CannotUseBaseImplementationBase.GetRequiredProperties(symbol, mustInitializeClassMetadata);
        foreach (var prop in requiredProperties)
        {
            var expr = GetPropertyDeclaration(prop, baseTypes);

            if (expr is not null) declaration = declaration.AddMembers(expr);
        }

        return (originalTypeDecl, declaration);
    }
}
