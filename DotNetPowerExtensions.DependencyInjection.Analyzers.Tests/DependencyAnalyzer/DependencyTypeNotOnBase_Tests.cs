
namespace DotNetPowerExtensions.Analyzers.Tests.DependencyManagement.DependencyAnalyzer;

internal class DependencyTypeNotOnBase_Tests : AnalyzerVerifierBase<DependencyTypeNotOnBase>
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
                                                .WithSpan(3, 2, 3, 27).WithMessage("`ScopedBaseAttribute` not found on `TestIFace`, add `ScopedBaseAttribute` to the decleration of `TestIFace`")).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_WorksWhenBaseNothing_WithParameter([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                [ValueSource(nameof(Attributes))] string baseAttribute, [Values("", nameof(Attribute))] string suffix, [Values(true, false)] bool isIface)
    {
        Assume.That(baseAttribute, Is.Not.EqualTo(attribute));

        var test = $$"""
        public {{(isIface ? "interface" : "class")}} ITestType {}
        [[|{{prefix}}{{attribute}}{{suffix}}(typeof(ITestType))|]]
        public class TestType : ITestType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
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
    public async Task Test_WorksWithTwoBases_ParamVersion([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                [ValueSource(nameof(Attributes))] string baseAttribute, [Values("", nameof(Attribute))] string suffix, [Values(true, false)] bool isIface)
    {
        Assume.That(baseAttribute, Is.Not.EqualTo(attribute));

        var test = $$"""
        [{{prefix}}{{attribute}}Base{{suffix}}] public {{(isIface ? "interface" : "class")}} ITestType {}
        [{{prefix}}{{baseAttribute}}Base{{suffix}}] public interface ITestType2 {}
        [[|{{prefix}}{{attribute}}{{suffix}}(typeof(ITestType), typeof(ITestType2))|]]
        public class TestType : ITestType, ITestType2
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_WorksWithBaseNothing_Generic([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
            [ValueSource(nameof(Attributes))] string baseAttribute, [Values("", nameof(Attribute))] string suffix, [Values(true, false)] bool isIface)
    {
        Assume.That(baseAttribute, Is.Not.EqualTo(attribute));

        var test = $$"""
        public {{(isIface ? "interface" : "class")}} ITestType {}
        [[|{{prefix}}{{attribute}}{{suffix}}<ITestType>|]]
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
    public async Task Test_WorksWithTwoBases_GenericVersion([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
            [ValueSource(nameof(Attributes))] string baseAttribute, [Values("", nameof(Attribute))] string suffix, [Values(true, false)] bool isIface)
    {
        Assume.That(baseAttribute, Is.Not.EqualTo(attribute));

        var test = $$"""
        [{{prefix}}{{baseAttribute}}Base{{suffix}}] public {{(isIface ? "interface" : "class")}} ITestType {}
        [{{prefix}}{{attribute}}Base{{suffix}}] public interface ITestType2 {}
        [[|{{prefix}}{{attribute}}{{suffix}}<ITestType, ITestType2>|]]
        public class TestType : ITestType, ITestType2
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
    public async Task Test_WarnsWhenTwo_ParamVersion([ValueSource(nameof(Prefixes))] string prefix,
                [ValueSource(nameof(Attributes))] string attribute1, [ValueSource(nameof(Attributes))] string attribute2,
                [Values("", nameof(Attribute))] string suffix, [Values(true, false)] bool isIface)
    {
        Assume.That(attribute1, Is.Not.EqualTo(attribute2));

        var test = $$"""
        [{{prefix}}{{attribute1}}Base{{suffix}}] public {{(isIface ? "interface" : "class")}} ITestType {}
        [{{prefix}}{{attribute1}}{{suffix}}(typeof(ITestType))]
        [[|{{prefix}}{{attribute2}}{{suffix}}(typeof(ITestType))|]]
        public class TestType : ITestType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenTwoAndMatching_ParamVersion([ValueSource(nameof(Prefixes))] string prefix,
            [ValueSource(nameof(Attributes))] string attribute1, [ValueSource(nameof(Attributes))] string attribute2,
            [Values("", nameof(Attribute))] string suffix, [Values(true, false)] bool isIface)
    {
        Assume.That(attribute1, Is.Not.EqualTo(attribute2));

        var test = $$"""
        [{{prefix}}{{attribute1}}Base{{suffix}}] [{{prefix}}{{attribute2}}Base{{suffix}}] public {{(isIface ? "interface" : "class")}} ITestType {}
        [{{prefix}}{{attribute1}}{{suffix}}(typeof(ITestType))]
        [{{prefix}}{{attribute2}}{{suffix}}(typeof(ITestType))]
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
    public async Task Test_WarnsWhenTwo_GenericVersion([ValueSource(nameof(Prefixes))] string prefix,
            [ValueSource(nameof(Attributes))] string attribute1, [ValueSource(nameof(Attributes))] string attribute2,
            [Values("", nameof(Attribute))] string suffix, [Values(true, false)] bool isIface)
    {
        Assume.That(attribute1, Is.Not.EqualTo(attribute2));

        var test = $$"""
        [{{prefix}}{{attribute1}}Base{{suffix}}] public {{(isIface ? "interface" : "class")}} ITestType {}
        [{{prefix}}{{attribute1}}{{suffix}}<ITestType>]
        [[|{{prefix}}{{attribute2}}{{suffix}}<ITestType>|]]
        public class TestType : ITestType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_WarnsWhenTwoAndMatching_GenericVersion([ValueSource(nameof(Prefixes))] string prefix,
        [ValueSource(nameof(Attributes))] string attribute1, [ValueSource(nameof(Attributes))] string attribute2,
        [Values("", nameof(Attribute))] string suffix, [Values(true, false)] bool isIface)
    {
        Assume.That(attribute1, Is.Not.EqualTo(attribute2));

        var test = $$"""
        [{{prefix}}{{attribute1}}Base{{suffix}}] [{{prefix}}{{attribute2}}Base{{suffix}}] public {{(isIface ? "interface" : "class")}} ITestType {}
        [{{prefix}}{{attribute1}}{{suffix}}<ITestType>]
        [{{prefix}}{{attribute2}}{{suffix}}<ITestType>]
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
