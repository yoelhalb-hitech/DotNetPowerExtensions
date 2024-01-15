using SequelPay.DotNetPowerExtensions;

namespace SequelPay.DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;

public abstract class ByAttributeAnalyzerBase : MustInitializeAnalyzerBase
{
    protected abstract Type AttributeType { get; }
    protected virtual string DescriptiveName => AttributeType.Name.Replace(typeof(Attribute).Name, "");

    protected virtual INamedTypeSymbol[] GetAttributeSymbol(INamedTypeSymbol[] typeSymbols)
        => typeSymbols
            .Where(s => s.Name == AttributeType.Name
                    || (IncludeInitializedAttribute && s.Name == typeof(InitializedAttribute).Name))
            .ToArray();

    protected override Diagnostic CreateDiagnostic(AttributeData attribute)
        => Diagnostic.Create(DiagnosticDesc, attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation(), DescriptiveName);

    protected override Diagnostic CreateDiagnostic(IPropertySymbol symbol)
        => Diagnostic.Create(DiagnosticDesc, symbol.DeclaringSyntaxReferences.First().GetSyntax().GetLocation(), DescriptiveName);
}
