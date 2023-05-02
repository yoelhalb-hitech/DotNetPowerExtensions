using DotNetPowerExtensions.Analyzers.MustInitialize.MightRequireAttribute;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;
using SequelPay.DotNetPowerExtensions;
using System.Collections.Immutable;
using System.Reflection;

namespace DotNetPowerExtensions.Analyzers.DependencyManagement.ILocalFactory.CompletionProviders;

// TODO... How do we test this?? Roslyn itslef doesn't have simple testing
// TODO... Maybe we should also provide Quick Info
[ExportCompletionProvider(nameof(LocalInitializerCompletionProvider), LanguageNames.CSharp)]
internal class LocalInitializerCompletionProvider : CompletionProvider
{
    private CompletionProvider provider;

    public LocalInitializerCompletionProvider()
    {
        var type = Reflect.CSharpFeaturesAssemblyDetails.Assembly
                .GetType("Microsoft.CodeAnalysis.CSharp.Completion.Providers.ObjectAndWithInitializerCompletionProvider");

        provider = (CompletionProvider)Activator.CreateInstance(type);
    }

    public override bool ShouldTriggerCompletion(SourceText text, int caretPosition, CompletionTrigger trigger, OptionSet options)
                        => provider.ShouldTriggerCompletion(text, caretPosition, trigger, options);

    public override Task<CompletionDescription?> GetDescriptionAsync(Document document, CompletionItem item, CancellationToken cancellationToken)
                        => provider.GetDescriptionAsync(document, item, cancellationToken);

