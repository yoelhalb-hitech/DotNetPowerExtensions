
namespace DotNetPowerExtensions.AutoMapper;

[Generator]
public class ObjectGenerator : ISourceGenerator
{
    private const string AttributeName = "GenerateObject";
    private const string AttributeArgumentName = "ObjectName";
    private const string AllowInternalArgumentName = "AllowInternal";

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (!(context.SyntaxReceiver is SyntaxReceiver receiver))
        {
            return;
        }

        var compilation = context.Compilation;
        var attributeSymbol = compilation.GetTypeByMetadataName(typeof(GenerateObjectAttribute).FullName);

        foreach (var classDeclaration in receiver.CandidateClasses)
        {
            var semanticModel = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);

            if (classSymbol == null || !classSymbol.GetAttributes().Any(a => a.AttributeClass.Equals(attributeSymbol)))
            {
                continue;
            }

            var attributeData = classSymbol.GetAttributes().First(a => a.AttributeClass.Equals(attributeSymbol));
            var objectName = attributeData.NamedArguments.FirstOrDefault(a => a.Key == AttributeArgumentName).Value.Value as string;
            var allowInternal = attributeData.NamedArguments.FirstOrDefault(a => a.Key == AllowInternalArgumentName).Value.Value as bool? ?? false;

            var properties = GetProperties(classSymbol, allowInternal);
            var fields = GetFields(classSymbol, allowInternal);

            var generatedObject = GenerateObject(classSymbol.Name, objectName, properties, fields);

            context.AddSource($"{classSymbol.Name}_{objectName}.cs", SourceText.From(generatedObject, Encoding.UTF8));
        }
    }

    private IEnumerable<IPropertySymbol> GetProperties(INamedTypeSymbol classSymbol, bool allowInternal)
    {
        return classSymbol.GetMembers().OfType<IPropertySymbol>()
            .Where(p => p.DeclaredAccessibility == Accessibility.Public || (allowInternal && p.DeclaredAccessibility == Accessibility.Internal));
    }

    private IEnumerable<IFieldSymbol> GetFields(INamedTypeSymbol classSymbol, bool allowInternal)
    {
        return classSymbol.GetMembers().OfType<IFieldSymbol>()
            .Where(f => f.DeclaredAccessibility == Accessibility.Public || (allowInternal && f.DeclaredAccessibility == Accessibility.Internal));
    }

    private string GenerateObject(string className, string objectName, IEnumerable<IPropertySymbol> properties, IEnumerable<IFieldSymbol> fields)
    {
        var propertiesAndFields = properties.Select(p => $"public {p.Type} {p.Name} {{ get; set; }}")
            .Concat(fields.Select(f => $"public {f.Type} {f.Name};"));

        return $@"
using System;

public class {objectName}
{{
    {string.Join(Environment.NewLine, propertiesAndFields)}
}}
";
    }

    private class SyntaxReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> CandidateClasses { get; } = new List<ClassDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax &&
                classDeclarationSyntax.AttributeLists.Count > 0)
            {
                CandidateClasses.Add(classDeclarationSyntax);
            }
        }
    }
}