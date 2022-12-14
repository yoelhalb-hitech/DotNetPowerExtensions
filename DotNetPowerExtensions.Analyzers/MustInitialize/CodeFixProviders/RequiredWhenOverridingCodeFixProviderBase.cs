using DotNetPowerExtensions.MustInitialize;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using DotNetPowerExtensionsAnalyzer.MustInitialize.Analyzers;
using DotNetPowerExtensionsAnalyzer.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetPowerExtensionsAnalyzer.MustInitialize.CodeFixProviders;

public abstract class RequiredWhenOverridingCodeFixProviderBase<TAnalyzer>
    : ByAttributeCodeFixProviderBase<TAnalyzer, PropertyDeclarationSyntax>
                        where TAnalyzer : MustInitializeRequiredWhenOverriding, IMustInitializeAnalyzer
{
    protected virtual AttributeSyntax GetAttribute(IPropertySymbol prop, INamedTypeSymbol mustInitializeSymbol)
    {
        var baseTypes = prop.ContainingType.GetAllBaseTypes();
        var name = prop.Name;

        foreach (var baseType in baseTypes)
        {
            var member = baseType.GetMembers(name).FirstOrDefault();
            if (member is null) continue;

            return member.GetAttribute(mustInitializeSymbol)?.ApplicationSyntaxReference?.GetSyntax() as AttributeSyntax ?? throw new Exception("Base prop issue");
        }

        throw new Exception("Base prop not found");
    }

    protected override async Task<(SyntaxNode declToReplace, SyntaxNode newDecl)?> CreateChanges(Document document, PropertyDeclarationSyntax propertyDecl, CancellationToken c)
    {
        var symbol = await document.GetDeclaredSymbol<IPropertySymbol>(propertyDecl, c).ConfigureAwait(false);
        var mustInitializeClassMetadata = await document.GetTypeByMetadataName(AttributeType, c).ConfigureAwait(false);
        if (symbol is null || mustInitializeClassMetadata is null) return null;

        var attributeList = SyntaxFactory.AttributeList()
                        .WithAttributes(SyntaxFactory.SeparatedList(new[] { GetAttribute(symbol, mustInitializeClassMetadata) }));

        return (propertyDecl, propertyDecl.AddAttributeLists(attributeList));
    }
}
