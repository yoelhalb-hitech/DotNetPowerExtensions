
namespace DotNetPowerExtensions.Analyzers.Tests.DependencyManagement.DependencyAnalyzer;

internal class UseLocalServiceOnlyInDependency_Tests : AnalyzerVerifierBase<UseLocalServiceOnlyInDependency>
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
        public class TransientType
        {
            public TransientType(ILocalFactory<LocalType> t){}
            public string TestProp { get; set; }
        }

        public class LocalType {}
        """;

        await VerifyAnalyzerAsync(test, new DiagnosticResult("DNPE0220", DiagnosticSeverity.Warning)
                                                .WithSpan(4, 26, 4, 52).WithMessage("Only use `ILocalFactory<>` in a class decorated with `Singleton/Scoped/Transient/Local` attribute")).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                                            [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class TransientType
        {
            public TransientType([|ILocalFactory<LocalType> t|]){}
            public string TestProp { get; set; }
        }

        public class LocalType {}
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenAttribute([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
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

        public class LocalType {}
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }


    [Test]
    public async Task Test_DoesNotWarnWhenOtherLocalFactory([ValueSource(nameof(Prefixes))] string prefix,
                                                [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class TransientType
        {
            public TransientType(ILocalFactory<LocalType> t){}
            public string TestProp { get; set; }
        }

        public interface ILocalFactory<T> {}
        public class LocalType {}
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }
}
