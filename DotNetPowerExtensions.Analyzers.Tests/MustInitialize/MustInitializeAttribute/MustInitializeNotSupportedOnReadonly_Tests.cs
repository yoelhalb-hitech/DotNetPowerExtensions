using DotNetPowerExtensionsAnalyzer.MustInitialize.Analyzers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetPowerExtensionsAnalyzer.Test.MustInitialize.MustInitializeAttribute;

internal class MustInitializeNotSupportedOnReadonly_Tests : MustInitializeAnalyzerVerifierBase<MustInitializeNotSupportedOnReadonly>
{
    [Test]
    public async Task Test_DoesNotWarn_WhenNoMustInitialize()
    {
        var test = $$"""        
        public class TypeName
        {
            public string TestProp { get; }
            public readonly string TestField;
        }

        """;

        await VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenOtherMustInitialize([ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class MustInitializeAttribute : System.Attribute {}
        public class TypeName
        {
            [MustInitialize{{suffix}}] public string TestProp { get; }
            [MustInitialize{{suffix}}] public readonly string TestField;
        }

        """;

        await VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task Test_MustInitialize_NoDiagnostic_OnReadWrite([ValueSource(nameof(Prefixes))] string prefix,
                                                                                [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class TypeName
        {
            [{{prefix}}MustInitialize{{suffix}}] string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] string TestField;
        }

        """;


        await VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task Test_MustInitialize_NoDiagnostic_OnReadInit([ValueSource(nameof(Prefixes))] string prefix,
                                                                            [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;

        namespace System.Runtime.CompilerServices { public class IsExternalInit{} } // Needed so far for compiling

        public class TypeName
        {
            [{{prefix}}MustInitialize{{suffix}}] string TestProp { get; init; }
        }

        """;


        await VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task Test_MustInitialize_AddsDiagnostic_OnReadOnly([ValueSource(nameof(Prefixes))] string prefix,
                                                                            [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class TypeName
        {
            [[|{{prefix}}MustInitialize{{suffix}}|]] string TestProp { get; }
            [[|{{prefix}}MustInitialize{{suffix}}|]] readonly string TestField;
        }

        """;

        await VerifyAnalyzerAsync(test);
    }
}
