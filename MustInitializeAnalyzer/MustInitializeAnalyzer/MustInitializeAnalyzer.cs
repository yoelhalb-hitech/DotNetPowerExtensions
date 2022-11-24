using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using DotNetPowerExtensions.MustInitialize;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MustInitializeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MustInitializeAnalyzer : DiagnosticAnalyzer
    {
        public static readonly string MustInitializeAttributeFullName = typeof(MustInitializeAttribute).FullName;

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

        public static Func<AttributeListSyntax, bool> BasicPredicate = al => al.Attributes
                        .Any(a =>
                        {
                            var name = a.Name as IdentifierNameSyntax ?? (a.Name as QualifiedNameSyntax)?.Right as IdentifierNameSyntax;
                            return name?.Identifier.ValueText == mustInitialize || name?.Identifier.ValueText == "MustInitializeAttribute";
                        });
        private void AnalyzeProperty(SyntaxNodeAnalysisContext context)
        {
            try
            {
                var expr = context.Node as PropertyDeclarationSyntax;
                if (expr is null || !expr.AttributeLists.Any(BasicPredicate)) return;

                // First we checked with syntax expressions for optimization, now we need to actually check if it is our attribute
                var mustInitializeClassMetadata = context.Compilation.GetTypeByMetadataName(MustInitializeAttributeFullName);
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

                if (!IsAccessibilityValid(prop.DeclaredAccessibility, expr.Parent, context))
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
                if (expr is null || !expr.AttributeLists.Any(BasicPredicate)) return;

                // First we checked with syntax expressions for optimization, now we need to actually check if it is our attribute
                var mustInitializeClassMetadata = context.Compilation.GetTypeByMetadataName(MustInitializeAttributeFullName);
                if (mustInitializeClassMetadata == null) return;

                // Note we need to filter by IFieldSymbol and not just take the first, in case the type and the field name are the same....
                var prop = context.Compilation.GetSymbolsWithName(expr.Declaration.Variables.First().Identifier.ValueText).OfType<IFieldSymbol>().First();
                if (!prop.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, mustInitializeClassMetadata))) return;

                if (prop.IsReadOnly)
                {
                    var diagnostic = Diagnostic.Create(WritableRule, Location.Create(expr.SyntaxTree, expr.Span));

                    context.ReportDiagnostic(diagnostic);
                }

                if (!IsAccessibilityValid(prop.DeclaredAccessibility, expr.Parent, context))
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

        private bool IsAccessibilityValid(Accessibility accessibility, SyntaxNode parent, SyntaxNodeAnalysisContext context)
        {
            var className = parent.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().First().Identifier.Text;
            var classSymbol = context.Compilation.GetSymbolsWithName(className).First() as INamedTypeSymbol;
            if(classSymbol is null)
            {
                Logger.LogInfo("Class missing for symbol, how is that possible?");
                return true;
            }

            var ctorAccessibilty = classSymbol.Constructors.Select(c => c.DeclaredAccessibility).Min();

            return accessibility >= ctorAccessibilty;
        }

        private void AnalyzeCreation(SyntaxNodeAnalysisContext context)
        {
            try
            {
                //if(context.SemanticModel.Compilation.get)
                var expr = context.Node as ObjectCreationExpressionSyntax;
                if (expr is null) return;

                var symbol = context.SemanticModel.GetTypeInfo(expr).Type as ITypeSymbol;

                // First we checked with syntax expressions for optimization, now we need to actually check if it is our attribute
                var mustInitializeClassMetadata = context.Compilation.GetTypeByMetadataName(MustInitializeAttributeFullName);
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