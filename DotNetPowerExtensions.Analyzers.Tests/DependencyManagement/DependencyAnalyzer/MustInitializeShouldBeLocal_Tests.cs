using DotNetPowerExtensions.Analyzers.DependencyManagement.DependencyAttribute.Analyzers;
using SequelPay.DotNetPowerExtensions;

namespace DotNetPowerExtensions.Analyzers.Tests.DependencyManagement.DependencyAnalyzer;

internal class MustInitializeShouldBeLocal_Tests : AnalyzerVerifierBase<MustInitializeShouldBeLocal>
{
    public static string[] Attributes => new string[] { nameof(SingletonAttribute), nameof(ScopedAttribute), nameof(TransientAttribute) }
                                                            .Select(n => n.Replace(nameof(Attribute), "", StringComparison.Ordinal))
                                                            .ToArray();

    [Test]
    public async Task Test_Works([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix,
                                                                                            [ValueSource(nameof(Attributes))] string attribute)
    {
        var test = $$"""
        [[|{{prefix}}{{attribute}}{{suffix}}|]]
        public class TransientType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
        }
        """;
        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnForInitialized([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix,
                                                                                        [ValueSource(nameof(Attributes))] string attribute)
    {
        var test = $$"""
        public class TransientTypeBase
        {
            [{{prefix}}MustInitialize{{suffix}}] public virtual string TestProp { get; set; }
        }
        [{{prefix}}{{attribute}}{{suffix}}]
        public class TransientType : TransientTypeBase
        {
            [{{prefix}}Initialized{{suffix}}] public override string TestProp { get; set; }
        }

        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_WithGeneric([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix,
                                                                                                [ValueSource(nameof(Attributes))] string attribute)
    {
        var genericSuffix = suffix.Contains("()", StringComparison.Ordinal)
                                ? suffix.Replace("()", "<TransientType>()", StringComparison.Ordinal)
                                : suffix + "<TransientType>";

        var test = $$"""
        [[|{{prefix}}{{attribute}}{{genericSuffix}}|]]
        public class TransientType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
        }

        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_WithField([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix,
                                                                                                         [ValueSource(nameof(Attributes))] string attribute)
    {
        var test = $$"""
        [[|{{prefix}}{{attribute}}{{suffix}}|]]
        public class TransientType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
        }

        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_WithField_AndGeneric([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix,
                                                                                                        [ValueSource(nameof(Attributes))] string attribute)
    {
        var genericSuffix = suffix.Contains("()", StringComparison.Ordinal)
                                    ? suffix.Replace("()", "<TransientType>()", StringComparison.Ordinal)
                                    : suffix + "<TransientType>";

        var test = $$"""
        [[|{{prefix}}{{attribute}}{{genericSuffix}}|]]
        public class TransientType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
        }

        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnForLocal([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        [{{prefix}}Local{{suffix}}]
        public class TransientType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
        }

        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenNoMustInitializeAttribute([ValueSource(nameof(Prefixes))] string prefix,
                                        [ValueSource(nameof(Suffixes))] string suffix, [ValueSource(nameof(Attributes))] string attribute)
    {
        var test = $$"""
        [{{prefix}}{{attribute}}{{suffix}}]
        public class TransientType
        {
            public string TestProp { get; set; }
        }

        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenNoAttribute([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class TransientType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
        }

        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnForOtherMustInitialize([ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        class MustInitializeAttribute : System.Attribute {}
        [Transient]
        public class TransientType
        {
            [MustInitialize{{suffix}}] public string TestProp { get; set; }
        }

        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnForOtherAttribute([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix,
                                                                        [ValueSource(nameof(Attributes))] string attribute)
    {
        var test = $$"""
        class {{attribute}}Attribute : System.Attribute {}
        [{{attribute}}{{suffix}}]
        public class TransientType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
        }

        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }
}
