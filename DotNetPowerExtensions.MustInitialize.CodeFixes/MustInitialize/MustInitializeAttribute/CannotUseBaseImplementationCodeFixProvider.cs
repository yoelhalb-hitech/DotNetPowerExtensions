using SequelPay.DotNetPowerExtensions.Analyzers.MustInitialize.CodeFixProviders;
using SequelPay.DotNetPowerExtensions.Analyzers.MustInitialize.MustInitializeAttribute.Analyzers;

namespace SequelPay.DotNetPowerExtensions.Analyzers.MustInitialize.MustInitializeAttribute.CodeFixProviders;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CannotUseBaseImplementationForMustInitializeCodeFixProvider)), Shared]
public class CannotUseBaseImplementationForMustInitializeCodeFixProvider
            : CannotUseBaseImplementationCodeFixProviderBase<CannotUseBaseImplementationForMustInitialize>
{
    protected override string Title => "Implement Required Properties";

    protected override Type AttributeType => typeof(SequelPay.DotNetPowerExtensions.MustInitializeAttribute);

    protected override string DiagnosticId => CannotUseBaseImplementationForMustInitialize.DiagnosticId;
}
