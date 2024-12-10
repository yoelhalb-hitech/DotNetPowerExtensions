using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SequelPay.DotNetPowerExtensions.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SequelPay.DotNetPowerExtensions.Roslyn;

public static class SyntaxGeneration
{
    /// <summary>
    /// Create a new <see cref="MethodDeclarationSyntax" /> for a method with no parameters.
    /// </summary>
    /// <param name="typeName">Name of the return type</param>
    /// <param name="name">Name of the method</param>
    public static MethodDeclarationSyntax Method(string typeName, string name)
        => MethodDeclaration(GetTypeSyntax(typeName), Identifier(name));

    /// <summary>
    /// Create a new <see cref="MethodDeclarationSyntax" /> for a method with no parameters and void return type.
    /// </summary>
    /// <param name="name">Name of the method</param>
    public static MethodDeclarationSyntax VoidMethod(string name)
        => Method("void", name);

    /// <summary>
    /// Create a new <see cref="MethodDeclarationSyntax" /> for a method with no parameters.
    /// </summary>
    /// <param name="returnType"><see cref="Type"> of the return type</param>
    /// <param name="name">Name of the method</param>
    public static MethodDeclarationSyntax Method(Type returnType, string name)
        => Method(returnType.ToCSharpTypeString(false, false, null), name);

    /// <summary>
    /// Create a new <see cref="MethodDeclarationSyntax" /> for a method with no parameters.
    /// </summary>
    /// <typeparam name="TReturn">The method return type</typeparam>
    /// <param name="name">Name of the method</param>
    public static MethodDeclarationSyntax Method<TReturn>(string name) => Method(typeof(TReturn), name);

    /// <summary>
    /// Create a new <see cref="MethodDeclarationSyntax" /> for a method with no parameters.
    /// </summary>
    /// <typeparam name="TReturn">The method return type</typeparam>
    /// <param name="name">Name of the method</param>
    /// <param name="parameters">Parameters of the method</param>
    /// <remarks>Parameters can be generated with one of the <see cref="Param"/> overloads</remarks>
    public static MethodDeclarationSyntax Method<TReturn>(string name, params ParameterSyntax[] parameters)
            => Method(typeof(TReturn), name).WithParameters(parameters);

    /// <summary>
    /// Create a new <see cref="MethodDeclarationSyntax" /> for an existing <see cref="MethodDeclarationSyntax"/> with added parameters.
    /// </summary>
    /// <param name="name">Existing <see cref="MethodDeclarationSyntax"/></param>
    /// <param name="parameters">Parameters of the method</param>
    /// <remarks>Parameters can be generated with one of the <see cref="Param"/> overloads</remarks>
    public static MethodDeclarationSyntax WithParameters(this MethodDeclarationSyntax method,
                        params ParameterSyntax[] parameters) =>
        method.WithParameterList(ParameterList(
                    SeparatedList<ParameterSyntax>(parameters)));

    /// <summary>
    /// Create a <see cref="ParameterSyntax" /> representing a method parameter
    /// </summary>
    /// <param name="typeName">Name of the parameter type</param>
    /// <param name="name">Name of the parameter</param>
    public static ParameterSyntax Param(string typeName, string name)
        => Parameter(Identifier(name)).WithType(GetTypeSyntax(typeName));

    /// <summary>
    /// Create a <see cref="ParameterSyntax" /> representing a method parameter
    /// </summary>
    /// <param name="paramType"><see cref="Type"> of the parameter</param>
    /// <param name="name">Name of the parameter</param>
    public static ParameterSyntax Param(Type paramType, string name)
        => Param(paramType.ToCSharpTypeString(false, false, null), name);

    /// <summary>
    /// Create a <see cref="ParameterSyntax" /> representing a method parameter
    /// </summary>
    /// <typeparam name="TParam">The parameter type</typeparam>
    /// <param name="name">Name of the parameter</param>
    public static ParameterSyntax Param<TParam>(string name)
        => Param(typeof(TParam), name);

    /// <summary>
    /// Create a <see cref="ParameterSyntax" /> representing a method parameter of type string
    /// </summary>
    /// <param name="name">Name of the parameter</param>
    public static ParameterSyntax StringParam(string name)
        => Param(typeof(string), name);

