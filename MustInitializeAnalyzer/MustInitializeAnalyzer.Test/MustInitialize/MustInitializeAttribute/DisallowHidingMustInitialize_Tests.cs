using DotNetPowerExtensionsAnalyzer.MustInitialize.Analyzers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetPowerExtensionsAnalyzer.Test.MustInitialize.MustInitializeAttribute;

internal class DisallowHidingMustInitialize_Tests : AnalyzerVerifierBase<DisallowHidingMustInitialize>
{
    public static string[] Suffixes = { "", "Attribute", "()", "Attribute()" };
    public static string[] Prefixes = {"", "DotNetPowerExtensions.MustInitialize.",
                                                                    "global::DotNetPowerExtensions.MustInitialize." };

    [Test]
    public async Task Test_DoesNotWarn_WhenNoMustInitialize()
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public class DeclareTypeBase
        {
            public virtual string TestProp { get; set; }
        }
        public class DeclareTypeSub : DeclareTypeBase
        {
            public new string TestProp { get; set; }
        }
        """;

        await VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenOtherMustInitialize([ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;
        
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

        await VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenOverride([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;
        
        public class DeclareTypeBase
        {
            [{{prefix}}MustInitialize{{suffix}}] public virtual string TestProp { get; set; }
        }
        public class DeclareTypeSub : DeclareTypeBase
        {
            public override string TestProp { get; set; }
        }
        """;

        await VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task Test_Warns([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix,
             [Values(true, false)] bool baseVirtual, [Values(true, false)] bool useNew, [Values(true, false)] bool newAbstract,
             [Values(true, false)] bool mustInitializeOnNew)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;
        
        public class DeclareTypeBase
        {
            [{{prefix}}MustInitialize{{suffix}}] public {{(baseVirtual ? "virtual" : "")}} string TestProp { get; set; }
        }
        public {{(newAbstract ? "abstract" : "")}} class DeclareTypeSub : DeclareTypeBase
        {
            [|{{(mustInitializeOnNew ? $"[{prefix}MustInitialize{suffix}] " : "")}}public {{(newAbstract ? "abstract" : "")}} {{(useNew ? "new" : "")}} string TestProp { get; set; }|]
        }
        """;

        await VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task Test_Warns_WhenSubclass([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix,
           [Values(true, false)] bool baseVirtual,
           [Values(true, false)] bool subShouldHaveDecleration,
           [Values(true, false)] bool newAbstract, [Values(true, false)] bool useNew, [Values(true, false)] bool mustInitializeOnNew)
    {
        Assume.That(!subShouldHaveDecleration || baseVirtual); // For subShouldHaveDecleration we need virtual or we will have a compile error

        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;
        
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

        await VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task Test_Warns_WhenSubclass_AndBaseAbstract([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix,
       [Values(true, false)] bool newAbstract, [Values(true, false)] bool useNew, [Values(true, false)] bool mustInitializeOnNew)
    {

        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;
        
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

        await VerifyAnalyzerAsync(test);
    }
}
