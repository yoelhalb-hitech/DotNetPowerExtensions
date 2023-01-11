using DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;
using System.Diagnostics.CodeAnalysis;

namespace DotNetPowerExtensions.Analyzers.MustInitialize.MustInitializeAttribute.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MustInitializeRequiredWhenImplementingInterface : RequiredWhenImplementingInterfaceBase
{
    public const string DiagnosticId = "DNPE0104";
    protected const string Title = "RequiredWhenImplementingInterface";
    protected const string Message = "{1} is required when the interface property is {1}.";

    protected override DiagnosticDescriptor DiagnosticDesc => Diagnostic;

    [SuppressMessage("Microsoft.Design", "CA1051: Do not declare visible instance fields", Justification = "The compiler only consideres fields when tracking analyzer releases")]
    protected DiagnosticDescriptor Diagnostic = new DiagnosticDescriptor(DiagnosticId, Title, Title, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Message);


    protected override Type AttributeType => typeof(DotNetPowerExtensions.MustInitializeAttribute);
}
