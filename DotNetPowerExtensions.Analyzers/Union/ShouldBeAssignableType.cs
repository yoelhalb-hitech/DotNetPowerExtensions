using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace DotNetPowerExtensions.Analyzers.Union;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ShouldBeAssignableType : DiagnosticAnalyzer
{
    protected const string Category = "Language";
    public const string DiagnosticId = "DNPE0401";
    protected const string Title = "ShouldBeAssignableType";
    protected const string Message = "The type parameter to As<> should be something that can be assigned staticallly to the generic types of `Of<>`";
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
                var typeName1 = typeof(DotNetPowerExtensions.Union<,>).FullName!;
                var typeName2 = typeof(DotNetPowerExtensions.Union<,,>).FullName!;
                var symbol1 = compilationContext.Compilation.GetTypeByMetadataName(typeName1);
                var symbol2 = compilationContext.Compilation.GetTypeByMetadataName(typeName2);
                if (symbol1 is null && symbol2 is null) return;

                compilationContext.RegisterSyntaxNodeAction(c => AnalyzeInvocation(c, new[] { symbol1, symbol2 }), SyntaxKind.InvocationExpression);
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }

    private void AnalyzeInvocation(SyntaxNodeAnalysisContext context, INamedTypeSymbol?[] symbols)
    {
        try
        {
            var invocation = context.Node as InvocationExpressionSyntax;
            if (invocation is null
                || context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol is not IMethodSymbol methodSymbol
                || methodSymbol.ReceiverType is not INamedTypeSymbol classType
                || !classType.IsGenericType
                || methodSymbol.Name != nameof(Union<object, object>.As)
                || !methodSymbol.IsGenericMethod) return;

            if (!symbols.ContainsGeneric(classType)) return;

            var genericClassArgs = classType.TypeArguments;
            var methodArg = methodSymbol.TypeArguments.FirstOrDefault();

            if(!genericClassArgs.Any() || methodArg is null) return;

            Func<Conversion, bool> isValid = c => c.Exists && (c.IsIdentity || c.IsReference || c.IsBoxing || c.IsUnboxing);
            Func<ITypeSymbol, ITypeSymbol, Conversion> convert = (source, dest) => context.Compilation.ClassifyConversion(source, dest);

            if (genericClassArgs.All(c => !isValid(convert(c, methodArg)) && !isValid(convert(methodArg, c))))
            {
                var diag = Microsoft.CodeAnalysis.Diagnostic.Create(Diagnostic,
                        invocation.Expression is MemberAccessExpressionSyntax mem ? mem.Name.GetLocation() : invocation.GetLocation());
                context.ReportDiagnostic(diag);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}
