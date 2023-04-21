using DotNetPowerExtensions.Analyzers.MustInitialize.MustInitializeAttribute.Analyzers;
using DotNetPowerExtensions.Analyzers.MustInitialize.MustInitializeAttribute.CodeFixProviders;

namespace DotNetPowerExtensions.Analyzers.Tests.MustInitialize.MustInitializeAttribute;

internal sealed class CannotUseBaseImplementation_Tests
                    : CodeFixVerifierBase<CannotUseBaseImplementationForMustInitialize, CannotUseBaseImplementationForMustInitializeCodeFixProvider>
{
    [Test]
    public async Task Test_DoesNotWarn_WhenNoMustInitialize()
    {
        var test = $$"""
        public interface IDeclareType
        {
            string TestProp { get; set; }
        }
        public class DeclareTypeBase
        {
            public string TestProp { get; set; }
        }
        public class DeclareType : DeclareTypeBase, IDeclareType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenOtherMustInitialize([ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class MustInitializeAttribute : System.Attribute {}
        public interface IDeclareType
        {
            [MustInitialize{{suffix}}] string TestProp { get; set; }
        }
        public class DeclareTypeBase
        {
            public string TestProp { get; set; }
        }
        public class DeclareType : DeclareTypeBase, IDeclareType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public interface IDeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] string TestPropProtected { set; }
        }
        public class DeclareTypeBase
        {
            public string TestProp { get; set; }
            public string TestPropProtected { protected get; set; }
        }
        public class [|[|DeclareType|]|] : DeclareTypeBase, IDeclareType
        {
        /::/
        }
        """;

        var codeFix = $$"""
            [MustInitialize]
            public new string TestProp { get => base.TestProp; set => base.TestProp = value; }
            [MustInitialize]
            public new string TestPropProtected { protected get => base.TestPropProtected; set => base.TestPropProtected = value; }

        """;

        await VerifyCodeFixAsync(test, codeFix).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenBaseMustInitialize([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public interface IDeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] string TestPropProtected { set; }
        }
        public class DeclareTypeBase
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestPropProtected { protected get; set; }
        }
        public class DeclareType : DeclareTypeBase, IDeclareType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenBaseInitialized([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public interface IDeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] string TestPropProtected { set; }
        }
        public class DeclareTypeBaseBase
        {
            [{{prefix}}MustInitialize{{suffix}}] public virtual string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public virtual string TestPropProtected { protected get; set; }
        }
        public class DeclareTypeBase : DeclareTypeBaseBase
        {
            [{{prefix}}Initialized{{suffix}}] public override string TestProp { get; set; }
            [{{prefix}}Initialized{{suffix}}] public override string TestPropProtected { protected get; set; }
        }
        public class DeclareType : DeclareTypeBase, IDeclareType
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_WithVirtual([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public interface IDeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] string TestProp { get; set; }
        }
        public class DeclareTypeBase
        {
            public virtual string TestProp { get; set; }
        }
        public class [|DeclareType|] : DeclareTypeBase, IDeclareType
        {
        /::/
        }
        """;

        var codeFix = $$"""
            [MustInitialize]
            public override string TestProp { get => base.TestProp; set => base.TestProp = value; }

        """;

        await VerifyCodeFixAsync(test, codeFix).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_WithOverride([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public interface IDeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] string TestProp { get; set; }
        }
        public class DeclareTypeBase
        {
            public virtual string TestProp { get; set; }
        }
        public class DeclareTypeBase2: DeclareTypeBase
        {
            public override string TestProp { get; set; }
        }
        public class [|DeclareType|] : DeclareTypeBase2, IDeclareType
        {
        /::/
        }
        """;

        var codeFix = $$"""
            [MustInitialize]
            public override string TestProp { get => base.TestProp; set => base.TestProp = value; }

        """;

        await VerifyCodeFixAsync(test, codeFix).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_WithAbstract([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public interface IDeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] string TestProp { get; set; }
        }
        public abstract class DeclareTypeBase
        {
            public abstract string TestProp { get; set; }
        }
        public abstract class [|DeclareType|] : DeclareTypeBase, IDeclareType
        {
        /::/
        }
        """;

        var codeFix = $$"""
            [MustInitialize]
            public override string TestProp { get; set; }

        """;

        await VerifyCodeFixAsync(test, codeFix).ConfigureAwait(false);
    }


    [Test]
    public async Task Test_NoDiagnostic_ForInterfaceChain([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public interface IDeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] string TestProp { get; set; }
        }
        public interface IDeclareType2
        {
            string TestProp { get; set; }
        }
        public interface IDeclareTypeSub : IDeclareType, IDeclareType2
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_WorksProperty_WithSubclassedInterface([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public interface IDeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] string TestProp { get; set; }
        }
        public interface IDeclareTypeSub : IDeclareType {}
        public class DeclareTypeBase
        {
            public string TestProp { get; set; }
        }
        public class [|DeclareType|] : DeclareTypeBase, IDeclareTypeSub
        {
        /::/
        }
        """;

        var fixCode = $$"""
            [MustInitialize]
            public new string TestProp { get => base.TestProp; set => base.TestProp = value; }

        """;

        await VerifyCodeFixAsync(test, fixCode).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_WorksProperty_WithInterface_AndOtherInterfaces([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public interface IDeclareType
        {
            string OtherMethod();
            string TestingOtherProp { get; }
            [{{prefix}}MustInitialize{{suffix}}] string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] string TestPropWithAttributesSingleLine { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] string TestPropWithAttributesMultiLine { get; set; }
            string OtherMethod2();
        }
        public interface IOther
        {
            string TestOtherProp { get; set; }
        }
        public class TestAttribute: System.Attribute {}
        public class DeclareTypeBase: IOther
        {
            public string TestProp { get; set; }
            [Test] public string TestPropWithAttributesSingleLine { get; set; }
            [Test]
            public virtual string TestPropWithAttributesMultiLine { get; set; }
            public string TestOtherProp { get; set; }
            public string TestOtherField;
        }
        public class [|[|[|DeclareType|]|]|] : DeclareTypeBase, IDeclareType, IOther
        {
            public string OtherMethod() => "Test";
            public string TestingOtherProp => "Test";
            public string OtherMethod2() => "Test";
        /::/
        }
        """;

        var fixCode = $$"""

            [MustInitialize]
            public new string TestProp { get => base.TestProp; set => base.TestProp = value; }
            [Test]
            [MustInitialize]
            public new string TestPropWithAttributesSingleLine { get => base.TestPropWithAttributesSingleLine; set => base.TestPropWithAttributesSingleLine = value; }
            [Test]
            [MustInitialize]
            public override string TestPropWithAttributesMultiLine { get => base.TestPropWithAttributesMultiLine; set => base.TestPropWithAttributesMultiLine = value; }

        """;

        await VerifyCodeFixAsync(test, fixCode).ConfigureAwait(false);
    }
}