    public override async Task ProvideCompletionsAsync(CompletionContext context)
    {
        try
        {
            var document = context.Document;
            var position = context.Position;
            var cancellationToken = context.CancellationToken;

            var semanticModel = await (Reflect.ReuseExistingSpeculativeModelAsync(document, position, cancellationToken)).ConfigureAwait(false);

            if (GetInitializedType(document, semanticModel, position, cancellationToken) is not var (type, initializerLocation))
            {
                return;
            }

            if (type is ITypeParameterSymbol typeParameterSymbol)
            {
                type = Reflect.GetNamedTypeSymbolConstraint(typeParameterSymbol);
            }

            if (type is not INamedTypeSymbol initializedType) { return; }

            context.IsExclusive = true;

            var enclosing = Reflect.GetEnclosingNamedType(semanticModel, position, cancellationToken);
            //Contract.ThrowIfNull(enclosing);

            // Find the members that can be initialized. If we have a NamedTypeSymbol, also get the overridden members.
            var members = semanticModel.LookupSymbols(position, initializedType).ToArray();

            Func<ISymbol, bool> isInitializable = member => (bool)provider.GetType()
                                                                    .GetMethod("IsInitializable", BindingFlags.Instance | BindingFlags.NonPublic)
                                                                    .Invoke(provider, new[] { member, enclosing });

            Func<ISymbol, bool> isLegalFieldOrProperty = member => (bool)provider.GetType().BaseType
                                                                             .GetMethod("IsLegalFieldOrProperty", BindingFlags.Static | BindingFlags.NonPublic)
                                                                             .Invoke(null, new[] { member });

            var mightRequireSymbols = await MightRequireUtils.GetMightRequireSymbols(document).ConfigureAwait(false);
            var mightRequireMembers = MightRequireUtils.GetMightRequiredInfos(type, mightRequireSymbols)
                                                            .Where(m => !members.Any(me => me.Name == m.Name))
                                                            .ToArray();

            members = members.Where(m => isInitializable(m) &&
                                         m.CanBeReferencedByName &&
                                         isLegalFieldOrProperty(m) &&
                                         !m.IsImplicitlyDeclared).ToArray();

            // Filter out those members that have already been typed
            var alreadyTypedMembers = (HashSet<string>)provider.GetType()
                                                            .GetMethod("GetInitializedMembers", BindingFlags.Instance | BindingFlags.NonPublic)
                                                            .Invoke(provider, new object[] { semanticModel.SyntaxTree, position, cancellationToken });
            var uninitializedMembers = members.Where(m => !alreadyTypedMembers.Contains(m.Name));

            // uninitializedMembers = uninitializedMembers.Where(m => m.IsEditorBrowsable(context.CompletionOptions.HideAdvancedMembers, semanticModel.Compilation));

            foreach (var uninitializedMember in uninitializedMembers)
            {
                context.AddItem(Reflect.CreateWithSymbolId(
                    displayText: Reflect.EscapeIdentifier(uninitializedMember.Name),
                    displayTextSuffix: "",
                    insertionText: null,
                    symbols: ImmutableArray.Create(uninitializedMember),
                    contextPosition: initializerLocation.SourceSpan.Start,
                    rules: CompletionItemRules.Create(enterKeyRule: EnterKeyRule.Never)));
            }

            var uninitialized = mightRequireMembers.Where(m => !alreadyTypedMembers.Contains(m.Name));
            foreach (var uninitializedMember in uninitialized)
            {
                context.AddItem(Reflect.CreateWithSymbolId(
                    displayText: Reflect.EscapeIdentifier(uninitializedMember.Name),
                    displayTextSuffix: "",
                    insertionText: null,
                    symbols: ImmutableArray<ISymbol>.Empty,
                    contextPosition: initializerLocation.SourceSpan.Start,
                    rules: CompletionItemRules.Create(enterKeyRule: EnterKeyRule.Never)));
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
            return;
        }
    }

    private Tuple<ITypeSymbol, Location>? GetInitializedType(Document document, SemanticModel semanticModel, int position, CancellationToken cancellationToken)
    {
        var tree = semanticModel.SyntaxTree;


        if (Reflect.IsInNonUserCode(tree, position, cancellationToken)) return null;

        var token = Reflect.FindTokenOnLeftOfPosition(tree, position, cancellationToken);
        token = Reflect.GetPreviousTokenIfTouchingWord(token, position);

        if (token.Kind() is not SyntaxKind.CommaToken and not SyntaxKind.OpenBraceToken
            || token.Parent is not AnonymousObjectCreationExpressionSyntax
            || token.Parent.Parent?.Parent?.Parent is not InvocationExpressionSyntax invocation
            || invocation is null
            || semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol is not IMethodSymbol methodSymbol
            || methodSymbol.ReceiverType is not INamedTypeSymbol classType
            || !classType.IsGenericType
            || methodSymbol.Name != nameof(ILocalFactory<object>.Create))
            return null;

        var innerClass = classType.TypeArguments.First();

        return Tuple.Create(innerClass, token.GetLocation());
    }

    public override Task<CompletionChange> GetChangeAsync(Document document, CompletionItem item, char? commitKey, CancellationToken cancellationToken)
            => provider.GetChangeAsync(document, item, commitKey, cancellationToken);

    private class Reflect
    {
        public class AssemblyDetails
        {
            private string assemblyName;
            private Assembly? assembly;
            public Assembly Assembly => assembly
                ?? (assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == assemblyName))
                ?? Assembly.Load(GetFullName(assemblyName));

            public AssemblyDetails(string assemblyName)
            {
                this.assemblyName = assemblyName;
            }
        }

        // TODO... maybe add in DotNetPowerExtensions a more simple solution similar to Refit or PInvoke
        private static string? featuresAssemblyFullName;
        private static string FeaturesAssemblyFullName => featuresAssemblyFullName ?? (featuresAssemblyFullName = typeof(CompletionContext).Assembly.FullName);
        private static string GetFullName(string name) => FeaturesAssemblyFullName.Replace("Microsoft.CodeAnalysis.Features", name);

        public static AssemblyDetails WorkspaceAssemblyDetails = new AssemblyDetails("Microsoft.CodeAnalysis.Workspaces");
        public static AssemblyDetails CSharpFeaturesAssemblyDetails = new AssemblyDetails("Microsoft.CodeAnalysis.CSharp.Features");
        public static AssemblyDetails CSharpWorkspacesAssemblyDetails = new AssemblyDetails("Microsoft.CodeAnalysis.CSharp.Workspaces");

        private static MethodInfo GetMethod(Type type, string name)
            => type.GetMethod(name, BindingFlags.Public | BindingFlags.Static);

        private static MethodInfo GetMethod(Type type, string name, params Type[] types)
            => type.GetMethod(name, BindingFlags.Public | BindingFlags.Static, null, types, null);

        public static async ValueTask<SemanticModel> ReuseExistingSpeculativeModelAsync(Document document, int position, CancellationToken cancellationToken)
        {
            var type = WorkspaceAssemblyDetails.Assembly.GetType("Microsoft.CodeAnalysis.Shared.Extensions.DocumentExtensions");

            var method = GetMethod(type, "ReuseExistingSpeculativeModelAsync", typeof(Document), typeof(int), typeof(CancellationToken));

            return await ((ValueTask<SemanticModel>)method.Invoke(null, new object[] { document, position, cancellationToken })).ConfigureAwait(false);
        }
        public static bool IsInNonUserCode(SyntaxTree tree, int position, CancellationToken cancellationToken)
        {
            var type = CSharpWorkspacesAssemblyDetails.Assembly
                    .GetType("Microsoft.CodeAnalysis.CSharp.Extensions.SyntaxTreeExtensions");

            var method = GetMethod(type, "IsInNonUserCode");
            return (bool)method.Invoke(null, new object[] { tree, position, cancellationToken });
        }

        public static SyntaxToken FindTokenOnLeftOfPosition(SyntaxTree syntaxTree, int position, CancellationToken cancellationToken)
        {
            var type = WorkspaceAssemblyDetails.Assembly
                                .GetType("Microsoft.CodeAnalysis.Shared.Extensions.SyntaxTreeExtensions");

            var method = GetMethod(type, "FindTokenOnLeftOfPosition");
            return (SyntaxToken)method.Invoke(null, new object[] { syntaxTree, position, cancellationToken, true, false, false });
        }

        public static SyntaxToken GetPreviousTokenIfTouchingWord(SyntaxToken token, int position)
        {
            var type = CSharpWorkspacesAssemblyDetails.Assembly
                .GetType("Microsoft.CodeAnalysis.CSharp.Extensions.SyntaxTokenExtensions");
            var method = GetMethod(type, "GetPreviousTokenIfTouchingWord");
            return (SyntaxToken)method.Invoke(null, new object[] { token, position });
        }

        public static INamedTypeSymbol? GetEnclosingNamedType(SemanticModel semanticModel, int position, CancellationToken cancellationToken)
        {
            var type = WorkspaceAssemblyDetails.Assembly
                                .GetType("Microsoft.CodeAnalysis.Shared.Extensions.SemanticModelExtensions");

            var method = GetMethod(type, "GetEnclosingNamedType");
            return (INamedTypeSymbol?)method.Invoke(null, new object[] { semanticModel, position, cancellationToken });
        }

        public static INamedTypeSymbol? GetNamedTypeSymbolConstraint(ITypeParameterSymbol typeParameter)
        {
            var type = WorkspaceAssemblyDetails.Assembly
                                .GetType("Microsoft.CodeAnalysis.Shared.Extensions.ITypeParameterSymbolExtensions");

            var method = GetMethod(type, "GetNamedTypeSymbolConstraint", typeof(ITypeSymbol));
            return (INamedTypeSymbol?)method.Invoke(null, new object[] { typeParameter });
        }

        public static CompletionItem CreateWithSymbolId(string displayText, string displayTextSuffix, string? insertionText, ImmutableArray<ISymbol> symbols, int contextPosition, object rules)
        {
            var type = typeof(CompletionContext).Assembly.GetType("Microsoft.CodeAnalysis.Completion.Providers.SymbolCompletionItem");

            var method = GetMethod(type, "CreateWithSymbolId",
        typeof(string), typeof(string), typeof(IReadOnlyList<ISymbol>),
                typeof(CompletionItemRules), typeof(int), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string),
                typeof(CompletionContext).Assembly.GetType("Microsoft.CodeAnalysis.Glyph"),
                typeof(CompletionContext).Assembly.GetType("Microsoft.CodeAnalysis.Shared.Utilities.SupportedPlatformData"),
                typeof(ImmutableDictionary<string, string>), typeof(ImmutableArray<string>), typeof(bool));

            return (CompletionItem)method.Invoke(null, new object?[] {
                    displayText, displayTextSuffix, symbols, rules, contextPosition,
                    null, insertionText, null, null, null, null, null, null, default, false
                });
        }

        public static string EscapeIdentifier(string identifier)
        {
            var type = CSharpWorkspacesAssemblyDetails.Assembly
                                .GetType("Microsoft.CodeAnalysis.CSharp.Extensions.StringExtensions");

            var method = GetMethod(type, "EscapeIdentifier");

            return (string)method.Invoke(null, new object?[] { identifier, false });
        }
    }
}