    /// <summary>
    /// Create a <see cref="ParameterSyntax" /> representing a method parameter of type int
    /// </summary>
    /// <param name="name">Name of the parameter</param>
    public static ParameterSyntax IntParam(string name)
        => Param(typeof(int), name);


    /// <summary>
    /// Create a <see langword="out"/> <see cref="ParameterSyntax" /> representing a method parameter
    /// </summary>
    /// <param name="typeName">Name of the parameter type</param>
    /// <param name="name">Name of the parameter</param>
    public static ParameterSyntax OutParam(string typeName, string name)
        => Param(typeName, name).WithModifiers(new SyntaxTokenList(Token(SyntaxKind.OutKeyword)));


    /// <summary>
    /// Create a <see langword="out"/> <see cref="ParameterSyntax" /> representing a method parameter
    /// </summary>
    /// <param name="paramType"><see cref="Type"> of the parameter</param>
    /// <param name="name">Name of the parameter</param>
    public static ParameterSyntax OutParam(Type paramType, string name)
        => Param(paramType, name).WithModifiers(new SyntaxTokenList(Token(SyntaxKind.OutKeyword)));


    /// <summary>
    /// Create a <see langword="out"/> <see cref="ParameterSyntax" /> representing a method parameter
    /// </summary>
    /// <typeparam name="TParam">The parameter type</typeparam>
    /// <param name="name">Name of the parameter</param>
    public static ParameterSyntax OutParam<TParam>(string name)
        => Param(typeof(TParam), name).WithModifiers(new SyntaxTokenList(Token(SyntaxKind.OutKeyword)));

    /// <summary>
    /// Create a <see langword="ref"/> <see cref="ParameterSyntax" /> representing a method parameter
    /// </summary>
    /// <param name="typeName">Name of the parameter type</param>
    /// <param name="name">Name of the parameter</param>
    public static ParameterSyntax RefParam(string typeName, string name)
        => Param(typeName, name).WithModifiers(new SyntaxTokenList(Token(SyntaxKind.RefKeyword)));

    /// <summary>
    /// Create a <see langword="ref"/> <see cref="ParameterSyntax" /> representing a method parameter
    /// </summary>
    /// <param name="paramType"><see cref="Type"> of the parameter</param>
    /// <param name="name">Name of the parameter</param>
    public static ParameterSyntax RefParam(Type paramType, string name)
        => Param(paramType, name).WithModifiers(new SyntaxTokenList(Token(SyntaxKind.RefKeyword)));

    /// <summary>
    /// Create a <see langword="ref"/> <see cref="ParameterSyntax" /> representing a method parameter
    /// </summary>
    /// <typeparam name="TParam">The parameter type</typeparam>
    /// <param name="name">Name of the parameter</param>
    public static ParameterSyntax RefParam<TParam>(string name)
        => Param(typeof(TParam), name).WithModifiers(new SyntaxTokenList(Token(SyntaxKind.RefKeyword)));

    private static TypeSyntax GetTypeSyntax(string typeName)
        =>  typeName switch
        {
            "bool" => SyntaxKind.BoolKeyword,
            "byte" => SyntaxKind.ByteKeyword,
            "sbyte" => SyntaxKind.SByteKeyword,
            "short" => SyntaxKind.ShortKeyword,
            "ushort" => SyntaxKind.UShortKeyword,
            "int" => SyntaxKind.IntKeyword,
            "uint" => SyntaxKind.UIntKeyword,
            "long" => SyntaxKind.LongKeyword,
            "ulong" => SyntaxKind.ULongKeyword,
            "double" => SyntaxKind.DoubleKeyword,
            "float" => SyntaxKind.FloatKeyword,
            "decimal" => SyntaxKind.DecimalKeyword,
            "string" => SyntaxKind.StringKeyword,
            "char" => SyntaxKind.CharKeyword,
            "void" => SyntaxKind.VoidKeyword,
            "object" => SyntaxKind.ObjectKeyword,
            _ => (SyntaxKind?)null
        } is var keyword && keyword.HasValue ? PredefinedType(Token(keyword.Value)) : IdentifierName(typeName);
}
