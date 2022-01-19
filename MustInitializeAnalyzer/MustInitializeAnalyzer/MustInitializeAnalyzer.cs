using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MustInitializeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MustInitializeAnalyzer : DiagnosticAnalyzer
    {
        private const string mustInitialize = "MustInitialize";

        public const string DiagnosticId = mustInitialize;
        public const string WritableDiagnosticId = mustInitialize + "Writable";
        public const string AccessibleDiagnosticId = mustInitialize + "Accessible";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        //private const string Category = "Naming";
        private const string Category = "Language";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);
        private const string Writable = mustInitialize + " property must be writable";
        private const string Accessible = mustInitialize + " accessibility cannot be less than the constuctur";
        private static DiagnosticDescriptor WritableRule = new DiagnosticDescriptor(WritableDiagnosticId, Writable, Writable, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Writable);
        private static DiagnosticDescriptor AccessibleRule = new DiagnosticDescriptor(AccessibleDiagnosticId, Accessible, Accessible, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Accessible);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule, WritableRule, AccessibleRule); } }

        public override void Initialize(AnalysisContext context)
        {
            try
            {
                context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
                context.EnableConcurrentExecution();

                // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
                // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information

                context.RegisterSyntaxNodeAction(AnalyzeProperty, new[] { SyntaxKind.PropertyDeclaration });
                context.RegisterSyntaxNodeAction(AnalyzeField, new[] { SyntaxKind.FieldDeclaration });
                context.RegisterSyntaxNodeAction(AnalyzeCreation, new[] { SyntaxKind.ObjectCreationExpression });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                throw;
            }
        }

        Func<AttributeListSyntax, bool> predicate = al => al.Attributes.Any(a => a.Name is IdentifierNameSyntax name && name.Identifier.ValueText == mustInitialize);
        private void AnalyzeProperty(SyntaxNodeAnalysisContext context)
        {
            try
            {
                var expr = context.Node as PropertyDeclarationSyntax;
                if (!expr?.AttributeLists.Any(predicate) ?? false) return;

                // First we checked with syntax expressions for optimization, now we need to actually check if it is our attribute
                var mustInitializeName = typeof(MustInitializeAttribute).FullName;
                var mustInitializeClassMetadata = context.Compilation.GetTypeByMetadataName(mustInitializeName);
                if (mustInitializeClassMetadata == null) return;

                var metadataDefinition = mustInitializeClassMetadata.OriginalDefinition;

                // Note we need to filter by IPropertySymbol and not just take the first, in case the type and the property name are the same...
                var prop = context.Compilation.GetSymbolsWithName(expr.Identifier.ValueText).OfType<IPropertySymbol>().First();         
                if (!prop.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, mustInitializeClassMetadata))) return;
             
                if (prop.IsReadOnly)
                {
                    var diagnostic = Diagnostic.Create(WritableRule, Location.Create(expr.SyntaxTree, expr.Span));

                    context.ReportDiagnostic(diagnostic);
                }

                var parent = context.Compilation.GetSymbolsWithName(expr.Parent.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().First().Identifier.Text).OfType<INamedTypeSymbol>().First();
                var accessibilty = parent.Constructors.Select(c => c.DeclaredAccessibility).Min();

                if (prop.DeclaredAccessibility < accessibilty)
                {
                    var diagnostic = Diagnostic.Create(AccessibleRule, Location.Create(expr.SyntaxTree, expr.Span));
                    context.ReportDiagnostic(diagnostic);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                throw;
            }
        }

        private void AnalyzeField(SyntaxNodeAnalysisContext context)
        {
            try
            {
                var expr = context.Node as FieldDeclarationSyntax;
                if (!expr.AttributeLists.Any(predicate)) return;

                // First we checked with syntax expressions for optimization, now we need to actually check if it is our attribute
                var mustInitializeName = typeof(MustInitializeAttribute).FullName;
                var mustInitializeClassMetadata = context.Compilation.GetTypeByMetadataName(mustInitializeName);
                if (mustInitializeClassMetadata == null) return;

                // Note we need to filter by IFieldSymbol and not just take the first, in case the type and the field name are the same....
                var prop = context.Compilation.GetSymbolsWithName(expr.Declaration.Variables.First().Identifier.ValueText).OfType<IFieldSymbol>().First();
                if (!prop.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, mustInitializeClassMetadata))) return;

                if (prop.IsReadOnly)
                {
                    var diagnostic = Diagnostic.Create(WritableRule, Location.Create(expr.SyntaxTree, expr.Span));

                    context.ReportDiagnostic(diagnostic);
                }

                var parent = context.Compilation.GetSymbolsWithName(expr.Parent.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().First().Identifier.Text).First() as INamedTypeSymbol;
                var accessibilty = parent.Constructors.Select(c => c.DeclaredAccessibility).Min();

                if (prop.DeclaredAccessibility < accessibilty)
                {
                    var diagnostic = Diagnostic.Create(AccessibleRule, Location.Create(expr.SyntaxTree, expr.Span));

                    context.ReportDiagnostic(diagnostic);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                throw;
            }
        }

        private void AnalyzeCreation(SyntaxNodeAnalysisContext context)
        {
            try
            {
                //if(context.SemanticModel.Compilation.get)
                var expr = context.Node as ObjectCreationExpressionSyntax;

                var symbol = context.SemanticModel.GetTypeInfo(expr).Type as ITypeSymbol;

                // First we checked with syntax expressions for optimization, now we need to actually check if it is our attribute
                var mustInitializeName = typeof(MustInitializeAttribute).FullName;
                var mustInitializeClassMetadata = context.Compilation.GetTypeByMetadataName(mustInitializeName);
                if (mustInitializeClassMetadata == null) return;

                var props = symbol.GetMembers()
                                    .OfType<IPropertySymbol>()
                                    .Where(p => !p.IsReadOnly && p.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, mustInitializeClassMetadata)))
                                    .Select(p => p.Name)
                                    .Union(
                                        symbol.GetMembers()
                                            .OfType<IFieldSymbol>()
                                            .Where(p => !p.IsReadOnly && p.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, mustInitializeClassMetadata)))
                                            .Select(p => p.Name));

                var objectInitializeExpr = expr.ChildNodes().OfType<InitializerExpressionSyntax>().SingleOrDefault();
                if (objectInitializeExpr != null)
                {
                    var childs = objectInitializeExpr.ChildNodes();
                    var propsInitialized = childs.OfType<IdentifierNameSyntax>()
                            .Union(childs.OfType<AssignmentExpressionSyntax>().Select(c => c.Left).OfType<IdentifierNameSyntax>())
                        .Select(c => c.Identifier.Text);

                    props = props.Except(propsInitialized);
                }
                if (props.Any())
                {
                    var combined = string.Join(", ", props);
                    var diagnostic = Diagnostic.Create(Rule, Location.Create(expr.SyntaxTree, expr.Span), props.Skip(1).Any() ? "ies" : "y", combined);

                    context.ReportDiagnostic(diagnostic);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                throw;
            }
        }
    }
}