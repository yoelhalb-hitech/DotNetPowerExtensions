using DotNetPowerExtensions.MustInitialize;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetPowerExtensionsAnalyzer.MustInitialize.Analyzers;

public abstract class MustInitializeAnalyzerBase : DiagnosticAnalyzer
{
    public abstract string RuleId { get; }
    protected abstract string Title { get; }
    protected abstract string Message { get; }

    protected virtual string Category => "Language";
    protected virtual DiagnosticDescriptor DiagnosticDesc => new(RuleId, Title, Title, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Message);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DiagnosticDesc);

    protected virtual Diagnostic CreateDiagnostic(AttributeData attribute)
        => Diagnostic.Create(DiagnosticDesc, attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation());

    protected virtual Diagnostic CreateDiagnostic(IPropertySymbol symbol)
        => Diagnostic.Create(DiagnosticDesc, symbol.DeclaringSyntaxReferences.First().GetSyntax().GetLocation());

    public abstract void Register(CompilationStartAnalysisContext compilationContext, INamedTypeSymbol[] mustInitializeSymbols);

    public virtual string[] AttributeNames =>
    new[]
    {
        typeof(DotNetPowerExtensions.MustInitialize.MustInitializeAttribute).FullName!,
        typeof(MustInitializeIfSetAttribute).FullName!,
        typeof(MustInitializeIfNotSetAttribute).FullName!,
    };

    public override void Initialize(AnalysisContext context)
    {
        try
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compilationContext =>
            {
                var symbols = AttributeNames.Select(n => compilationContext.Compilation.GetTypeByMetadataName(n));
                if (symbols.Any(s => s is null)) return;

                Register(compilationContext, symbols.OfType<INamedTypeSymbol>().ToArray());
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}
