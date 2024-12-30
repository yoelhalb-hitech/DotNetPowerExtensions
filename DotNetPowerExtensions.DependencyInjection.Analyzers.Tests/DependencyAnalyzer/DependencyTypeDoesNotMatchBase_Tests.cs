using SequelPay.DotNetPowerExtensions.Analyzers.DependencyManagement.DependencyAttribute.Analyzers;
using Microsoft.CodeAnalysis.Testing;
using Newtonsoft.Json.Linq;
using SequelPay.DotNetPowerExtensions;
using System;

namespace DotNetPowerExtensions.Analyzers.Tests.DependencyManagement.DependencyAnalyzer;

internal class DependencyTypeDoesNotMatchBase_Tests : AnalyzerVerifierBase<DependencyTypeDoesNotMatchBase>
{
    public static string[] Attributes => new string[] { nameof(SingletonAttribute), nameof(ScopedAttribute), nameof(TransientAttribute), nameof(LocalAttribute) }
                                                        .Select(n => n.Replace(nameof(Attribute), "", StringComparison.Ordinal))
                                                        .ToArray();

    [Test]
    public async Task Test_MessageIsCorrect()
    {
        var test = $$"""
        [TransientBase()] public interface TestIFace {}
        [Scoped(typeof(TestIFace))]
        public class TestType : TestIFace
        {
        }
        """;

        await VerifyAnalyzerAsync(test, new DiagnosticResult("DNPE0212", DiagnosticSeverity.Warning)
                                                .WithSpan(3, 2, 3, 27).WithMessage("Use `Transient` for `TestIFace` and not `Scoped`")).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_WorksWithParameter([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                    [ValueSource(nameof(Attributes))] string baseAttribute, [Values("", nameof(Attribute))] string suffix, [Values(true, false)] bool isIface)
    {
        Assume.That(baseAttribute, Is.Not.EqualTo(attribute));

        var test = $$"""
        [{{prefix}}{{baseAttribute}}Base{{suffix}}] public {{(isIface ? "interface" : "class")}} ITestType {}
        [[|{{prefix}}{{attribute}}{{suffix}}(typeof(ITestType))|]]
        public class TestType : ITestType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_WorksWithGeneric([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                [ValueSource(nameof(Attributes))] string baseAttribute, [Values("", nameof(Attribute))] string suffix, [Values(true, false)] bool isIface)
    {
        Assume.That(baseAttribute, Is.Not.EqualTo(attribute));

        var test = $$"""
        [{{prefix}}{{baseAttribute}}Base{{suffix}}] public {{(isIface ? "interface" : "class")}} ITestType {}
        [[|{{prefix}}{{attribute}}{{suffix}}<ITestType>|]]
        public class TestType : ITestType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenSameAndParameter([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                    [Values("", nameof(Attribute))] string suffix, [Values(true, false)] bool isIface)
    {
        var test = $$"""
        [{{prefix}}{{attribute}}Base{{suffix}}] public {{(isIface ? "interface" : "class")}} ITestType {}
        [{{prefix}}{{attribute}}{{suffix}}(typeof(ITestType))]
        public class TestType : ITestType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenSameAndGeneric([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                [Values("", nameof(Attribute))] string suffix, [Values(true, false)] bool isIface)
    {
        var test = $$"""
        [{{prefix}}{{attribute}}Base{{suffix}}] public {{(isIface ? "interface" : "class")}} ITestType {}
        [{{prefix}}{{attribute}}{{suffix}}<ITestType>]
        public class TestType : ITestType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }


    [Test]
    public async Task Test_DoesNotWarnWhenOtherAttribute([ValueSource(nameof(Attributes))] string attribute,
                [ValueSource(nameof(Attributes))] string baseAttribute, [Values("", nameof(Attribute))] string suffix)
    {
        Assume.That(baseAttribute, Is.Not.EqualTo(attribute));

        var test = $$"""
        public class {{attribute}}Attribute<T> : System.Attribute {}
        public class {{baseAttribute}}BaseAttribute : System.Attribute {}

        [{{baseAttribute}}Base{{suffix}}] public interface ITestType {}
        [{{attribute}}{{suffix}}<ITestType>]
        public abstract class TestType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }
}
