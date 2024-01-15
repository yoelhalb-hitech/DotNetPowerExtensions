using SequelPay.DotNetPowerExtensions.Analyzers.AccessControl;

namespace DotNetPowerExtensions.Analyzers.Tests.AccessControl;

internal class NonDelegateShouldNotBeAssigned_Tests : AnalyzerVerifierBase<NonDelegateShouldNotBeAssigned>
{
    [Test]
    public async Task Test_DoesNotWarn_WhenNoNonDelegate()
    {
        var testCode = """
        class A
        {
            public void Testing() {}
        }

        class Program { void Main(){ System.Action t = new A().Testing; }}
        """;

        await VerifyAnalyzerAsync(testCode).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenOtherMustInitialize([ValueSource(nameof(Suffixes))] string suffix)
    {
        var testCode = $$"""
        public class NonDelegateAttribute : System.Attribute {}
        class A
        {
            [NonDelegate{{suffix}}] public void Testing() { }
        }

        class Program { void Main() { System.Action t = new A().Testing; } }
        """;

        await VerifyAnalyzerAsync(testCode).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var testCode = $$"""
        class A
        {
            [{{prefix}}NonDelegate{{suffix}}] public void Testing() {}
        }

        class Program { void Main(){ System.Action t = new A().[|Testing|]; } }
        """;

        await VerifyAnalyzerAsync(testCode).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_ImplictlyTyped([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var testCode = $$"""
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
        class Program
        {
            void Main()
            {
                [{{prefix}}NonDelegate{{suffix}}]
                void Testing() {}

                var t = [|Testing|];
            }
        }
        """;

        await VerifyAnalyzerAsync(testCode).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_AsArgument([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var testCode = $$"""
        class A
        {
            [{{prefix}}NonDelegate{{suffix}}] public void Testing() {}
            public void TestArg(System.Action a){}
        }

        class Program { void Main(){ new A().TestArg(new A().[|Testing|]); } }
        """;

        await VerifyAnalyzerAsync(testCode).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_IndexedExpression([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var testCode = $$"""
        class A
        {
            [{{prefix}}NonDelegate{{suffix}}] public void Testing() {}
        }

        class Program { void Main(){ var a = new System.Action[1]; a[0] = new A().[|Testing|]; } }
        """;

        await VerifyAnalyzerAsync(testCode).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_CollectionInitializerExpression([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var testCode = $$"""
        class A
        {
            [{{prefix}}NonDelegate{{suffix}}] public void Testing() {}
        }

        class Program { void Main(){ var a = new System.Action[]{ new A().[|Testing|] }; } }
        """;

        await VerifyAnalyzerAsync(testCode).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_ListAdding([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var testCode = $$"""
        using System.Collections.Generic;
        class A
        {
            [{{prefix}}NonDelegate{{suffix}}] public void Testing() {}
        }

        class Program { void Main(){ var a = new List<System.Action>(); a.Add(new A().[|Testing|]); } }
        """;

        await VerifyAnalyzerAsync(testCode).ConfigureAwait(false);
    }
}
