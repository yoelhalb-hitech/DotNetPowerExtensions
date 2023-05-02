using DotNetPowerExtensions.Analyzers.MustInitialize;
using DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;
using DotNetPowerExtensions.Analyzers.MustInitialize.MightRequireAttribute;

namespace DotNetPowerExtensions.Analyzers.DependencyManagement.ILocalFactory.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TypeMismatchForILocalFactory : MustInitializeRequiredMembersBase
{
    public const string DiagnosticId = "DNPE0203";
    protected const string Title = "TypeMismatchForLocalService";
    protected const string Message = "Type of Member '{0}' should be '{1}'";
    protected const string Description = "Type mismatch between initlaizer and members decorated with the MustInitialize attribute.";
    protected override DiagnosticDescriptor DiagnosticDesc => Diagnostic;

    [SuppressMessage("Microsoft.Design", "CA1051: Do not declare visible instance fields", Justification = "The compiler only consideres fields when tracking analyzer releases")]
    protected DiagnosticDescriptor Diagnostic = new DiagnosticDescriptor(DiagnosticId, Title, Message, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);


    public override void Register(CompilationStartAnalysisContext compilationContext, INamedTypeSymbol[] mustInitializeSymbols)
    {
        Func<Type, INamedTypeSymbol?> metadata = t => compilationContext.Compilation.GetTypeByMetadataName(t.FullName!);

        var localSymbol = metadata(typeof(ILocalFactory<>));
        if (localSymbol is null) return;

        var mightRequireSymbols = MightRequireUtils.Attributes.Select(a => metadata(a)).OfType<INamedTypeSymbol>().ToArray();
        if (!mightRequireSymbols.Any()) return;

        var intializedSymbol = metadata(typeof(InitializedAttribute));

        // TODO... maybe use an IOperation instead...
        compilationContext.RegisterSyntaxNodeAction(c => AnalyzeInvocation(c, mustInitializeSymbols, mightRequireSymbols, localSymbol, intializedSymbol),
                                                                                                                SyntaxKind.InvocationExpression);
    }

    private void AnalyzeInvocation(SyntaxNodeAnalysisContext context, INamedTypeSymbol[] mustInitializeSymbols,
                                            INamedTypeSymbol[] mightRequireSymbols, INamedTypeSymbol serviceTypeSymbol, INamedTypeSymbol? initializedSymbol)
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

            var props = MustInitializeUtils.GetRequiredToInitialize(innerClass, mustInitializeSymbols, mightRequireSymbols, initializedSymbol)
                                                                                                                .ToDictionary(m => m.name, m => m.type);

            var declared = creation.Initializers.Where(i => !string.IsNullOrWhiteSpace(i.GetName())).ToDictionary(i => i.GetName()!, i => i.Expression);

            var nonMatchings = declared.Where(i => i.Key is not null && props.ContainsKey(i.Key)
                                                            && !context.SemanticModel.GetTypeInfo(i.Value).Type.IsEqualTo(props[i.Key]));

            foreach (var nonMatching in nonMatchings)
            {
                var diag = Microsoft.CodeAnalysis.Diagnostic.Create(Diagnostic, nonMatching.Value.GetLocation(),
                                                                        nonMatching.Key, props[nonMatching.Key!].ToStringWithoutNamesapce());
                context.ReportDiagnostic(diag);
            }

        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}
