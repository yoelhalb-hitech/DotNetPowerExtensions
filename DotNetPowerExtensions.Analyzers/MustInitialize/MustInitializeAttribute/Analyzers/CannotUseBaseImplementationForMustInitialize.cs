using DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;

namespace DotNetPowerExtensions.Analyzers.MustInitialize.MustInitializeAttribute.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CannotUseBaseImplementationForMustInitialize : CannotUseBaseImplementationBase
{
    public const string DiagnosticId = "DNPE0105";
    protected const string Title = "CannotUseBaseImplementation";
    protected const string Message = "Cannot use base implementation of {0} because it lacks {1}.";

    protected override DiagnosticDescriptor DiagnosticDesc => Diagnostic;

    protected DiagnosticDescriptor Diagnostic = new DiagnosticDescriptor(DiagnosticId, Title, Title, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Message);


    protected override Type AttributeType => typeof(DotNetPowerExtensions.MustInitialize.MustInitializeAttribute);
}
