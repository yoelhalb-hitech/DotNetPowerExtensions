using SequelPay.DotNetPowerExtensions.Analyzers.DependencyManagement.CodeFixProviders;
using SequelPay.DotNetPowerExtensions.Analyzers.DependencyManagement.DependencyAttribute.Analyzers;
using Microsoft.CodeAnalysis.Testing;
using SequelPay.DotNetPowerExtensions;

namespace DotNetPowerExtensions.Analyzers.Tests.DependencyManagement;

internal sealed class MustInitializeShouldAddMightRequire_Tests
                    : CodeFixVerifierBase<MustInitializeShouldAddMightRequire, MustInitializeShouldAddMightRequireCodeFixProvider>
{
    public static string[] Attributes => new string[] { nameof(SingletonAttribute), nameof(ScopedAttribute), nameof(TransientAttribute), nameof(LocalAttribute) }
                                                    .Select(n => n.Replace(nameof(Attribute), "", StringComparison.Ordinal))
                                                    .ToArray();

    [Test]
    public async Task Test_HasCorrectMessage([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                                   [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        public interface IDeclareType {}
        [{{prefix}}{{attribute}}{{suffix}}<IDeclareType>]
        public class DeclareType : IDeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
        }
        """;

        await VerifyAnalyzerAsync(test, new DiagnosticResult("DNPE0215", DiagnosticSeverity.Warning)
                                            .WithSpan(3, 2, 3, 16 + prefix.Length + attribute.Length + suffix.Length)
                                            .WithMessage("Add MightRequire on `IDeclareType` for `TestProp,TestField`"))
            .ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                                    [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        using System;
        using System.Collections.Generic;
        /::/
        public interface IDeclareType {}
        [[|{{prefix}}{{attribute}}{{suffix}}<IDeclareType>|]]
        public class DeclareType : IDeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public AppDomain TestGeneralName { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public List<(string, int)> TestField;
        }
        """;

        var fixCode = $$"""

        [MightRequire("TestProp", typeof(string))]
        [MightRequire("TestGeneralName", typeof(AppDomain))]
        [MightRequire("TestField", typeof(List<(string, int)>))]
        """;

        await VerifyCodeFixAsync(test, fixCode).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenHasMightRequire([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                                                [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [{{prefix}}MightRequire{{suffix}}<string>("TestProp")]
        [{{prefix}}MightRequire{{suffix}}<string>("TestField")]
        [{{prefix}}MightRequire{{suffix}}<string>("TestProp1")]
        [{{prefix}}MightRequire{{suffix}}<string>("TestField1")]
        public class DeclareTypeBase
        {
        }

        [{{prefix}}{{attribute}}{{suffix}}<DeclareTypeBase>]
        public class DeclareTypeSub : DeclareTypeBase
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp1 { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField1;
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenHasMightRequireWithParenthesis([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                                            [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [{{prefix}}MightRequire{{suffix}}<string>((("TestProp")))]
        [{{prefix}}MightRequire{{suffix}}<string>((("TestField")))]
        [{{prefix}}MightRequire{{suffix}}<string>((("TestProp1")))]
        [{{prefix}}MightRequire{{suffix}}<string>((("TestField1")))]
        public class DeclareTypeBase
        {
        }

        [{{prefix}}{{attribute}}{{suffix}}<DeclareTypeBase>]
        public class DeclareTypeSub : DeclareTypeBase
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp1 { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField1;
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenHasMightRequireWithNameOf([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                                            [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [{{prefix}}MightRequire{{suffix}}<string>(nameof(DeclareTypeSub.TestProp))]
        [{{prefix}}MightRequire{{suffix}}<string>(nameof(DeclareTypeSub.TestField))]
        [{{prefix}}MightRequire{{suffix}}<string>(nameof(DeclareTypeSub.TestProp1))]
        [{{prefix}}MightRequire{{suffix}}<string>(nameof(DeclareTypeSub.TestField1))]
        public class DeclareTypeBase
        {
        }

        [{{prefix}}{{attribute}}{{suffix}}<DeclareTypeBase>]
        public class DeclareTypeSub : DeclareTypeBase
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp1 { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField1;
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenHasMightRequireWithNameOfAndParenthesis([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                                        [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [{{prefix}}MightRequire{{suffix}}<string>(((nameof(DeclareTypeSub.TestProp))))]
        [{{prefix}}MightRequire{{suffix}}<string>(((nameof(DeclareTypeSub.TestField))))]
        [{{prefix}}MightRequire{{suffix}}<string>(((nameof(DeclareTypeSub.TestProp1))))]
        [{{prefix}}MightRequire{{suffix}}<string>(((nameof(DeclareTypeSub.TestField1))))]
        public class DeclareTypeBase
        {
        }

        [{{prefix}}{{attribute}}{{suffix}}<DeclareTypeBase>]
        public class DeclareTypeSub : DeclareTypeBase
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp1 { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField1;
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenHasMightRequireWithConstant([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                                        [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [{{prefix}}MightRequire{{suffix}}<string>(DeclareTypeBase.A)]
        [{{prefix}}MightRequire{{suffix}}<string>(DeclareTypeBase.B)]
        [{{prefix}}MightRequire{{suffix}}<string>(DeclareTypeBase.C)]
        [{{prefix}}MightRequire{{suffix}}<string>(DeclareTypeBase.D)]
        public class DeclareTypeBase
        {
            public const string A = "TestProp";
            public const string B = "TestField";
            public const string C = "TestProp1";
            public const string D = "TestField1";
        }

        [{{prefix}}{{attribute}}{{suffix}}<DeclareTypeBase>]
        public class DeclareTypeSub : DeclareTypeBase
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp1 { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField1;
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenHasMightRequireWithConstantAndNameof([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                                    [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [{{prefix}}MightRequire{{suffix}}<string>(DeclareTypeBase.A)]
        [{{prefix}}MightRequire{{suffix}}<string>(DeclareTypeBase.B)]
        [{{prefix}}MightRequire{{suffix}}<string>(DeclareTypeBase.C)]
        [{{prefix}}MightRequire{{suffix}}<string>(DeclareTypeBase.D)]
        public class DeclareTypeBase
        {
            public const string A = nameof(DeclareTypeSub.TestProp);
            public const string B = nameof(DeclareTypeSub.TestField);
            public const string C = nameof(DeclareTypeSub.TestProp1);
            public const string D = nameof(DeclareTypeSub.TestField1);
        }

        [{{prefix}}{{attribute}}{{suffix}}<DeclareTypeBase>]
        public class DeclareTypeSub : DeclareTypeBase
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp1 { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField1;
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenHasMightRequireWithConstantAndNameofAndParenthesis([ValueSource(nameof(Prefixes))] string prefix,
                                            [ValueSource(nameof(Attributes))] string attribute, [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [{{prefix}}MightRequire{{suffix}}<string>(((DeclareTypeBase.A)))]
        [{{prefix}}MightRequire{{suffix}}<string>(((DeclareTypeBase.B)))]
        [{{prefix}}MightRequire{{suffix}}<string>(((DeclareTypeBase.C)))]
        [{{prefix}}MightRequire{{suffix}}<string>(((DeclareTypeBase.D)))]
        public class DeclareTypeBase
        {
            public const string A = ((nameof(DeclareTypeSub.TestProp)));
            public const string B = ((nameof(DeclareTypeSub.TestField)));
            public const string C = ((nameof(DeclareTypeSub.TestProp1)));
            public const string D = ((nameof(DeclareTypeSub.TestField1)));
        }

        [{{prefix}}{{attribute}}{{suffix}}<DeclareTypeBase>]
        public class DeclareTypeSub : DeclareTypeBase
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp1 { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField1;
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenHasMightRequireWithNonGeneric([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                                [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [{{prefix}}MightRequire{{suffix}}("TestProp",typeof(string))]
        [{{prefix}}MightRequire{{suffix}}("TestField",typeof(string))]
        [{{prefix}}MightRequire{{suffix}}("TestProp1",typeof(string))]
        [{{prefix}}MightRequire{{suffix}}("TestField1",typeof(string))]
        public class DeclareTypeBase
        {
        }

        [{{prefix}}{{attribute}}{{suffix}}<DeclareTypeBase>]
        public class DeclareTypeSub : DeclareTypeBase
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp1 { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField1;
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenHasMightRequireWithNonGenericAndParenthesis([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                            [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        [{{prefix}}MightRequire{{suffix}}((("TestProp")),((typeof(string))))]
        [{{prefix}}MightRequire{{suffix}}((("TestField")),((typeof(string))))]
        [{{prefix}}MightRequire{{suffix}}((("TestProp1")),((typeof(string))))]
        [{{prefix}}MightRequire{{suffix}}((("TestField1")),((typeof(string))))]
        public class DeclareTypeBase
        {
        }

        [{{prefix}}{{attribute}}{{suffix}}<DeclareTypeBase>]
        public class DeclareTypeSub : DeclareTypeBase
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp1 { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField1;
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_ForInitialized([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                                            [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        public interface IDeclareType
        {
        }
        public class DeclareTypeBase : IDeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public virtual string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public virtual string TestProp1 { get; set; }
        }
        [{{prefix}}{{attribute}}{{suffix}}<IDeclareType>]
        public class DeclareTypeSub:  DeclareTypeBase, IDeclareType
        {
            [{{prefix}}Initialized{{suffix}}] public override string TestProp { get; set; }
            [{{prefix}}Initialized{{suffix}}] public override string TestProp1 { get; set; }
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenNoMustInitialize([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                                            [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        public interface IDeclareType
        {
        }
        [{{prefix}}{{attribute}}{{suffix}}<IDeclareType>]
        public class DeclareType : IDeclareType
        {
            public string TestProp { get; set; }
            public string TestField;
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenOtherMustInitialize([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                                            [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        public interface IDeclareType
        {
        }
        public class MustInitializeAttribute : System.Attribute {}
        [{{prefix}}{{attribute}}{{suffix}}<IDeclareType>]
        public class DeclareType : IDeclareType
        {
            [MustInitialize{{suffix}}] public string TestProp { get; set; }
            [MustInitialize{{suffix}}] public string TestField;
        }

        class Program { void Main() => (null as ILocalFactory<DeclareType>).Create(); }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenOtherDependencyAttribute([ValueSource(nameof(Prefixes))] string prefix,
                                                     [ValueSource(nameof(Attributes))] string attribute, [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        public class {{attribute}}Attribute<T> : System.Attribute {}
        public interface IDeclareType
        {
        }
        [{{attribute}}{{suffix}}<IDeclareType>]
        public class DeclareType : IDeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_WithSubclass([ValueSource(nameof(Prefixes))] string prefix,
                                                        [ValueSource(nameof(Attributes))] string attribute, [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        public interface IDeclareType
        {
        }
        public class DeclareType : IDeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
        }

        [[|{{prefix}}{{attribute}}{{suffix}}<IDeclareType>|]]
        public class Subclass : DeclareType, IDeclareType {}
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Subclass_HasCorrectMessage_OnlyOnceForEach([ValueSource(nameof(Prefixes))] string prefix,
                                                                [ValueSource(nameof(Attributes))] string attribute, [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        public interface IDeclareType
        {
        }
        public class DeclareType : IDeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public virtual string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
        }
        [{{prefix}}{{attribute}}{{suffix}}<IDeclareType>]
        public class Subclass : DeclareType, IDeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public override string TestProp { get; set; }
        }

        class Program { void Main() => (null as ILocalFactory<Subclass>).Create(); }
        """;

        await VerifyAnalyzerAsync(test, new DiagnosticResult("DNPE0215", DiagnosticSeverity.Warning)
                                            .WithSpan(10, 2, 10, 16 + prefix.Length + attribute.Length + suffix.Length)
                                            .WithMessage("Add MightRequire on `IDeclareType` for `TestProp,TestField`"))
            .ConfigureAwait(false);
    }

    // Basically this is to test that if the MustInitalize is on both the base and the sub it doesn't need to rrquire twice...
    [Test]
    public async Task Test_Subclass_NoWarningIfDoneOnce([ValueSource(nameof(Prefixes))] string prefix,
                                                            [ValueSource(nameof(Attributes))] string attribute, [Values("", nameof(Attribute))] string suffix)
    {

        var test = $$"""
        [{{prefix}}MightRequire{{suffix}}<string>("TestProp")]
        public interface IDeclareType
        {
        }
        public class DeclareType : IDeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public virtual string TestProp { get; set; }
        }
        [{{prefix}}{{attribute}}{{suffix}}<IDeclareType>]
        public class Subclass : DeclareType, IDeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public override string TestProp { get; set; }
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }
}