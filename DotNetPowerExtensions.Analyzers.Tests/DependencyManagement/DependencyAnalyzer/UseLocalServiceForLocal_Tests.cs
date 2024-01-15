using SequelPay.DotNetPowerExtensions.Analyzers.DependencyManagement.DependencyAttribute.Analyzers;
using SequelPay.DotNetPowerExtensions;

namespace DotNetPowerExtensions.Analyzers.Tests.DependencyManagement.DependencyAnalyzer;

internal class UseLocalServiceForLocal_Tests : AnalyzerVerifierBase<UseLocalServiceForLocal>
{
    public static string[] Attributes => new string[] { nameof(SingletonAttribute), nameof(ScopedAttribute), nameof(TransientAttribute) }
                                                        .Select(n => n.Replace(nameof(Attribute), "", StringComparison.Ordinal))
                                                        .ToArray();

    [Test]
    public async Task Test_Works([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                                            [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        [{{prefix}}{{attribute}}{{suffix}}]
        public class TransientType
        {
            public TransientType([|LocalType t|]){}
            public string TestProp { get; set; }
        }

        [{{prefix}}Local{{suffix}}]
        public class LocalType {}
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_WithGenerics([ValueSource(nameof(Prefixes))] string prefix,
                                                [ValueSource(nameof(Attributes))] string attribute, [ValueSource(nameof(Suffixes))] string suffix)
    {

        var genericSuffix = suffix.Contains("()", StringComparison.Ordinal)
                                    ? suffix.Replace("()", "<TransientType>()", StringComparison.Ordinal)
                                    : suffix + "<TransientType>";

        var test = $$"""
        [{{prefix}}{{attribute}}{{genericSuffix}}]
        public class TransientType
        {
            public TransientType([|LocalType t|]){}
            public string TestProp { get; set; }
        }

        [{{prefix}}Local{{suffix}}]
        public class LocalType {}
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenNoAttribute()
    {
        var test = $$"""
        public class TransientType
        {
            public TransientType(LocalType t){}
            public string TestProp { get; set; }
        }

        public class LocalType {}
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenOtherLocal([ValueSource(nameof(Prefixes))] string prefix,
                                                [ValueSource(nameof(Attributes))] string attribute, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        [{{prefix}}{{attribute}}{{suffix}}]
        public class TransientType
        {
            public TransientType(LocalType t){}
            public string TestProp { get; set; }
        }

        public class LocalAttribute : System.Attribute {}
        [Local{{suffix}}]
        public class LocalType {}
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }
}
