extern alias Features;
extern alias Workspaces;

using Features::Microsoft.CodeAnalysis;
using Features::Microsoft.CodeAnalysis.Completion.Providers;
using Microsoft.CodeAnalysis.CSharp.Completion.Providers;
using Microsoft.CodeAnalysis.CSharp.Extensions;
using Microsoft.CodeAnalysis.Operations;
using System.Diagnostics;
using Workspaces::Roslyn.Utilities;

namespace SequelPay.DotNetPowerExtensions.Analyzers.DependencyManagement.ILocalFactory.Features;

[ExportCompletionProvider(nameof(MustInitializeInitializerCompletionProvider), LanguageNames.CSharp)]
internal class MustInitializeInitializerCompletionProvider : ObjectAndWithInitializerCompletionProvider
{
    [ImportingConstructor]
    [Obsolete(MefConstruction.ImportingConstructorMessage, error: false)]
    public MustInitializeInitializerCompletionProvider()
    {
    }

    public override async Task ProvideCompletionsAsync(CompletionContext context)
    {
        try
        {
            // Not using the base implmentation as we want to add the "MustInitialize" description

            var document = context.Document;
            var position = context.Position;
            var cancellationToken = context.CancellationToken;

            var semanticModel = await document.ReuseExistingSpeculativeModelAsync(position, cancellationToken).ConfigureAwait(false);

            if (GetInitializedType(document, semanticModel, position, cancellationToken) is not var (type, initializerLocation)) return;

            if (type is ITypeParameterSymbol typeParameterSymbol) type = typeParameterSymbol.GetNamedTypeSymbolConstraint();
            if (type is not INamedTypeSymbol initializedType) return;

            if (await IsExclusiveAsync(document, position, cancellationToken).ConfigureAwait(false)) context.IsExclusive = true;

            var enclosing = semanticModel.GetEnclosingNamedType(position, cancellationToken);
            if (enclosing is null) return;

            var worker = new MustInitializeWorker(semanticModel);
            var ctor = GetCtor(semanticModel, position, cancellationToken);
            var requiredTo = worker.GetMustInitialize(initializedType, ctor, out _, cancellationToken).Select(m => m.As<ISymbol>()!.Name).ToList();

            // Find the members that can be initialized. If we have a NamedTypeSymbol, also get the overridden members.
            IEnumerable<ISymbol> members = semanticModel.LookupSymbols(position, initializedType);
            members = members.Where(m => IsInitializable(m, enclosing) &&
                                         m.CanBeReferencedByName &&
                                         IsLegalFieldOrProperty(m) &&
                                         !m.IsImplicitlyDeclared);


            // Filter out those members that have already been typed
            var alreadyTypedMembers = GetInitializedMembers(semanticModel.SyntaxTree, position, cancellationToken);
            var uninitializedMembers = members.Where(m => !alreadyTypedMembers.Contains(m.Name));

            uninitializedMembers = uninitializedMembers.Where(m => requiredTo.Contains(m.Name)
                                            || m.IsEditorBrowsable(context.CompletionOptions.HideAdvancedMembers, semanticModel.Compilation))
                                        .OrderByDescending(m => m.IsRequired() || requiredTo.Contains(m.Name)) // Remember that false is before true...
                                        .ThenBy(m => m.Name);

            var firstUnitializedRequiredMember = false;

            foreach (var uninitializedMember in uninitializedMembers)
            {
                var rules = s_rules;

                // We'll hard select the first required member to make it a bit easier to type out an object initializer
                // with a bunch of members.
                if (firstUnitializedRequiredMember && uninitializedMember.IsRequired())
                {
                    rules = rules.WithSelectionBehavior(CompletionItemSelectionBehavior.HardSelection).WithMatchPriority(MatchPriority.Preselect);
                    firstUnitializedRequiredMember = false;
                }

                context.AddItem(SymbolCompletionItem.CreateWithSymbolId(
                    displayText: EscapeIdentifier(uninitializedMember),
                    displayTextSuffix: "",
                    insertionText: null,
                    sortText: (uninitializedMember.IsRequired() || requiredTo.Contains(uninitializedMember.Name) ? "A" : "z") + EscapeIdentifier(uninitializedMember),
                    symbols: ImmutableArray.Create(uninitializedMember),
                    contextPosition: initializerLocation.SourceSpan.Start,
                    inlineDescription: uninitializedMember.IsRequired() ? FeaturesResources.Required :
                                                requiredTo.Contains(uninitializedMember.Name) ? "MustInitialize": null,
                    rules: rules));
            }
        }
        catch { }
    }

    private static readonly CompletionItemRules s_rules = CompletionItemRules.Create(enterKeyRule: EnterKeyRule.Never);

    private static bool IsLegalFieldOrProperty(ISymbol symbol)
    {
        return symbol.IsWriteableFieldOrProperty()
            || symbol.ContainingType.IsAnonymousType
            || CanSupportObjectInitializer(symbol);
    }

    private static bool CanSupportObjectInitializer(ISymbol symbol)
    {
        Debug.Assert(!symbol.IsWriteableFieldOrProperty(), "Assertion failed - expected writable field/property check before calling this method.");

        if (symbol is IFieldSymbol fieldSymbol) return !fieldSymbol.Type.IsStructType();
        else if (symbol is IPropertySymbol propertySymbol) return !propertySymbol.Type.IsStructType();

        throw ExceptionUtilities.Unreachable;
    }

    private IMethodSymbol? GetCtor(SemanticModel semanticModel, int position, CancellationToken cancellationToken)
    {
        var token = semanticModel.SyntaxTree.FindTokenOnLeftOfPosition(position, cancellationToken)
                           .GetPreviousTokenIfTouchingWord(position);

        if (!(token.Kind() is SyntaxKind.CommaToken or SyntaxKind.OpenBraceToken)
            || token.Parent is not InitializerExpressionSyntax initializer
            || initializer.Parent is not ObjectCreationExpressionSyntax objectCreation) return null;

        return (semanticModel.GetOperation(objectCreation, cancellationToken) as IObjectCreationOperation)?.Constructor;
    }
}
