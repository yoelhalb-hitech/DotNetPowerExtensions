using DotNetPowerExtensions.MustInitialize;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using DotNetPowerExtensionsAnalyzer.MustInitialize.Analyzers;
using DotNetPowerExtensionsAnalyzer.MustInitialize.MustInitializeAttribute.Analyzers;
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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CannotUseBaseImplementationForMustInitializeCodeFixProvider)), Shared]
public class CannotUseBaseImplementationForMustInitializeCodeFixProvider : CannotUseBaseImplementationCodeFixProviderBase<CannotUseBaseImplementationForMustInitialize>
{
    protected override string Title => "Implement Required Properties";

    protected override Type AttributeType => typeof(DotNetPowerExtensions.MustInitialize.MustInitializeAttribute);

    protected override AttributeSyntax GetAttribute(IPropertySymbol prop, INamedTypeSymbol mustInitializeSymbol)
        => SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(AttributeType.Name.Replace(nameof(Attribute), ""))); // No need to go through the interfaces
}
