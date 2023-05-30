﻿extern alias Features;
extern alias Workspaces;
using Features::Microsoft.CodeAnalysis.QuickInfo;
using Workspaces::Microsoft.CodeAnalysis.Host;
using Workspaces::Microsoft.CodeAnalysis.Shared.Extensions;
using Workspaces::System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Features::Microsoft.CodeAnalysis.LanguageService;

namespace DotNetPowerExtensions.Analyzers.DependencyManagement.ILocalFactory.CompletionProviders;

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
        if (!context.Document.Project.AnalyzerReferences.Any(a => a.Display == nameof(DotNetPowerExtensions) + "." + nameof(DotNetPowerExtensions.Analyzers)))
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
                    //context.GetType().GetProperty("Options").GetValue(context), // The runtime is getting crazy on the type...
                    context.Options,
                cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
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
        catch(Exception ex)
        {
            Logger.LogError(ex);
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
