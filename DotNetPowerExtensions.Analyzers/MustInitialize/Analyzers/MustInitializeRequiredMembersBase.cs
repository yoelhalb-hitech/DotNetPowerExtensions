
using DotNetPowerExtensions.Of;
using Microsoft.CodeAnalysis;
using System.Reflection;

namespace DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;

public abstract class MustInitializeRequiredMembersBase : MustInitializeAnalyzerBase
{
    private static IEnumerable<Of<IPropertySymbol, IFieldSymbol>> GetMembersWithMustInitialize(IEnumerable<ITypeSymbol> symbols, INamedTypeSymbol[] mustInitializeSymbols)
    {
        Func<AttributeData, bool> hasMustInitialize = a => mustInitializeSymbols.ContainsSymbol(a.AttributeClass);

        return symbols.SelectMany(s => s.GetMembers()
                                    .OfType<IPropertySymbol>()
                                    .Where(p => !p.IsReadOnly && p.GetAttributes().Any(hasMustInitialize))
                                    .Select(p => new Of<IPropertySymbol, IFieldSymbol>(p))
                                .Concat(
                                    s.GetMembers()
                                        .OfType<IFieldSymbol>()
                                        .Where(p => !p.IsReadOnly && p.GetAttributes().Any(hasMustInitialize))
                                        .Select(p => new Of<IPropertySymbol, IFieldSymbol>(p))));
    }

    public static IEnumerable<Of<IPropertySymbol, IFieldSymbol>> GetMembersWithMustInitialize(ITypeSymbol symbol, INamedTypeSymbol[] mustInitializeSymbols)
    {
        // We don't need the interfaces, since we require to specify it directly on the implementation, and c# 8 default interfaces are not allowed
        var symbols = new[] { symbol }.Concat(symbol.GetAllBaseTypes());
        var result = GetMembersWithMustInitialize(symbols, mustInitializeSymbols);

        var byNames = result.GroupBy(r => r.As<ISymbol>()!.Name);
        return byNames.Select(n => n.First());
    }


    public static IEnumerable<string> GetNotInitializedNames(ObjectCreationExpressionSyntax typeDecl, ITypeSymbol symbol, INamedTypeSymbol[] mustInitializeSymbols)
    {
        var props = GetMembersWithMustInitialize(symbol, mustInitializeSymbols).Select(m => m.As<ISymbol>()!.Name);

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

    protected void ReportDiagnostics(SyntaxNodeAnalysisContext context, CSharpSyntaxNode node, IEnumerable<string> props)
    {
        if (!props.Any()) return;

        var combined = string.Join(", ", props);
        var diagnostic = Diagnostic.Create(DiagnosticDesc, node.GetLocation(), combined);

        context.ReportDiagnostic(diagnostic);
    }
}
