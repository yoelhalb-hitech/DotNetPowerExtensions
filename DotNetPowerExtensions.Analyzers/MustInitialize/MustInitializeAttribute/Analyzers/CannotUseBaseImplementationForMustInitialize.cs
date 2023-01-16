using DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;
using System.Diagnostics.CodeAnalysis;

namespace DotNetPowerExtensions.Analyzers.MustInitialize.MustInitializeAttribute.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CannotUseBaseImplementationForMustInitialize : CannotUseBaseImplementationBase
{
    public const string DiagnosticId = "DNPE0105";
    protected const string Title = "CannotUseBaseImplementation";
    protected const string Message = "Cannot use base implementation of {0} because it lacks {1}";
    protected const string Description = Message + ".";

    protected override DiagnosticDescriptor DiagnosticDesc => Diagnostic;

    [SuppressMessage("Microsoft.Design", "CA1051: Do not declare visible instance fields", Justification = "The compiler only consideres fields when tracking analyzer releases")]
    protected DiagnosticDescriptor Diagnostic = new DiagnosticDescriptor(DiagnosticId, Title, Message, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);


    protected override Type AttributeType => typeof(DotNetPowerExtensions.MustInitializeAttribute);
}
