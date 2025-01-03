
namespace DotNetPowerExtensions.Analyzers.Tests.DependencyManagement.DependencyAnalyzer;

// Actually the analyzer just checks for abstract while the attribute itself prevents using it on interface
internal class DependencyShouldNotBeInterface_Tests : AnalyzerVerifierBase<DependencyShouldNotBeAbstract>
{
    public static string[] Attributes => new string[] { nameof(SingletonAttribute), nameof(ScopedAttribute), nameof(TransientAttribute), nameof(LocalAttribute) }
                                                        .Select(n => n.Replace(nameof(Attribute), "", StringComparison.Ordinal))
                                                        .ToArray();

    [Test]
    public async Task Test_Works([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [[|{|CS0592:{{prefix}}{{attribute}}{{suffix}}|}|]]
        public interface ITestType
        {
        }

        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_MessageIsCorrect()
    {
        var test = $$"""
        [{|CS0592:Transient|}]
        public interface ITestType
        {
        }

        """;

        await VerifyAnalyzerAsync(test, new DiagnosticResult("DNPE0209", DiagnosticSeverity.Warning)
                                                .WithSpan(2, 2, 2, 11).WithMessage("Use `TransientBase` instead of `Transient` when class is abstract")).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenNonInterface([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                            [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [{{prefix}}{{attribute}}{{suffix}}]
        public class TestType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }


    [Test]
    public async Task Test_DoesNotWarnWhenOtherAttribute([ValueSource(nameof(Attributes))] string attribute, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class {{attribute}}Attribute : System.Attribute { public System.Type Use { get; set; } }
        [{{attribute}}{{suffix}}]
        public interface ITestType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }
}
