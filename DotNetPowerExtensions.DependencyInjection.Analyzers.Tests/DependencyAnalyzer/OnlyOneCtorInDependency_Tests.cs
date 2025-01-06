namespace DotNetPowerExtensions.Analyzers.Tests.DependencyManagement.DependencyAnalyzer;

internal class OnlyOneCtorInDependency_Tests : AnalyzerVerifierBase<OnlyOneCtorInDependency>
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
        [{{prefix}}{{attribute}}{{suffix}}]
        public class TransientType
        {
            public TransientType() {}
            public TransientType(int value) {}
        }
        """;

        await VerifyAnalyzerAsync(test, new DiagnosticResult("DNPE0226", DiagnosticSeverity.Warning)
                                                .WithSpan(2, 1, 7, 2).WithMessage("Only one public ctor is allowed in a class decorated with `Singleton/Scoped/Transient/Local` attribute")).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenSingleCtor([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        [{{prefix}}{{attribute}}{{suffix}}]
        public class TransientType
        {
            public TransientType() {}
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenNoCtor([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        [{{prefix}}{{attribute}}{{suffix}}]
        public class TransientType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenMultiplePrivateCtors([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        [{{prefix}}{{attribute}}{{suffix}}]
        public class TransientType
        {
            private TransientType() {}
            private TransientType(int value) {}
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenMultipleProtectedCtors([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        [{{prefix}}{{attribute}}{{suffix}}]
        public class TransientType
        {
            protected TransientType() {}
            protected TransientType(int value) {}
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_WarnsWhenMultiplePublicCtors([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        [|[{{prefix}}{{attribute}}{{suffix}}]
        public class SingletonType
        {
            public SingletonType() {}
            public SingletonType(int value) {}
        }|]
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_WarnsWhenMultipleInternalCtors([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        [|[{{prefix}}{{attribute}}{{suffix}}]
        public class ScopedType
        {
            internal ScopedType() {}
            internal ScopedType(int value) {}
        }|]
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_WarnsWhenPublicAndInternalCtors([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        [|[{{prefix}}{{attribute}}{{suffix}}]
        public class ScopedType
        {
            public ScopedType() {}
            internal ScopedType(int value) {}
        }|]
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenOtherAttribute([ValueSource(nameof(Attributes))] string attribute, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class {{attribute}}Attribute : System.Attribute {}

        [{{attribute}}{{suffix}}]
        public class TransientType
        {
            public TransientType() {}
            public TransientType(int value) {}
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }
}