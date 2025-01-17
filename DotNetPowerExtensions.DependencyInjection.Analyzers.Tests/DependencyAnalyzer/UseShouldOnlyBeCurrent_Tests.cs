﻿
namespace DotNetPowerExtensions.Analyzers.Tests.DependencyManagement.DependencyAnalyzer;

internal class UseShouldOnlyBeCurrent_Tests : AnalyzerVerifierBase<UseShouldOnlyBeCurrent>
{
    public static string[] Attributes => new string[] { nameof(SingletonAttribute), nameof(ScopedAttribute), nameof(TransientAttribute), nameof(LocalAttribute) }
                                                        .Select(n => n.Replace(nameof(Attribute), "", StringComparison.Ordinal))
                                                        .ToArray();

    [Test]
    public async Task Test_MessageIsCorrect()
    {
        var test = $$"""
        public interface ITestType {}
        public interface ITestType2 {}
        [Transient<ITestType>(Use=typeof(System.Collections.Generic.List<string>))]
        public class TestType<T> : ITestType, ITestType2
        {
        }
        """;

        await VerifyAnalyzerAsync(test, new DiagnosticResult("DNPE0207", DiagnosticSeverity.Warning)
                                                .WithSpan(4, 23, 4, 74).WithMessage("The `Use` attribute should only be the current generic type")).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                            [Values("", nameof(Attribute))] string suffix, [Values("", "<ITestType>", "<ITestType, ITestType2>")] string generics)
    {
        var test = $$"""
        public interface ITestType {}
        public interface ITestType2 {}
        [{{prefix}}{{attribute}}{{suffix}}{{generics}}([|Use=typeof(System.Collections.Generic.List<string>)|])]
        public class TestType<T> : ITestType, ITestType2
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_WithParenthesis([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                            [Values("", nameof(Attribute))] string suffix, [Values("", "<ITestType>", "<ITestType, ITestType2>")] string generics)
    {
        var test = $$"""
        public interface ITestType {}
        public interface ITestType2 {}
        [{{prefix}}{{attribute}}{{suffix}}{{generics}}([|Use=((typeof(System.Collections.Generic.List<string>)))|])]
        public class TestType<T> : ITestType, ITestType2
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenNoUse([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                            [Values("", nameof(Attribute))] string suffix, [Values("", "<TestType>")] string generics, [Values("", "()")] string paren)
    {
        var test = $$"""
        [{{prefix}}{{attribute}}{{suffix}}{{generics}}{{paren}}]
        public class TestType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenCurrent([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                [Values("", nameof(Attribute))] string suffix, [Values("", "<ITestType>")] string generics)
    {
        var test = $$"""
        public interface ITestType {}
        [{{prefix}}{{attribute}}{{suffix}}{{generics}}(Use=typeof(TestType<string>))]
        public class TestType<T> : ITestType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenCurrentFullQualified([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                            [Values("", nameof(Attribute))] string suffix, [Values("", "<ITestType>")] string generics)
    {
        var test = $$"""
        namespace Outer.Inner;
        public class OuterClass
        {
            public interface ITestType {}
            [{{prefix}}{{attribute}}{{suffix}}{{generics}}(Use=typeof(Outer.Inner.OuterClass.TestType<string>))]
            public class TestType<T> : ITestType
            {
            }
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenCurrentAndParenthesis([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                [Values("", nameof(Attribute))] string suffix, [Values("", "<ITestType>")] string generics)
    {
        var test = $$"""
        public interface ITestType {}
        [{{prefix}}{{attribute}}{{suffix}}{{generics}}(Use=((typeof(TestType<string>))))]
        public class TestType<T> : ITestType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenOtherAttribute([ValueSource(nameof(Attributes))] string attribute, [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        public class {{attribute}}Attribute : System.Attribute { public System.Type Use { get; set; } }
        [{{attribute}}{{suffix}}(Use=typeof(System.Type))]
        public class TestType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }
}
