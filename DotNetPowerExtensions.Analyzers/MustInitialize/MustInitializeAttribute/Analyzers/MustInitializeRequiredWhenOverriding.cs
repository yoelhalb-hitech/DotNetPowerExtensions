using DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;

namespace DotNetPowerExtensions.Analyzers.MustInitialize.MustInitializeAttribute.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MustInitializeRequiredWhenOverriding : RequiredWhenOverridingBase, IMustInitializeAnalyzer
{
    public static string DiagnosticId => "DNPE0110";

    public override string RuleId => DiagnosticId;

    protected override Type AttributeType => typeof(DotNetPowerExtensions.MustInitialize.MustInitializeAttribute);
}
