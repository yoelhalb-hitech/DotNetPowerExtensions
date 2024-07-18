using DotNetPowerExtensions.Analyzers.DependencyManagement.DependencyAttribute.Analyzers;
using Microsoft.CodeAnalysis.Testing;
using SequelPay.DotNetPowerExtensions;
using System.Security.AccessControl;

namespace DotNetPowerExtensions.Analyzers.Tests.DependencyManagement.DependencyAnalyzer;

internal class ForTypeMustBeParent_Tests : AnalyzerVerifierBase<ForTypeMustBeParent>
{
    public static string[] Attributes => new string[] { nameof(SingletonAttribute), nameof(ScopedAttribute), nameof(TransientAttribute), nameof(LocalAttribute) }
                                                        .Select(n => n.Replace(nameof(Attribute), "", StringComparison.Ordinal))
                                                        .ToArray();

    [Test]
    public async Task Test_WorksWithNonGeneric([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                                    [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [[|{{prefix}}{{attribute}}{{suffix}}(typeof(string))|]]
        public class TestType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_MessageIsCorrect()
    {
        var test = $$"""
        public interface TestIFace {}
        [Transient(typeof(TestIFace))]
        public class TestType<T>
        {
        }
        """;

        await VerifyAnalyzerAsync(test, new DiagnosticResult("DNPE0210", DiagnosticSeverity.Warning)
                                                .WithSpan(3, 2, 3, 30).WithMessage("TestIFace is not a base class or interface")).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_WorksWithSingleGeneric([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                                [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [[|{{prefix}}{{attribute}}{{suffix}}<string>|]]
        public class TestType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_WorksWithTwoGenerics([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                            [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [[|[|{{prefix}}{{attribute}}{{suffix}}<string, int>|]|]]
        public class TestType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }


    [Test]
    public async Task Test_Works_WithArgumentAndParenthesis([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                                                [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        public interface ITestType {}
        public interface ITestType2 {}
        [[|{{prefix}}{{attribute}}{{suffix}}(((typeof(System.Collections.Generic.List<string>))))|]]
        public class TestType<T> : ITestType, ITestType2
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenNoArg([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                                            [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        [{{prefix}}{{attribute}}{{suffix}}]
        public class TestType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenSameType([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                                        [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [{{prefix}}{{attribute}}{{suffix}}<TestType>]
        public class TestType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenBaseAndGeneric([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                [Values("", nameof(Attribute))] string suffix, [Values("interface", "class")] string baseType)
    {
        var test = $$"""
        public {{baseType}} ITestType {}
        [{{prefix}}{{attribute}}{{suffix}}<ITestType>()]
        public class TestType : ITestType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenBaseAndNonGeneric([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                    [Values("", nameof(Attribute))] string suffix, [Values("interface", "class")] string baseType)
    {
        var test = $$"""
        public {{baseType}} ITestType {}
        [{{prefix}}{{attribute}}{{suffix}}<ITestType>()]
        public class TestType : ITestType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenOtherAttribute([ValueSource(nameof(Attributes))] string attribute, [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        public class {{attribute}}Attribute<T> : System.Attribute { }
        [{{attribute}}{{suffix}}<System.Type>]
        public class TestType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }
}
