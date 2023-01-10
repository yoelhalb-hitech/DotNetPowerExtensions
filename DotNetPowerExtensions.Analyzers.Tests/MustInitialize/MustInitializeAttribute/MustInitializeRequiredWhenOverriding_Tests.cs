using DotNetPowerExtensions.Analyzers.MustInitialize.MustInitializeAttribute.Analyzers;
using DotNetPowerExtensions.Analyzers.MustInitialize.MustInitializeAttribute.CodeFixProviders;

namespace DotNetPowerExtensions.Analyzers.Tests.MustInitialize.MustInitializeAttribute;

internal sealed class MustInitializeRequiredWhenOverriding_Tests
    : MustInitializeCodeFixVerifierBase<MustInitializeRequiredWhenOverriding, MustInitializeRequiredWhenOverridingCodeFixProvider, PropertyDeclarationSyntax>
{
    [Test]
    public async Task Test_DoesNotWarn_WhenNoMustInitialize()
    {
        var test = $$"""
        public class DeclareTypeBase
        {
            public virtual string TestProp { get; set; }
        }
        public class DeclareTypeSub : DeclareTypeBase
        {
            public override string TestProp { get; set; }
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenOtherMustInitialize([ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class MustInitializeAttribute : System.Attribute {}
        public class DeclareTypeBase
        {
            [MustInitialize{{suffix}}] public virtual string TestProp { get; set; }
        }
        public class DeclareTypeSub : DeclareTypeBase
        {
            public override string TestProp { get; set; }
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix,
        [Values(true, false)] bool baseAbstract, [Values(true, false)] bool basePropAbstract,
        [Values(true, false)] bool subAbstract, [Values(true, false)] bool subPropAbstract)
    {
        Assume.That(!basePropAbstract || baseAbstract); // Otherwise we have a compile error
        Assume.That(!subPropAbstract || subAbstract); // Otherwise we have a compile error

        var test = $$"""
        public {{(baseAbstract ? "abstract" : "")}} class DeclareTypeBase
        {
            [{{prefix}}MustInitialize{{suffix}}] public {{(basePropAbstract ? "abstract" : "virtual")}} string TestProp { get; set; }
        }
        public {{(subAbstract ? "abstract" : "")}} class DeclareTypeSub : DeclareTypeBase
        {
            /::/[|public {{(subPropAbstract ? "abstract" : "")}} override string TestProp { get; set; }|]
        }
        """;

        var codeFix = $"[MustInitialize]{Environment.NewLine}    ";

        await VerifyCodeFixAsync(test, codeFix).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenNotDeclared([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix,
    [Values(true, false)] bool baseAbstract, [Values(true, false)] bool basePropAbstract,
    [Values(true, false)] bool subAbstract)
    {
        Assume.That(!basePropAbstract || baseAbstract); // Otherwise we have a compile error        
        Assume.That(!basePropAbstract || subAbstract); // Otherwise we have a compile error        

        var test = $$"""
        public {{(baseAbstract ? "abstract" : "")}} class DeclareTypeBase
        {
            [{{prefix}}MustInitialize{{suffix}}] public {{(basePropAbstract ? "abstract" : "virtual")}} string TestProp { get; set; }
        }
        public {{(subAbstract ? "abstract" : "")}} class DeclareTypeSub : DeclareTypeBase
        {
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenNotOverride([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix,
        [Values(true, false)] bool subAbstract, [Values(true, false)] bool subPropAbstract, [Values(true, false)] bool useNew)
    {
        Assume.That(!subPropAbstract || subAbstract); // Otherwise we have a compile error        

        var test = $$"""
        public class DeclareTypeBase
        {
            [{{prefix}}MustInitialize{{suffix}}] public virtual string TestProp { get; set; }
        }
        public {{(subAbstract ? "abstract" : "")}} class DeclareTypeSub : DeclareTypeBase
        {
            public {{(subPropAbstract ? "abstract" : "")}} {{(useNew ? "new" : "")}} string TestProp { get; set; }
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_WithMultiple([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public class DeclareTypeBase
        {
            public string OtherMethod() => "Test";
            public string TestingOtherProp { get; }
            [{{prefix}}MustInitialize{{suffix}}] public virtual string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public virtual string TestPropWithAttributesSingleLine { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public virtual string TestPropWithAttributesMultiLine { get; set; }
            public virtual string OtherMethod2() => "T";
        }
        public interface IOther
        {
            string TestOtherProp { get; set; }            
        }
        public class TestAttribute: System.Attribute {}
        public class DeclareType : DeclareTypeBase, IOther 
        {
            public new string OtherMethod() => "Test";
            public new string TestingOtherProp => "Test";
        /::/[|public override string TestProp { get; set; }|]
            public override string OtherMethod2() => "Test";
            public string TestOtherProp { get; set; }
            public string TestOtherField;
            [|[Test]/::/ public override string TestPropWithAttributesSingleLine { get; set; }|]
            [|[Test]
            /::/public override string TestPropWithAttributesMultiLine { get; set; }|]
        }
        """;

        var fixCode = new string[]
        {
            Environment.NewLine + $"    [MustInitialize]{Environment.NewLine}    ",
            "[MustInitialize]",
            $"[MustInitialize]{Environment.NewLine}    ",
        };

        await VerifyCodeFixAsync(test, fixCode).ConfigureAwait(false);
    }
}
