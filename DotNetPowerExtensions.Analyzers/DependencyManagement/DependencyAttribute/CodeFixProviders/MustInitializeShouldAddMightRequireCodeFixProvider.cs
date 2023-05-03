using DotNetPowerExtensions.Analyzers.MustInitialize.MightRequireAttribute;
using DotNetPowerExtensions.Analyzers.DependencyManagement.DependencyAttribute.Analyzers;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.CodeActions;
using DotNetPowerExtensions.Analyzers.MustInitialize;

namespace DotNetPowerExtensions.Analyzers.DependencyManagement.CodeFixProviders;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MustInitializeShouldAddMightRequireCodeFixProvider)), Shared]
public class MustInitializeShouldAddMightRequireCodeFixProvider : CodeFixProvider
{
    protected string Title => "Initialize Required Properties";
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticId);
    protected string DiagnosticId => MustInitializeShouldAddMightRequire.DiagnosticId;

    public override FixAllProvider GetFixAllProvider()
    {
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
        return WellKnownFixAllProviders.BatchFixer;
    }
    private Type[] MustInitializeAttributes =
    {
        typeof(SequelPay.DotNetPowerExtensions.MustInitializeAttribute),
    };

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        try
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root is null) return;

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var declaration = root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();
            if (declaration is null) return;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: async c => await CreateFixedInterfaces(context.Document, diagnostic, declaration, c).ConfigureAwait(false),
                    equivalenceKey: Title),
                diagnostic);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
            throw;
        }
    }


    protected async Task<Document> CreateFixedInterfaces(Document document, Diagnostic diagnostic, TypeDeclarationSyntax declaration, CancellationToken c)
    {
        try
        {
            var typeName = diagnostic.Properties["Name"];
            if (string.IsNullOrWhiteSpace(typeName)) return document;

            var semanticModel = await document.GetSemanticModelAsync(c).ConfigureAwait(false);
            if (semanticModel is null) return document;

            var symbol = await document.GetDeclaredSymbolAsync<ITypeSymbol>(declaration, c).ConfigureAwait(false);
            if (symbol is null) return document;

            var bases = symbol.GetAllBaseTypes().Concat(symbol.AllInterfaces).Where(b => b.Name == typeName).ToArray();
            if (!bases.Any() || !bases.Any(b => b.Name == typeName)) return document;

            if (bases.Length > 1) bases = bases.Where(b => b.GetContainerFullName() == diagnostic.Properties["Namespace"]).ToArray();
            if (!bases.Any()) return document;

            var worker = new MustInitializeWorker(semanticModel);

            var members = MustInitializeShouldAddMightRequire.GetMightRequireCandidates(symbol, bases, worker);
            if (members is null) return document;

            var documentEditor = await DocumentEditor.CreateAsync(document, c).ConfigureAwait(false);
            foreach (var b in bases) // In case we have 2 with the same name
            {
                var baseDecl = b.GetSyntax<TypeDeclarationSyntax>().FirstOrDefault();
                if(baseDecl is null) continue;

                documentEditor.ReplaceNode(baseDecl, baseDecl.AddAttributeLists(GetAttributeList(members[b]).ToArray()));
            }

            return documentEditor.GetChangedDocument();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
            throw;
        }
    }

    private IEnumerable<AttributeListSyntax> GetAttributeList(List<Union<IPropertySymbol, IFieldSymbol>> list)
    {
        foreach (var item in list)
        {
            var attrName = nameof(MightRequireAttribute).Replace(nameof(Attribute), "");
            var typeName = (item.First?.Type ?? item.Second?.Type)!.ToStringWithoutNamesapce();

            var expression = $"""[{attrName}("{item.As<ISymbol>()!.Name}", typeof({typeName}))]""";

            yield return SyntaxFactoryExtensions.ParseAttribute(expression);
        }
    }
}