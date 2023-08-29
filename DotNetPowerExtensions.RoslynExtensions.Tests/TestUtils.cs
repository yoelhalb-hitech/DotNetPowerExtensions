using System.Globalization;
using System.Text;

namespace DotNetPowerExtensions.RoslynExtensions.Tests;

internal class TestUtils
{
    public static Compilation GetCompilation(string name, IEnumerable<SyntaxTree> trees, IEnumerable<string> extraReferenceFiles)
            => CSharpCompilation.Create(name,
                        syntaxTrees: trees,
                        options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                        references: new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) }
                                                        .Concat(extraReferenceFiles.Select(r => MetadataReference.CreateFromFile(r))));

    public static (SemanticModel, INamedTypeSymbol) GetModelAndTypeSymbol(params string[] sources)
    {
        var trees = sources.Select(s => SyntaxFactory.ParseSyntaxTree(s)).ToList();
        var treeToUse = trees.Last();

        var compilation = GetCompilation("Test", trees, Array.Empty<string>());

        var semanticModel = compilation.GetSemanticModel(treeToUse);

        var syntax = treeToUse.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>().Last();

        return (semanticModel, semanticModel.GetDeclaredSymbol(syntax)!);
    }

    public static SemanticModel GetSemanticModel(params SyntaxTree[] trees) => CSharpCompilation.Create("Test",
                            syntaxTrees: trees, references: new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) })
                        .GetSemanticModel(trees.Last());

    public static string GetClassNamesCode(bool outerNS, bool innerNS,
                                            bool isStructOuter, int outerGenericCount,
                                            bool hasInner, bool isStructInner, int innerGenericCount,
                                            bool hasInnerInner, bool isStructInnerInner, int innerInnerGenericCount)
    {
        var sb = new StringBuilder();
        if (outerNS) { sb.AppendLine("namespace Outer {"); }
        if (innerNS) { sb.AppendLine("namespace Inner {"); }

        sb.Append(CultureInfo.InvariantCulture, $"public {(isStructOuter ? "struct" : "class")} OuterType");
        if (outerGenericCount > 0) sb.Append("<" + Enumerable.Range(0, outerGenericCount).Select(i => "TOuter" + i).Join(",") + ">");
        sb.Append('{');
        if (hasInner)
        {
            sb.AppendLine();
            sb.Append(CultureInfo.InvariantCulture, $"public {(isStructInner ? "struct" : "class")} InnerType");
            if (innerGenericCount > 0) sb.Append("<" + Enumerable.Range(0, innerGenericCount).Select(i => "TInner" + i).Join(",") + ">");
            sb.Append('{');
            if (hasInnerInner)
            {
                sb.AppendLine();
                sb.Append(CultureInfo.InvariantCulture, $"public {(isStructInnerInner ? "struct" : "class")} InnerInnerType");
                if (innerInnerGenericCount > 0) sb.Append("<" + Enumerable.Range(0, innerInnerGenericCount).Select(i => "TInnerInner" + i).Join(",") + ">");
                sb.AppendLine("{}");
            }
            sb.AppendLine("}");
        }
        sb.AppendLine("}");
        if (innerNS) { sb.AppendLine("}"); }
        if (outerNS) { sb.AppendLine("}"); }

        return sb.ToString();
    }
}
