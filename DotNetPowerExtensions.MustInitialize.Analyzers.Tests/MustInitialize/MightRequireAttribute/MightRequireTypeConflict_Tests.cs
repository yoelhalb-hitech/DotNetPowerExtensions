using Microsoft.CodeAnalysis.Testing;

namespace DotNetPowerExtensions.Analyzers.Tests.MustInitialize.MightRequireAttribute;

internal class MightRequireTypeConflict_Tests : AnalyzerVerifierBase<MightRequireTypeConflict>
{
    public static string[] Attributes => new string[] { nameof(MightRequireAttribute) }
                                                        .Select(n => n.Replace(nameof(Attribute), "", StringComparison.Ordinal))
                                                        .ToArray();

    [Test]
    public async Task Test_MessageIsCorrect()
    {
        var test = $$"""
        [MightRequire<int>("TestName")]
        [MightRequire<string>("TestName")]
        public class TestType
        {
        }
        """;

        await VerifyAnalyzerAsync(test, new DiagnosticResult("DNPE0214", DiagnosticSeverity.Warning)
                                            .WithSpan(2, 2, 2, 31).WithMessage("Ambiguous MightRequire definiton for `TestName` having types `Int32, String`"),
                                         new DiagnosticResult("DNPE0214", DiagnosticSeverity.Warning)
                                            .WithSpan(3, 2, 3, 34).WithMessage("Ambiguous MightRequire definiton for `TestName` having types `Int32, String`"))
            .ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_WithArguments([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                                [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [[|{{prefix}}{{attribute}}{{suffix}}("TestName", typeof(int))|]]
        [[|{{prefix}}{{attribute}}{{suffix}}("TestName", typeof(string))|]]
        public class TestType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_WithGenerics([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                            [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [[|{{prefix}}{{attribute}}{{suffix}}<int>("TestName")|]]
        [[|{{prefix}}{{attribute}}{{suffix}}<string>("TestName")|]]
        public class TestType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_WithBase([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                        [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [[|{{prefix}}{{attribute}}{{suffix}}<int>("TestName")|]]
        public class TestTypeBase
        {
        }

        [[|{{prefix}}{{attribute}}{{suffix}}<string>("TestName")|]]
        public class TestType : TestTypeBase
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_WithInterface([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                    [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [[|{{prefix}}{{attribute}}{{suffix}}<int>("TestName")|]]
        public interface TestTypeBase
        {
        }

        [[|{{prefix}}{{attribute}}{{suffix}}<string>("TestName")|]]
        public class TestType : TestTypeBase
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_WithMultipleInterface([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [[|{{prefix}}{{attribute}}{{suffix}}<int>("TestName")|]]
        public interface TestTypeBase1
        {
        }

        [[|{{prefix}}{{attribute}}{{suffix}}<string>("TestName")|]]
        public interface TestTypeBase2
        {
        }

        public class TestType : TestTypeBase1, TestTypeBase2
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_WarnsOnBaseWhenBaseHasAll([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                            [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [[|{{prefix}}{{attribute}}{{suffix}}<int>("TestName")|]]
        [[|{{prefix}}{{attribute}}{{suffix}}<string>("TestName")|]]
        public interface TestTypeBase
        {
        }

        public class TestType : TestTypeBase
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_WithArgumentsAndParenthesis([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                            [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [[|{{prefix}}{{attribute}}{{suffix}}("TestName", ((typeof(int))))|]]
        [[|{{prefix}}{{attribute}}{{suffix}}((("TestName")), typeof(string))|]]
        public class TestType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_WithArgumentsAndGenerics([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                        [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [[|{{prefix}}{{attribute}}{{suffix}}("TestName", ((typeof(int))))|]]
        [[|{{prefix}}{{attribute}}{{suffix}}<string>((("TestName")))|]]
        public class TestType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_WithMoreThanTwo([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                    [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [[|{{prefix}}{{attribute}}{{suffix}}("TestName", ((typeof(int))))|]]
        [[|{{prefix}}{{attribute}}{{suffix}}<string>((("TestName")))|]]
        [[|{{prefix}}{{attribute}}{{suffix}}<string[]>((("TestName")))|]]
        public class TestType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_WithPartial([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [[|{{prefix}}{{attribute}}{{suffix}}("TestName", ((typeof(int))))|]]
        public partial class TestType
        {
        }

        public partial class TestType
        {
        }

        [[|{{prefix}}{{attribute}}{{suffix}}<string>((("TestName")))|]]
        public partial class TestType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_WhenOtherValid([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                            [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [[|{{prefix}}{{attribute}}{{suffix}}<int>("TestName")|]]
        [{{prefix}}{{attribute}}{{suffix}}<int>("TestingName")]
        [[|{{prefix}}{{attribute}}{{suffix}}<string>("TestName")|]]
        public class TestType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenOtherName([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                        [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [{{prefix}}{{attribute}}{{suffix}}<int>("TestName")]
        [{{prefix}}{{attribute}}{{suffix}}<string>("TestingName")]
        public class TestType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenOtherAttribute([ValueSource(nameof(Attributes))] string attribute, [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        namespace A // Not to conflict with the imported attribute
        {
            [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple=true)]
            public class {{attribute}}Attribute<TType> : System.Attribute { public {{attribute}}Attribute(string name){} }
            [{{attribute}}{{suffix}}<int>("TestName")]
            [{{attribute}}{{suffix}}<string>("TestName")]
            public class TestType
            {
            }
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }
}
