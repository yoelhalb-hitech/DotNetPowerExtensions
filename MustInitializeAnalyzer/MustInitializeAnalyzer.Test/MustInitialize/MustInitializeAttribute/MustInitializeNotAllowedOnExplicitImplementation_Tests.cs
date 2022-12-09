using DotNetPowerExtensionsAnalyzer.MustInitialize.Analyzers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetPowerExtensionsAnalyzer.Test.MustInitialize.MustInitializeAttribute;

internal class MustInitializeNotAllowedOnExplicitImplementation_Tests : MustInitializeAnalyzerVerifierBase<MustInitializeNotAllowedOnExplicitImplementation>
{
    [Test]
    public async Task Test_DoesNotWarn_WhenNoMustInitialize()
    {
        var test = $$"""
        public interface IDeclareType
        {
            string TestProp { get; set; }
        }
        public class DeclareType : IDeclareType 
        {
            string IDeclareType.TestProp { get; set; }
        }
        """;

        await VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenOtherMustInitialize([ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class MustInitializeAttribute : System.Attribute {}
        public interface IDeclareType
        {
            string TestProp { get; set; }
        }
        public class DeclareType : IDeclareType 
        {
            [MustInitialize{{suffix}}] string IDeclareType.TestProp { get; set; }
        }
        """;

        await VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task Test_Works([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public interface IDeclareType
        {
            string TestProp { get; set; }
        }
        public class DeclareType : IDeclareType 
        {
            [[|{{prefix}}MustInitialize{{suffix}}|]] string IDeclareType.TestProp { get; set; }
        }
        """;

        await VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task Test_Works_WithSubclassedInterface([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public interface IDeclareType
        {
            string TestProp { get; set; }
        }
        public interface IDeclareTypeSub : IDeclareType {}
        public class DeclareType : IDeclareTypeSub
        {
            [[|{{prefix}}MustInitialize{{suffix}}|]] string IDeclareType.TestProp { get; set; }
        }
        """;

        await VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task Test_Works_WithInterfaceAndBase([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public interface IDeclareType
        {
            string TestProp { get; set; }
        }
        public class DeclareTypeBase {}
        public class DeclareType : DeclareTypeBase, IDeclareType 
        {
            [[|{{prefix}}MustInitialize{{suffix}}|]] string IDeclareType.TestProp { get; set; }
        }
        """;

        await VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task Test_Works_WithInterface_AndOtherInterfaces([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public interface IDeclareType
        {
            string OtherMethod();
            string TestingOtherProp { get; }
            string TestProp { get; set; }
            string OtherMethod2();
        }
        public interface IOther 
        {
            string TestOtherProp { get; set; }
        }
        public class DeclareType : IDeclareType, IOther 
        {
            string IDeclareType.OtherMethod() => "Test";
            public string TestingOtherProp => "Test";
            [[|{{prefix}}MustInitialize{{suffix}}|]] string IDeclareType.TestProp { get; set; }
            public string OtherMethod2() => "Test";
            string IOther.TestOtherProp { get; set; }
            public string TestOtherField;
        }
        """;

        await VerifyAnalyzerAsync(test);
    }
}
