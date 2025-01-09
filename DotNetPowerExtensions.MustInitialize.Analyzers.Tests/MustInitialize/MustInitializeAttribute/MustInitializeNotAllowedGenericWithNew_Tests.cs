using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using SequelPay.DotNetPowerExtensions.Analyzers.MustInitialize.MustInitializeAttribute.CodeFixProviders;

namespace DotNetPowerExtensions.Analyzers.Tests.MustInitialize.MustInitializeAttribute;

internal sealed class MustInitializeNotAllowedGenericWithNew_Tests
                    : AnalyzerVerifierBase<MustInitializeNotAllowedGenericWithNew>
{
    [Test]
    public async Task Test_HasCorrectMessage([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class GenericType<T> where T : new () { }

        public class DeclareType : GenericType<DeclareType>
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
        }
        """;

        await VerifyAnalyzerAsync(test, new DiagnosticResult("DNPE0219", DiagnosticSeverity.Warning)
                                            .WithSpan(4, 28, 4, 52)
                                            .WithMessage("Cannot use `DeclareType` as type argument for GenericType because GenericType requires `new()` but DeclareType has `MustInitialize`"))
            .ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenNoMustInitialize()
    {
        var test = $$"""
        public class GenericType<T> where T : new () { }

        public class DeclareType : GenericType<DeclareType>
        {
            public string TestProp { get; set; }
            public string TestField;
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenUsingInitializesAllRequired([ValueSource(nameof(Prefixes))] string prefix,
                                                                                            [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class GenericType<T> where T : new () { }

        public class DeclareType : GenericType<DeclareType>
        {
            [{{prefix}}InitializesAllRequired{{suffix}}] public DeclareType(){ TestProp = TestField = ""; }
            public DeclareType(int i){}
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Warns_WhenInitializesAllRequired_IsOnNonDefaultCtor([ValueSource(nameof(Prefixes))] string prefix,
                                                                                    [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class GenericType<T> where T : new () { }

        public class DeclareType : [|GenericType<DeclareType>|]
        {
            public DeclareType(){}
            [{{prefix}}InitializesAllRequired{{suffix}}] public DeclareType(int i){}
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenUsingInitializes_AndCoversAll([ValueSource(nameof(Prefixes))] string prefix,
                                                                                [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
            public class GenericType<T> where T : new () { }

            public class DeclareType : GenericType<DeclareType>
            {
                [{{prefix}}Initializes{{suffix}}("TestProp", "TestField")]  public DeclareType(){}
                [{{prefix}}Initializes{{suffix}}("TestProp")] public DeclareType(int i){ TestProp = TestField = ""; }
                [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
                [{{prefix}}MustInitialize{{suffix}}] public string TestField;
            }
            """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenUsingSetsRequiredMembersAttribute([ValueSource(nameof(Prefixes))] string prefix,
                                                                                        [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
            using System.Diagnostics.CodeAnalysis;
            namespace System.Diagnostics.CodeAnalysis { public class SetsRequiredMembersAttribute : System.Attribute {} }
            public class GenericType<T> where T : new () { }

            public class DeclareType : GenericType<DeclareType>
            {
                [SetsRequiredMembers{{suffix}}] public DeclareType(){ TestProp = TestField = ""; }
                public DeclareType(int i){}
                [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
                [{{prefix}}MustInitialize{{suffix}}] public string TestField;
            }
            """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenInitialized([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
            public interface IGenericType<T> where T : new () { }

            public class DeclareTypeBase
            {
                [{{prefix}}MustInitialize{{suffix}}] public virtual string TestProp { get; set; }
                [{{prefix}}MustInitialize{{suffix}}] public string TestField;
            }
            public class DeclareType : DeclareTypeBase, IGenericType<DeclareType>
            {
                [{{prefix}}Initialized{{suffix}}] public override string TestProp { get; set; }
                [{{prefix}}Initialized{{suffix}}] public new string TestField;
            }
            """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenOtherMustInitialize([ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
            public class MustInitializeAttribute : System.Attribute {}

            public class GenericType<T> where T : new () { }

            public class DeclareType : GenericType<DeclareType>
            {
                [MustInitialize{{suffix}}] public string TestProp { get; set; }
                [MustInitialize{{suffix}}] public string TestField;
            }
            """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Warns_WhenJustCode([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
            public class GenericType<T> where T : new () { }

            public class DeclareType
            {
                [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
                [{{prefix}}MustInitialize{{suffix}}] public string TestField;
            }

            class Program { void Main() => new [|GenericType<DeclareType>|](); }
            """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }
}
