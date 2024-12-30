
namespace SequelPay.DotNetPowerExtensions.Analyzers.DependencyManagement.ILocalFactory.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TypeMismatchForILocalFactory : DiagnosticAnalyzer
{
    protected const string Category = "Language";
    public const string DiagnosticId = "DNPE0203";
    protected const string Title = "TypeMismatchForLocalService";
    protected const string Message = "Type of Member '{0}' should be '{1}'";
    protected const string Description = "Type mismatch between initlaizer and members decorated with the MustInitialize attribute.";

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
                var typedSymbol = compilationContext.Compilation.GetTypeSymbol(typeof(ILocalFactory<>));
                if (typedSymbol is null) return;

                // TODO... maybe use an IOperation instead...
                compilationContext.RegisterSyntaxNodeAction(c => AnalyzeInvocation(c, typedSymbol), SyntaxKind.InvocationExpression);

            });
        }
        catch { }
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


            var worker = new MustInitializeWorker(context.Compilation, context.SemanticModel);

            if (!classType.IsGenericEqual(serviceTypeSymbol)) return;

            var innerClass = classType.TypeArguments.FirstOrDefault();
            if (innerClass is null) return;

            var props = worker.GetMembersGroupedByName(innerClass).Select(g => (g.Key, g.First().First?.Type ?? g.First().Second!.Type))
                .Concat(MightRequireUtils.GetMightRequiredInfos(innerClass, worker.MightRequireSymbols).Select(m => (m.Name, m.Type)))
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
