﻿using SequelPay.DotNetPowerExtensions.RoslynExtensions;
using System.Linq;
using static DotNetPowerExtensions.DependencyInjection.Analyzers.AnalyzerBase;

namespace SequelPay.DotNetPowerExtensions.Analyzers.DependencyManagement.DependencyAttribute.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class OnlyUseServiceInDependency : AnalyzerBase
{
    public const string DiagnosticId = "DNPE0222";
    protected const string Title = "OnlyUseServiceInDependency";
    protected const string Message = "In a class decorated with `Singleton/Scoped/Transient/Local` attribute only use a service";

    protected DiagnosticDescriptor Diagnostic = new DiagnosticDescriptor(DiagnosticId, Title, Message, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Message + ".");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Diagnostic);

    protected override void Register(CompilationStartAnalysisContext compilationContext, MetadataUtil metadataUtil)
    {
        // We do not do it for the base attributes as they are not instantiated directly and can be used for a subclass
        var symbols = metadataUtil.GetTypeSymbols(DependencyAnalyzerUtils.AllDependencies);

        compilationContext
            .RegisterSyntaxNodeAction(c => AnalyzeConstructor(c, symbols),
                                        SyntaxKind.ConstructorDeclaration);
    }

    private void AnalyzeConstructor(SyntaxNodeAnalysisContext context, INamedTypeSymbol[] attributeSymbols)
    {
        try
        {
            var ctor = context.Node as ConstructorDeclarationSyntax;

            if (ctor is null || !ctor.ParameterList.Parameters.Any()
                || context.SemanticModel.GetDeclaredSymbol(ctor, context.CancellationToken) is not IMethodSymbol methodSymbol) return;

            // Check if this is a service
            if (methodSymbol.ContainingType.GetAttributes()
                            .Where(a => a.AttributeClass is not null)
                            .All(a => !attributeSymbols.ContainsGeneric(a.AttributeClass!))) return;

            foreach (var parameter in ctor.ParameterList.Parameters)
            {
                try
                {
                    var t = parameter.Type;
                    if (t is null) continue;

                    var symbol = context.SemanticModel.GetSymbolInfo(t, context.CancellationToken).Symbol;
                    if (symbol is null) continue;

                    if (!symbol.OriginalDefinition.ContainingModule.ReferencedAssemblySymbols.Contains(attributeSymbols.First().ContainingAssembly)) return; // It is not from this code base and so we don't expect it to have been declared with our attributes

                    if (!symbol.HasAttribute(attributeSymbols))
                    {
                        var diagnostic = Microsoft.CodeAnalysis.Diagnostic.Create(Diagnostic, parameter.GetLocation(), symbol.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
                catch { }
            }
        }
        catch { }
    }
}