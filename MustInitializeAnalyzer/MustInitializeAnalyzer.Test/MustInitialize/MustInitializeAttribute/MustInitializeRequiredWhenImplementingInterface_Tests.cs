using DotNetPowerExtensionsAnalyzer.MustInitialize.Analyzers;
using DotNetPowerExtensionsAnalyzer.MustInitialize.CodeFixProviders;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetPowerExtensionsAnalyzer.Test.MustInitialize.MustInitializeAttribute;

internal class MustInitializeRequiredWhenImplementingInterface_Tests
                    : CodeFixVerifierBase<MustInitializeRequiredWhenImplementingInterface, MustInitializeRequiredWhenImplementingInterfaceCodeFixProvider>
{
    public static string[] Suffixes = { "", "Attribute", "()", "Attribute()" };
    public static string[] Prefixes = {"", "DotNetPowerExtensions.MustInitialize.",
                                                                    "global::DotNetPowerExtensions.MustInitialize." };

    [Test]
    public async Task Test_DoesNotWarn_WhenNoMustInitialize()
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public interface IDeclareType
        {            
            string TestProp { get; set; }
        }
        public class DeclareType : IDeclareType 
        {
            public string TestProp { get; set; }
        }
        """;

        await VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenOtherMustInitialize([ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public class MustInitializeAttribute : System.Attribute {}
        public interface IDeclareType
        {            
            [MustInitialize{{suffix}}] string TestProp { get; set; }
        }
        public class DeclareType : IDeclareType 
        {
            public string TestProp { get; set; }
        }
        """;

        await VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task Test_NoDiagnostic_ForInterfaceChain([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public interface IDeclareType
        {            
            [{{prefix}}MustInitialize{{suffix}}] string TestProp { get; set; }            
        }
        public interface IDeclareTypeSub : IDeclareType 
        {
            string TestProp { get; set; }
        }
        """;

        await VerifyAnalyzerAsync(test);
    }


    [Test]
    public async Task Test_WorksProperty_WithInterface([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public interface IDeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] string TestProp { get; set; }
        }
        public class DeclareType : IDeclareType 
        {
            [|public string TestProp { get; set; }|]
        }
        """;

        var fixCode = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public interface IDeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] string TestProp { get; set; }
        }
        public class DeclareType : IDeclareType 
        {
            [MustInitialize]
            public string TestProp { get; set; }
        }
        """;

        await VerifyCodeFixAsync(test, fixCode);
    }

    [Test]
    public async Task Test_WorksProperty_WithInterface_AndAbstract([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public interface IDeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] string TestProp { get; set; }
        }
        public abstract class DeclareType : IDeclareType
        {
            [|public abstract string TestProp { get; set; }|]
        }
        """;

        var fixCode = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public interface IDeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] string TestProp { get; set; }
        }
        public abstract class DeclareType : IDeclareType
        {
            [MustInitialize]
            public abstract string TestProp { get; set; }
        }
        """;

        await VerifyCodeFixAsync(test, fixCode);
    }

    [Test]
    public async Task Test_WorksProperty_WithSubclassedInterface([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public interface IDeclareType
        {            
            [{{prefix}}MustInitialize{{suffix}}] string TestProp { get; set; }
        }
        public interface IDeclareTypeSub : IDeclareType {}
        public class DeclareType : IDeclareTypeSub
        {
            [|public string TestProp { get; set; }|]
        }
        """;

        var fixCode = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public interface IDeclareType
        {            
            [{{prefix}}MustInitialize{{suffix}}] string TestProp { get; set; }
        }
        public interface IDeclareTypeSub : IDeclareType {}
        public class DeclareType : IDeclareTypeSub
        {
            [MustInitialize]
            public string TestProp { get; set; }
        }
        """;

        await VerifyCodeFixAsync(test, fixCode);
    }

    [Test]
    public async Task Test_WorksProperty_WithInterfaceAndBase([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public interface IDeclareType
        {            
            [{{prefix}}MustInitialize{{suffix}}] string TestProp { get; set; }
        }
        public class DeclareTypeBase {}
        public class DeclareType : DeclareTypeBase, IDeclareType 
        {
            [|public string TestProp { get; set; }|]
        }
        """;

        var fixCode = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public interface IDeclareType
        {            
            [{{prefix}}MustInitialize{{suffix}}] string TestProp { get; set; }
        }
        public class DeclareTypeBase {}
        public class DeclareType : DeclareTypeBase, IDeclareType 
        {
            [MustInitialize]
            public string TestProp { get; set; }
        }
        """;

        await VerifyCodeFixAsync(test, fixCode);
    }

    [Test]
    public async Task Test_Works_WithInterfaceAndBase_WhenBaseImplements([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public interface IDeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] string TestProp { get; set; }
        }
        public class DeclareTypeBase
        {
            public string TestProp { get; set; }
        }
        public class DeclareType : DeclareTypeBase, IDeclareType
        {
            [|public string TestProp { get; set; }|]
        }
        """;

        var fixCode = $$"""
        using DotNetPowerExtensions.MustInitialize;
        
        public interface IDeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] string TestProp { get; set; }
        }
        public class DeclareTypeBase
        {
            public string TestProp { get; set; }
        }
        public class DeclareType : DeclareTypeBase, IDeclareType
        {
            [MustInitialize]
            public string TestProp { get; set; }
        }
        """;

        // No code fix for this situation
        await VerifyCodeFixAsync(test, fixCode);
    }

    [Test]
    public async Task TestWarns_WhenBasePrivate([ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;
        
        public interface IDeclareType
        {            
            [MustInitialize{{suffix}}] string TestProp { get; set; }
        }
        public class DeclareTypeBase
        {
            private string TestProp { get; set; }
        }
        public class DeclareType : DeclareTypeBase, IDeclareType
        {
            [|public string TestProp { get; set; }|]
        }
        """;

        var codeFix = $$"""
        using DotNetPowerExtensions.MustInitialize;
        
        public interface IDeclareType
        {            
            [MustInitialize{{suffix}}] string TestProp { get; set; }
        }
        public class DeclareTypeBase
        {
            private string TestProp { get; set; }
        }
        public class DeclareType : DeclareTypeBase, IDeclareType
        {
            [MustInitialize]
            public string TestProp { get; set; }
        }
        """;

        await VerifyCodeFixAsync(test, codeFix);
    }

    [Test]
    public async Task Test_WorksProperty_WithInterface_AndOtherInterfaces([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public interface IDeclareType
        {
            string OtherMethod();
            string TestingOtherProp { get; }
            [{{prefix}}MustInitialize{{suffix}}] string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] string TestPropWithAttributesSingleLine { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] string TestPropWithAttributesMultiLine { get; set; }
            string OtherMethod2();
        }
        public interface IOther 
        {
            string TestOtherProp { get; set; }            
        }
        public class TestAttribute: System.Attribute {}
        public class DeclareType : IDeclareType, IOther 
        {
            public string OtherMethod() => "Test";
            public string TestingOtherProp => "Test";
            [|public string TestProp { get; set; }|]
            public string OtherMethod2() => "Test";
            public string TestOtherProp { get; set; }
            public string TestOtherField;
            [|[Test] public string TestPropWithAttributesSingleLine { get; set; }|]
            [|[Test]
            public string TestPropWithAttributesMultiLine { get; set; }|]
        }
        """;

        var fixCode = $$"""
        using DotNetPowerExtensions.MustInitialize;
        
        public interface IDeclareType
        {
            string OtherMethod();
            string TestingOtherProp { get; }
            [{{prefix}}MustInitialize{{suffix}}] string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] string TestPropWithAttributesSingleLine { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] string TestPropWithAttributesMultiLine { get; set; }
            string OtherMethod2();
        }
        public interface IOther 
        {
            string TestOtherProp { get; set; }            
        }
        public class TestAttribute: System.Attribute {}
        public class DeclareType : IDeclareType, IOther 
        {
            public string OtherMethod() => "Test";
            public string TestingOtherProp => "Test";

            [MustInitialize]
            public string TestProp { get; set; }
            public string OtherMethod2() => "Test";
            public string TestOtherProp { get; set; }
            public string TestOtherField;
            [Test][MustInitialize] public string TestPropWithAttributesSingleLine { get; set; }
            [Test]
            [MustInitialize]
            public string TestPropWithAttributesMultiLine { get; set; }
        }
        """;

        await VerifyCodeFixAsync(test, fixCode);
    }
}
