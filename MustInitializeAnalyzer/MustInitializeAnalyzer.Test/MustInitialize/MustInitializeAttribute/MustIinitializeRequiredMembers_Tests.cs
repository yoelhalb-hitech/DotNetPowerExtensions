using DotNetPowerExtensionsAnalyzer.MustInitialize.Analyzers;
using DotNetPowerExtensionsAnalyzer.MustInitialize.CodeFixProviders;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetPowerExtensionsAnalyzer.Test.MustInitialize.MustInitializeAttribute;

internal class MustIinitializeRequiredMembers_Tests : CodeFixVerifierBase<MustInitializeRequiredMembers, MustInitializeRequiredMembersCodeFixProvider>
{
    public static string[] Suffixes = { "", "Attribute", "()", "Attribute()" };
    public static string[] Prefixes = {"", "DotNetPowerExtensions.MustInitialize.",
                                                                    "global::DotNetPowerExtensions.MustInitialize." };

    [Test]
    public async Task Test_DoesNotWarn_WhenNoMustInitialize()
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;
        
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
        using DotNetPowerExtensions.MustInitialize;

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
        using DotNetPowerExtensions.MustInitialize;

        public class DeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
        }

        class Program { void Main() => [|new DeclareType{}|]; }
        """;

        var fixCode = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public class DeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
        }

        class Program { void Main() => new DeclareType{ TestProp = default, TestField = default }; }
        """;

        await VerifyCodeFixAsync(test, fixCode);
    }

    [Test]
    public async Task Test_Works_WithNoInitializer([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public class DeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
        }

        class Program { void Main() => [|new DeclareType()|]; }
        """;

        var fixCode = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public class DeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
        }

        class Program { void Main() => new DeclareType() { TestProp = default, TestField = default }; }
        """;

        await VerifyCodeFixAsync(test, fixCode);
    }

    [Test]
    public async Task Test_Works_WithNonEmptyInitializer([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public class DeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
            public string TestOther { get; set; }
        }

        class Program { void Main() => [|new DeclareType{ TestOther = "Testing" }|]; }
        """;

        var fixCode = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public class DeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
            public string TestOther { get; set; }
        }

        class Program { void Main() => new DeclareType{ TestOther = "Testing", TestProp = default, TestField = default }; }
        """;

        await VerifyCodeFixAsync(test, fixCode);
    }

    [Test]
    public async Task Test_Works_WithCtorAndNonEmptyInitializer([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public class DeclareType
        {
            public DeclareType(int t){}
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
            public string TestOther { get; set; }
        }

        class Program { void Main() => [|new DeclareType(10){ TestOther = "Testing" }|]; }
        """;

        var fixCode = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public class DeclareType
        {
            public DeclareType(int t){}
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
            public string TestOther { get; set; }
        }

        class Program { void Main() => new DeclareType(10){ TestOther = "Testing", TestProp = default, TestField = default }; }
        """;

        await VerifyCodeFixAsync(test, fixCode);
    }

    [Test]
    public async Task Test_Works_WithPartialInit([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public class DeclareType
        {            
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;            
        }

        class Program { void Main() => [|new DeclareType(){ TestProp = "Testing" }|]; }
        """;

        var fixCode = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public class DeclareType
        {            
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;            
        }

        class Program { void Main() => new DeclareType(){ TestProp = "Testing", TestField = default }; }
        """;

        await VerifyCodeFixAsync(test, fixCode);
    }

    [Test]
    public async Task Test_Works_WithSubclass([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public class DeclareType
        {            
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;            
        }
        public class Subclass : DeclareType {}

        class Program { void Main() => [|new Subclass(){}|]; }
        """;

        var fixCode = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public class DeclareType
        {            
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;            
        }
        public class Subclass : DeclareType {}

        class Program { void Main() => new Subclass(){ TestProp = default, TestField = default }; }
        """;

        await VerifyCodeFixAsync(test, fixCode);
    }

    [Test]
    public async Task Test_Subclass_Codefix_OnlyFixesOnceForEach([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public class DeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public virtual string TestProp { get; set; }
        }
        public class Subclass : DeclareType 
        {
            [{{prefix}}MustInitialize{{suffix}}] public override string TestProp { get; set; }
        }

        class Program { void Main() => [|new Subclass(){}|]; }
        """;

        var fixCode = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public class DeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public virtual string TestProp { get; set; }
        }
        public class Subclass : DeclareType 
        {
            [{{prefix}}MustInitialize{{suffix}}] public override string TestProp { get; set; }
        }

        class Program { void Main() => new Subclass(){ TestProp = default }; }
        """;

        await VerifyCodeFixAsync(test, fixCode);
    }

    // Basically this is to test that if the MustInitalize is on both the base and the sub it doesn't need to initalize twice...
    [Test]
    public async Task Test_Subclass_NoWarningIfDoneOnce([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {

        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;

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
