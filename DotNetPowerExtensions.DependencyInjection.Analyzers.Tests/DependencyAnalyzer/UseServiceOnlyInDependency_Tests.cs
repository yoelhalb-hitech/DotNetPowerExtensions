
namespace DotNetPowerExtensions.Analyzers.Tests.DependencyManagement.DependencyAnalyzer;

internal class UseServiceOnlyInDependency_Tests : AnalyzerVerifierBase<UseServiceOnlyInDependency>
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
            public TransientType(LocalType t){}
            public string TestProp { get; set; }
        }

        [Transient]
        public class LocalType {}
        """;

        await VerifyAnalyzerAsync(test, new DiagnosticResult("DNPE0221", DiagnosticSeverity.Warning)
                                                .WithSpan(4, 26, 4, 37).WithMessage("Use a service only in a class decorated with `Singleton/Scoped/Transient/Local` attribute")).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                                            [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class TransientType
        {
            public TransientType([|LocalType t|]){}
            public string TestProp { get; set; }
        }

        [{{prefix}}{{attribute}}{{suffix}}]
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
            public TransientType(LocalType t){}
            public string TestProp { get; set; }
        }

        [{{prefix}}{{attribute}}{{suffix}}]
        public class LocalType {}
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }


    [Test]
    public async Task Test_DoesNotWarnWhenOtherAttribute([ValueSource(nameof(Prefixes))] string prefix,
                        [ValueSource(nameof(Attributes))] string attribute,
                        [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class TransientType
        {
            public TransientType(LocalType t){}
            public string TestProp { get; set; }
        }

        public class {{attribute}}Attribute : System.Attribute { public System.Type Use { get; set; } }

        [{{attribute}}{{suffix}}]
        public class LocalType {}
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_WarnsWhenOtherAttribute_OnClass([ValueSource(nameof(Prefixes))] string prefix,
                    [ValueSource(nameof(Attributes))] string attribute,
                    [ValueSource(nameof(Attributes))] string attribute2,
                    [ValueSource(nameof(Suffixes))] string suffix)
    {

        Assume.That(attribute, Is.Not.EqualTo(attribute2));

        var test = $$"""
        public class {{attribute2}}Attribute : System.Attribute { public System.Type Use { get; set; } }

        [{{attribute2}}{{suffix}}]
        public class TransientType
        {
            public TransientType([|LocalType t|]){}
            public string TestProp { get; set; }
        }


        [{{prefix}}{{attribute}}{{suffix}}]
        public class LocalType {}
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }
}
