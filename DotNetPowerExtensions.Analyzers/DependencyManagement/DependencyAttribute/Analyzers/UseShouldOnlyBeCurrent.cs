﻿using SequelPay.DotNetPowerExtensions;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace DotNetPowerExtensions.Analyzers.DependencyManagement.DependencyAttribute.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UseShouldOnlyBeCurrent : DiagnosticAnalyzer
{
    protected const string Category = "Language";
    public const string DiagnosticId = "DNPE0207";
    protected const string Title = "UseShouldBeCurrent";
    protected const string Message = "The `Use` attribute should only be the current generic type";
    protected const string Description = Message + ".";

    [SuppressMessage("Microsoft.Design", "CA1051: Do not declare visible instance fields", Justification = "The compiler only consideres fields when tracking analyzer releases")]
    protected DiagnosticDescriptor Diagnostic = new DiagnosticDescriptor(DiagnosticId, Title, Message, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Diagnostic);
    public override void Initialize(AnalysisContext context)
    {
        try
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compilationContext =>
            {
                Func<Type, INamedTypeSymbol?> metadata = t => compilationContext.Compilation.GetTypeByMetadataName(t.FullName!);

                var allAttributeTypes = new[]
                {
                    typeof(LocalAttribute),
                    typeof(LocalAttribute<>),
                    typeof(SingletonAttribute),
                    typeof(SingletonAttribute<>),
                    typeof(ScopedAttribute),
                    typeof(ScopedAttribute<>),
                    typeof(TransientAttribute),
                    typeof(TransientAttribute<>),
                    typeof(SequelPay.DotNetPowerExtensions.DependencyAttribute),
                    typeof(NonDependencyAttribute),
                    typeof(NonDependencyAttribute<>),
                };
                var symbols = allAttributeTypes.Select(t => metadata(t)).Where(x => x is not null).Select(x => x!).ToArray();

                compilationContext
                    .RegisterSyntaxNodeAction(c => AnalyzeClass(c, symbols), SyntaxKind.Attribute);
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }

    private string[] DependencyAttributeNames =
    {
        nameof(SingletonAttribute), nameof(ScopedAttribute), nameof(TransientAttribute),
        nameof(LocalAttribute), nameof(NonDependencyAttribute), nameof(SequelPay.DotNetPowerExtensions.DependencyAttribute),
    };

    private void AnalyzeClass(SyntaxNodeAnalysisContext context, INamedTypeSymbol[] attributeSymbols)
    {
        try
        {
            // Since a class decleration can be partial we will only report it on the attribute
            var attr = context.Node as AttributeSyntax;
            var attrName = attr?.Name.GetUnqualifiedName()?.Replace(nameof(Attribute), "");
            if (attrName is null || !DependencyAttributeNames.Contains(attrName + nameof(Attribute)) || attr!.ArgumentList is null)
                return;

            var useExpression = attr.ArgumentList.Arguments.FirstOrDefault(a => a.NameEquals?.Name is IdentifierNameSyntax name
                        && name.Identifier.Text == nameof(SequelPay.DotNetPowerExtensions.DependencyAttribute.Use));
            if (useExpression is null) return;

            var innerExpression = useExpression.Expression;
            while (innerExpression is ParenthesizedExpressionSyntax paren && paren.Expression is not null) innerExpression = paren.Expression;
            if (innerExpression is not TypeOfExpressionSyntax typeExpression) return;

            if (context.SemanticModel.GetSymbolInfo(attr!, context.CancellationToken).Symbol is not IMethodSymbol methodSymbol
                || !attributeSymbols.ContainsGeneric(methodSymbol.ContainingType)) return;

            var parent = context.Node.Parent;
            while (parent is not null && !object.ReferenceEquals(parent, parent.Parent) && parent is not TypeDeclarationSyntax) parent = parent.Parent;

            if (parent is not TypeDeclarationSyntax) return;

            if (context.SemanticModel.GetDeclaredSymbol(parent!, context.CancellationToken) is not INamedTypeSymbol classSymbol) return;
            if (!classSymbol.IsGenericType) return;

            if (context.SemanticModel.GetSymbolInfo(typeExpression.Type!, context.CancellationToken).Symbol is not INamedTypeSymbol typeSymbol) return;
            if (typeSymbol.IsGenericType && classSymbol.ConstructUnboundGenericType().IsEqualTo(typeSymbol.ConstructUnboundGenericType())) return;

            var diagnostic = Microsoft.CodeAnalysis.Diagnostic.Create(Diagnostic, useExpression!.GetLocation());

            context.ReportDiagnostic(diagnostic);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}