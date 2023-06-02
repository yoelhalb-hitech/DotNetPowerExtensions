extern alias Features;
extern alias Workspaces;

using Features::Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Text;
using System.Reflection;
using Workspaces::Microsoft.CodeAnalysis.Host;
using Workspaces::Microsoft.CodeAnalysis.Options;

namespace DotNetPowerExtensions.Analyzers.DependencyManagement.ILocalFactory.Features;

[ExportCompletionProvider(nameof(MustInitializeInitializerCompletionRegistrationProvider), LanguageNames.CSharp)]
internal class MustInitializeInitializerCompletionRegistrationProvider : CommonCompletionProvider
{
    public override string Language => LanguageNames.CSharp;

    private static bool handled;
    public override bool ShouldTriggerCompletion(LanguageServices languageServices, SourceText text, int caretPosition, CompletionTrigger trigger, CompletionOptions options, OptionSet passThroughOptions)
    {
        return !handled;
    }

    public override async Task ProvideCompletionsAsync(CompletionContext context)
    {
        try
        {
            if (handled) return;

            var cs = CompletionService.GetService(context.Document);
            if (cs is null) return;

            var @private = BindingFlags.Instance | BindingFlags.NonPublic;

            // In the old version it was directly in CompletionServiceWithProviders which was the base of CommonCompletionService
            // Now days it is in CompletionService.ProviderManager and CompletionService is the base
            var completionBase = typeof(CommonCompletionService).BaseType;

            var providerManager = completionBase == typeof(CompletionService)
                    ? typeof(CompletionService).GetField("_providerManager", @private)?.GetValue(cs)
                    : cs;

#pragma warning disable CS0618 // Type or member is obsolete
            var newProvider = new MustInitializeInitializerCompletionProvider();
#pragma warning restore CS0618 // Type or member is obsolete

            var providerManagerType = providerManager?.GetType();
            var nameToProvider = providerManagerType?.GetField("_nameToProvider", @private)?.GetValue(providerManager) as Dictionary<string, CompletionProvider>;

            const string existingName = "Microsoft.CodeAnalysis.CSharp.Completion.Providers.ObjectAndWithInitializerCompletionProvider";
            var existingProvider = nameToProvider?[existingName];

            if (existingProvider is null)
            {
                await cs.GetCompletionsAsync(context.Document, 0, CompletionTrigger.CreateInsertionTrigger(' ')).ConfigureAwait(false);
                existingProvider = nameToProvider?[existingName];
            }

            if (existingProvider is null) return;

            nameToProvider![existingName] = newProvider;

            var rolesToProvider = providerManagerType!.GetField("_rolesToProviders", @private)?.GetValue(providerManager)
                                                            as Dictionary<ImmutableHashSet<string>, ImmutableArray<CompletionProvider>>;

            var providers = rolesToProvider?[ImmutableHashSet<string>.Empty];

            var index = providers?.IndexOf(existingProvider);
            if (index is null || index < 0) return;

            var innerArray = typeof(ImmutableArray<CompletionProvider>).GetField("array", @private)?.GetValue(providers) as Array;
            innerArray?.SetValue(newProvider, index.Value);

            if (providers?[index.Value] == newProvider) handled = true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}
