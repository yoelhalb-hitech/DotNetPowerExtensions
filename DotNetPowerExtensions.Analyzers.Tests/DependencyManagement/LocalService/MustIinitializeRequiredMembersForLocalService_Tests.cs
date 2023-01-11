using DotNetPowerExtensions.Analyzers.DependencyManagement.LocalService.Analyzers;
using DotNetPowerExtensions.Analyzers.DependencyManagement.LocalService.CodeFixProviders;
using DotNetPowerExtensions.Analyzers.Tests.MustInitialize;
using Microsoft.CodeAnalysis.Testing;

namespace DotNetPowerExtensions.Analyzers.Tests.DependencyManagement.LocalService;

internal sealed class MustIinitializeRequiredMembersForLocal_Tests
    : MustInitializeCodeFixVerifierBase<MustIinitializeRequiredMembersForLocalService, MustInitializeRequiredMembersForLocalCodeFixProvider, InvocationExpressionSyntax>
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

        class Program { void Main() => new DotNetPowerExtensions.DependencyManagement.LocalService<DeclareType>(null).Get(); }
        """;

        await VerifyAnalyzerAsync(test, new DiagnosticResult("DNPE0201", DiagnosticSeverity.Warning)
                                            .WithSpan(8, 114, 8, 116)
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

        class Program { void Main() => new DotNetPowerExtensions.DependencyManagement.LocalService<DeclareType>(null).Get[|(/::/)|]; }
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
                _ = new DotNetPowerExtensions.DependencyManagement.LocalService<DeclareType>(null).Get(new { TestProp = default(string), TestField, new Type1().TestProp1, t?.TestField1 });
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
        
        class Program { void Main() => new DotNetPowerExtensions.DependencyManagement.LocalService<DeclareType>(null).Get(); }
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
        
        class Program { void Main() => new DotNetPowerExtensions.DependencyManagement.LocalService<DeclareType>(null).Get(); }
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

        class Program { void Main() => new DotNetPowerExtensions.DependencyManagement.LocalService<DeclareType>(null).Get([|new {/::/}|]); }
        """;

        var fixCode = $$""" TestProp = default(string), TestField = default(string) """;

        await VerifyCodeFixAsync(test, fixCode).ConfigureAwait(false);
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

        class Program { void Main() => new DotNetPowerExtensions.DependencyManagement.LocalService<DeclareType>(null).Get(new DeclareType{}); }
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

        class Program { void Main() => new DotNetPowerExtensions.DependencyManagement.LocalService<DeclareType>(null).Get(new DeclareType()); }
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

        class Program { void Main() => new DotNetPowerExtensions.DependencyManagement.LocalService<DeclareType>(null).Get([|new { TestOther = "Testing"/::/ }|]); }
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

        class Program { void Main() => 
                new DotNetPowerExtensions.DependencyManagement.LocalService<DeclareType>(null).Get([|new { TestProp = "Testing"/::/ }|]); }
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

        class Program { void Main() => new DotNetPowerExtensions.DependencyManagement.LocalService<Subclass>(null).Get[|(/::/)|]; }
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

        class Program { void Main() => new DotNetPowerExtensions.DependencyManagement.LocalService<Subclass>(null).Get(); }
        """;

        await VerifyAnalyzerAsync(test, new DiagnosticResult("DNPE0201", DiagnosticSeverity.Warning)
                                            .WithSpan(12, 111, 12, 113)
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

        class Program { void Main() => new DotNetPowerExtensions.DependencyManagement.LocalService<Subclass>(null).Get[|(/::/)|]; }
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

        class Program { void Main() => new DotNetPowerExtensions.DependencyManagement.LocalService<Subclass>(null).Get(new { TestProp = ""}); }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }
}