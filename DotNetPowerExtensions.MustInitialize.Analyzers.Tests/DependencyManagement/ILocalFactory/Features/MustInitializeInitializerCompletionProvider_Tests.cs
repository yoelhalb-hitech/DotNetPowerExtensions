using Microsoft.CodeAnalysis.Completion;

namespace DotNetPowerExtensions.Analyzers.Tests.DependencyManagement.ILocalFactory.Features;

internal class MustInitializeInitializerCompletionProvider_Tests
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
            internal int AProp { get; set; }
        	[MustInitialize] internal int InternalProp { get; set; }
            [MustInitialize] int XField; // Naming is o make sure that MustInitialize is indeed the first

        	public static void Test(ILocalFactory<TestClass> factory)
        	{
        		var result = factory.Create(new TestClass{ PublicProp = 5 });
        	}
        }
        ";

        var document = FeaturesTestUtils.GetInitializedDocument(code);

        var position = code.LastIndexOf("new TestClass{ ", StringComparison.Ordinal) + "new TestClass{ ".Length;

        var text = await document.GetTextAsync().ConfigureAwait(false);
        var insertionTrigger = CompletionTrigger.CreateInsertionTrigger(text[position]);

        var completionService = CompletionService.GetService(document)!;
        var results = await completionService.GetCompletionsAsync(document, position, insertionTrigger).ConfigureAwait(false);

        Assert.That(results.ItemsList.Count, Is.EqualTo(4));

        Assert.That(results.ItemsList[0]!.DisplayText, Is.EqualTo("InternalProp"));
        Assert.That(results.ItemsList[0]!.InlineDescription, Is.EqualTo("MustInitialize"));

        Assert.That(results.ItemsList[1]!.DisplayText, Is.EqualTo("XField"));
        Assert.That(results.ItemsList[1]!.InlineDescription, Is.EqualTo("MustInitialize"));

        Assert.That(results.ItemsList[2]!.DisplayText, Is.EqualTo("AProp"));
        Assert.That(results.ItemsList[2]!.InlineDescription, Is.Empty);

        Assert.That(results.ItemsList[3]!.DisplayText, Is.EqualTo("PublicField"));
        Assert.That(results.ItemsList[3]!.InlineDescription, Is.Empty);
    }
}
