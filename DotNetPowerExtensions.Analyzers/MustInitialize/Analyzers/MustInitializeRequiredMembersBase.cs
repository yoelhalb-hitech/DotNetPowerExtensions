using DotNetPowerExtensions.Analyzers.Utils;
using SequelPay.DotNetPowerExtensions;
using System.Linq;

namespace DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;

public abstract class MustInitializeRequiredMembersBase : MustInitializeAnalyzerBase
{
    protected override bool IncludeInitializedAttribute => false;

    public static IEnumerable<Union<IPropertySymbol, IFieldSymbol>> GetMembersWithMustInitialize(ITypeSymbol symbol, INamedTypeSymbol[] mustInitializeSymbols)
    {
        // We don't need the interfaces, since we require to specify it directly on the implementation, and c# 8 default interfaces are not allowed
        var bases = symbol.GetAllBaseTypes().ToList();
        var symbols = new[] { symbol }.Concat(bases);
        var allMembers = symbols.SelectMany(s => s.GetMembers()
                                                    .OfType<IPropertySymbol>()
                                                    .Select(p => new Union<IPropertySymbol, IFieldSymbol>(p))
                                                    .Concat(
                                                            s.GetMembers()
                                                            .OfType<IFieldSymbol>()
                                                            .Select(p => new Union<IPropertySymbol, IFieldSymbol>(p))));

        var byNames = allMembers.GroupBy(r => r.As<ISymbol>()!.Name);

        // We take the closest base, this way it has been marked with `Initialized` instead we will be fine
        // Remember that each override must be marked and hiding is not allowed (unless `Initialized`)
        return byNames
                .Select(n => n.OrderBy(x => bases.IndexOf(x.As<ISymbol>()!.ContainingType)).First())
                .Where(n => n.As<ISymbol>()!.HasAttribute(mustInitializeSymbols));
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
