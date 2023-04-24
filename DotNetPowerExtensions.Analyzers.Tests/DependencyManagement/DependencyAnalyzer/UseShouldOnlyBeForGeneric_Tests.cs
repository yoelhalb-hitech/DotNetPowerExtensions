﻿using DotNetPowerExtensions.Analyzers.DependencyManagement.DependencyAttribute.Analyzers;
using SequelPay.DotNetPowerExtensions;

namespace DotNetPowerExtensions.Analyzers.Tests.DependencyManagement.DependencyAnalyzer;

internal class UseShouldOnlyBeForGeneric_Tests : AnalyzerVerifierBase<UseShouldOnlyBeForGeneric>
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1307:Specify StringComparison for clarity", Justification = "Older frameworks don't support it")]
    public static string[] Attributes => new string[] { nameof(SingletonAttribute), nameof(ScopedAttribute), nameof(TransientAttribute), nameof(LocalAttribute) }
                                                        .Select(n => n.Replace(nameof(Attribute), ""))
                                                        .ToArray();

    [Test]
    public async Task Test_Works([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                [Values("", nameof(Attribute))] string suffix, [Values("", "<TestType>")] string generics)
    {
        var test = $$"""
        [{{prefix}}{{attribute}}{{suffix}}{{generics}}([|Use=typeof(TestType)|])]
        public class TestType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_WorksWithParenthesis([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                            [Values("", nameof(Attribute))] string suffix, [Values("", "<TestType>")] string generics)
    {
        var test = $$"""
        [{{prefix}}{{attribute}}{{suffix}}{{generics}}([|Use=(typeof(TestType))|])]
        public class TestType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnOnNull([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                            [Values("", nameof(Attribute))] string suffix, [Values("", "<TestType>")] string generics)
    {
        var test = $$"""
        [{{prefix}}{{attribute}}{{suffix}}{{generics}}(Use=null)]
        public class TestType
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
    public async Task Test_DoesNotWarnWhenGeneric([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
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
    public async Task Test_DoesNotWarnWhenOtherAttribute([ValueSource(nameof(Attributes))] string attribute, [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        public class {{attribute}}Attribute : System.Attribute { public System.Type Use { get; set; } }
        [{{attribute}}{{suffix}}(Use=typeof(TestType))]
        public class TestType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }
}
