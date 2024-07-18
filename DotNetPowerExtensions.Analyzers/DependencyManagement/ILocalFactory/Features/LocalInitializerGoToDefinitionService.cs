extern alias Features;
extern alias Workspaces;
using Features::Microsoft.CodeAnalysis.GoToDefinition;
using Workspaces::Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using Workspaces::Microsoft.CodeAnalysis;

namespace DotNetPowerExtensions.Analyzers.DependencyManagement.ILocalFactory.Features;

[ExportLanguageService(typeof(IGoToDefinitionSymbolService), LanguageNames.CSharp), Shared]
internal class LocalInitializerGoToDefinitionService : AbstractGoToDefinitionSymbolService
{
    //public Task<(ISymbol?, TextSpan)> GetSymbolAndBoundSpanAsync(Document document, int position, bool includeType, CancellationToken cancellationToken)
    //{
    //    throw new NotImplementedException();
    //}

    //public Task<int?> GetTargetIfControlFlowAsync(Document document, int position, CancellationToken cancellationToken)
    //{
    //    //var a = new System.Diagnostics.Process { EnableRaisingEvents = true };
    //    //var a1 = new OneAndTwo { Test = 1 };
    //    throw new NotImplementedException();
    //}
    protected override ISymbol FindRelatedExplicitlyDeclaredSymbol(ISymbol symbol, Compilation compilation)
    {
        var identifier = symbol.GetSyntax<IdentifierNameSyntax>().FirstOrDefault();
        if (identifier is null) return symbol;

        var result = FeatureUtils.BindIDentifierToDeclaringSymbol(compilation.GetSemanticModel(compilation.SyntaxTrees.First()), identifier);
        if(result is null) return symbol;

        return result;
    }

    protected override int? GetTargetPositionIfControlFlow(SemanticModel semanticModel, SyntaxToken token)
        => FeatureUtils.BindTokenToDeclaringSymbol(semanticModel, token)?.Locations.FirstOrNone().SourceSpan.Start;
}

//interface IOne
//{
//    public int Test { get; set; }
//}
//interface ITwo
//{
//    public int Test { get; set; }
//}
//class One : IOne
//{
//    public int Test { get; set; }
//}
//class Two : ITwo
//{
//    public int Test { get; set; }
//}
//class OneAndTwo : IOne, ITwo
//{
//    public int Test { get; set; }
//}