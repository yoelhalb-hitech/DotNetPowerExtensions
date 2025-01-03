
namespace DotNetPowerExtensions.Analyzers.Tests.DependencyManagement.DependencyAnalyzer;

internal class UseTransientOnlyInTransient_Tests : AnalyzerVerifierBase<UseTransientOnlyInTransient>
{
    public static string[] Attributes => new string[]
    {
        nameof(LocalAttribute),
        nameof(SingletonAttribute),
        nameof(ScopedAttribute),
    }
    .Select(n => n.Replace(nameof(Attribute), "", StringComparison.Ordinal))
    .ToArray();

    [Test]
    public async Task Test_MessageIsCorrect()
    {
        var test = $$"""
        [Scoped]
        public class TransientType
        {
            public TransientType(LocalType t){}
            public string TestProp { get; set; }
        }

        [Transient]
        public class LocalType {}
        """;

        await VerifyAnalyzerAsync(test, new DiagnosticResult("DNPE0223", DiagnosticSeverity.Warning)
                                                .WithSpan(5, 26, 5, 37).WithMessage("A transient service should only be used in a class decorated with `Transient` or `Local`")).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                                            [ValueSource(nameof(Suffixes))] string suffix)
    {
        Assume.That(attribute, Is.Not.EqualTo("Local"));

        var test = $$"""
        [{{prefix}}{{attribute}}{{suffix}}]
        public class TransientType
        {
            public TransientType([|LocalType t|]){}
            public string TestProp { get; set; }
        }

        [{{prefix}}Transient{{suffix}}]
        public class LocalType {}
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenNotTransient([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                                        [ValueSource(nameof(Suffixes))] string suffix)
    {
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
    public async Task Test_DoesNotWarn_WhenClassIsTransient([ValueSource(nameof(Prefixes))] string prefix,
                                                                                                    [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        [{{prefix}}Transient{{suffix}}]
        public class TransientType
        {
            public TransientType(LocalType t){}
            public string TestProp { get; set; }
        }

        [{{prefix}}Transient{{suffix}}]
        public class LocalType {}
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_WarnsWhenClassIsAlsoTransient([ValueSource(nameof(Prefixes))] string prefix,
                            [ValueSource(nameof(Attributes))] string attribute, [ValueSource(nameof(Suffixes))] string suffix)
    {
        Assume.That(attribute, Is.Not.EqualTo("Local"));

        var test = $$"""
        [{{prefix}}{{attribute}}{{suffix}}]
        [{{prefix}}Transient{{suffix}}]
        [{{prefix}}Local{{suffix}}]
        public class TransientType
        {
            public TransientType([|LocalType t|]){}
            public string TestProp { get; set; }
        }

        [{{prefix}}Transient{{suffix}}]
        public class LocalType {}
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenClassIsLocal([ValueSource(nameof(Prefixes))] string prefix,
                                                                                                [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        [{{prefix}}Local{{suffix}}]
        public class TransientType
        {
            public TransientType(LocalType t){}
            public string TestProp { get; set; }
        }

        [{{prefix}}Transient{{suffix}}]
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

        [{{prefix}}{{attribute}}{{suffix}}]
        public class TransientType
        {
            public TransientType(LocalType t){}
            public string TestProp { get; set; }
        }

        public class TransientAttribute : System.Attribute { public System.Type Use { get; set; } }

        [Transient{{suffix}}]
        public class LocalType {}
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }
}
