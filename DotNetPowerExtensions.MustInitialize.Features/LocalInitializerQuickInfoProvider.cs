extern alias Features;
extern alias Workspaces;

using Features::Microsoft.CodeAnalysis.LanguageService;
using Features::Microsoft.CodeAnalysis.QuickInfo;
using Workspaces::System.Diagnostics.CodeAnalysis;

namespace SequelPay.DotNetPowerExtensions.Analyzers.DependencyManagement.ILocalFactory.Features;

[ExportQuickInfoProvider("LocalInitializer", LanguageNames.CSharp), Shared]
[ExtensionOrder(Before = QuickInfoProviderNames.Semantic)]
[SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035:Do not use APIs banned for analyzers")]

public class LocalInitializerQuickInfoProvider : CommonSemanticQuickInfoProvider
{
    [ImportingConstructor]
    //[Obsolete(MefConstruction.ImportingConstructorMessage, error: false)]
    public LocalInitializerQuickInfoProvider()
    {
    }

    public override Task<QuickInfoItem?> GetQuickInfoAsync(QuickInfoContext context)
    {
        var assmeblyName = Assembly.GetExecutingAssembly().GetName().Name;
        if (!context.Document.Project.AnalyzerReferences.Any(a => a.Display == assmeblyName))
            return Task.FromResult<QuickInfoItem?>(null);

        return base.GetQuickInfoAsync(context);
    }

    protected override async Task<QuickInfoItem?> BuildQuickInfoAsync(QuickInfoContext context, SyntaxToken token)
    {
        try
        {
            var cancellationToken = context.CancellationToken;
            var semanticModel = await context.Document.GetRequiredSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var services = context.Document.Project.Solution.Services;

            // TODO... do we need to handle linked documents?

            return await BuildQuickInfoInternalAsync(services, semanticModel, context.Position, token,
                    context.Options,
                cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            return null;
        }
    }

    protected override Task<QuickInfoItem?> BuildQuickInfoAsync(CommonQuickInfoContext context, SyntaxToken token)
        => BuildQuickInfoInternalAsync(context.Services, context.SemanticModel, context.Position, token, context.Options, context.CancellationToken);


    private async Task<QuickInfoItem?> BuildQuickInfoInternalAsync(SolutionServices services, SemanticModel semanticModel,
            int position, SyntaxToken token, SymbolDescriptionOptions options,
            CancellationToken cancellationToken)
    {
        try
        {
            var symbol = FeatureUtils.BindTokenToDeclaringSymbol(semanticModel, token, cancellationToken);
            if (symbol is null) return null;

            var tokenInfo = new TokenInformation(ImmutableArray.Create(symbol));
            return await CreateContentAsync(services, semanticModel, token, tokenInfo, null, options, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            return null;
        }
    }

    protected override bool GetBindableNodeForTokenIndicatingLambda(SyntaxToken token, [NotNullWhen(true)] out SyntaxNode? found)
    {
        found = null;
        return false;
    }

    protected override bool GetBindableNodeForTokenIndicatingPossibleIndexerAccess(SyntaxToken token, [NotNullWhen(true)] out SyntaxNode? found)
    {
        found = null;
        return false;
    }

    protected override bool GetBindableNodeForTokenIndicatingMemberAccess(SyntaxToken token, out SyntaxToken found)
    {
        found = default;
        return false;
    }
}
