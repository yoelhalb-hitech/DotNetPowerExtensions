extern alias Features;
extern alias Workspaces;

using Features::Microsoft.CodeAnalysis.Completion.Providers;
using Microsoft.CodeAnalysis.CSharp.Extensions;

using SequelPay.DotNetPowerExtensions.Reflection;

namespace SequelPay.DotNetPowerExtensions.Analyzers.DependencyManagement.ILocalFactory.Features;

[ExportCompletionProvider(nameof(LocalInitializerCompletionProvider), LanguageNames.CSharp)]
public class LocalInitializerCompletionProvider : LSPCompletionProvider
{
    public override string Language => LanguageNames.CSharp;

    public override ImmutableHashSet<char> TriggerCharacters
            => Microsoft.CodeAnalysis.CSharp.Completion.Providers.CompletionUtilities.CommonTriggerCharacters.Add(' ');

    [ImportingConstructor]
    [Obsolete(MefConstruction.ImportingConstructorMessage, error: false)]
    public LocalInitializerCompletionProvider()
    {
    }

    public override bool IsInsertionTrigger(SourceText text, int characterPosition, CompletionOptions options)
        => Microsoft.CodeAnalysis.CSharp.Completion.Providers.CompletionUtilities.IsTriggerCharacter(text, characterPosition, options)
                                                            || text[characterPosition] == ' ';

    public override async Task ProvideCompletionsAsync(CompletionContext context)
    {
        try
        {
            // Not using the base implmentation as we still need to include MightRequire and we have to also include members with mustinitialize not in the standard intellisense scope...
            // And on the other hand we don't want to give intellisense for all properties in scope as it might confuse the user

            var document = context.Document;
            var position = context.Position;
            var cancellationToken = context.CancellationToken;

            var semanticModel = await document.ReuseExistingSpeculativeModelAsync(position, cancellationToken).ConfigureAwait(false);

            if (GetInitializedType(document, semanticModel, position, cancellationToken) is not var (type, initializerLocation)) return;
            if (type is not INamedTypeSymbol initializedType) return;

            context.IsExclusive = true;

            var enclosing = semanticModel.GetEnclosingNamedType(position, cancellationToken);
            if (enclosing is null) return;

            var worker = new MustInitializeWorker(semanticModel);
            var requiredTo = worker.GetRequiredToInitialize(type, null, context.CancellationToken);

            // Filter out those members that have already been typed
            var alreadyTypedMembers = GetInitializedMembers(semanticModel.SyntaxTree, position, cancellationToken);

            var uninitialized = requiredTo.Where(m => !alreadyTypedMembers.Contains(m.name));
            foreach (var uninitializedMember in uninitialized)
            {
                // We don't invoke it directly because different versions of Roslyn have different signatures
                var item = typeof(SymbolCompletionItem).InvokeMethod(
                    nameof(SymbolCompletionItem.CreateWithSymbolId), null,
                    new Dictionary<string, object?>
                    {
                        ["displayText"] = uninitializedMember.name.EscapeIdentifier(),
                        ["displayTextSuffix"] = "",
                        ["insertionText"] = null,
                        ["symbols"] = ImmutableArray.Create(uninitializedMember.symbol),
                        ["contextPosition"] = initializerLocation.SourceSpan.Start,
                        ["inlineDescription"] = "MustInitialize",
                        ["rules"] = CompletionItemRules.Create(enterKeyRule: EnterKeyRule.Never)
                    }) as CompletionItem;
                context.AddItem(item!);
            }
        }
        catch { }
    }

    protected HashSet<string> GetInitializedMembers(SyntaxTree tree, int position, CancellationToken cancellationToken)
    {
        var token = tree.FindTokenOnLeftOfPosition(position, cancellationToken)
                        .GetPreviousTokenIfTouchingWord(position);

        // We should have gotten back a { or ,
        if (!(token.Kind() is SyntaxKind.CommaToken or SyntaxKind.OpenBraceToken) || token.Parent is not AnonymousObjectCreationExpressionSyntax creation)
            return new HashSet<string>();

        return new HashSet<string>(creation.Initializers.Select(i => i.GetName()).OfType<string>());
    }

    protected (ITypeSymbol, Location)? GetInitializedType(Document document, SemanticModel semanticModel, int position, CancellationToken cancellationToken)
    {
        var token = FeatureUtils.GetToken(semanticModel, position, cancellationToken);
        if (token?.Kind() is not SyntaxKind.CommaToken and not SyntaxKind.OpenBraceToken
               || token?.Parent is not AnonymousObjectCreationExpressionSyntax creation) return null;

        var result = FeatureUtils.GetInitializedType(semanticModel, creation, cancellationToken);
        return result is not null ? (result, token.Value.GetLocation()) : null;
    }
}