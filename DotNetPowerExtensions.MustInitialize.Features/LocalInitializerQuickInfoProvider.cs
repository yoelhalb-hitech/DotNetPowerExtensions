extern alias Features;
extern alias Workspaces;

using Features::Microsoft.CodeAnalysis;
using Features::Microsoft.CodeAnalysis.LanguageService;
using Features::Microsoft.CodeAnalysis.QuickInfo;
using SequelPay.DotNetPowerExtensions.Reflection;
using Workspaces::System.Diagnostics.CodeAnalysis;
using static Features::Microsoft.CodeAnalysis.QuickInfo.CommonSemanticQuickInfoProvider;

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
            var (symbol, mightRquire) = FeatureUtils.BindTokenToDeclaringSymbol(semanticModel, token, cancellationToken);
            if (symbol is null && mightRquire is null) return null;

            if (symbol is not null)
            {
                // We don't invoke it directly because different versions of Roslyn have different signatures
                var invoked = this.GetType().InvokeMethod(nameof(CreateContentAsync), null,
                        new Dictionary<string, object?>
                        {
                            ["services"] = services,
                            ["semanticModel"] = semanticModel,
                            ["token"] = token,
                            ["tokenInformation"] = new TokenInformation(ImmutableArray.Create(symbol)),
                            ["options"] = options,
                            ["cancellationToken"] = cancellationToken
                        }) as Task<QuickInfoItem?>;
                return await invoked!.ConfigureAwait(false);
            }

            var text = $"{(mightRquire!.Type)} MightRequire<{mightRquire.ContainingSymbol.Name}>.{mightRquire.Name}";
            var info = QuickInfoItem.Create(
                token.Span,
                ImmutableArray.Create("MightRequire"),
                ImmutableArray.Create(
                    QuickInfoSection.Create(
                        QuickInfoSectionKinds.Description, ImmutableArray.Create(new TaggedText(QuickInfoSectionKinds.Description, text)) // result[SymbolDescriptionGroups.MainDescription]
                    )
                )
            );
            return info;
        }
        catch
        {
            return null;
        }
    }

    private string ToName(INamedTypeSymbol? namedType)
    => namedType?.SpecialType switch
    {
        SpecialType.System_String => "string",
        SpecialType.System_Boolean => "bool",
        SpecialType.System_Int32 => "int",
        SpecialType.System_UInt32 => "uint",
        SpecialType.System_Char => "char",
        SpecialType.System_Single => "float",
        SpecialType.System_Double => "double",
        SpecialType.System_Decimal => "decimal",
        SpecialType.System_Byte => "byte",
        SpecialType.System_SByte => "sbyte",
        SpecialType.System_Int16 => "short",
        SpecialType.System_UInt16 => "ushort",
        SpecialType.System_Int64 => "long",
        SpecialType.System_UInt64 => "ulong",
        SpecialType.System_Object => "object",
        SpecialType.System_IntPtr => "nint",
        SpecialType.System_UIntPtr => "nuint",
        SpecialType.System_ValueType => "(" + string.Join(",", namedType.GetAllTypeArguments().Select(t => ToName(t as INamedTypeSymbol))) + ")",
        SpecialType.System_Void => "void",
        SpecialType.System_Array => ToName(namedType.GetAllTypeArguments().First() as INamedTypeSymbol) + "[]",
        SpecialType.System_Nullable_T => ToName(namedType.GetAllTypeArguments().First() as INamedTypeSymbol) + "?",
        { } => namedType.Name,
        _ => "",
};


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
