using DotNetPowerExtensions.Analyzers.AccessControl;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace DotNetPowerExtensions.Analyzers.Tests.AccessControl;

internal class NonDelegateShouldNotBeAssigned_Tests : AnalyzerVerifierBase<NonDelegateShouldNotBeAssigned>
{
    const string NamespaceString = $"{nameof(DotNetPowerExtensions)}.{nameof(DotNetPowerExtensions.AccessControl)}";
    public static string[] Suffixes = { "", nameof(Attribute), "()", $"{nameof(Attribute)}()" };
    public static string[] Prefixes = {"", NamespaceString + ".",
                                                                    $"global::{NamespaceString}." };


    [Test]
    public async Task Test_DoesNotWarn_WhenNoNonDelegate()
    {
        var testCode = """
        using System;        
        
        class A
        {
            public void Testing() {}
        }

        class Program { void Main(){ Action t = new A().Testing; }}
        """;

        await VerifyAnalyzerAsync(testCode).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenOtherMustInitialize([ValueSource(nameof(Suffixes))] string suffix)
    {
        var testCode = $$"""
        using System;
        public class NonDelegateAttribute : System.Attribute {}

        class A
        {
            [NonDelegate{{suffix}}] public void Testing() { }
        }

        class Program { void Main() { Action t = new A().Testing; } }
        """;

        await VerifyAnalyzerAsync(testCode).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var testCode = $$"""
        using System;
        using DotNetPowerExtensions.AccessControl;
        
        class A
        {
            [{{prefix}}NonDelegate{{suffix}}] public void Testing() {}
        }

        class Program { void Main(){ Action t = new A().[|Testing|]; } }
        """;

        await VerifyAnalyzerAsync(testCode).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_ImplictlyTyped([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var testCode = $$"""
        using System;
        using DotNetPowerExtensions.AccessControl;
        
        class A
        {
            [{{prefix}}NonDelegate{{suffix}}] public void Testing() {}
        }

        class Program { void Main(){ var t = new A().[|Testing|]; } }
        """;

        await VerifyAnalyzerAsync(testCode).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_WithStatic([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var testCode = $$"""
        using System;
        using DotNetPowerExtensions.AccessControl;
        
        class A
        {
            [{{prefix}}NonDelegate{{suffix}}] public static void Testing() {}
        }

        class Program { void Main(){ var t = A.[|Testing|]; } }
        """;

        await VerifyAnalyzerAsync(testCode).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_WithDoubleAccess([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var testCode = $$"""
        using System;
        using DotNetPowerExtensions.AccessControl;
        
        class A
        {
            [{{prefix}}NonDelegate{{suffix}}] public void Testing() {}
        }
        class B
        {
        	public A a = new A();
        }

        class Program { void Main(){ var t = new B().a.[|Testing|]; } }
        """;

        await VerifyAnalyzerAsync(testCode).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_SavedInVariable([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var testCode = $$"""
        using System;
        using DotNetPowerExtensions.AccessControl;
        
        class A
        {
            [{{prefix}}NonDelegate{{suffix}}] public void Testing() {}
        }

        class Program { void Main(){ var a = new A(); var t = a.[|Testing|]; } }
        """;

        await VerifyAnalyzerAsync(testCode).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_InnerMethod([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var testCode = $$"""
        using System;
        using DotNetPowerExtensions.AccessControl;

        class Program { void Main(){ [{{prefix}}NonDelegate{{suffix}}] void Testing() {} var t = [|Testing|]; } }
        """;

        await VerifyAnalyzerAsync(testCode).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_AsArgument([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var testCode = $$"""
        using System;
        using DotNetPowerExtensions.AccessControl;

        class A
        {
            [{{prefix}}NonDelegate{{suffix}}] public void Testing() {}
            public void TestArg(Action a){}
        }

        class Program { void Main(){ new A().TestArg(new A().[|Testing|]); } }
        """;

        await VerifyAnalyzerAsync(testCode).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_IndexedExpression([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var testCode = $$"""
        using System;
        using DotNetPowerExtensions.AccessControl;

        class A
        {
            [{{prefix}}NonDelegate{{suffix}}] public void Testing() {}            
        }

        class Program { void Main(){ var a = new Action[1]; a[0] = new A().[|Testing|]; } }
        """;

        await VerifyAnalyzerAsync(testCode).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_CollectionInitializerExpression([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var testCode = $$"""
        using System;
        using DotNetPowerExtensions.AccessControl;

        class A
        {
            [{{prefix}}NonDelegate{{suffix}}] public void Testing() {}            
        }

        class Program { void Main(){ var a = new Action[]{ new A().[|Testing|] }; } }
        """;

        await VerifyAnalyzerAsync(testCode).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_ListAdding([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var testCode = $$"""
        using System;
        using System.Collections.Generic;
        using DotNetPowerExtensions.AccessControl;

        class A
        {
            [{{prefix}}NonDelegate{{suffix}}] public void Testing() {}            
        }

        class Program { void Main(){ var a = new List<Action>(); a.Add(new A().[|Testing|]); } }
        """;

        await VerifyAnalyzerAsync(testCode).ConfigureAwait(false);
    }
}
