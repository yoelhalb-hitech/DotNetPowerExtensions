using DotNetPowerExtensions.Analyzers.MustInitialize.MustInitializeAttribute.Analyzers;
using DotNetPowerExtensions.Analyzers.MustInitialize.MustInitializeAttribute.CodeFixProviders;
using Microsoft.CodeAnalysis.Testing;

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
