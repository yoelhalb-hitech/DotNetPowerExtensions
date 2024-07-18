using DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;
using DotNetPowerExtensions.Analyzers.MustInitialize.MustInitializeAttribute.Analyzers;

namespace DotNetPowerExtensions.Analyzers.MustInitialize.CodeFixProviders;

public abstract class RequiredWhenOverridingCodeFixProviderBase<TAnalyzer>
    : ByAttributeCodeFixProviderBase<TAnalyzer, PropertyDeclarationSyntax>
                        where TAnalyzer : MustInitializeRequiredWhenOverriding
{
    protected override Task<(SyntaxNode declToReplace, SyntaxNode newDecl)?> CreateChanges(Document document, PropertyDeclarationSyntax declaration, CancellationToken c)
        => Task.FromResult<(SyntaxNode, SyntaxNode)?>((declaration, declaration.AddAttributeLists(MustInitializeWorker.GetAttributeSyntax())));
}
