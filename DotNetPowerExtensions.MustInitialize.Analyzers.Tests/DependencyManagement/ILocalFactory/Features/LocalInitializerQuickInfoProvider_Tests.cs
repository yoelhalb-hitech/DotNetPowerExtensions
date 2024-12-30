using Microsoft.CodeAnalysis.QuickInfo;

namespace DotNetPowerExtensions.Analyzers.Tests.DependencyManagement.ILocalFactory.Features;

internal class LocalInitializerQuickInfoProvider_Tests
{
    [Test]
    public async Task Test()
    {
        var code = @"
        using SequelPay.DotNetPowerExtensions;

        [MightRequire<string>(""Testing"")]
        public class TestClass
        {
            /// <summary>
            /// Test text.
            /// </summary>
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

        var qi = QuickInfoService.GetService(document)!;
        var results = await qi.GetQuickInfoAsync(document, code.LastIndexOf("PublicProp", StringComparison.Ordinal)).ConfigureAwait(false);

        Assert.That(results, Is.Not.Null);
        Assert.That(results.Sections.Count, Is.EqualTo(2));
        Assert.That(results.Sections[0].Text, Is.EqualTo("int TestClass.PublicProp { get; set; }"));
        Assert.That(results.Sections[1].Text, Is.EqualTo("Test text."));
        Assert.That(results.Tags.Count, Is.EqualTo(2));
        Assert.That(results.Tags[0], Is.EqualTo("Property"));
        Assert.That(results.Tags[1], Is.EqualTo("Public"));
    }
}
