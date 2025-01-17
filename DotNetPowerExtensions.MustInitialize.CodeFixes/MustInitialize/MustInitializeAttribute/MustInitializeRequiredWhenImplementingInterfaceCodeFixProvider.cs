﻿using SequelPay.DotNetPowerExtensions.Analyzers.MustInitialize.CodeFixProviders;

namespace SequelPay.DotNetPowerExtensions.Analyzers.MustInitialize.MustInitializeAttribute.CodeFixProviders;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MustInitializeRequiredWhenImplementingInterfaceCodeFixProvider)), Shared]
public class MustInitializeRequiredWhenImplementingInterfaceCodeFixProvider
                        : RequiredWhenImplementingInterfaceCodeFixProviderBase<MustInitializeRequiredWhenImplementingInterface>
{
    protected override string Title => "Add MustInitialize";

    protected override Type AttributeType => typeof(SequelPay.DotNetPowerExtensions.MustInitializeAttribute);

    protected override string DiagnosticId => MustInitializeRequiredWhenImplementingInterface.DiagnosticId;

    protected override AttributeSyntax GetAttribute(IPropertySymbol prop, INamedTypeSymbol mustInitializeSymbol)
        => SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(AttributeType.Name.Replace(nameof(Attribute), ""))); // No need to go through the interfaces

}
