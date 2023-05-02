using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SequelPay.DotNetPowerExtensions.RoslynExtensions.Utils;

namespace SequelPay.DotNetPowerExtensions.RoslynExtensions;

public static class SyntaxFactoryExtensions
{
    public static AttributeListSyntax ParseAttribute(string attributeText)
        => AttributeList(ParseStatement(attributeText).AttributeLists.First().Attributes);

    public static PropertyDeclarationSyntax CreatePropertyOverride(PropertyDeclarationSyntax baseSyntax)
        => PropertyOverrideFactory.CreatePropertyOverride(baseSyntax);
}
