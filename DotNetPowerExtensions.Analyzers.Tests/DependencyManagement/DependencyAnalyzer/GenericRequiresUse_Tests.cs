using DotNetPowerExtensions.Analyzers.DependencyManagement.DependencyAttribute.Analyzers;
using SequelPay.DotNetPowerExtensions;

namespace DotNetPowerExtensions.Analyzers.Tests.DependencyManagement.DependencyAnalyzer;

internal class GenericRequiresUse_Tests : AnalyzerVerifierBase<GenericRequiresUse>
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1307:Specify StringComparison for clarity", Justification = "Older frameworks don't support it")]
    public static string[] Attributes => new string[] { nameof(SingletonAttribute), nameof(ScopedAttribute), nameof(TransientAttribute), nameof(LocalAttribute) }
                                                        .Select(n => n.Replace(nameof(Attribute), ""))
                                                        .ToArray();

    [Test]
    public async Task Test_Works([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                                 [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        [[|{{prefix}}{{attribute}}{{suffix}}|]]
        public class TestType<T>
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_WithNull([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                             [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [{{prefix}}{{attribute}}{{suffix}}([|Use=null|])]
        public class TestType<T>
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_WorksWithParenthesisAndNull([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                                            [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [{{prefix}}{{attribute}}{{suffix}}([|Use=((null))|])]
        public class TestType<T>
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnOnNotNull([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                                    [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [{{prefix}}{{attribute}}{{suffix}}(Use=typeof(TestType<string>))]
        public class TestType<T>
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenNotGeneric([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                                            [ValueSource(nameof(Suffixes))] string suffix)
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
        public class TestType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }
}
