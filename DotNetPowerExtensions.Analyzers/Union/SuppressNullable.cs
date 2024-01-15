using SequelPay.DotNetPowerExtensions.RoslynExtensions;
using SequelPay.DotNetPowerExtensions;
using System.Collections.Immutable;

namespace SequelPay.DotNetPowerExtensions.Analyzers.Union;

#if !NET45 && !NET46

// This has to be in a different assembly than the other analyzer for it to work..
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class SuppressNullableAnalyzer : DiagnosticSuppressor
{
    private static readonly SuppressionDescriptor OfRule = new(
        id: "YH10001",
        suppressedDiagnosticId: "CS8600",
        justification: "`AsShouldBeAssignableType` is handling this");

    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions => ImmutableArray.Create(OfRule);

    public override void ReportSuppressions(SuppressionAnalysisContext context)
    {
        try
        {
            foreach (var diagnostic in context.ReportedDiagnostics)
            {
                AnalyzeDiagnostic(diagnostic, context);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
            throw;
        }
    }

    private static bool ContainsMustInitialize(MemberDeclarationSyntax member, SuppressionAnalysisContext context, string name)
    {
        // Make sure it is the correct type and not just something with the same name...
        var mustInitializeDecl = context.Compilation.GetTypeSymbol(typeof(MustInitializeAttribute));
        if (mustInitializeDecl is null) return false;

        var propSymbols = context.Compilation.GetSymbolsWithName(name, cancellationToken: context.CancellationToken);
        if (!propSymbols.Any()) return false; // TODO...

        var propSymbol = context.GetSemanticModel(member.SyntaxTree).GetDeclaredSymbol(member);

        return propSymbol?.HasAttribute(mustInitializeDecl) ?? false;
    }

    private static void AnalyzeDiagnostic(Diagnostic diagnostic, SuppressionAnalysisContext context)
    {
        try
        {
            var node = diagnostic.Location.SourceTree?.GetRoot(context.CancellationToken).FindNode(diagnostic.Location.SourceSpan);

            if (node is not InvocationExpressionSyntax invocation) return;

            if (invocation is null
                || context.GetSemanticModel(invocation.SyntaxTree).GetSymbolInfo(invocation, context.CancellationToken).Symbol is not IMethodSymbol methodSymbol
                || methodSymbol.ReceiverType is not INamedTypeSymbol classType
                || !classType.IsGenericType
                || methodSymbol.Name != nameof(Union<object, object>.As)
                || !methodSymbol.IsGenericMethod) return;

            var symbol1 = context.Compilation.GetTypeSymbol(typeof(Union<,>));
            var symbol2 = context.Compilation.GetTypeSymbol(typeof(Union<,,>));
            if (!new[] { symbol1, symbol2 }.ContainsGeneric(classType)) return;

            context.ReportSuppression(Suppression.Create(OfRule, diagnostic));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
            throw;
        }
    }
}

#endif
