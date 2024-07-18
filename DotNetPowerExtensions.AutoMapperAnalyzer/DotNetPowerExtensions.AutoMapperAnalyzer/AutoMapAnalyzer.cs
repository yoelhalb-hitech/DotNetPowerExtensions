
using Microsoft.CodeAnalysis;

namespace SequelPay.DotNetPowerExtensions.AutoMapper;

// Diagnostic analyzer to check for correct usage of the AutoMap attribute
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AutoMapAnalyzer : DiagnosticAnalyzer
{
    private const string DiagnosticId = "YourDiagnosticId";
    private const string Category = "Usage";

    // Diagnostic descriptors for the different rules
    private static readonly DiagnosticDescriptor Rule1 = new DiagnosticDescriptor(
        DiagnosticId + "1",
        "AutoMap attribute usage",
        "AutoMap attribute is not used correctly",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor Rule2 = new DiagnosticDescriptor(
        DiagnosticId + "2",
        "Common field or property missing",
        "The classes do not have a common field or property",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor Rule3 = new DiagnosticDescriptor(
        DiagnosticId + "3",
        "Mismatched field or property",
        "The classes have a mismatched field or property",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor Rule4 = new DiagnosticDescriptor(
        DiagnosticId + "4",
        "MustInitialize attribute usage",
        "MustInitialize attribute is not used correctly",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor Rule5 = new DiagnosticDescriptor(
    DiagnosticId + "5",
    "Mismatched MightRequire attribute",
    "The classes have a mismatched MightRequire attribute",
    Category,
    DiagnosticSeverity.Error,
    isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule1, Rule2, Rule3, Rule4, Rule5);

    public override void Initialize(AnalysisContext context)
    {
        context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzePropertyDeclaration, SyntaxKind.PropertyDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeFieldDeclaration, SyntaxKind.FieldDeclaration);
    }

    // Analyze class declarations
    private void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        // Check if the class has the AutoMap attribute
        var autoMapAttribute = classDeclaration.AttributeLists
            .SelectMany(list => list.Attributes)
            .FirstOrDefault(attribute => attribute.Name.ToString() == "AutoMap");

        if (autoMapAttribute == null)
        {
            return;
        }

        // Get the type passed as an argument to the AutoMap attribute
        var argumentType = GetAutoMapArgumentType(autoMapAttribute, context.SemanticModel);
        if (argumentType == null)
        {
            return;
        }
        // TODO... check that it doesn't have a generator on this or the other

        // Check if the argument type has the AutoMap attribute with the current class as an argument
        var argumentTypeSymbol = context.SemanticModel.GetSymbolInfo(argumentType).Symbol as INamedTypeSymbol;
        if (argumentTypeSymbol == null || !HasAutoMapAttributeWithArgument(argumentTypeSymbol, classDeclaration.Identifier.Text))
        {
            // TODO... check that the mapinternal match by both
            var diagnostic = Diagnostic.Create(Rule1, autoMapAttribute.GetLocation());
            context.ReportDiagnostic(diagnostic);
            return;
        }

        // Check if the classes have at least one common public field or property
        if (!HasCommonFieldOrProperty(classDeclaration, argumentTypeSymbol))
        {
            var diagnostic = Diagnostic.Create(Rule2, autoMapAttribute.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }

        // Check if the classes have matching fields or properties
        CheckMatchingFieldsAndProperties(classDeclaration, argumentTypeSymbol, context);

        // Check if the classes have matching MightRequire attributes with the same arguments
        CheckMatchingMightRequireAttributes(classDeclaration, argumentTypeSymbol, context);
    }
    // Analyze property declarations
    private void AnalyzePropertyDeclaration(SyntaxNodeAnalysisContext context)
    {
        var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;
        // Check if the property has the MustInitialize attribute
        var mustInitializeAttribute = propertyDeclaration.AttributeLists
            .SelectMany(list => list.Attributes)
            .FirstOrDefault(attribute => attribute.Name.ToString() == "MustInitialize");
        if (mustInitializeAttribute == null)
        {
            return;
        }
        // Check if the property is not public or is internal when not allowed by the AutoMap attribute
        var propertySymbol = context.SemanticModel.GetDeclaredSymbol(propertyDeclaration);
        if (propertySymbol.DeclaredAccessibility != Accessibility.Public ||
            (propertySymbol.DeclaredAccessibility == Accessibility.Internal && !IsInternalMappingAllowed(propertyDeclaration)))
        {
            var diagnostic = Diagnostic.Create(Rule4, mustInitializeAttribute.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
    // Analyze field declarations
    private void AnalyzeFieldDeclaration(SyntaxNodeAnalysisContext context)
    {
        var fieldDeclaration = (FieldDeclarationSyntax)context.Node;
        // Check if the field has the MustInitialize attribute
        var mustInitializeAttribute = fieldDeclaration.AttributeLists
            .SelectMany(list => list.Attributes)
            .FirstOrDefault(attribute => attribute.Name.ToString() == "MustInitialize");
        if (mustInitializeAttribute == null)
        {
            return;
        }
        // Check if the field is not public or is internal when not allowed by the AutoMap attribute
        var fieldSymbol = context.SemanticModel.GetDeclaredSymbol(fieldDeclaration.Declaration.Variables.First());
        if (fieldSymbol.DeclaredAccessibility != Accessibility.Public ||
            (fieldSymbol.DeclaredAccessibility == Accessibility.Internal && !IsInternalMappingAllowed(fieldDeclaration)))
        {
            var diagnostic = Diagnostic.Create(Rule4, mustInitializeAttribute.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
    private void CheckMatchingMightRequireAttributes(ClassDeclarationSyntax classDeclaration, INamedTypeSymbol argumentTypeSymbol, SyntaxNodeAnalysisContext context)
    {
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
        var argumentClassSymbol = context.SemanticModel.GetSymbolInfo(argumentTypeSymbol.DeclaringSyntaxReferences.First().GetSyntax()).Symbol as INamedTypeSymbol;

        if (classSymbol == null || argumentClassSymbol == null)
        {
            return;
        }

        var classMightRequireAttributes = classSymbol.GetAttributes()
            .Where(attribute => attribute.AttributeClass.Name == "MightRequire")
            .ToList();

        var argumentMightRequireAttributes = argumentClassSymbol.GetAttributes()
            .Where(attribute => attribute.AttributeClass.Name == "MightRequire")
            .ToList();

        foreach (var classAttribute in classMightRequireAttributes)
        {
            var matchingAttribute = argumentMightRequireAttributes.FirstOrDefault(attribute =>
                attribute.ConstructorArguments.SequenceEqual(classAttribute.ConstructorArguments));

            if (matchingAttribute == null)
            {
                var diagnostic = Diagnostic.Create(Rule5, classDeclaration.Identifier.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
    // Get the type passed as an argument to the AutoMap attribute
    private TypeSyntax GetAutoMapArgumentType(AttributeSyntax autoMapAttribute, SemanticModel semanticModel)
    {
        var argument = autoMapAttribute.ArgumentList?.Arguments.FirstOrDefault();
        if (argument?.Expression is TypeOfExpressionSyntax typeOfExpression)
        {
            return typeOfExpression.Type;
        }

        var attributeSymbol = semanticModel.GetSymbolInfo(autoMapAttribute).Symbol as IMethodSymbol;
        if (attributeSymbol?.ReceiverType != null)
        {
            return SyntaxFactory.ParseTypeName(attributeSymbol.ReceiverType.ToDisplayString());
        }

        return null;
    }
    // Check if the argument type has the AutoMap attribute with the current class as an argument
    private bool HasAutoMapAttributeWithArgument(INamedTypeSymbol typeSymbol, string argumentClassName)
    {
        var autoMapAttribute = typeSymbol.GetAttributes()
            .FirstOrDefault(attribute => attribute.AttributeClass.Name == "AutoMap");

        if (autoMapAttribute == null)
        {
            return false;
        }

        var argument = autoMapAttribute.ConstructorArguments.FirstOrDefault();
        if (argument.Kind == TypedConstantKind.Type && argument.Type?.Name == "Type")
        {
            var argumentType = (INamedTypeSymbol)argument.Value;
            return argumentType.Name == argumentClassName;
        }

        return false;
    }
    // Check if the classes have at least one common public field or property
    private bool HasCommonFieldOrProperty(ClassDeclarationSyntax classDeclaration, INamedTypeSymbol argumentTypeSymbol)
    {
        var classSymbol = argumentTypeSymbol.ContainingAssembly.GetTypeByMetadataName(classDeclaration.Identifier.Text);
        if (classSymbol == null)
        {
            return false;
        }

        // Version 1
        //var commonMembers = classDeclaration.Members
        //    .Where(member => member is FieldDeclarationSyntax || member is PropertyDeclarationSyntax)
        //    .Select(member => member.ToString())
        //    .Intersect(classSymbol.GetMembers().Select(member => member.ToString()));

        //return commonMembers.Any();

        var classMembers = classDeclaration.Members
    .Where(member => member is FieldDeclarationSyntax || member is PropertyDeclarationSyntax)
    .OfType<FieldDeclarationSyntax>()
    .Select(member => member.ToString())
    .SelectMany(field => field.Declaration.Variables.Select(variable => variable.Identifier.Text))
    .Intersect(classSymbol.GetMembers().Select(member => member.ToString()));
                .Concat(classDeclaration.Members.OfType<PropertyDeclarationSyntax>().Select(property => property.Identifier.Text));
        var argumentMembers = argumentTypeSymbol.GetMembers()
            .OfType<IFieldSymbol>()
            .Select(field => field.Name)
            .Concat(argumentTypeSymbol.GetMembers().OfType<IPropertySymbol>().Select(property => property.Name));
        return commonMembers.Any();
        return classMembers.Intersect(argumentMembers).Any();
    }

    // Check if the classes have matching fields or properties
    private void CheckMatchingFieldsAndProperties(ClassDeclarationSyntax classDeclaration, INamedTypeSymbol argumentTypeSymbol, SyntaxNodeAnalysisContext context)
    {
        // Get the symbols for the current class and the argument class
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
        var argumentClassSymbol = context.SemanticModel.GetSymbolInfo(argumentTypeSymbol.DeclaringSyntaxReferences.First().GetSyntax()).Symbol as INamedTypeSymbol;

        // Check if either symbol is null
        if (classSymbol == null || argumentClassSymbol == null)
        {
            return;
        }

        // Get the members (fields and properties) of both classes
        var classMembers = classSymbol.GetMembers().OfType<IFieldSymbol>().Concat(classSymbol.GetMembers().OfType<IPropertySymbol>());
        var argumentMembers = argumentClassSymbol.GetMembers().OfType<IFieldSymbol>().Concat(argumentClassSymbol.GetMembers().OfType<IPropertySymbol>());

        // Iterate over each member in the current class
        foreach (var classMember in classMembers)
        {
            // Skip members with the "Ignore" attribute
            if (classMember.GetAttributes().Any(attribute => attribute.AttributeClass.Name == "Ignore"))
            {
                continue;
            }

            // Find a matching member in the argument class
            var matchingMember = argumentMembers.FirstOrDefault(member =>
                member.Name == classMember.Name &&
                member.Type.Equals(classMember.Type) &&
                member.DeclaredAccessibility == Accessibility.Public &&
                (member is IFieldSymbol || ((IPropertySymbol)member).GetMethod.DeclaredAccessibility == Accessibility.Public));

            // If no matching member is found, report a diagnostic for a mismatched field or property
            if (matchingMember == null)
            {
                var diagnostic = Diagnostic.Create(Rule3, classDeclaration.Identifier.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
            // If a matching member is found with internal accessibility and different type, report a diagnostic for a mismatched field or property
            else if (matchingMember.DeclaredAccessibility == Accessibility.Internal && !classMember.Type.Equals(matchingMember.Type))
            {
                var diagnostic = Diagnostic.Create(Rule3, classDeclaration.Identifier.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

        // Check if internal mapping is allowed for a member
        private bool IsInternalMappingAllowed(MemberDeclarationSyntax memberDeclaration)
        {
            var autoMapAttribute = memberDeclaration.AttributeLists
                .SelectMany(list => list.Attributes)
                .FirstOrDefault(attribute => attribute.Name.ToString() == "AutoMap");
            if (autoMapAttribute == null)
            {
                return false;
            }
            var allowInternalArgument = autoMapAttribute.ArgumentList?.Arguments.FirstOrDefault(argument =>
                argument.NameEquals?.Name.Identifier.Text == "AllowInternal" &&
                argument.Expression is LiteralExpressionSyntax literalExpression &&
                literalExpression.Kind() == SyntaxKind.TrueLiteralExpression);
            return allowInternalArgument != null;
        }
    }
}
