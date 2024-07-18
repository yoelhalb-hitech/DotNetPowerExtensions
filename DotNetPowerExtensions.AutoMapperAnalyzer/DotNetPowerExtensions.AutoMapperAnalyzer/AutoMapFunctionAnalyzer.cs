
namespace SequelPay.DotNetPowerExtensions.AutoMapper;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AutoMapFunctionAnalyzer : DiagnosticAnalyzer
{
    private const string DiagnosticId = "YourDiagnosticId";
    private const string Category = "Usage";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "AutoMap function usage",
        "AutoMap function is not used correctly",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.RegisterSyntaxNodeAction(AnalyzeInvocationExpression, SyntaxKind.InvocationExpression);
    }

    // TODO... handle if from generator and add tests

    private void AnalyzeInvocationExpression(SyntaxNodeAnalysisContext context)
    {
        var invocationExpression = (InvocationExpressionSyntax)context.Node;

        // Check if the invocation is calling the AutoMap function
        var memberAccessExpression = invocationExpression.Expression as MemberAccessExpressionSyntax;
        if (memberAccessExpression?.Name.ToString() != "AutoMap")
        {
            return;
        }

        // Get the type arguments passed to the AutoMap function
        var typeArguments = invocationExpression.TypeArgumentList?.Arguments;
        if (typeArguments == null || typeArguments.Count != 2)
        {
            return;
        }

        // Check if the type arguments have the AutoMap attribute with each other as an argument
        var sourceType = context.SemanticModel.GetTypeInfo(typeArguments[0]).Type;
        var targetType = context.SemanticModel.GetTypeInfo(typeArguments[1]).Type;

        var sourceAttribute = sourceType.GetAttributes().FirstOrDefault(attr => attr.AttributeClass.Name == "AutoMap");
        var targetAttribute = targetType.GetAttributes().FirstOrDefault(attr => attr.AttributeClass.Name == "AutoMap");

        if (sourceAttribute == null || targetAttribute == null ||
            !sourceAttribute.ConstructorArguments.Any(arg => arg.Value.Equals(targetType)) ||
            !targetAttribute.ConstructorArguments.Any(arg => arg.Value.Equals(sourceType)))
        {
            var diagnostic = Diagnostic.Create(Rule, invocationExpression.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}