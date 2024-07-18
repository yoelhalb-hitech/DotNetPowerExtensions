using DotNetPowerExtensions.Analyzers.Throws;
using Microsoft.CodeAnalysis.Testing;
using SequelPay.DotNetPowerExtensions;
using System;

namespace DotNetPowerExtensions.Analyzers.Tests.Throws.Analyzers;

internal class MemberInvokedDoesNotDocument_Tests : AnalyzerVerifierBase<MemberInvokedDoesNotDocument>
{
    public static string[] Attributes => new string[] { nameof(ThrowsByDocCommentAttribute), nameof(ThrowsAttribute), nameof(DoesNotThrowAttribute) }
                                               .Select(n => n.Replace(nameof(Attribute), "", StringComparison.Ordinal))
                                               .ToArray();

    [Test]
    public async Task Test_Works([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                        [ValueSource(nameof(Suffixes))] string suffix, [Values(true, false)] bool isIface)
    {
        var test = $$"""
        public {{(isIface ? "interface" : "class")}} TestTypeDeclare
        {
            public static int TestMethod(int i) => i;
            public static int TestProp { get; set; }
            public static event System.EventHandler TestEvent;
        }
        public {{(isIface ? "interface" : "class")}} TestType
        {
            [{{prefix}}{{attribute}}{{suffix}}]
            public void TestMethod1(){ _ = [|TestTypeDeclare.TestMethod(10)|];}
            [{{prefix}}{{attribute}}{{suffix}}]
            public void TestMethod2(){ _ = [|TestTypeDeclare.TestProp|];}
            [{{prefix}}{{attribute}}{{suffix}}]
            public void TestMethod3(){ [|TestTypeDeclare.TestEvent|] += (_, _) => {};}

            [{{prefix}}{{attribute}}{{suffix}}]
            public int TestProp1{ get => [|TestTypeDeclare.TestMethod(10)|]; set => [|TestTypeDeclare.TestMethod(10)|];}
            [{{prefix}}{{attribute}}{{suffix}}]
            public int TestProp2{ get => [|TestTypeDeclare.TestProp|]; set => [|TestTypeDeclare.TestProp|] = value;}
            [{{prefix}}{{attribute}}{{suffix}}]
            public System.EventHandler TestProp3{ set => [|TestTypeDeclare.TestEvent|] += value;}

            [{{prefix}}{{attribute}}{{suffix}}]
            public event System.EventHandler TestEvent1{ add => _ = [|TestTypeDeclare.TestMethod(10)|]; remove => _ = [|TestTypeDeclare.TestMethod(10)|];}
            [{{prefix}}{{attribute}}{{suffix}}]
            public event System.EventHandler TestEvent2{ add => _ = [|TestTypeDeclare.TestProp|]; remove => _ = [|TestTypeDeclare.TestProp|] = 10;}
            [{{prefix}}{{attribute}}{{suffix}}]
            public event System.EventHandler TestEvent3{ add => [|TestTypeDeclare.TestEvent|] += value; remove => [|TestTypeDeclare.TestEvent|] -= value; }
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_MessageIsCorrect([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                        [ValueSource(nameof(Suffixes))] string suffix, [Values(true, false)] bool isIface)
    {
        var test = $$"""
        public {{(isIface ? "interface" : "class")}} TestTypeDeclare
        {
            public static int TestMethod(int i) => i;
        }
        public {{(isIface ? "interface" : "class")}} TestType
        {
            [{{prefix}}{{attribute}}{{suffix}}]
            public void TestMethod(){ _ = TestTypeDeclare.TestMethod(10);}
        }
        """;

        await VerifyAnalyzerAsync(test, new DiagnosticResult("DNPE0502", DiagnosticSeverity.Warning)
                                                .WithSpan(9, 35, 9, 65).WithMessage("Member `TestMethod` referenced/invoked does not have exception documentation")).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenCalleeHasDocComment([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                        [ValueSource(nameof(Suffixes))] string suffix, [Values(true, false)] bool isIface)
    {
        var test = $$"""
        public {{(isIface ? "interface" : "class")}} TestTypeDeclare
        {
            /// <summary>
            /// </summary>
            public static int TestMethod(int i) => i;
            /// <summary>
            /// </summary>
            public static int TestProp { get; set; }
            /// <summary>
            /// </summary>
            public static event System.EventHandler TestEvent;
        }
        public {{(isIface ? "interface" : "class")}} TestType
        {
            [{{prefix}}{{attribute}}{{suffix}}]
            public void TestMethod1(){ _ = TestTypeDeclare.TestMethod(10);}
            [{{prefix}}{{attribute}}{{suffix}}]
            public void TestMethod2(){ _ = TestTypeDeclare.TestProp;}
            [{{prefix}}{{attribute}}{{suffix}}]
            public void TestMethod3(){ TestTypeDeclare.TestEvent += (_, _) => {};}

            [{{prefix}}{{attribute}}{{suffix}}]
            public int TestProp1{ get => TestTypeDeclare.TestMethod(10); set => TestTypeDeclare.TestMethod(10);}
            [{{prefix}}{{attribute}}{{suffix}}]
            public int TestProp2{ get => TestTypeDeclare.TestProp; set => TestTypeDeclare.TestProp = value;}
            [{{prefix}}{{attribute}}{{suffix}}]
            public System.EventHandler TestProp3{ set => TestTypeDeclare.TestEvent += value;}

            [{{prefix}}{{attribute}}{{suffix}}]
            public event System.EventHandler TestEvent1{ add => _ = TestTypeDeclare.TestMethod(10); remove => _ = TestTypeDeclare.TestMethod(10); }
            [{{prefix}}{{attribute}}{{suffix}}]
            public event System.EventHandler TestEvent2{ add => _ = TestTypeDeclare.TestProp; remove => _ = TestTypeDeclare.TestProp = 10; }
            [{{prefix}}{{attribute}}{{suffix}}]
            public event System.EventHandler TestEvent3{ add => TestTypeDeclare.TestEvent += value; remove => TestTypeDeclare.TestEvent -= value; }
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenCalleeHasAttribute([ValueSource(nameof(Prefixes))] string prefix,
                                                                    [ValueSource(nameof(Attributes))] string attributeForCallee,
                                                                    [ValueSource(nameof(Attributes))] string attribute,
                                                                    [ValueSource(nameof(Suffixes))] string suffix, [Values(true, false)] bool isIface)
    {
        var test = $$"""
        public {{(isIface ? "interface" : "class")}} TestTypeDeclare
        {
            [{{prefix}}{{attributeForCallee}}{{suffix}}]
            public static int TestMethod(int i) => i;
            [{{prefix}}{{attributeForCallee}}{{suffix}}]
            public static int TestProp { get; set; }
            [{{prefix}}{{attributeForCallee}}{{suffix}}]
            public static event System.EventHandler TestEvent;
        }
        public {{(isIface ? "interface" : "class")}} TestType
        {
            [{{prefix}}{{attribute}}{{suffix}}]
            public void TestMethod1(){ _ = TestTypeDeclare.TestMethod(10);}
            [{{prefix}}{{attribute}}{{suffix}}]
            public void TestMethod2(){ _ = TestTypeDeclare.TestProp;}
            [{{prefix}}{{attribute}}{{suffix}}]
            public void TestMethod3(){ TestTypeDeclare.TestEvent += (_, _) => {};}

            [{{prefix}}{{attribute}}{{suffix}}]
            public int TestProp1{ get => TestTypeDeclare.TestMethod(10); set => TestTypeDeclare.TestMethod(10);}
            [{{prefix}}{{attribute}}{{suffix}}]
            public int TestProp2{ get => TestTypeDeclare.TestProp; set => TestTypeDeclare.TestProp = value;}
            [{{prefix}}{{attribute}}{{suffix}}]
            public System.EventHandler TestProp3{ set => TestTypeDeclare.TestEvent += value;}

            [{{prefix}}{{attribute}}{{suffix}}]
            public event System.EventHandler TestEvent1{ add => _ = TestTypeDeclare.TestMethod(10); remove => _ = TestTypeDeclare.TestMethod(10);}
            [{{prefix}}{{attribute}}{{suffix}}]
            public event System.EventHandler TestEvent2{ add => _ = TestTypeDeclare.TestProp; remove => _ = TestTypeDeclare.TestProp = 10; }
            [{{prefix}}{{attribute}}{{suffix}}]
            public event System.EventHandler TestEvent3{ add => TestTypeDeclare.TestEvent += value; remove => TestTypeDeclare.TestEvent -= value; }
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }


    [Test]
    public async Task Test_DoesNotWarnWhenNoAttribute([ValueSource(nameof(Suffixes))] string suffix, [Values(true, false)] bool isIface)
    {
        var test = $$"""
        public {{(isIface ? "interface" : "class")}} TestTypeDeclare
        {
            public static int TestMethod(int i) => i;
            public static int TestProp { get; set; }
            public static event System.EventHandler TestEvent;
        }
        public {{(isIface ? "interface" : "class")}} TestType
        {
            public void TestMethod1(){ _ = TestTypeDeclare.TestMethod(10);}
            public void TestMethod2(){ _ = TestTypeDeclare.TestProp;}
            public void TestMethod3(){ TestTypeDeclare.TestEvent += (_, _) => {};}

            public int TestProp1{ get => TestTypeDeclare.TestMethod(10); set => TestTypeDeclare.TestMethod(10);}
            public int TestProp2{ get => TestTypeDeclare.TestProp; set => TestTypeDeclare.TestProp = value;}
            public System.EventHandler TestProp3{ set => TestTypeDeclare.TestEvent += value;}

            public event System.EventHandler TestEvent1{ add => _ = TestTypeDeclare.TestMethod(10); remove => _ = TestTypeDeclare.TestMethod(10); }
            public event System.EventHandler TestEvent2{ add => _ = TestTypeDeclare.TestProp; remove => _ = TestTypeDeclare.TestProp = 10; }
            public event System.EventHandler TestEvent3{ add => TestTypeDeclare.TestEvent += value; remove => TestTypeDeclare.TestEvent -= value; }
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenOtherAttribute([ValueSource(nameof(Attributes))] string attribute,
                                                            [ValueSource(nameof(Suffixes))] string suffix, [Values(true, false)] bool isIface)
    {
        var test = $$"""
        public class {{attribute}}Attribute : System.Attribute {}
        public {{(isIface ? "interface" : "class")}} TestTypeDeclare
        {
            public static int TestMethod(int i) => i;
            public static int TestProp { get; set; }
            public static event System.EventHandler TestEvent;
        }
        public {{(isIface ? "interface" : "class")}} TestType
        {
            [{{attribute}}{{suffix}}]
            public void TestMethod1(){ _ = TestTypeDeclare.TestMethod(10);}
            [{{attribute}}{{suffix}}]
            public void TestMethod2(){ _ = TestTypeDeclare.TestProp;}
            [{{attribute}}{{suffix}}]
            public void TestMethod3(){ TestTypeDeclare.TestEvent += (_, _) => {};}

            [{{attribute}}{{suffix}}]
            public int TestProp1{ get => TestTypeDeclare.TestMethod(10); set => TestTypeDeclare.TestMethod(10);}
            [{{attribute}}{{suffix}}]
            public int TestProp2{ get => TestTypeDeclare.TestProp; set => TestTypeDeclare.TestProp = value;}
            [{{attribute}}{{suffix}}]
            public System.EventHandler TestProp3{ set => TestTypeDeclare.TestEvent += value;}

            [{{attribute}}{{suffix}}]
            public event System.EventHandler TestEvent1{ add => _ = TestTypeDeclare.TestMethod(10); remove => _ = TestTypeDeclare.TestMethod(10);}
            [{{attribute}}{{suffix}}]
            public event System.EventHandler TestEvent2{ add => _ = TestTypeDeclare.TestProp; remove => _ = TestTypeDeclare.TestProp = 10;}
            [{{attribute}}{{suffix}}]
            public event System.EventHandler TestEvent3{ add => TestTypeDeclare.TestEvent += value; remove => TestTypeDeclare.TestEvent -= value; }
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }
}
