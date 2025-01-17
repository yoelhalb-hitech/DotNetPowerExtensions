﻿using SequelPay.DotNetPowerExtensions.Analyzers.MustInitialize.CodeFixProviders;

namespace SequelPay.DotNetPowerExtensions.Analyzers.MustInitialize.MustInitializeAttribute.CodeFixProviders;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MustInitializeRequiredMembersCodeFixProvider)), Shared]
public class MustInitializeRequiredMembersCodeFixProvider
                    : MustInitializeRequiredMembersCodeFixProviderBase<MustInitializeRequiredMembers, BaseObjectCreationExpressionSyntax>
{
    protected override string DiagnosticId => MustInitializeRequiredMembers.DiagnosticId;

    protected override Task<(SyntaxNode declToReplace, SyntaxNode newDecl)?> CreateChanges(Document document,
                                                                    BaseObjectCreationExpressionSyntax declaration, CancellationToken c)
            => GetInitializerChanges(document, declaration, c);
}
