extern alias Features;
extern alias Workspaces;

using Features::Microsoft.CodeAnalysis.QuickInfo;
using Workspaces::Microsoft.CodeAnalysis.Options;

namespace SequelPay.DotNetPowerExtensions.Analyzers.DependencyManagement.ILocalFactory.Features;

[ExportCompletionProvider(nameof(QuickInfoRegistrationProvider), LanguageNames.CSharp)]
internal class QuickInfoRegistrationProvider : CommonCompletionProvider
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

            var qi = QuickInfoService.GetService(context.Document);
            if (qi is null) return;

            var f = qi.GetType().BaseType.GetField("_providers", BindingFlags.Instance | BindingFlags.NonPublic);
            var providers = (ImmutableArray<QuickInfoProvider>)f.GetValue(qi);
            if (providers.IsDefault)
            {
                await qi.GetQuickInfoAsync(context.Document, 0).ConfigureAwait(false);
                providers = (ImmutableArray<QuickInfoProvider>)f.GetValue(qi);
            }
            if (!providers.Any(p => p.GetType() == typeof(LocalInitializerQuickInfoProvider)))
            {
                f.SetValue(qi, ImmutableArray.Create<QuickInfoProvider>(new LocalInitializerQuickInfoProvider()).AddRange(providers));
            }

            handled = true;
        }
        catch { }
    }
}
