using DotNetPowerExtensions.MustInitialize;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using DotNetPowerExtensionsAnalyzer.MustInitialize.Analyzers;
using DotNetPowerExtensionsAnalyzer.Utils;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetPowerExtensionsAnalyzer.MustInitialize.CodeFixProviders;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MustInitializeRequiredMembersCodeFixProvider)), Shared]
public class MustInitializeRequiredMembersCodeFixProvider : MustInitializeCodeFixProviderBase<MustInitializeRequiredMembers, ObjectCreationExpressionSyntax>
{
    protected override string Title => "Initialize Required Properties";

    protected Type[] Attributes =
    {
        typeof(DotNetPowerExtensions.MustInitialize.MustInitializeAttribute),
    };

    protected override async Task<(SyntaxNode, SyntaxNode)?> CreateChanges(Document document, ObjectCreationExpressionSyntax typeDecl, CancellationToken cancellationToken)
    {
        var symbol = (await document.GetTypeInfo(typeDecl, cancellationToken).ConfigureAwait(false))?.Type;

        var mustInitializeSymbols = await Task.WhenAll(
                Attributes.Select(async a => await document.GetTypeByMetadataName(a).ConfigureAwait(false))
        ).ConfigureAwait(false);
        if (symbol is null || mustInitializeSymbols.Any(s => s is null)) return null;

        var props = MustInitializeRequiredMembers.GetNotInitializedNames(typeDecl, symbol, mustInitializeSymbols.OfType<INamedTypeSymbol>().ToArray());

        var initalizer = typeDecl.Initializer
                        ?? SyntaxFactory.InitializerExpression(SyntaxKind.ObjectInitializerExpression);
        foreach (var prop in props ?? new string[] { })
        {
            var expr = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                                        SyntaxFactory.IdentifierName(prop),
                                                        SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression));
            initalizer = initalizer.AddExpressions(expr);
        }

        // Remmeber that everything is immutable
        if (typeDecl.Initializer is null)
        {
            return (typeDecl, typeDecl.WithInitializer(initalizer));
        }
            
        return (typeDecl.Initializer, initalizer); 
    }
}