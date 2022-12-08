using DotNetPowerExtensionsAnalyzer.MustInitialize.Analyzers;
using DotNetPowerExtensionsAnalyzer.MustInitialize.CodeFixProviders;
using DotNetPowerExtensionsAnalyzer.MustInitialize.MustInitializeAttribute.Analyzers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetPowerExtensionsAnalyzer.Test.MustInitialize.MustInitializeAttribute;

internal class CannotUseBaseImplementation_Tests 
    : CodeFixVerifierBase<CannotUseBaseImplementationForMustInitialize, CannotUseBaseImplementationForMustInitializeCodeFixProvider>
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
        public class DeclareTypeBase
        {
            public string TestProp { get; set; }            
        }
        public class DeclareType : DeclareTypeBase, IDeclareType
        {
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
        public class DeclareTypeBase
        {
            public string TestProp { get; set; }
        }
        public class DeclareType : DeclareTypeBase, IDeclareType
        {
        }
        """;

        await VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task Test_Works([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public interface IDeclareType
        {            
            [{{prefix}}MustInitialize{{suffix}}] string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] string TestPropProtected { set; }
        }
        public class DeclareTypeBase
        {
            public string TestProp { get; set; }
            public string TestPropProtected { protected get; set; }
        }
        public class [|[|DeclareType|]|] : DeclareTypeBase, IDeclareType
        {
        }
        """;

        var codeFix = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public interface IDeclareType
        {            
            [{{prefix}}MustInitialize{{suffix}}] string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] string TestPropProtected { set; }
        }
        public class DeclareTypeBase
        {
            public string TestProp { get; set; }
            public string TestPropProtected { protected get; set; }
        }
        public class DeclareType : DeclareTypeBase, IDeclareType
        {
            [MustInitialize]
            public new string TestProp { get => base.TestProp; set => base.TestProp = value; }
            [MustInitialize]
            public new string TestPropProtected { protected get => base.TestPropProtected; set => base.TestPropProtected = value; }
        }
        """;

        await VerifyCodeFixAsync(test, codeFix);
    }

    [Test]
    public async Task Test_Works_WithVirtual([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public interface IDeclareType
        {            
            [{{prefix}}MustInitialize{{suffix}}] string TestProp { get; set; }
        }
        public class DeclareTypeBase
        {
            public virtual string TestProp { get; set; }
        }
        public class [|DeclareType|] : DeclareTypeBase, IDeclareType
        {
        }
        """;

        var codeFix = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public interface IDeclareType
        {            
            [{{prefix}}MustInitialize{{suffix}}] string TestProp { get; set; }
        }
        public class DeclareTypeBase
        {
            public virtual string TestProp { get; set; }
        }
        public class DeclareType : DeclareTypeBase, IDeclareType
        {
            [MustInitialize]
            public override string TestProp { get => base.TestProp; set => base.TestProp = value; }
        }
        """;

        await VerifyCodeFixAsync(test, codeFix);
    }

    [Test]
    public async Task Test_Works_WithOverride([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public interface IDeclareType
        {            
            [{{prefix}}MustInitialize{{suffix}}] string TestProp { get; set; }
        }
        public class DeclareTypeBase
        {
            public virtual string TestProp { get; set; }
        }
        public class DeclareTypeBase2: DeclareTypeBase
        {
            public override string TestProp { get; set; }
        }
        public class [|DeclareType|] : DeclareTypeBase2, IDeclareType
        {
        }
        """;

        var codeFix = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public interface IDeclareType
        {            
            [{{prefix}}MustInitialize{{suffix}}] string TestProp { get; set; }
        }
        public class DeclareTypeBase
        {
            public virtual string TestProp { get; set; }
        }
        public class DeclareTypeBase2: DeclareTypeBase
        {
            public override string TestProp { get; set; }
        }
        public class DeclareType : DeclareTypeBase2, IDeclareType
        {
            [MustInitialize]
            public override string TestProp { get => base.TestProp; set => base.TestProp = value; }
        }
        """;

        await VerifyCodeFixAsync(test, codeFix);
    }

    [Test]
    public async Task Test_Works_WithAbstract([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public interface IDeclareType
        {            
            [{{prefix}}MustInitialize{{suffix}}] string TestProp { get; set; }
        }
        public abstract class DeclareTypeBase
        {
            public abstract string TestProp { get; set; }
        }
        public abstract class [|DeclareType|] : DeclareTypeBase, IDeclareType
        {
        }
        """;

        var codeFix = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public interface IDeclareType
        {            
            [{{prefix}}MustInitialize{{suffix}}] string TestProp { get; set; }
        }
        public abstract class DeclareTypeBase
        {
            public abstract string TestProp { get; set; }
        }
        public abstract class DeclareType : DeclareTypeBase, IDeclareType
        {
            [MustInitialize]
            public override string TestProp { get; set; }
        }
        """;

        await VerifyCodeFixAsync(test, codeFix);
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
        public interface IDeclareType2
        {
            string TestProp { get; set; }
        }
        public interface IDeclareTypeSub : IDeclareType, IDeclareType2
        {
        }
        """;

        await VerifyAnalyzerAsync(test);
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
        public class DeclareTypeBase
        {
            public string TestProp { get; set; }
        }
        public class [|DeclareType|] : DeclareTypeBase, IDeclareTypeSub
        {
        }
        """;

        var fixCode = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public interface IDeclareType
        {            
            [{{prefix}}MustInitialize{{suffix}}] string TestProp { get; set; }
        }
        public interface IDeclareTypeSub : IDeclareType {}
        public class DeclareTypeBase
        {
            public string TestProp { get; set; }
        }
        public class DeclareType : DeclareTypeBase, IDeclareTypeSub
        {
            [MustInitialize]
            public new string TestProp { get => base.TestProp; set => base.TestProp = value; }
        }
        """;

        await VerifyCodeFixAsync(test, fixCode);
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
        public class DeclareTypeBase: IOther
        {
            public string TestProp { get; set; }
            [Test] public string TestPropWithAttributesSingleLine { get; set; }
            [Test]
            public virtual string TestPropWithAttributesMultiLine { get; set; }
            public string TestOtherProp { get; set; }
            public string TestOtherField;
        }
        public class [|[|[|DeclareType|]|]|] : DeclareTypeBase, IDeclareType, IOther
        {
            public string OtherMethod() => "Test";
            public string TestingOtherProp => "Test";            
            public string OtherMethod2() => "Test";
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
        public class DeclareTypeBase: IOther
        {
            public string TestProp { get; set; }
            [Test] public string TestPropWithAttributesSingleLine { get; set; }
            [Test]
            public virtual string TestPropWithAttributesMultiLine { get; set; }
            public string TestOtherProp { get; set; }
            public string TestOtherField;
        }
        public class DeclareType : DeclareTypeBase, IDeclareType, IOther
        {
            public string OtherMethod() => "Test";
            public string TestingOtherProp => "Test";            
            public string OtherMethod2() => "Test";

            [MustInitialize]
            public new string TestProp { get => base.TestProp; set => base.TestProp = value; }
            [Test]
            [MustInitialize]
            public new string TestPropWithAttributesSingleLine { get => base.TestPropWithAttributesSingleLine; set => base.TestPropWithAttributesSingleLine = value; }
            [Test]
            [MustInitialize]
            public override string TestPropWithAttributesMultiLine { get => base.TestPropWithAttributesMultiLine; set => base.TestPropWithAttributesMultiLine = value; }
        }
        """;

        await VerifyCodeFixAsync(test, fixCode);
    }
}

