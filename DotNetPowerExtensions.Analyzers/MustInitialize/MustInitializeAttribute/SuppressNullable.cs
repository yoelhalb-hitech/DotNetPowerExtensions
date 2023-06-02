using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace DotNetPowerExtensions.Analyzers.MustInitialize.MustInitializeAttribute;

#if !NET45 && !NET46

// This has to be in a different assembly than the other analyzer for it to work..
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class SuppressNullableAnalyzer : DiagnosticSuppressor
{
    private static readonly SuppressionDescriptor MustInitializeRule = new SuppressionDescriptor(
        id: "YH10001",
        suppressedDiagnosticId: "CS8618",
        justification: "MustInitialize is handling this");

    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions => ImmutableArray.Create(MustInitializeRule);

    public override void ReportSuppressions(SuppressionAnalysisContext context)
    {
        try
        {
            foreach (var diagnostic in context.ReportedDiagnostics)
            {
                SuppressNullableAnalyzer.AnalyzeDiagnostic(diagnostic, context);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
            throw;
        }
    }
    private const string mustInitialize = "MustInitialize";
    private static bool ContainsMustInitialize(MemberDeclarationSyntax member, SuppressionAnalysisContext context, string name)
    {
        // Make sure it is the correct type and not just something with the same name...
        var mustInitializeDecl = context.Compilation
                    .GetTypeByMetadataName(typeof(SequelPay.DotNetPowerExtensions.MustInitializeAttribute).FullName!);
        if (mustInitializeDecl is null) return false;

        var propSymbols = context.Compilation.GetSymbolsWithName(name, cancellationToken: context.CancellationToken);
        if (!propSymbols.Any()) return false; // TODO...

        var lazy = new Lazy<string>(() => member.GetContainerFullName());
        context.GetSemanticModel(member.SyntaxTree);
        // We have no way to get the semnatic model in a supressor and Compilation.GetSemnaticModel is not recommended
        //var propSymbol = !propSymbols.Skip(1).Any() ? propSymbols.First() : propSymbols.FirstOrDefault(p => p.GetContainerFullName() == lazy.Value);
        //if (propSymbol is null) return false;

        var propSymbol = context.GetSemanticModel(member.SyntaxTree).GetDeclaredSymbol(member, context.CancellationToken);

        return propSymbol?.HasAttribute(mustInitializeDecl) ?? false;
    }

    private static void AnalyzeDiagnostic(Diagnostic diagnostic, SuppressionAnalysisContext context)
    {
        try
        {
            var node = diagnostic.Location.SourceTree?.GetRoot(context.CancellationToken).FindNode(diagnostic.Location.SourceSpan);

            if (node is PropertyDeclarationSyntax prop)
            {
                if (!ContainsMustInitialize(prop, context, prop.Identifier.ValueText)) return;
            }
            else if (node?.Parent?.Parent is FieldDeclarationSyntax f)
            {
                if (!ContainsMustInitialize(f, context, (node as VariableDeclaratorSyntax)?.Identifier.Text ?? "")) return;
            }
            else if (node is ConstructorDeclarationSyntax) // Because sometimes the warning is on the constructor instead of the proeprty/field
            {
                var regex = new Regex(@"(\S*)\s*'(.*)'");
#pragma warning disable CA1305 // The behavior of '{0}' could vary based on the current user's locale settings. Provide a value for the 'IFormatProvider' argument.
                var match = regex.Match(diagnostic.GetMessage());
#pragma warning restore CA1305 // The behavior of '{0}' could vary based on the current user's locale settings. Provide a value for the 'IFormatProvider' argument.
                var type = match.Groups[1].Value;
                var name = match.Groups[2].Value;

                if (node.Parent is null)
                {
                    Logger.LogInfo("Parent missing for symbol, how is that possible?");
                    return;
                }

                if (type == "field")
                {
                    var fieldDecl = node.Parent.DescendantNodes().OfType<FieldDeclarationSyntax>().First(n => n.Declaration.Variables.Any(v => v.Identifier.ValueText == name));
                    if (!ContainsMustInitialize(fieldDecl, context, name)) return;
                }
                else
                {
                    var propDecl = node.Parent.DescendantNodes().OfType<PropertyDeclarationSyntax>().First(p => p.Identifier.ValueText == name);
                    if (!ContainsMustInitialize(propDecl, context, name)) return;
                }
            }

            context.ReportSuppression(Suppression.Create(MustInitializeRule, diagnostic));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
            throw;
        }
    }
}

#endif
