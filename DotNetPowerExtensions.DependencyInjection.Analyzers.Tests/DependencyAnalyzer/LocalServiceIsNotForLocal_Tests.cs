
namespace DotNetPowerExtensions.Analyzers.Tests.DependencyManagement.DependencyAnalyzer;

internal class LocalServiceIsNotForLocal_Tests : AnalyzerVerifierBase<LocalServiceIsNotForLocal>
{
    public static string[] Attributes => new string[]
    {
        nameof(SingletonAttribute),
        nameof(ScopedAttribute),
        nameof(TransientAttribute),
        nameof(LocalAttribute),
    }
    .Select(n => n.Replace(nameof(Attribute), "", StringComparison.Ordinal))
    .ToArray();

    [Test]
    public async Task Test_MessageIsCorrect()
    {
        var test = $$"""
        [Transient]
        public class TransientType
        {
            public TransientType(ILocalFactory<LocalType> t){}
            public string TestProp { get; set; }
        }

        [Transient]
        public class LocalType {}
        """;

        await VerifyAnalyzerAsync(test, new DiagnosticResult("DNPE0219", DiagnosticSeverity.Warning)
                                                .WithSpan(5, 26, 5, 52).WithMessage("`LocalType` is not decorated with the `Local` attribute")).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works([ValueSource(nameof(Prefixes))] string prefix,
            [ValueSource(nameof(Attributes))] string attribute,
            [ValueSource(nameof(Attributes))] string originalAttribute,
            [ValueSource(nameof(Suffixes))] string suffix)
    {
        Assume.That(originalAttribute, Is.Not.EqualTo("Local"));

        var test = $$"""
        [{{prefix}}{{attribute}}{{suffix}}]
        public class TransientType
        {
            public TransientType([|ILocalFactory<LocalType> t|]){}
            public string TestProp { get; set; }
        }

        [{{prefix}}{{originalAttribute}}{{suffix}}]
        public class LocalType {}
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_WithGenerics([ValueSource(nameof(Prefixes))] string prefix,
                                                [ValueSource(nameof(Attributes))] string attribute,
                                                [ValueSource(nameof(Attributes))] string originalAttribute,
                                                [ValueSource(nameof(Suffixes))] string suffix)
    {
        Assume.That(originalAttribute, Is.Not.EqualTo("Local"));

        var genericSuffix = suffix.Contains("()", StringComparison.Ordinal)
                                    ? suffix.Replace("()", "<TransientType>()", StringComparison.Ordinal)
                                    : suffix + "<TransientType>";

        var test = $$"""
        [{{prefix}}{{attribute}}{{genericSuffix}}]
        public class TransientType
        {
            public TransientType([|ILocalFactory<LocalType> t|]){}
            public string TestProp { get; set; }
        }

        [{{prefix}}{{originalAttribute}}{{suffix}}]
        public class LocalType {}
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenOriginalTypeHasOther([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                                        [ValueSource(nameof(Suffixes))] string suffix)
    {
        Assume.That(attribute, Is.Not.EqualTo("Local"));

        var test = $$"""
        [{{prefix}}{{attribute}}{{suffix}}]
        public class TransientType
        {
            public TransientType(ILocalFactory<LocalType> t){}
            public string TestProp { get; set; }
        }

        [{{prefix}}{{attribute}}{{suffix}}]
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
            public TransientType(ILocalFactory<LocalType> t){}
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
            public TransientType(ILocalFactory<LocalType> t){}
            public string TestProp { get; set; }
        }

        public interface ILocalFactory<T>{}
        public class LocalType {}
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }
}
