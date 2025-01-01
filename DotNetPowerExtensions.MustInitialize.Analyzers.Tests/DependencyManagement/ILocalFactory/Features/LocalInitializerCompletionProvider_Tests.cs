using Microsoft.CodeAnalysis.Completion;

namespace DotNetPowerExtensions.Analyzers.Tests.DependencyManagement.ILocalFactory.Features;

internal class LocalInitializerCompletionProvider_Tests
{
    // TODO... Make tests for completion providers:
    // 1) Empty
    // A) MustInitialzie and B) MightRequire
    // 2) Partial intialize
    // 3) Full Initialize
    // On A) The first { b) A comma c) a space
    // On A) The same document B) another document the same project C) another project in solution D) a compiled refernced project
    // TODO What about linked documents? what are they? do we need to test them?
    // Also add tests that it shows for members that are out of scope of normal intellisense
    // Also add tests that it not shows members that are not must intialize or might require

    [Test]
    public async Task Test()
    {
        var code = @"
        using SequelPay.DotNetPowerExtensions;

        [MightRequire<string>(""Testing"")]
        public class TestClass
        {
        	[MustInitialize] public int PublicProp { get; set; }
        	public int PublicField;
        	[MustInitialize] internal int InternalProp { get; set; }

        	public static void Test(ILocalFactory<TestClass> factory)
        	{
        		var result = factory.Create(new { PublicProp = 5 });
        	}
        }
        ";

        var document = FeaturesTestUtils.GetInitializedDocument(code);

        var position = code.LastIndexOf("new { ", StringComparison.Ordinal) + "new { ".Length;

        var text = await document.GetTextAsync().ConfigureAwait(false);
        var insertionTrigger = CompletionTrigger.CreateInsertionTrigger(text[position]);

        var completionService = CompletionService.GetService(document)!;
        var results = await completionService.GetCompletionsAsync(document, position, insertionTrigger).ConfigureAwait(false);

        Assert.That(results.ItemsList.Count, Is.EqualTo(2));

        Assert.That(results.ItemsList[0]!.DisplayText, Is.EqualTo("Testing"));
        Assert.That(results.ItemsList[0]!.InlineDescription, Is.EqualTo("MustInitialize"));
        Assert.That(results.ItemsList[0]!.Properties["Symbols"], Is.EqualTo("5 (M \".ctor\" (D (N \"DotNetPowerExtensions\" 0 (N \"SequelPay\" 0 (N \"\" 1 (U (S \"SequelPay.DotNetPowerExtensions.MustInitialize.Common\" 6) 5) 4) 3) 2) \"MightRequireAttribute\" 1 ! 0 (% 1 (D (N \"System\" 0 (N \"\" 1 (U (S \"System.Private.CoreLib\" 11) 10) 9) 8) \"String\" 0 ! 0 (% 0) 7)) 1) 0 0 (% 1 0) ! (% 1 (# 7)) 0)"));

        Assert.That(results.ItemsList[1]!.DisplayText, Is.EqualTo("InternalProp"));
        Assert.That(results.ItemsList[1]!.InlineDescription, Is.EqualTo("MustInitialize"));
        Assert.That(results.ItemsList[1]!.Properties["Symbols"], Is.EqualTo("5 (Q \"InternalProp\" (D (N \"\" 1 (U (S \"MyProject\" 4) 3) 2) \"TestClass\" 0 ! 0 (% 0) 1) 0 (% 0) (% 0) 0)"));
    }
}
