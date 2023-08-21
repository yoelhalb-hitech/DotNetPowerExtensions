using DotNetPowerExtensions.Analyzers.MustInitialize.MustInitializeAttribute.Analyzers;
using DotNetPowerExtensions.Analyzers.MustInitialize.MustInitializeAttribute.CodeFixProviders;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace DotNetPowerExtensions.Analyzers.Tests.MustInitialize.MustInitializeAttribute;

internal sealed class MustInitializeRequiredMembers_Tests
                    : CodeFixVerifierBase<MustInitializeRequiredMembers, MustInitializeRequiredMembersCodeFixProvider>
{
    [Test]
    public async Task Test_HasCorrectMessage([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class DeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
        }

        class Program { void Main() => new DeclareType{}; }
        """;

        await VerifyAnalyzerAsync(test, new DiagnosticResult("DNPE0103", DiagnosticSeverity.Warning)
                                            .WithSpan(8, 32, 8, 49)
                                            .WithMessage("Must initialize 'TestProp, TestField'"))
            .ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenNoMustInitialize()
    {
        var test = $$"""
        public class DeclareType
        {
            public string TestProp { get; set; }
            public string TestField;
        }

        class Program { void Main() => new DeclareType{}; }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenUsingInitializesAllRequired([ValueSource(nameof(Prefixes))] string prefix,
                                                                                            [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class DeclareType
        {
            [{{prefix}}InitializesAllRequired{{suffix}}] public DeclareType(){ TestProp = TestField = ""; }
            public DeclareType(int i){}
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
        }

        class Program { void Main() => new DeclareType{}; }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Warns_WhenUsingNonDefaultCtor([ValueSource(nameof(Prefixes))] string prefix,
                                                                                    [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class DeclareType
        {
            public DeclareType(){}
            public DeclareType(int i){}
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
        }

        class Program { void Main() => [|new DeclareType(10){/::/}|]; }
        """;

        var fixCode = $$""" TestProp = default, TestField = default """;

        await VerifyCodeFixAsync(test, fixCode).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Warns_WhenUsingCtorCallingAnother_MultipleSourceFiles([ValueSource(nameof(Prefixes))] string prefix,
                                                                                [ValueSource(nameof(Suffixes))] string suffix)
    {
        var code1 = $$"""
        public class DeclareTypeBase
        {
            public DeclareTypeBase() : this(10){}
            public DeclareTypeBase(int i){}
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
        }
        """;
        var code2 = $$"""
        public class DeclareType : DeclareTypeBase
        {
            public DeclareType() : this(10){}
            public DeclareType(int i): base(){}
        }
        """;
        var code3 = "class Program { void Main() => [|new DeclareType{}|]; }";

        var test = new CSharpAnalyzerTest<MustInitializeRequiredMembers, NUnitVerifier>
        {
            TestState =
            {
                Sources = { AnalyzerVerifierBase<MustInitializeRequiredMembers>.NamespacePart + code1, code2, code3 },
            },
        };
        test.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(
                                                typeof(SequelPay.DotNetPowerExtensions.MustInitializeAttribute).Assembly.Location));
        await test.RunAsync().ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Warns_WhenUsingCtorCallingAnother_AndDefaultBase_MultipleSourceFiles([ValueSource(nameof(Prefixes))] string prefix,
                                                                            [ValueSource(nameof(Suffixes))] string suffix)
    {
        var code1 = $$"""
        public class DeclareTypeBase
        {
            public DeclareTypeBase() : this(10){}
            public DeclareTypeBase(int i){}
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
        }
        """;
        var code2 = $$"""
        public class DeclareType : DeclareTypeBase
        {
            public DeclareType() : this(10){}
            public DeclareType(int i){} // Calls base implictly
        }
        """;
        var code3 = "class Program { void Main() => [|new DeclareType{}|]; }";

        var test = new CSharpAnalyzerTest<MustInitializeRequiredMembers, NUnitVerifier>
        {
            TestState =
            {
                Sources = { AnalyzerVerifierBase<MustInitializeRequiredMembers>.NamespacePart + code1, code2, code3 },
            },
        };
        test.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(
                                                typeof(SequelPay.DotNetPowerExtensions.MustInitializeAttribute).Assembly.Location));
        await test.RunAsync().ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenUsingInitializesAllRequired_ViaCtorCallingAnother_MultipleSourceFiles([ValueSource(nameof(Prefixes))] string prefix,
                                                                                        [ValueSource(nameof(Suffixes))] string suffix)
    {
        var code1 = $$"""
        public class DeclareTypeBase
        {
            public DeclareTypeBase() : this(10){}
            [{{prefix}}InitializesAllRequired{{suffix}}] public DeclareTypeBase(int i){ TestProp = TestField = ""; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
        }
        """;
        var code2 = $$"""
        public class DeclareType : DeclareTypeBase
        {
            public DeclareType() : this(10){}
            public DeclareType(int i): base(){}
        }
        """;
        var code3 = "class Program { void Main() => new DeclareType{}; }";

        var test = new CSharpAnalyzerTest<MustInitializeRequiredMembers, NUnitVerifier>
        {
            TestState =
            {
                Sources = { AnalyzerVerifierBase<MustInitializeRequiredMembers>.NamespacePart + code1, code2, code3 },
            },
        };
        test.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(
                                                typeof(SequelPay.DotNetPowerExtensions.MustInitializeAttribute).Assembly.Location));
        await test.RunAsync().ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenUsingInitializesAllRequired_ViaCtorCallingAnother_AndDefualtBase_MultipleSourceFiles([ValueSource(nameof(Prefixes))] string prefix,
                                                                                    [ValueSource(nameof(Suffixes))] string suffix)
    {
        var code1 = $$"""
        public class DeclareTypeBase
        {
            public DeclareTypeBase() : this("test"){}
            [{{prefix}}InitializesAllRequired{{suffix}}] public DeclareTypeBase(string s){TestProp = TestField = "";}
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
        }
        """;
        var code2 = $$"""
        public class DeclareType : DeclareTypeBase
        {
            public DeclareType() : this(10){}
            public DeclareType(int i){} // Calls base implictly
        }
        """;
        var code3 = "class Program { void Main() => new DeclareType{}; }";

        var test = new CSharpAnalyzerTest<MustInitializeRequiredMembers, NUnitVerifier>
        {
            TestState =
            {
                Sources = { AnalyzerVerifierBase<MustInitializeRequiredMembers>.NamespacePart + code1, code2, code3 },
            },
        };
        test.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(
                                                typeof(SequelPay.DotNetPowerExtensions.MustInitializeAttribute).Assembly.Location));
        await test.RunAsync().ConfigureAwait(false);
    }


    [Test]
    public async Task Test_Warns_WhenUsingOtherCtor([ValueSource(nameof(Prefixes))] string prefix,
                                                                                        [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class DeclareType
        {
            public DeclareType(){}
            [{{prefix}}InitializesAllRequired{{suffix}}] public DeclareType(int i){ TestProp = TestField = ""; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
        }

        class Program { void Main() => [|new DeclareType{/::/}|]; }
        """;

        var fixCode = $$""" TestProp = default, TestField = default """;

        await VerifyCodeFixAsync(test, fixCode).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Warns_WhenUsingSubclassCtor([ValueSource(nameof(Prefixes))] string prefix,
                                                                                    [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class DeclareType
        {
            public DeclareType(){}
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
        }

        public class SubType : DeclareType{}

        class Program { void Main() => [|new SubType{/::/}|]; }
        """;

        var fixCode = $$""" TestProp = default, TestField = default """;

        await VerifyCodeFixAsync(test, fixCode).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Warns_OnlyForNonInitialized_WhenUsingInitializes([ValueSource(nameof(Prefixes))] string prefix,
                                                                                    [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        public class DeclareType
        {
            [{{prefix}}Initializes{{suffix}}("TestProp")] public DeclareType(){}
            [{{prefix}}Initializes{{suffix}}("TestProp", "TestField")] public DeclareType(int i){ TestProp = TestField = ""; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
        }

        class Program { void Main() => [|new DeclareType{/::/}|]; }
        """;

        var fixCode = $$""" TestField = default """;

        await VerifyCodeFixAsync(test, fixCode).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Warns_OnlyForNonInitialized_WhenUsingInitializesWithBase([ValueSource(nameof(Prefixes))] string prefix,
                                                                                [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        public class DeclareTypeBase
        {
            [{{prefix}}Initializes{{suffix}}(nameof(DeclareTypeBase.TestProp))] public DeclareTypeBase(){}
            public DeclareTypeBase(int i){ }
            [{{prefix}}MustInitialize{{suffix}}] public virtual string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public virtual string TestProp1 { get; set; }
        }
        public class DeclareType : DeclareTypeBase
        {
            public DeclareType() : this(10){}
            [{{prefix}}Initializes{{suffix}}(nameof(DeclareTypeBase.TestProp1))] public DeclareType(int i){ TestProp = TestField = ""; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
        }

        class Program { void Main() => [|new DeclareType{/::/}|]; }
        """;

        var fixCode = $$""" TestField = default """;

        await VerifyCodeFixAsync(test, fixCode).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Warns_OnlyForNonInitialized_WhenBaseHas_InitializesAllRequired([ValueSource(nameof(Prefixes))] string prefix,
                                                                                            [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class DeclareTypeBase
        {
            [{{prefix}}InitializesAllRequired{{suffix}}] public DeclareTypeBase(){}
            public DeclareTypeBase(int i){ }
            [{{prefix}}MustInitialize{{suffix}}] public virtual string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public virtual string TestProp1 { get; set; }
        }
        public class DeclareType : DeclareTypeBase
        {
            [{{prefix}}MustInitialize{{suffix}}] public override string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public new string TestProp1 { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
        }

        class Program { void Main() => [|new DeclareType{/::/}|]; }
        """;

        var fixCode = $$""" TestField = default, TestProp1 = default """;

        await VerifyCodeFixAsync(test, fixCode).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenUsingInitializes_AndCoversAll([ValueSource(nameof(Prefixes))] string prefix,
                                                                                [Values("", nameof(Attribute))] string suffix)
    {
        var test = $$"""
        public class DeclareType
        {
            [{{prefix}}Initializes{{suffix}}("TestProp", "TestField")]  public DeclareType(){}
            [{{prefix}}Initializes{{suffix}}("TestProp")] public DeclareType(int i){ TestProp = TestField = ""; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
        }

        class Program { void Main() => new DeclareType{}; }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenUsingInitializesAllRequiredWithNoInitializer([ValueSource(nameof(Prefixes))] string prefix,
                                                                                        [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class DeclareType
        {
            [{{prefix}}InitializesAllRequired{{suffix}}] public DeclareType(){ TestProp = TestField = ""; }
            public DeclareType(int i){}
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
        }

        class Program { void Main() => new DeclareType(); }
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
        public class DeclareType
        {
            [SetsRequiredMembers{{suffix}}] public DeclareType(){ TestProp = TestField = ""; }
            public DeclareType(int i){}
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
        }

        class Program { void Main() => new DeclareType{}; }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenInitialized([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class DeclareTypeBase
        {
            [{{prefix}}MustInitialize{{suffix}}] public virtual string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
        }
        public class DeclareType : DeclareTypeBase
        {
            [{{prefix}}Initialized{{suffix}}] public override string TestProp { get; set; }
            [{{prefix}}Initialized{{suffix}}] public new string TestField;
        }

        class Program { void Main() => new DeclareType{}; }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenOtherMustInitialize([ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class MustInitializeAttribute : System.Attribute {}
        public class DeclareType
        {
            [MustInitialize{{suffix}}] public string TestProp { get; set; }
            [MustInitialize{{suffix}}] public string TestField;
        }

        class Program { void Main() => new DeclareType{}; }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_WithEmptyInitializer([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class DeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
        }

        class Program { void Main() => [|new DeclareType{/::/}|]; }
        """;

        var fixCode = $$""" TestProp = default, TestField = default """;

        await VerifyCodeFixAsync(test, fixCode).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_WithNoInitializer([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class DeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
        }

        class Program { void Main() => [|new DeclareType()/::/|]; }
        """;

        var fixCode = $$""" { TestProp = default, TestField = default }""";

        await VerifyCodeFixAsync(test, fixCode).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_WithNonEmptyInitializer([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class DeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
            public string TestOther { get; set; }
        }

        class Program { void Main() => [|new DeclareType{ TestOther = "Testing"/::/ }|]; }
        """;

        var fixCode = $$""", TestProp = default, TestField = default""";

        await VerifyCodeFixAsync(test, fixCode).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_WithCtorAndNonEmptyInitializer([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class DeclareType
        {
            public DeclareType(int t){}
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
            public string TestOther { get; set; }
        }

        class Program { void Main() => [|new DeclareType(10){ TestOther = "Testing"/::/ }|]; }
        """;

        var fixCode = $$""", TestProp = default, TestField = default""";

        await VerifyCodeFixAsync(test, fixCode).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_WithPartialInit([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class DeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
        }

        class Program { void Main() => [|new DeclareType(){ TestProp = "Testing"/::/ }|]; }
        """;

        var fixCode = $$""", TestField = default""";

        await VerifyCodeFixAsync(test, fixCode).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_WithSubclass([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class DeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
        }
        public class Subclass : DeclareType {}

        class Program { void Main() => [|new Subclass(){/::/}|]; }
        """;

        var fixCode = $$""" TestProp = default, TestField = default """;

        await VerifyCodeFixAsync(test, fixCode).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Subclass_Codefix_OnlyFixesOnceForEach([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class DeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public virtual string TestProp { get; set; }
        }
        public class Subclass : DeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public override string TestProp { get; set; }
        }

        class Program { void Main() => [|new Subclass(){/::/}|]; }
        """;

        var fixCode = $$""" TestProp = default """;

        await VerifyCodeFixAsync(test, fixCode).ConfigureAwait(false);
    }

    // Basically this is to test that if the MustInitalize is on both the base and the sub it doesn't need to initalize twice...
    [Test]
    public async Task Test_Subclass_NoWarningIfDoneOnce([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {

        var test = $$"""
        public class DeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public virtual string TestProp { get; set; }
        }
        public class Subclass : DeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public override string TestProp { get; set; }
        }

        class Program { void Main() => new Subclass(){ TestProp = "" }; }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }
}
