using DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;

namespace DotNetPowerExtensions.Analyzers.MustInitialize.MustInitializeAttribute.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MustInitializeRequiredWhenOverriding : RequiredWhenOverridingBase
{
    public const string DiagnosticId = "DNPE0110";
    protected const string Title = "RequiredWhenOverriding";
    protected const string Message = "{1} is required when oevrriding a property with {1}.";

    protected override DiagnosticDescriptor DiagnosticDesc => Diagnostic;

    protected DiagnosticDescriptor Diagnostic = new DiagnosticDescriptor(DiagnosticId, Title, Title, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Message);


    protected override Type AttributeType => typeof(DotNetPowerExtensions.MustInitialize.MustInitializeAttribute);
}
