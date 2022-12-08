using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetPowerExtensionsAnalyzer.Utils;

internal static class SyntaxExtensions
{
    // From https://andrewlock.net/creating-a-source-generator-part-5-finding-a-type-declarations-namespace-and-type-hierarchy/#finding-the-namespace-for-a-class-syntax
    public static string GetNamespace(this BaseTypeDeclarationSyntax syntax)
    {
        string nameSpace = string.Empty;

        var namespaceParent = syntax.FirstAncestorOrSelf<BaseNamespaceDeclarationSyntax>();
        if (namespaceParent is null) return nameSpace;

        nameSpace = namespaceParent.Name.ToString();
        
        while ((namespaceParent = namespaceParent!.FirstAncestorOrSelf<BaseNamespaceDeclarationSyntax>()) is not null)
            nameSpace = $"{namespaceParent.Name}.{nameSpace}";

        return nameSpace;
    }

    // TODO... so far this doesn't work on inner methods...
    public static string GetContainerFullName(this MemberDeclarationSyntax syntax)
    {
        string name = string.Empty;

        var classDecl = syntax.FirstAncestorOrSelf<BaseTypeDeclarationSyntax>();
        if (classDecl is null) return name;

        name = classDecl.Identifier.Text.ToString();

        
        while (true)
        {
            var newDecl = classDecl!.FirstAncestorOrSelf<BaseTypeDeclarationSyntax>();
            if (newDecl is null) break;

            classDecl = newDecl;
            name = $"{classDecl.Identifier.Text}+{name}";
        }

        return GetNamespace(classDecl) + "." + name;

    }
}
