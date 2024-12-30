using SequelPay.DotNetPowerExtensions.Analyzers.DependencyManagement.DependencyAttribute.Analyzers;
using Microsoft.CodeAnalysis.Testing;
using SequelPay.DotNetPowerExtensions;

namespace DotNetPowerExtensions.Analyzers.Tests.DependencyManagement;

internal sealed class MustInitializeConflictsWithMightRequire_Tests : AnalyzerVerifierBase<MustInitializeConflictsWithMightRequire>
{
    public static string[] Attributes => new string[] { nameof(SingletonAttribute), nameof(ScopedAttribute), nameof(TransientAttribute), nameof(LocalAttribute) }
                                                    .Select(n => n.Replace(nameof(Attribute), "", StringComparison.Ordinal))
                                                    .ToArray();

    [Test]
    public async Task Test_HasCorrectMessage([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                                    [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [{{prefix}}MightRequire{{suffix}}<int>("TestProp")]
        public interface IDeclareType {}
        [{{prefix}}{{attribute}}{{suffix}}<IDeclareType>]
        public class DeclareType : IDeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
        }
        """;

        await VerifyAnalyzerAsync(test, new DiagnosticResult("DNPE0216", DiagnosticSeverity.Warning)
                                            .WithSpan(4, 2, 4, 16 + prefix.Length + attribute.Length + suffix.Length)
                                            .WithMessage("The type of member `TestProp` with `MustInitialize` conflicts with `MightRequire` on `IDeclareType`"))
            .ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                                    [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [{{prefix}}MightRequire{{suffix}}<int>("TestProp")]
        public interface IDeclareType {}
        [[|{{prefix}}{{attribute}}{{suffix}}<IDeclareType>|]]
        public class DeclareType : IDeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_WithParenthesis([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                                [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [{{prefix}}MightRequire{{suffix}}<int>((("TestProp")))]
        public interface IDeclareType {}
        [[|{{prefix}}{{attribute}}{{suffix}}<IDeclareType>|]]
        public class DeclareType : IDeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_WorksWithNonGeneric([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                                [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [{{prefix}}MightRequire{{suffix}}("TestProp", typeof(int))]
        public interface IDeclareType {}
        [[|{{prefix}}{{attribute}}{{suffix}}<IDeclareType>|]]
        public class DeclareType : IDeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_WorksWithNonGenericAndParenthesis([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                            [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [{{prefix}}MightRequire{{suffix}}((("TestProp")), ((typeof(int))))]
        public interface IDeclareType {}
        [[|{{prefix}}{{attribute}}{{suffix}}<IDeclareType>|]]
        public class DeclareType : IDeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_WorksWithNameof([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                            [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [{{prefix}}MightRequire{{suffix}}<int>(nameof(DeclareType.TestProp))]
        public interface IDeclareType {}
        [[|{{prefix}}{{attribute}}{{suffix}}<IDeclareType>|]]
        public class DeclareType : IDeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_WorksWithNameofAndPArenthesis([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                        [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [{{prefix}}MightRequire{{suffix}}<int>(((nameof(DeclareType.TestProp))))]
        public interface IDeclareType {}
        [[|{{prefix}}{{attribute}}{{suffix}}<IDeclareType>|]]
        public class DeclareType : IDeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }


    [Test]
    public async Task Test_WorksWithConst([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                        [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [{{prefix}}MightRequire{{suffix}}<int>(DeclareType.Name)]
        public interface IDeclareType {}
        [[|{{prefix}}{{attribute}}{{suffix}}<IDeclareType>|]]
        public class DeclareType : IDeclareType
        {
            public const string Name = "TestProp";
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_WorksWithConstAndNameof([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                    [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [{{prefix}}MightRequire{{suffix}}<int>(DeclareType.Name)]
        public interface IDeclareType {}
        [[|{{prefix}}{{attribute}}<IDeclareType>|]]
        public class DeclareType : IDeclareType
        {
            public const string Name = nameof(DeclareType.TestProp);
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_WorksWithConstAndNameofAndParenthesis([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [{{prefix}}MightRequire{{suffix}}<int>(((DeclareType.Name)))]
        public interface IDeclareType {}
        [[|{{prefix}}{{attribute}}<IDeclareType>|]]
        public class DeclareType : IDeclareType
        {
            public const string Name = ((nameof(DeclareType.TestProp)));
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }


    [Test]
    public async Task Test_DoesNotWarnWhenSame([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                                        [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [{{prefix}}MightRequire{{suffix}}<string>("TestProp")]
        public interface IDeclareType {}

        [{{prefix}}{{attribute}}<IDeclareType>]
        public class DeclareType : IDeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenOtherMustInitialize([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                                        [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        public class MustInitializeAttribute : System.Attribute {}
        [{{prefix}}MightRequire{{suffix}}<string>("TestProp")]
        public interface IDeclareType {}

        [{{prefix}}{{attribute}}<IDeclareType>]
        public class DeclareType : IDeclareType
        {
            [MustInitialize{{suffix}}] public string TestProp { get; set; }
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenOtherDependencyAttribute([ValueSource(nameof(Prefixes))] string prefix,
                                                    [ValueSource(nameof(Attributes))] string attribute, [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        public class {{attribute}}Attribute : System.Attribute {}
        [{{prefix}}MightRequire{{suffix}}<string>("TestProp")]
        public interface IDeclareType {}

        [{{attribute}}{{suffix}}<IDeclareType>]
        public class DeclareType : IDeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenOtherMightRequire([ValueSource(nameof(Prefixes))] string prefix,
                                                [ValueSource(nameof(Attributes))] string attribute, [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        public class MightRequireAttribute : System.Attribute {}
        [MightRequire{{suffix}}<string>("TestProp")]
        public interface IDeclareType {}

        [{{prefix}}{{attribute}}{{suffix}}<IDeclareType>]
        public class DeclareType : IDeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_WithSubclass([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                                        [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [{{prefix}}MightRequire{{suffix}}<int>("TestProp")]
        [{{prefix}}MightRequire{{suffix}}<int>("TestField")]
        public interface IDeclareType {}
        public class DeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
        }

        [[|[|{{prefix}}{{attribute}}{{suffix}}<IDeclareType>|]|]]
        public class Subclass : DeclareType, IDeclareType {}
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Subclass_HasCorrectMessage_OnlyOnceForEach([ValueSource(nameof(Prefixes))] string prefix,
                                                 [ValueSource(nameof(Attributes))] string attribute, [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [{{prefix}}MightRequire{{suffix}}<int>("TestProp")]
        [{{prefix}}MightRequire{{suffix}}<int>("TestField")]
        public interface IDeclareType {}
        public class DeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public virtual string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
        }

        [{{prefix}}{{attribute}}{{suffix}}<IDeclareType>]
        public class Subclass : DeclareType, IDeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public override string TestProp { get; set; }
        }
        """;

        await VerifyAnalyzerAsync(test, new DiagnosticResult("DNPE0216", DiagnosticSeverity.Warning)
                                            .WithSpan(11, 2, 11, 16 + prefix.Length + attribute.Length + suffix.Length)
                                            .WithMessage("The type of member `TestField` with `MustInitialize` conflicts with `MightRequire` on `IDeclareType`")
                                        , new DiagnosticResult("DNPE0216", DiagnosticSeverity.Warning)
                                            .WithSpan(11, 2, 11, 16 + prefix.Length + attribute.Length + suffix.Length)
                                            .WithMessage("The type of member `TestProp` with `MustInitialize` conflicts with `MightRequire` on `IDeclareType`")).ConfigureAwait(false);
    }
}