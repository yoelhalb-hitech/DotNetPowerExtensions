using DotNetPowerExtensions.Analyzers.Throws;
using Microsoft.CodeAnalysis.Testing;

namespace DotNetPowerExtensions.Analyzers.Tests.Throws.Analyzers;

internal class ThrowsByDocCommentDoesNotHaveDocComment_Tests : AnalyzerVerifierBase<ThrowsByDocCommentDoesNotHaveDocComment>
{
    [Test]
    public async Task Test_Works([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix, [Values(true, false)] bool isIface)
    {
        var test = $$"""
        public {{(isIface ? "interface" : "class")}} TestType
        {
            [{{prefix}}ThrowsByDocComment{{suffix}}]
            public static int [|TestProp|] { get; set; }
            [{{prefix}}ThrowsByDocComment{{suffix}}]
            public static int [|Test|](int i) => i;
            [{{prefix}}ThrowsByDocComment{{suffix}}]
            public static event System.EventHandler [|TestEvent|];
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_MessageIsCorrect()
    {
        var test = $$"""
        public class TestType
        {
            [ThrowsByDocComment]
            public static int Test(int i) => i;
        }
        """;

        await VerifyAnalyzerAsync(test, new DiagnosticResult("DNPE0501", DiagnosticSeverity.Warning)
                                                .WithSpan(5, 23, 5, 27).WithMessage("When `ThrowsByDocComment` then Doc comment is required")).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenDocComment([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix, [Values(true, false)] bool isIface)
    {
        var test = $$"""
        public {{(isIface ? "interface" : "class")}} TestType
        {
            /// <summary>
            /// </summary>
            [{{prefix}}ThrowsByDocComment{{suffix}}]
            public static int TestProp { get; set; }
            /// <summary>
            /// </summary>
            [{{prefix}}ThrowsByDocComment{{suffix}}]
            public static int Test(int i) => i;
            /// <summary>
            /// </summary>
            [{{prefix}}ThrowsByDocComment{{suffix}}]
            public static event System.EventHandler TestEvent;
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }


    [Test]
    public async Task Test_DoesNotWarnWhenNoAttribute([Values(true, false)] bool isIface)
    {
        var test = $$"""
        public {{(isIface ? "interface" : "class")}} TestType
        {
            public static int TestProp { get; set; }
            public static int Test(int i) => i;
            public static event System.EventHandler TestEvent;
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenOtherAttribute([ValueSource(nameof(Suffixes))] string suffix, [Values(true, false)] bool isIface)
    {
        var test = $$"""
        public class ThrowsByDocCommentAttribute : System.Attribute {}
        public {{(isIface ? "interface" : "class")}} TestType
        {
            [ThrowsByDocComment{{suffix}}]
            public static int TestProp { get; set; }
            [ThrowsByDocComment{{suffix}}]
            public static int Test(int i) => i;
            [ThrowsByDocComment{{suffix}}]
            public static event System.EventHandler TestEvent;
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }
}
