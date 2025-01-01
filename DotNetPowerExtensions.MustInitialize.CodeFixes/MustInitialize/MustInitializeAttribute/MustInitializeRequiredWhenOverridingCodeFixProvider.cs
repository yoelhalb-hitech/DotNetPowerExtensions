using SequelPay.DotNetPowerExtensions.Analyzers.MustInitialize.CodeFixProviders;

namespace SequelPay.DotNetPowerExtensions.Analyzers.MustInitialize.MustInitializeAttribute.CodeFixProviders;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MustInitializeRequiredWhenOverridingCodeFixProvider)), Shared]
public class MustInitializeRequiredWhenOverridingCodeFixProvider
                    : RequiredWhenOverridingCodeFixProviderBase<MustInitializeRequiredWhenOverriding>
{
    protected override string Title => "Add MustInitialize";

    protected override Type AttributeType => typeof(SequelPay.DotNetPowerExtensions.MustInitializeAttribute);

    protected override string DiagnosticId => MustInitializeRequiredWhenOverriding.DiagnosticId;
}
