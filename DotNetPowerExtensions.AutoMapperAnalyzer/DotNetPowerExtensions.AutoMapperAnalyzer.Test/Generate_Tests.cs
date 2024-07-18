
namespace DotNetPowerExtensions.AutoMapper;

internal class Generate_Tests
{
    [Test]
    public void Generator_GeneratesObjectWithPublicPropertiesAndFields()
    {
        // Arrange
        var generator = new ObjectGenerator();
        var context = new GeneratorExecutionContext();
        var classDeclaration = SyntaxFactory.ClassDeclaration("MyClass")
            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
            .WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>(new[]
            {
            SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName("int"), "Id")
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithAccessorList(SyntaxFactory.AccessorList(SyntaxFactory.List(new[]
                {
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                }))),
            SyntaxFactory.FieldDeclaration(SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName("string"))
                .WithVariables(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.VariableDeclarator("Name"))))
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword))),
            SyntaxFactory.FieldDeclaration(SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName("int"))
                .WithVariables(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.VariableDeclarator("Age"))))
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
            }));

        // Act
        generator.Execute(context);
        var generatedObject = context.AdditionalFiles.Single().GetText().ToString();

        // Assert
        Assert.AreEqual(@"using System;

public class GeneratedObject
{
    public int Id { get; set; }
    public string Name;
    public int Age;
}
", generatedObject);
    }

    [Test]
    public void Generator_GeneratesObjectWithInternalPropertiesAndFields()
    {
        // Arrange
        var generator = new ObjectGenerator();
        var context = new GeneratorExecutionContext();
        var classDeclaration = SyntaxFactory.ClassDeclaration("MyClass")
            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
            .WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>(new[]
            {
            SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName("int"), "Id")
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithAccessorList(SyntaxFactory.AccessorList(SyntaxFactory.List(new[]
                {
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                }))),
            SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName("string"), "Name")
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.InternalKeyword)))
                .WithAccessorList(SyntaxFactory.AccessorList(SyntaxFactory.List(new[]
                {
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                }))),
            SyntaxFactory.FieldDeclaration(SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName("int"))
                .WithVariables(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.VariableDeclarator("Age"))))
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.InternalKeyword)))
            }));

        // Act
        generator.Execute(context);
        var generatedObject = context.AdditionalFiles.Single().GetText().ToString();

        // Assert
        Assert.AreEqual(@"using System;

public class GeneratedObject
{
    public int Id { get; set; }
    internal string Name { get; set; }
    internal int Age;
}
", generatedObject);
    }
}
