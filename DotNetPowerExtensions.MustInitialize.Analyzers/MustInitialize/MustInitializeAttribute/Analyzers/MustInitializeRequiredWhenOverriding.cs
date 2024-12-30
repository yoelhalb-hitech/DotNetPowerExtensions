
namespace DotNetPowerExtensions.MustInitialize.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MustInitializeRequiredWhenOverriding : RequiredWhenOverridingBase
{
    public const string DiagnosticId = "DNPE0110";
    protected const string Title = "RequiredWhenOverriding";
    protected const string Message = "Either `MustInitialize` or `Initialized` is required when overriding a property with `MustInitialize` or `Initialized`";
    protected const string Description = Message + ".";

    protected override DiagnosticDescriptor DiagnosticDesc => Diagnostic;

    [SuppressMessage("Microsoft.Design", "CA1051: Do not declare visible instance fields", Justification = "The compiler only consideres fields when tracking analyzer releases")]
    protected DiagnosticDescriptor Diagnostic = new DiagnosticDescriptor(DiagnosticId, Title, Message, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);


    protected override Type AttributeType => typeof(SequelPay.DotNetPowerExtensions.MustInitializeAttribute);
}
