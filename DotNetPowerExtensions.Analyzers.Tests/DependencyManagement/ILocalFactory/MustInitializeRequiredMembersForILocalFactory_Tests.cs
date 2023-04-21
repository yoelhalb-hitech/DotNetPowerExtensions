using DotNetPowerExtensions.Analyzers.DependencyManagement.ILocalFactory.Analyzers;
using DotNetPowerExtensions.Analyzers.DependencyManagement.ILocalFactory.CodeFixProviders;
using Microsoft.CodeAnalysis.Testing;

namespace DotNetPowerExtensions.Analyzers.Tests.DependencyManagement.ILocalFactory;

internal sealed class MustInitializeRequiredMembersForILocalFactory_Tests
                    : CodeFixVerifierBase<MustInitializeRequiredMembersForILocalFactory, MustInitializeRequiredMembersForLocalCodeFixProvider>
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

        class Program { void Main() => (null as ILocalFactory<DeclareType>).Create(); }
        """;

        await VerifyAnalyzerAsync(test, new DiagnosticResult("DNPE0201", DiagnosticSeverity.Warning)
                                            .WithSpan(8, 75, 8, 77)
                                            .WithMessage("Must initialize 'TestProp, TestField'"))
            .ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_WithEmptyArguments([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using System;
        using System.Collections.Generic;
        public class DeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public AppDomain TestGeneralName { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public List<(string, int)> TestField;
        }

        class Program { void Main() => (null as ILocalFactory<DeclareType>).Create[|(/::/)|]; }
        """;

        var fixCode = $$"""new { TestProp = default(string), TestGeneralName = default(AppDomain), TestField = default(List<(string, int)>) }""";

        await VerifyCodeFixAsync(test, fixCode).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenInitialized([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class DeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp1 { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField1;
        }
        public class Type1 { public string TestProp1 { get; set; } }
        public class Type2 { public string TestField1 { get; set; } }
        class Program
        {
            void Main()
            {
                var TestField = "Test";
                var t = new Type2();
                _ = (null as ILocalFactory<DeclareType>).Create(new { TestProp = default(string), TestField, new Type1().TestProp1, t?.TestField1 });
            }
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
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

        class Program { void Main() => (null as ILocalFactory<DeclareType>).Create(); }
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

        class Program { void Main() => (null as ILocalFactory<DeclareType>).Create(); }
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

        class Program { void Main() => (null as ILocalFactory<DeclareType>).Create([|new {/::/}|]); }
        """;

        var fixCode = $$""" TestProp = default(string), TestField = default(string) """;

        await VerifyCodeFixAsync(test, fixCode).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnForInitialized([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
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

        class Program { void Main() => (null as ILocalFactory<DeclareType>).Create(new {}); }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_ForClassInitializer([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        // Because it will be handled by another analzyer
        var test = $$"""
        public class DeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
        }

        class Program { void Main() => (null as ILocalFactory<DeclareType>).Create(new DeclareType{}); }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_ForClassNoInitializer([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        // Because it will be handled by another analzyer
        var test = $$"""
        public class DeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
        }

        class Program { void Main() => (null as ILocalFactory<DeclareType>).Create(new DeclareType()); }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
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

        class Program { void Main() => (null as ILocalFactory<DeclareType>).Create([|new { TestOther = "Testing"/::/ }|]); }
        """;

        var fixCode = $$""", TestProp = default(string), TestField = default(string)""";

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

        class Program { void Main() => (null as ILocalFactory<DeclareType>).Create([|new { TestProp = "Testing"/::/ }|]); }
        """;

        var fixCode = $$""", TestField = default(string)""";

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

        class Program { void Main() => (null as ILocalFactory<Subclass>).Create[|(/::/)|]; }
        """;

        var fixCode = $$"""new { TestProp = default(string), TestField = default(string) }""";

        await VerifyCodeFixAsync(test, fixCode).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Subclass_HasCorrectMessage_OnlyOnceForEach([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class DeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public virtual string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
        }
        public class Subclass : DeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public override string TestProp { get; set; }
        }

        class Program { void Main() => (null as ILocalFactory<Subclass>).Create(); }
        """;

        await VerifyAnalyzerAsync(test, new DiagnosticResult("DNPE0201", DiagnosticSeverity.Warning)
                                            .WithSpan(12, 72, 12, 74)
                                            .WithMessage("Must initialize 'TestProp, TestField'"))
            .ConfigureAwait(false);
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

        class Program { void Main() => (null as ILocalFactory<Subclass>).Create[|(/::/)|]; }
        """;

        var fixCode = $$"""new { TestProp = default(string) }""";

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

        class Program { void Main() => (null as ILocalFactory<Subclass>).Create(new { TestProp = ""}); }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }
}