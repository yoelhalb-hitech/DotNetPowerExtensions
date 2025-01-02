
namespace DotNetPowerExtensions.Analyzers.Tests.DependencyManagement.DependencyAnalyzer;

internal class DependencyRequiredWhenBase_Tests : AnalyzerVerifierBase<DependencyRequiredWhenBase>
{
     public static string[] Attributes => new string[] { nameof(SingletonAttribute), nameof(ScopedAttribute), nameof(TransientAttribute), nameof(LocalAttribute) }
                                                        .Select(n => n.Replace(nameof(Attribute), "", StringComparison.Ordinal))
                                                        .ToArray();

    [Test]
    public async Task Test_Works([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string baseAttribute,
                                                            [ValueSource(nameof(Suffixes))] string suffix, [Values(true, false)] bool isIface)
    {
        var test = $$"""
        [{{prefix}}{{baseAttribute}}Base{{suffix}}] public {{(isIface ? "interface" : "class")}} ITestType {}
        [|public class TestType : ITestType
        {
        }|]
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenAbstract([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string baseAttribute,
                                                        [ValueSource(nameof(Suffixes))] string suffix, [Values(true, false)] bool isIface)
    {
        var test = $$"""
        [{{prefix}}{{baseAttribute}}Base{{suffix}}] public {{(isIface ? "interface" : "class")}} ITestType {}
        public abstract class TestType : ITestType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_MessageIsCorrect()
    {
        var test = $$"""
        [TransientBase()] public interface TestIFace {}
        public class TestType : TestIFace
        {
        }
        """;

        await VerifyAnalyzerAsync(test, new DiagnosticResult("DNPE0211", DiagnosticSeverity.Warning)
                                                .WithSpan(3, 1, 5, 2).WithMessage("`TransientAttribute` for TestIFace is required, unless marked `abstract`")).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_WorksWhenClassHasAttribute([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                [ValueSource(nameof(Attributes))] string baseAttribute, [ValueSource(nameof(Suffixes))] string suffix, [Values(true, false)] bool isIface)
    {
        var test = $$"""
        [{{prefix}}{{baseAttribute}}Base{{suffix}}] public {{(isIface ? "interface" : "class")}} ITestType {}
        [|[{{prefix}}{{attribute}}{{suffix}}]
        public class TestType : ITestType
        {
        }|]
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_WarnsWhenNotBaseType([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
            [Values("", nameof(Attribute))] string suffix, [Values(true, false)] bool isIface)
    {
        var test = $$"""
        [{{prefix}}{{attribute}}Base{{suffix}}] public {{(isIface ? "interface" : "class")}} ITestType {}
        [|[{{prefix}}{{attribute}}{{suffix}}]
        public class TestType : ITestType
        {
        }|]
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenParam([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
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
    public async Task Test_WarnsWhenDifferent_ParamVersion([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
    [ValueSource(nameof(Attributes))] string baseAttribute, [Values("", nameof(Attribute))] string suffix, [Values(true, false)] bool isIface)
    {
        Assume.That(baseAttribute, Is.Not.EqualTo(attribute));

        var test = $$"""
        [{{prefix}}{{baseAttribute}}Base{{suffix}}] public {{(isIface ? "interface" : "class")}} ITestType {}
        [|[{{prefix}}{{attribute}}{{suffix}}(typeof(ITestType))]
        public class TestType : ITestType
        {
        }|]
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenGeneric([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                        [Values("", nameof(Attribute))] string suffix,
                                        [Values("", "()")] string paren, [Values(true, false)] bool isIface)
    {
        var test = $$"""
        [{{prefix}}{{attribute}}Base{{suffix}}{{paren}}] public {{(isIface ? "interface" : "class")}} ITestType {}
        [{{prefix}}{{attribute}}{{suffix}}<ITestType>{{paren}}]
        public class TestType : ITestType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_WarnsWhenDifferent_GenericVersion([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                    [ValueSource(nameof(Attributes))] string baseAttribute, [Values("", nameof(Attribute))] string suffix,
                                    [Values("", "()")] string paren, [Values(true, false)] bool isIface)
    {
        Assume.That(baseAttribute, Is.Not.EqualTo(attribute));

        var test = $$"""
        [{{prefix}}{{baseAttribute}}Base{{suffix}}{{paren}}] public {{(isIface ? "interface" : "class")}} ITestType {}
        [|[{{prefix}}{{attribute}}{{suffix}}<ITestType>{{paren}}]
        public class TestType : ITestType
        {
        }|]
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenBaseHasTwoMatching_ParamVersion([ValueSource(nameof(Prefixes))] string prefix,
        [ValueSource(nameof(Attributes))] string attribute1,
    [ValueSource(nameof(Attributes))] string attribute2, [Values("", nameof(Attribute))] string suffix, [Values(true, false)] bool isIface)
    {
        Assume.That(attribute1, Is.Not.EqualTo(attribute2));

        var test = $$"""
        [{{prefix}}{{attribute1}}Base{{suffix}}]
        [{{prefix}}{{attribute2}}Base{{suffix}}]
        public {{(isIface ? "interface" : "class")}} ITestType {}
        [{{prefix}}{{attribute1}}{{suffix}}(typeof(ITestType))]
        [{{prefix}}{{attribute2}}{{suffix}}(typeof(ITestType))]
        public class TestType : ITestType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_WarnsWhenBaseHasTwo_ParamVersion([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
        [ValueSource(nameof(Attributes))] string baseAttribute, [Values("", nameof(Attribute))] string suffix, [Values(true, false)] bool isIface)
    {
        Assume.That(baseAttribute, Is.Not.EqualTo(attribute));

        var test = $$"""
        [{{prefix}}{{baseAttribute}}Base{{suffix}}]
        [{{prefix}}{{attribute}}Base{{suffix}}]
        public {{(isIface ? "interface" : "class")}} ITestType {}
        [|[{{prefix}}{{attribute}}{{suffix}}(typeof(ITestType))]
        public class TestType : ITestType
        {
        }|]
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenBaseHasTwoMatching_GenericVersion([ValueSource(nameof(Prefixes))] string prefix,
                [ValueSource(nameof(Attributes))] string attribute1,
                [ValueSource(nameof(Attributes))] string attribute2, [Values("", nameof(Attribute))] string suffix,
                                    [Values("", "()")] string paren, [Values(true, false)] bool isIface)
    {
        Assume.That(attribute1, Is.Not.EqualTo(attribute2));

        var test = $$"""
        [{{prefix}}{{attribute1}}Base{{suffix}}{{paren}}]
        [{{prefix}}{{attribute2}}Base{{suffix}}{{paren}}]
        public {{(isIface ? "interface" : "class")}} ITestType {}
        [{{prefix}}{{attribute1}}{{suffix}}<ITestType>{{paren}}]
        [{{prefix}}{{attribute2}}{{suffix}}<ITestType>{{paren}}]
        public class TestType : ITestType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_WarnsWhenBaseHasTwo_GenericVersion([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
    [ValueSource(nameof(Attributes))] string baseAttribute, [Values("", nameof(Attribute))] string suffix,
                                        [Values("", "()")] string paren, [Values(true, false)] bool isIface)
    {
        Assume.That(baseAttribute, Is.Not.EqualTo(attribute));

        var test = $$"""
        [{{prefix}}{{baseAttribute}}Base{{suffix}}{{paren}}]
        [{{prefix}}{{attribute}}Base{{suffix}}{{paren}}]
        public {{(isIface ? "interface" : "class")}} ITestType {}
        [|[{{prefix}}{{attribute}}{{suffix}}<ITestType>{{paren}}]
        public class TestType : ITestType
        {
        }|]
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_WorksWhenClassHasOtherParam([ValueSource(nameof(Prefixes))] string prefix,
             [ValueSource(nameof(Attributes))] string attribute,
             [ValueSource(nameof(Attributes))] string attribute2,
             [Values("", nameof(Attribute))] string suffix, [Values(true, false)] bool isIface)
    {
        var test = $$"""
        [{{prefix}}{{attribute}}Base{{suffix}}] public {{(isIface ? "class" : "interface")}} ITestType {}
        [{{prefix}}{{attribute2}}Base{{suffix}}] public interface ITestType2 {}
        [|[{{prefix}}{{attribute}}{{suffix}}(typeof(ITestType))]
        public class TestType : ITestType, ITestType2
        {
        }|]
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_WorksWhenClassHasOtherGeneric([ValueSource(nameof(Prefixes))] string prefix,
        [ValueSource(nameof(Attributes))] string attribute,
        [ValueSource(nameof(Attributes))] string attribute2,
        [Values("", nameof(Attribute))] string suffix, [Values(true, false)] bool isIface)
    {
        var test = $$"""
        [{{prefix}}{{attribute}}Base{{suffix}}] public {{(isIface ? "class" : "interface")}} ITestType {}
        [{{prefix}}{{attribute2}}Base{{suffix}}] public interface ITestType2 {}
        [|[{{prefix}}{{attribute}}{{suffix}}<ITestType>]
        public class TestType : ITestType, ITestType2
        {
        }|]
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }


    [Test]
    public async Task Test_DoesNotWarnWhenOtherAttribute([ValueSource(nameof(Attributes))] string attribute,
                                            [ValueSource(nameof(Suffixes))] string suffix, [Values(true, false)] bool isIface)
    {
        var test = $$"""
        public class {{attribute}}BaseAttribute : System.Attribute { public System.Type Use { get; set; } }
        [{{attribute}}Base{{suffix}}] public {{(isIface ? "class" : "interface")}} ITestType {}
        public abstract class TestType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }
}
