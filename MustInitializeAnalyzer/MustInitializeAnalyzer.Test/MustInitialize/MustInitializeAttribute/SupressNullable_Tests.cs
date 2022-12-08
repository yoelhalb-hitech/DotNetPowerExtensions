using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DotNetPowerExtensionsAnalyzer.Test.MustInitialize.MustInitializeAttribute;

internal class SupressNullable_Tests : NullableAnalyzerVerifierBase<SuppressNullableAnalyzer>
{
    public static string[] Suffixes = { "", "Attribute", "()", "Attribute()" };
    public static string[] Prefixes = {"", "DotNetPowerExtensions.MustInitialize.",
                                                                    "global::DotNetPowerExtensions.MustInitialize." };

    [Test]
    public async Task Test_Warns_WhenNoMustInitialize()
    {
        var code = """            
            public class Test
            {
                public string {|CS8618:TestStr|} { get; set; }
            }
        """;
        await NullableVerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Test_Warns_WhenOtherMustInitialize([ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public class MustInitializeAttribute : System.Attribute {}
        public class Test
        {
            [MustInitialize{{suffix}}] public string {|CS8618:TestStr|} { get; set; }
        }
        """;

        await NullableVerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenMustInitialize([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;
        
        public class Test
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField { get; set; }
        }
        """;

        await NullableVerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenMustInitialize_AndCtor([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;
        
        public class Test
        {
            public Test(string test){}
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField { get; set; }
        }
        """;

        await NullableVerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenMustInitialize_AndMultipleCtors([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;
        
        public class Test
        {
            public Test(string test){}
            public Test(){}
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField { get; set; }
        }
        """;

        await NullableVerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task Test_Works_WithOthers([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public interface IDeclareType
        {
            string OtherMethod();
            string TestingOtherProp { get; }
            [{{prefix}}MustInitialize{{suffix}}] string TestProp { get; set; }
            string OtherMethod2();
        }
        public interface IOther 
        {
            string TestOtherProp { get; set; }
            string TestThirdProp { get; set; }
        }
        public class DeclareTypeBase : IDeclareType, IOther 
        {
            string IDeclareType.OtherMethod() => "Test";
            public string TestingOtherProp => "Test";
            [{{prefix}}MustInitialize{{suffix}}] public virtual string TestProp { get; set; }
            public string OtherMethod2() => "Test";
            string IOther.{|CS8618:TestOtherProp|} { get; set; }
            public string {|CS8618:TestOtherField|};
            public string {|CS8618:TestThirdProp|} { get; set; }
        }
        public class DeclareTypeSub : DeclareTypeBase
        {
            [{{prefix}}MustInitialize{{suffix}}] public override string TestProp { get; set; }            
            [{{prefix}}MustInitialize{{suffix}}] public new string TestThirdProp { get; set; }
        }
        """;

        await NullableVerifyAnalyzerAsync(test);
    }
}
