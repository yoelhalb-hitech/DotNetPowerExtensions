
namespace DotNetPowerExtensions.Analyzers.Tests.DependencyManagement.DependencyAnalyzer;

internal class NoScopedInSingleton_Tests : AnalyzerVerifierBase<NoScopedInSingleton>
{
    public static string[] Attributes => new string[]
    {
        nameof(LocalAttribute),
        nameof(SingletonAttribute),
        nameof(ScopedAttribute),
        nameof(TransientAttribute),
    }
    .Select(n => n.Replace(nameof(Attribute), "", StringComparison.Ordinal))
    .ToArray();

    [Test]
    public async Task Test_MessageIsCorrect()
    {
        var test = $$"""
        [Singleton]
        public class TransientType
        {
            public TransientType(LocalType t){}
            public string TestProp { get; set; }
        }

        [Scoped]
        public class LocalType {}
        """;

        await VerifyAnalyzerAsync(test, new DiagnosticResult("DNPE0224", DiagnosticSeverity.Warning)
                                                .WithSpan(5, 26, 5, 37).WithMessage("Do not use scoped service in a class decorated with `Singleton`")).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        [{{prefix}}Singleton{{suffix}}]
        public class TransientType
        {
            public TransientType([|LocalType t|]){}
            public string TestProp { get; set; }
        }

        [{{prefix}}Scoped{{suffix}}]
        public class LocalType {}
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenNotScoped([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                                        [ValueSource(nameof(Suffixes))] string suffix)
    {
        Assume.That(attribute, Is.Not.EqualTo("Scoped"));

        var test = $$"""
        [{{prefix}}Singleton{{suffix}}]
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
    public async Task Test_DoesNotWarn_WhenClassIsNotSingleton([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                                    [ValueSource(nameof(Suffixes))] string suffix)
    {
        Assume.That(attribute, Is.Not.EqualTo("Singleton"));

        var test = $$"""
        [{{prefix}}{{attribute}}{{suffix}}]
        public class TransientType
        {
            public TransientType(LocalType t){}
            public string TestProp { get; set; }
        }

        [{{prefix}}Scoped{{suffix}}]
        public class LocalType {}
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_WarnsWhenClassIsAlsoOther([ValueSource(nameof(Prefixes))] string prefix,
                            [ValueSource(nameof(Attributes))] string attribute, [ValueSource(nameof(Suffixes))] string suffix)
    {
        Assume.That(attribute, Is.Not.EqualTo("Singleton"));

        var test = $$"""
        [{{prefix}}{{attribute}}{{suffix}}]
        [{{prefix}}Singleton{{suffix}}]
        public class TransientType
        {
            public TransientType([|LocalType t|]){}
            public string TestProp { get; set; }
        }

        [{{prefix}}Scoped{{suffix}}]
        public class LocalType {}
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenOtherScopedAttribute([ValueSource(nameof(Prefixes))] string prefix,
                        [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""

        [{{prefix}}Singleton{{suffix}}]
        public class TransientType
        {
            public TransientType(LocalType t){}
            public string TestProp { get; set; }
        }

        public class ScopedAttribute : System.Attribute { public System.Type Use { get; set; } }

        [Scoped{{suffix}}]
        public class LocalType {}
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenOtherSingletonAttribute([ValueSource(nameof(Prefixes))] string prefix,
                    [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""

        public class SingletonAttribute : System.Attribute { public System.Type Use { get; set; } }

        [Singleton{{suffix}}]
        public class TransientType
        {
            public TransientType(LocalType t){}
            public string TestProp { get; set; }
        }


        [{{prefix}}Scoped{{suffix}}]
        public class LocalType {}
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }
}
