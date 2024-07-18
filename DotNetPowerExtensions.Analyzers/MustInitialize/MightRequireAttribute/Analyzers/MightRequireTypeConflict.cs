using SequelPay.DotNetPowerExtensions.RoslynExtensions;
using SequelPay.DotNetPowerExtensions;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace DotNetPowerExtensions.Analyzers.MustInitialize.MightRequireAttribute.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MightRequireTypeConflict : DiagnosticAnalyzer
{
    protected const string Category = "Language";
    public const string DiagnosticId = "DNPE0214";
    protected const string Title = "MightRequireShouldBeLocal";
    protected const string Message = "Ambiguous MightRequire definiton for `{0}` having types `{1}`";
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
                Func<Type, INamedTypeSymbol?> metadata = t => compilationContext.Compilation.GetTypeSymbol(t);

                var mightRequireSymbols = MightRequireUtils.Attributes.Select(a => metadata(a)).OfType<INamedTypeSymbol>().ToArray();
                if (!mightRequireSymbols.Any()) return;

                compilationContext
                    .RegisterSyntaxNodeAction(c => AnalyzeClass(c, mightRequireSymbols), SyntaxKind.ClassDeclaration, SyntaxKind.InterfaceDeclaration,
                                                                            SyntaxKind.RecordDeclaration, SyntaxKind.StructDeclaration, SyntaxKind.RecordStructDeclaration);
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }

    private void AnalyzeClass(SyntaxNodeAnalysisContext context, INamedTypeSymbol[] mightRequireSymbols)
    {
        try
        {
            // Since a class decleration can be partial we have to go by the symbol
            var decl = context.Node as TypeDeclarationSyntax;
            if (decl is null) return;

            var symbol = context.SemanticModel.GetDeclaredSymbol(decl);
            if (symbol is null || symbol.GetSyntax<TypeDeclarationSyntax>(context.CancellationToken).First() != decl) return; // For a parital class only do it the first time

            var attributes = MightRequireUtils.GetMightRequiredInfos(symbol, mightRequireSymbols);
            var attributesGrouped = attributes.GroupBy(a => a.Name);
            var attributesWithMultiple = attributesGrouped
                                                .Where(g => g.Any(g1 => !g1.Type.IsEqualTo(g.First().Type)))
                                                // Either it is on the current type or they are from different interfaces/base, otherwise the base/interface should handle it
                                                // TODO... if they are different interfaces/base but via a single base/interface then we don't need to warn in the sub
                                                .Where(g => g.All(g1 => g1.ContainingSymbol.IsEqualTo(symbol)) || g.Any(g1 => !g1.ContainingSymbol.IsEqualTo(g.First().ContainingSymbol)));

            foreach (var attGroup in attributesWithMultiple)
            {
                var types = string.Join(", ", attGroup.Select(g => (g.ContainingSymbol.IsEqualTo(symbol) ? "" : g.ContainingSymbol.Name + ".") + g.Type?.Name).Distinct());
                var locations = attGroup.Select(a => a.Attribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation()).OfType<Location>();

                foreach (var location in locations)
                {
                    var diagnostic = Microsoft.CodeAnalysis.Diagnostic.Create(Diagnostic, location, attGroup.Key, types);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}
