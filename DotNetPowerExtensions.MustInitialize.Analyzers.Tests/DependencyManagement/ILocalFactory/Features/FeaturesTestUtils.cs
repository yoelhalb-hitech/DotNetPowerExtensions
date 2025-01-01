using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using SequelPay.DotNetPowerExtensions.Analyzers.DependencyManagement.ILocalFactory.Features;
using System.Reflection;

namespace DotNetPowerExtensions.Analyzers.Tests.DependencyManagement.ILocalFactory.Features;

internal class FeaturesTestUtils
{
    private static AdhocWorkspace GetWorkspace()
    {
        var host = MefHostServices.Create(MefHostServices.DefaultAssemblies);
        var workspace = new AdhocWorkspace(host);

        var loader = workspace.Services.GetRequiredService<IAnalyzerService>().GetLoader();
        var analyzerPath = typeof(LocalInitializerCompletionProvider).Assembly.Location!;

        var projectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), "MyProject", "MyProject", LanguageNames.CSharp).
           WithMetadataReferences(new[]
           {
               MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
               MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
               MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location),
               MetadataReference.CreateFromFile(typeof(SequelPay.DotNetPowerExtensions.MustInitializeAttribute).Assembly.Location),
               MetadataReference.CreateFromFile(typeof(SequelPay.DotNetPowerExtensions.ILocalFactory<>).Assembly.Location),
           })
           .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
           .WithAnalyzerReferences(new AnalyzerReference[]
           {
               new AnalyzerFileReference(analyzerPath, loader),
               new AnalyzerFileReference(Path.Combine(Path.GetDirectoryName(analyzerPath)!, "DotNetPowerExtensions.RoslynExtensions.dll"), loader)
           });

        var project = workspace.CurrentSolution.AddProject(projectInfo);
        workspace.TryApplyChanges(project);

        return workspace;
    }

    public static Document GetInitializedDocument(string code)
    {
        using var workspace = GetWorkspace();

        var document = workspace.AddDocument(workspace.CurrentSolution.Projects.First().Id, "MyFile.cs", SourceText.From(code));

        var completionService = CompletionService.GetService(document)!;

        // We need to do it once first to register it
        _ = completionService.GetCompletionsAsync(document, 0, CompletionTrigger.CreateInsertionTrigger(' ')).Result;

        return document;
    }
}
