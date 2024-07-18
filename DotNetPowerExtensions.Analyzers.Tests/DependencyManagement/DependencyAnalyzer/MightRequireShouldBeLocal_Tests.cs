using DotNetPowerExtensions.Analyzers.DependencyManagement.DependencyAttribute.Analyzers;
using SequelPay.DotNetPowerExtensions;

namespace DotNetPowerExtensions.Analyzers.Tests.DependencyManagement.DependencyAnalyzer;

internal class MightRequireShouldBeLocal_Tests : AnalyzerVerifierBase<MightRequireShouldBeLocal>
{
    public static string[] Attributes => new string[] { nameof(SingletonAttribute), nameof(ScopedAttribute), nameof(TransientAttribute) }
                                                            .Select(n => n.Replace(nameof(Attribute), "", StringComparison.Ordinal))
                                                            .ToArray();

    [Test]
    public async Task Test_Works([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                            [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [{{prefix}}MightRequire{{suffix}}<int>("TestProp")]
        public interface IDeclareType {}

        [[|{{prefix}}{{attribute}}{{suffix}}<IDeclareType>|]]
        public class DeclareType : IDeclareType
        {
        }
        """;
        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_WithNonGeneric([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                                        [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [{{prefix}}MightRequire{{suffix}}("TestProp", typeof(int))]
        public interface IDeclareType {}

        [[|{{prefix}}{{attribute}}{{suffix}}(typeof(IDeclareType))|]]
        public class DeclareType : IDeclareType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnForLocal([ValueSource(nameof(Prefixes))] string prefix, [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [{{prefix}}MightRequire{{suffix}}("TestProp", typeof(int))]
        public interface IDeclareType {}

        [{{prefix}}Local{{suffix}}<IDeclareType>]
        public class TransientType : IDeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
        }

        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenNoMightRequireAttribute([ValueSource(nameof(Prefixes))] string prefix,
                                            [ValueSource(nameof(Attributes))] string attribute, [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        public interface IDeclareType {}

        [{{prefix}}{{attribute}}{{suffix}}<IDeclareType>]
        public class TransientType : IDeclareType
        {
            public string TestProp { get; set; }
        }

        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnForOtherMightRequire([ValueSource(nameof(Prefixes))] string prefix,
                                            [ValueSource(nameof(Attributes))] string attribute, [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        class MightRequireAttribute<T> : System.Attribute { public MightRequireAttribute(string s){} }

        [MightRequire{{suffix}}<int>("TestProp")]
        public interface IDeclareType {}

        [{{prefix}}{{attribute}}{{suffix}}<IDeclareType>]
        public class DeclareType : IDeclareType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnForOtherDependencyAttribute([ValueSource(nameof(Prefixes))] string prefix,
                                                [ValueSource(nameof(Attributes))] string attribute, [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$""""
        class {{attribute}}Attribute<T> : System.Attribute {}

        [{{prefix}}MightRequire{{suffix}}<int>("TestProp")]
        public interface IDeclareType {}

        [{{attribute}}{{suffix}}<IDeclareType>]
        public class DeclareType : IDeclareType
        {
        }
        """";

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }
}
