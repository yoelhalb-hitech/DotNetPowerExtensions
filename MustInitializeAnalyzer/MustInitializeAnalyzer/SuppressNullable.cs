using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using DotNetPowerExtensions.MustInitialize;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MustInitializeAnalyzer
{
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
                    AnalyzeDiagnostic(diagnostic, context);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                throw;
            }
        }
        private const string mustInitialize = "MustInitialize";
        private bool ContainsMustInitialize(MemberDeclarationSyntax member, SuppressionAnalysisContext context, string name)
        {
            if (!member.AttributeLists.Any(al => al.Attributes.Any(a => a.Name is IdentifierNameSyntax identifierName &&
                    identifierName.Identifier.ValueText == mustInitialize))) return false;

            // Make sure it is the correct type and not just something with the same name...            
            var mustInitializeDecl = context.Compilation.GetTypeByMetadataName(typeof(MustInitializeAttribute).FullName);
            if (mustInitializeDecl == null) return false;

            //var propSymbol = context.Compilation.GetSymbolsWithName(name).First(); This confuses everything in the project...
            var propSymbol = context.Compilation.GetSemanticModel(member.SyntaxTree).GetDeclaredSymbol(member);
            return propSymbol.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, mustInitializeDecl));
        }

        private void AnalyzeDiagnostic(Diagnostic diagnostic, SuppressionAnalysisContext context)
        {
            try
            {
                var node = diagnostic.Location.SourceTree.GetRoot(context.CancellationToken).FindNode(diagnostic.Location.SourceSpan);

                if (node is PropertyDeclarationSyntax prop)
                {
                    if (!ContainsMustInitialize(prop, context, prop.Identifier.ValueText)) return;
                }
                else if ((node.Parent.Parent) is FieldDeclarationSyntax f)
                {
                    if (!ContainsMustInitialize(f, context, (node as VariableDeclaratorSyntax).Identifier.Text)) return;
                }
                else if (node is ConstructorDeclarationSyntax)
                {
                    var regex = new Regex(@"(\S*)\s*'(.*)'");
                    var match = regex.Match(diagnostic.GetMessage());
                    var type = match.Groups[1].Value;
                    var name = match.Groups[2].Value;

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
}
