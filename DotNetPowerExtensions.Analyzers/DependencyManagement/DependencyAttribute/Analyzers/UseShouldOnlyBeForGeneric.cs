using SequelPay.DotNetPowerExtensions.RoslynExtensions;
using SequelPay.DotNetPowerExtensions;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace SequelPay.DotNetPowerExtensions.Analyzers.DependencyManagement.DependencyAttribute.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UseShouldOnlyBeForGeneric : DiagnosticAnalyzer
{
    protected const string Category = "Language";
    public const string DiagnosticId = "DNPE0206";
    protected const string Title = "UseIsOnlyForGeneric";
    protected const string Message = "The `Use` attribute is only for generic types";
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
                Func<Type, INamedTypeSymbol?> metadata = t => compilationContext.Compilation.GetTypeSymbol(t);

                var allAttributeTypes = new[]
                {
                    typeof(LocalAttribute),
                    typeof(LocalAttribute<>),
                    typeof(SingletonAttribute),
                    typeof(SingletonAttribute<>),
                    typeof(ScopedAttribute),
                    typeof(ScopedAttribute<>),
                    typeof(TransientAttribute),
                    typeof(TransientAttribute<>),
                    typeof(SequelPay.DotNetPowerExtensions.DependencyAttribute),
                    typeof(NonDependencyAttribute),
                    typeof(NonDependencyAttribute<>),
                };
                var symbols = allAttributeTypes.Select(t => metadata(t)).Where(x => x is not null).Select(x => x!).ToArray();

                compilationContext
                    .RegisterSyntaxNodeAction(c => AnalyzeClass(c, symbols), SyntaxKind.Attribute);
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }

    private string[] DependencyAttributeNames =
    {
        nameof(SingletonAttribute), nameof(ScopedAttribute), nameof(TransientAttribute),
        nameof(LocalAttribute), nameof(NonDependencyAttribute), nameof(SequelPay.DotNetPowerExtensions.DependencyAttribute),
    };

    private void AnalyzeClass(SyntaxNodeAnalysisContext context, INamedTypeSymbol[] attributeSymbols)
    {
        try
        {
            // Since a class decleration can be partial we will only report it on the attribute
            var result = DependencyAnalyzerUtils.GetAttributeInfo(context,
                                                            DependencyAnalyzerUtils.DependencyAttributeNames, attributeSymbols);
            if (result is null) return;
            var (attr, attrName, methodSymbol) = result.Value;

            var (useExpression, innerExpression) = DependencyAnalyzerUtils.GetUse(attr);
            if (innerExpression is not TypeOfExpressionSyntax) return;

            var parent = context.Node.FirstAncestorOrSelf<TypeDeclarationSyntax>();
            if (parent is null) return;

            if (context.SemanticModel.GetDeclaredSymbol(parent!, context.CancellationToken) is not INamedTypeSymbol classSymbol) return;
            if (classSymbol.IsGenericType) return;

            var diagnostic = Microsoft.CodeAnalysis.Diagnostic.Create(Diagnostic, useExpression!.GetLocation());

            context.ReportDiagnostic(diagnostic);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}