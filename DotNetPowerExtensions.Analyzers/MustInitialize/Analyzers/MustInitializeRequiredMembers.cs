
namespace DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MustInitializeRequiredMembers : MustInitializeAnalyzerBase
{
    public const string DiagnosticId = "DNPE0103";
   
    protected const string Title = "MustInitializeRequiredMembers";
    protected const string Message = "Must initilalize all memebers decorated with MustInitialize.";

    protected override DiagnosticDescriptor DiagnosticDesc => Diagnostics;

    protected DiagnosticDescriptor Diagnostics = new DiagnosticDescriptor(DiagnosticId, Title, Title, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Message);


    public override void Register(CompilationStartAnalysisContext compilationContext, INamedTypeSymbol[] mustInitializeSymbols)
    {
        // TODO... maybe use an IOperation instead...
        compilationContext.RegisterSyntaxNodeAction(c => AnalyzeCreation(c, mustInitializeSymbols), SyntaxKind.ObjectCreationExpression);
    }

    public static IEnumerable<string> GetNotInitializedNames(ObjectCreationExpressionSyntax typeDecl, ITypeSymbol symbol, INamedTypeSymbol[] mustInitializeSymbols)
    {
        // We don't need the interfaces, since we require to specify it directly on the implementation, and c# 8 default interfaces are not allowed
        var symbols = new[] { symbol }.Concat(symbol.GetAllBaseTypes());

#if NETSTANDARD2_0_OR_GREATER
        Func<AttributeData, bool> hasMustInitialize = a => mustInitializeSymbols.Any(s => SymbolEqualityComparer.Default.Equals(a.AttributeClass, s));
#else
        Func<AttributeData, bool> hasMustInitialize = a => mustInitializeSymbols.Any(s => s.Equals(a.AttributeClass));
#endif

        var props = symbols.SelectMany(s => s.GetMembers()
                                    .OfType<IPropertySymbol>()
                                    .Where(p => !p.IsReadOnly && p.GetAttributes().Any(hasMustInitialize))
                                    .Select(p => p.Name)
                                .Concat(
                                    s.GetMembers()
                                        .OfType<IFieldSymbol>()
                                        .Where(p => !p.IsReadOnly && p.GetAttributes().Any(hasMustInitialize))
                                        .Select(p => p.Name)));

        if (typeDecl.Initializer is not null)
        {
            var childs = typeDecl.Initializer.ChildNodes();
            var propsInitialized = childs.OfType<IdentifierNameSyntax>()
                    .Union(childs.OfType<AssignmentExpressionSyntax>().Select(c => c.Left).OfType<IdentifierNameSyntax>())
                .Select(c => c.Identifier.Text);

            props = props.Except(propsInitialized);
        }

        return props;
    }

    private void AnalyzeCreation(SyntaxNodeAnalysisContext context, INamedTypeSymbol[] mustInitializeSymbols)
    {
        try
        {
            var expr = context.Node as ObjectCreationExpressionSyntax;
            if (expr is null) return;

            var symbol = context.SemanticModel.GetTypeInfo(expr).Type as ITypeSymbol;
            if (symbol is null) return;

            var props = GetNotInitializedNames(expr, symbol, mustInitializeSymbols);
            if (!props.Any()) return;

            var combined = string.Join(", ", props);
            var diagnostic = Diagnostic.Create(DiagnosticDesc, expr.GetLocation(), props.Skip(1).Any() ? "ies" : "y", combined);

            context.ReportDiagnostic(diagnostic);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}