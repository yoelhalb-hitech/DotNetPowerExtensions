using Microsoft.CodeAnalysis.CSharp;
using SequelPay.DotNetPowerExtensions;
using System.Reflection;

namespace DotNetPowerExtensions.Analyzers.Tests.Throws.Utils;

internal class ThrowsTestUtils
{
    public static SemanticModel GetSemanticModel(SyntaxTree tree) => CSharpCompilation.Create("Test",
                        syntaxTrees: new[] { tree },
                        references: new[]
                        {
                                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                                MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location),
                                MetadataReference.CreateFromFile(Assembly.Load("System.Collections").Location),
                                MetadataReference.CreateFromFile(typeof(System.Linq.Expressions.Expression).Assembly.Location),
                                MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location),
                                MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
                                MetadataReference.CreateFromFile(typeof(ThrowsAttribute).Assembly.Location),
                        })
                    .GetSemanticModel(tree);

    public static IMethodSymbol? GetFirstMethodSymbol(SemanticModel semanticModel)
    {
        var method = semanticModel.Compilation.SyntaxTrees.First().GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First();
        return semanticModel.GetDeclaredSymbol(method);
    }

    public static IOperation? GetFirstMethodOperation(SemanticModel semanticModel)
    {
        var method = semanticModel.Compilation.SyntaxTrees.First().GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First();
        return semanticModel.GetOperation(method);
    }
}
