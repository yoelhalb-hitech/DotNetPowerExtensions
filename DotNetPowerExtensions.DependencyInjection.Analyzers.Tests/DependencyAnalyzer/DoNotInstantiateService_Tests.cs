namespace DotNetPowerExtensions.Analyzers.Tests.DependencyManagement.DependencyAnalyzer;

internal class DoNotInstantiateService_Tests : AnalyzerVerifierBase<DoNotInstantiateService>
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
    public async Task Test_MessageIsCorrect([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class TestType
        {
            public void Method()
            {
                var service = new LocalType();
            }
        }

        [{{prefix}}{{attribute}}{{suffix}}]
        public class LocalType {}
        """;

        await VerifyAnalyzerAsync(test, new DiagnosticResult("DNPE0225", DiagnosticSeverity.Warning)
                                                .WithSpan(6, 23, 6, 38).WithMessage("Do not instantiate a service manually, use DI instead")).ConfigureAwait(false);
    }

    /* Does not work with the current testing situation
    [Test]
    public async Task Test_WarnsForImplicitDecleration([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class TestType
        {
            public void Method()
            {
                var service = [|new()|];
            }
        }

        [{{prefix}}{{attribute}}{{suffix}}]
        public class LocalType {}
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }
    */

    [Test]
    public async Task Test_WarnsForInitializer([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class TestType
        {
            public void Method()
            {
                var service = [|new LocalType{}|];
            }
        }

        [{{prefix}}{{attribute}}{{suffix}}]
        public class LocalType {}
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenNoInstantiation([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class TestType
        {
            public void Method(LocalType lt)
            {
                LocalType lt2 = lt;
                LocalType? lt3 = null;
            }
        }

        [{{prefix}}{{attribute}}{{suffix}}]
        public class LocalType {}
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenOtherAttribute([ValueSource(nameof(Attributes))] string attribute, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class TestType
        {
            public void Method()
            {
                var service = new LocalType();
            }
        }

        public class {{attribute}}Attribute : System.Attribute {}

        [{{attribute}}{{suffix}}]
        public class LocalType {}
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }
}