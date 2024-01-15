using SequelPay.DotNetPowerExtensions.Analyzers.MustInitialize.CodeFixProviders;
using SequelPay.DotNetPowerExtensions.Analyzers.MustInitialize.MustInitializeAttribute.Analyzers;

namespace SequelPay.DotNetPowerExtensions.Analyzers.MustInitialize.MustInitializeAttribute.CodeFixProviders;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MustInitializeRequiredMembersCodeFixProvider)), Shared]
public class MustInitializeRequiredMembersCodeFixProvider
                    : MustInitializeRequiredMembersCodeFixProviderBase<MustInitializeRequiredMembers, ObjectCreationExpressionSyntax>
{
    protected override string DiagnosticId => MustInitializeRequiredMembers.DiagnosticId;

    protected override Task<(SyntaxNode declToReplace, SyntaxNode newDecl)?> CreateChanges(Document document,
                                                                    ObjectCreationExpressionSyntax declaration, CancellationToken c)
            => GetInitializerChanges(document, declaration, c);
}
