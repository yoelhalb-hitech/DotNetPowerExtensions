﻿using DotNetPowerExtensions.Analyzers.MustInitialize.CodeFixProviders;
using DotNetPowerExtensions.Analyzers.MustInitialize.MustInitializeAttribute.Analyzers;

namespace DotNetPowerExtensions.Analyzers.MustInitialize.MustInitializeAttribute.CodeFixProviders;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CannotUseBaseImplementationForMustInitializeCodeFixProvider)), Shared]
public class CannotUseBaseImplementationForMustInitializeCodeFixProvider
            : CannotUseBaseImplementationCodeFixProviderBase<CannotUseBaseImplementationForMustInitialize>
{
    protected override string Title => "Implement Required Properties";

    protected override Type AttributeType => typeof(DotNetPowerExtensions.MustInitialize.MustInitializeAttribute);

    protected override AttributeSyntax GetAttribute(IPropertySymbol prop, INamedTypeSymbol mustInitializeSymbol)
        => SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(AttributeType.Name.Replace(nameof(Attribute), ""))); // No need to go through the interfaces
}