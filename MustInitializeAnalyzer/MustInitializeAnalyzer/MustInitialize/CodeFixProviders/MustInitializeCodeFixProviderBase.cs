﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using DotNetPowerExtensionsAnalyzer.MustInitialize.Analyzers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetPowerExtensionsAnalyzer.MustInitialize.CodeFixProviders;

public abstract class MustInitializeCodeFixProviderBase<TAnalyzer, TNode> : CodeFixProvider
                                            where TAnalyzer : MustInitializeAnalyzerBase, IMustInitializeAnalyzer
                                            where TNode : CSharpSyntaxNode
{
    protected abstract string Title { get; }

    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(TAnalyzer.DiagnosticId);

    public override FixAllProvider GetFixAllProvider()
    {
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
        return WellKnownFixAllProviders.BatchFixer;
    }

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        try
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root is null) return;

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var declaration = root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<TNode>().First();
            if (declaration is null) return;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: async c => await CreateFixedDocument(context.Document, declaration, c).ConfigureAwait(false),
                    equivalenceKey: Title),
                diagnostic);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
            throw;
        }
    }

    protected abstract Task<(SyntaxNode declToReplace, SyntaxNode newDecl)?> CreateChanges(Document document, TNode declaration, CancellationToken c);

    protected async Task<Document> CreateFixedDocument(Document document, TNode declaration, CancellationToken c)
    {
        try
        {
            var documentEditor = await DocumentEditor.CreateAsync(document).ConfigureAwait(false);

            var result = await CreateChanges(document, declaration, c).ConfigureAwait(false);
            if (!result.HasValue) return document;

            var (declToReplace, newDecl) = result.Value;

            documentEditor.ReplaceNode(declToReplace, newDecl);

            return documentEditor.GetChangedDocument();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
            throw;
        }
    }
}
