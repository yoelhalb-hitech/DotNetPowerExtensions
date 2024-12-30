
namespace DotNetPowerExtensions.Analyzers.Tests.DependencyManagement.ILocalFactory;

internal class OriginalNotExisting_Tests : AnalyzerVerifierBase<OriginalNotExisting>
{
    [Test]
    public async Task Test_HasCorrectMessage([ValueSource(nameof(Prefixes))] string prefix)
    {
        var test = $$"""
        public class DeclareType {}

        class Program { void Main() =>
            (null as {{prefix}}ILocalFactory<DeclareType>).Create(new
            {
                TestProp = 10
            }); }
        """;

        await VerifyAnalyzerAsync(test, new DiagnosticResult("DNPE0217", DiagnosticSeverity.Warning)
                                            .WithSpan(7, 9, 7, 17)
                                            .WithMessage("Member 'TestProp' does not exist"))
            .ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenNotILocalFactory()
    {
        var test = $$"""
        public class DeclareType{}

        class Program { void Main() =>
            (null as dynamic).Create(new { TestProp = 10 }); }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenOtherILocalFactory()
    {
        var test = $$"""
        public interface ILocalFactory<T> { public void Create(object obj); }
        public class DeclareType
        {
        }

        class Program { void Main() =>
            (null as ILocalFactory<DeclareType>).Create(new
            {
                TestProp = 10,
                TestGeneralName = "",
                TestField = true
            }); }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works([ValueSource(nameof(Prefixes))] string prefix)
    {
        var test = $$"""
        public class DeclareType
        {
        }

        class Program { void Main() =>
            (null as {{prefix}}ILocalFactory<DeclareType>).Create(new
            {
                [|TestProp|] = 10,
                [|TestGeneralName|] = "",
                [|TestField|] = true
            }); }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenHasProperty([ValueSource(nameof(Prefixes))] string prefix)
    {
        var test = $$"""
        public class DeclareType
        {
            public int TestProp { get; set; }
        }

        class Program { void Main() =>
            (null as {{prefix}}ILocalFactory<DeclareType>).Create(new
            {
                TestProp = 10,
            }); }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenHasField([ValueSource(nameof(Prefixes))] string prefix)
    {
        var test = $$"""
        public class DeclareType
        {
            public int TestField;
        }

        class Program { void Main() =>
            (null as {{prefix}}ILocalFactory<DeclareType>).Create(new
            {
                TestField = 10,
            }); }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenHasMightRequire([ValueSource(nameof(Prefixes))] string prefix, [Values(nameof(Attribute), "")] string suffix)
    {
        var test = $$"""
        [{{prefix}}MightRequire{{suffix}}<int>("TestMightRequire")]
        public class DeclareType
        {
        }

        class Program { void Main() =>
            (null as ILocalFactory<DeclareType>).Create(new
            {
                TestMightRequire = 10,
            }); }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }
}
