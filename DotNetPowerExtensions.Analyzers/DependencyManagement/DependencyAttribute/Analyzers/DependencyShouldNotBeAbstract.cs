
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SequelPay.DotNetPowerExtensions;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata;

namespace DotNetPowerExtensions.Analyzers.DependencyManagement.DependencyAttribute.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DependencyShouldNotBeAbstract : DiagnosticAnalyzer
{
    protected const string Category = "Language";
    public const string DiagnosticId = "DNPE0209";
    protected const string Title = "DependencyShouldNotBeAbstract";
    protected const string Message = "Use `{0}Base` instead of `{0}` when class is abstract";
    protected const string Description = Message + ".";

    [SuppressMessage("Microsoft.Design", "CA1051: Do not declare visible instance fields", Justification = "The compiler only consideres fields when tracking analyzer releases")]
    protected DiagnosticDescriptor Diagnostic = new DiagnosticDescriptor(DiagnosticId, Title, Message, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Diagnostic);

    public override void Initialize(AnalysisContext context)
    {
        try
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compilationContext =>
            {
                Func<Type, INamedTypeSymbol?> metadata = t => compilationContext.Compilation.GetTypeByMetadataName(t.FullName!);

                var symbols = DependencyAnalyzerUtils.AllDependencies.Select(t => metadata(t)).Where(x => x is not null).Select(x => x!).ToArray();

                compilationContext
                    .RegisterSyntaxNodeAction(c => AnalyzeClass(c, symbols), SyntaxKind.Attribute);
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }

    private void AnalyzeClass(SyntaxNodeAnalysisContext context, INamedTypeSymbol[] attributeSymbols)
    {
        try
        {
            // Since a class decleration can be partial we will only report it on the attribute
            var attr = context.Node as AttributeSyntax;
            var attrName = attr?.Name.GetUnqualifiedName()?.Replace(nameof(Attribute), "");
            if (attrName is null || !DependencyAnalyzerUtils.DependencyAttributeNames.Contains(attrName + nameof(Attribute)))
                return;

            if (context.SemanticModel.GetSymbolInfo(attr!, context.CancellationToken).Symbol is not IMethodSymbol methodSymbol
                || !attributeSymbols.ContainsGeneric(methodSymbol.ContainingType)) return;

            var parent = context.Node.Parent;
            while (parent is not null && !object.ReferenceEquals(parent, parent.Parent) && parent is not TypeDeclarationSyntax) parent = parent.Parent;

            if (parent is not ClassDeclarationSyntax) return;

            var classSymbol = context.SemanticModel.GetDeclaredSymbol(parent);
            if (classSymbol is null || !classSymbol.IsAbstract) return;

            var diagnostic = Microsoft.CodeAnalysis.Diagnostic.Create(Diagnostic, attr!.GetLocation(), attrName);
            context.ReportDiagnostic(diagnostic);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}
