using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Editing;
using System;

namespace MustInitializeAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MustInitializeCodeFixProvider)), Shared]
    public class MustInitializeCodeFixProvider : CodeFixProvider
    {
        private const string title = "Initialize Required Properties";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(MustInitializeAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            try
            {
                var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

                // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
                var diagnostic = context.Diagnostics.First();
                var diagnosticSpan = diagnostic.Location.SourceSpan;

                // Find the type declaration identified by the diagnostic.
                //var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();
                var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<ObjectCreationExpressionSyntax>().First();

                // Register a code action that will invoke the fix.
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: title,
                        //createChangedSolution: c => MakeUppercaseAsync(context.Document, declaration, c),
                        createChangedDocument: c => MakeInitializeAsync(context.Document, declaration, c),
                        equivalenceKey: title),
                    diagnostic);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                throw;
            }
        }

        private async Task<Document> MakeInitializeAsync(Document document, ObjectCreationExpressionSyntax typeDecl, CancellationToken cancellationToken)
        {
            try
            {
                var documentEditor = await DocumentEditor.CreateAsync(document);

                var initalizer = typeDecl.Initializer;
                if (typeDecl.Initializer == null)
                {
                    initalizer = SyntaxFactory.InitializerExpression(SyntaxKind.ObjectInitializerExpression);
                }

                var symbol = document.GetSemanticModelAsync().Result.GetTypeInfo(typeDecl).Type as ITypeSymbol;
                var mustInitializeName = typeof(MustInitializeAttribute).FullName;

                var mustInitializeClassMetadata = document.GetSemanticModelAsync().Result.Compilation.GetTypeByMetadataName(mustInitializeName);
                var props = symbol.GetMembers()
                                    .OfType<IPropertySymbol>()
                                    .Where(p => !p.IsReadOnly && p.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, mustInitializeClassMetadata)))
                                    .Select(p => p.Name)
                                    .Union(
                                        symbol.GetMembers()
                                            .OfType<IFieldSymbol>()
                                            .Where(p => !p.IsReadOnly && p.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, mustInitializeClassMetadata)))
                                            .Select(p => p.Name));

                if (typeDecl.Initializer != null)
                {
                    var childs = typeDecl.Initializer.ChildNodes();
                    var propsInitialized = childs.OfType<IdentifierNameSyntax>()
                            .Union(childs.OfType<AssignmentExpressionSyntax>().Select(c => c.Left).OfType<IdentifierNameSyntax>())
                        .Select(c => c.Identifier.Text);

                    props = props.Except(propsInitialized);
                }

                foreach (var prop in props)
                {
                    var expr = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                                                    SyntaxFactory.IdentifierName(prop),
                                                                    SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression));
                    initalizer = initalizer.AddExpressions(expr);
                }

                // Remmeber that everything is immutable
                if (typeDecl.Initializer == null)
                {
                    documentEditor.ReplaceNode(typeDecl, typeDecl.WithInitializer(initalizer));
                }
                else
                {
                    documentEditor.ReplaceNode(typeDecl.Initializer, initalizer);
                }

                return documentEditor.GetChangedDocument();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                throw;
            }
        } 
    }
}
