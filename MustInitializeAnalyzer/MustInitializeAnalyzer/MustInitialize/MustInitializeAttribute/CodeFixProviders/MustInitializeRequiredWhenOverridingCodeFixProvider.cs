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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MustInitializeRequiredWhenOverridingCodeFixProvider)), Shared]
public class MustInitializeRequiredWhenOverridingCodeFixProvider
                    : RequiredWhenOverridingCodeFixProviderBase<MustInitializeRequiredWhenOverriding>
{
    protected override string Title => "Add MustInitialize";

    protected override Type AttributeType => typeof(DotNetPowerExtensions.MustInitialize.MustInitializeAttribute);

    protected override AttributeSyntax GetAttribute(IPropertySymbol prop, INamedTypeSymbol mustInitializeSymbol)
        => SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(AttributeType.Name.Replace(nameof(Attribute), ""))); // No need to go through the base class
}
