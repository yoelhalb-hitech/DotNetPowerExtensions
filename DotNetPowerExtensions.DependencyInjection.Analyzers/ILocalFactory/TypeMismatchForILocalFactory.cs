
namespace SequelPay.DotNetPowerExtensions.Analyzers.DependencyManagement.ILocalFactory.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TypeMismatchForILocalFactory : AnalyzerBase
{
    public const string DiagnosticId = "DNPE0203";
    protected const string Title = "TypeMismatchForLocalService";
    protected const string Message = "Type of Member '{0}' should be '{1}'";
    protected const string Description = "Type mismatch between initializer and member type.";

    protected DiagnosticDescriptor Diagnostic = new DiagnosticDescriptor(DiagnosticId, Title, Message, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Diagnostic);

    protected override void Register(CompilationStartAnalysisContext compilationContext, MetadataUtil metadataUtil)
    {
        var typedSymbol = metadataUtil.GetTypeSymbol(typeof(ILocalFactory<>));
        if (typedSymbol is null) return;

        // TODO... maybe use an IOperation instead...
        compilationContext.RegisterSyntaxNodeAction(c => AnalyzeInvocation(c, typedSymbol), SyntaxKind.InvocationExpression);

    }

    private void AnalyzeInvocation(SyntaxNodeAnalysisContext context, INamedTypeSymbol serviceTypeSymbol)
    {
        try
        {
            var invocation = context.Node as InvocationExpressionSyntax;
            if (invocation is null
                || invocation.ArgumentList.Arguments.FirstOrDefault()?.Expression is not AnonymousObjectCreationExpressionSyntax creation
                || context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol is not IMethodSymbol methodSymbol
                || methodSymbol.ReceiverType is not INamedTypeSymbol classType
                || !classType.IsGenericType) return;

            if (methodSymbol.Name != nameof(ILocalFactory<object>.Create)) return;

            if (!classType.IsGenericEqual(serviceTypeSymbol)) return;

            var innerClass = classType.TypeArguments.FirstOrDefault();
            if (innerClass is null) return;

            var props = innerClass.GetAllProperties().Select(p => (p.Name, p.Type))
                    .Concat(innerClass.GetAllFields().Select(p => (p.Name, p.Type)))
                    .ToDictionary(x => x.Item1, x => x.Item2);

            var declared = creation.Initializers.Where(i => !string.IsNullOrWhiteSpace(i.GetName())).ToDictionary(i => i.GetName()!, i => i.Expression);

            var nonMatchings = declared.Where(i => i.Key is not null && props.ContainsKey(i.Key)
                                                        && !context.SemanticModel.GetTypeInfo(i.Value, context.CancellationToken).Type.IsEqualTo(props[i.Key]));

            foreach (var nonMatching in nonMatchings)
            {
                var diag = Microsoft.CodeAnalysis.Diagnostic.Create(Diagnostic, nonMatching.Value.GetLocation(),
                                                                        nonMatching.Key, props[nonMatching.Key!].ToStringWithoutNamesapce());
                context.ReportDiagnostic(diag);
            }

        }
        catch { }
    }
}
