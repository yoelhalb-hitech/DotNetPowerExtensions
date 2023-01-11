using DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;

namespace DotNetPowerExtensions.Analyzers.Tests.MustInitialize.MustInitializeAttribute;

internal sealed class DisallowHidingMustInitialize_Tests : AnalyzerVerifierBase<DisallowHidingMustInitialize>
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
            public new string TestProp { get; set; }
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
            [MustInitialize{{suffix}}] public new string TestProp { get; set; }
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenOverride([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class DeclareTypeBase
        {
            [{{prefix}}MustInitialize{{suffix}}] public virtual string TestProp { get; set; }
        }
        public class DeclareTypeSub : DeclareTypeBase
        {
            public override string TestProp { get; set; }
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Warns([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix,
             [Values(true, false)] bool baseVirtual, [Values(true, false)] bool useNew, [Values(true, false)] bool newAbstract,
             [Values(true, false)] bool mustInitializeOnNew)
    {
        var test = $$"""
        public class DeclareTypeBase
        {
            [{{prefix}}MustInitialize{{suffix}}] public {{(baseVirtual ? "virtual" : "")}} string TestProp { get; set; }
        }
        public {{(newAbstract ? "abstract" : "")}} class DeclareTypeSub : DeclareTypeBase
        {
            [|{{(mustInitializeOnNew ? $"[{prefix}MustInitialize{suffix}] " : "")}}public {{(newAbstract ? "abstract" : "")}} {{(useNew ? "new" : "")}} string TestProp { get; set; }|]
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Warns_WhenSubclass([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix,
           [Values(true, false)] bool baseVirtual,
           [Values(true, false)] bool subShouldHaveDecleration,
           [Values(true, false)] bool newAbstract, [Values(true, false)] bool useNew, [Values(true, false)] bool mustInitializeOnNew)
    {
        Assume.That(!subShouldHaveDecleration || baseVirtual); // For subShouldHaveDecleration we need virtual or we will have a compile error

        var test = $$"""
        public class DeclareTypeBase
        {
            [{{prefix}}MustInitialize{{suffix}}] public {{(baseVirtual ? "virtual" : "")}} string TestProp { get; set; }
        }
        public class DeclareTypeSub1 : DeclareTypeBase
        {
            {{(subShouldHaveDecleration ? $$"""[{{prefix}}MustInitialize{{suffix}}] public override string TestProp { get; set; } """ : "")}}
        }
        public {{(newAbstract ? "abstract" : "")}} class DeclareTypeSub : DeclareTypeSub1
        {
            [|{{(mustInitializeOnNew ? $"[{prefix}MustInitialize{suffix}] " : "")}}public {{(newAbstract ? "abstract" : "")}} {{(useNew ? "new" : "")}} string TestProp { get; set; }|]
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Warns_WhenSubclass_AndBaseAbstract([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix,
       [Values(true, false)] bool newAbstract, [Values(true, false)] bool useNew, [Values(true, false)] bool mustInitializeOnNew)
    {

        var test = $$"""
        public abstract class DeclareTypeBase
        {
            [{{prefix}}MustInitialize{{suffix}}] public abstract string TestProp { get; set; }
        }
        public class DeclareTypeSub1 : DeclareTypeBase
        {
            [{{prefix}}MustInitialize{{suffix}}] public override string TestProp { get; set; }
        }
        public {{(newAbstract ? "abstract" : "")}} class DeclareTypeSub : DeclareTypeSub1
        {
            [|{{(mustInitializeOnNew ? $"[{prefix}MustInitialize{suffix}] " : "")}}public {{(newAbstract ? "abstract" : "")}} {{(useNew ? "new" : "")}} string TestProp { get; set; }|]
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }
}
