﻿using DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;
using DotNetPowerExtensions.Analyzers.MustInitialize.CodeFixProviders;

namespace DotNetPowerExtensions.Analyzers.Tests.MustInitialize.MustInitializeAttribute;

internal class MustIinitializeRequiredMembers_Tests
    : MustInitializeCodeFixVerifierBase<MustInitializeRequiredMembers, MustInitializeRequiredMembersCodeFixProvider, ObjectCreationExpressionSyntax>
{
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

        await VerifyAnalyzerAsync(test);
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

        await VerifyAnalyzerAsync(test);
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

        await VerifyCodeFixAsync(test, fixCode);
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

        await VerifyCodeFixAsync(test, fixCode);
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

        await VerifyCodeFixAsync(test, fixCode);
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

        await VerifyCodeFixAsync(test, fixCode);
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

        await VerifyCodeFixAsync(test, fixCode);
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

        await VerifyCodeFixAsync(test, fixCode);
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

        await VerifyCodeFixAsync(test, fixCode);
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

        await VerifyAnalyzerAsync(test);
    }
}
