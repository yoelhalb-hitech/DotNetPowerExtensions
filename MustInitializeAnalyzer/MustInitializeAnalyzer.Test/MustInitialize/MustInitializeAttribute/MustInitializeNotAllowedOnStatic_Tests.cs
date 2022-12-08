using DotNetPowerExtensionsAnalyzer.MustInitialize.Analyzers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetPowerExtensionsAnalyzer.Test.MustInitialize.MustInitializeAttribute;

internal class MustInitializeNotAllowedOnStatic_Tests : AnalyzerVerifierBase<MustInitializeNotSupportedOnStatic>
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
            static string TestProp { get; set; }
            static string TestField;
        }
        public class DeclareType
        {
            static string TestProp { get; set; }
            static string TestField;
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
            [MustInitialize{{suffix}}] static string TestProp { get; set; }
            [MustInitialize{{suffix}}] static string TestField;
        }
        public class DeclareType
        {
            static string TestProp { get; set; }
            static string TestField;
        }
        """;

        await VerifyAnalyzerAsync(test);
    }

    #region Property

    [Test]
    public async Task Test_WorksOnProperty_WithClass([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public class DeclareType
        {
            [[|{{prefix}}MustInitialize{{suffix}}|]] public static string TestProp { get; set; }
        }
        """;

        await VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task Test_WorksOnProperty_WithInterface([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public interface IDeclareType
        {            
            [[|{{prefix}}MustInitialize{{suffix}}|]] public static string TestProp { get; set; }
        }
        """;

        await VerifyAnalyzerAsync(test);
    }

    #endregion

    #region Property And Field

    [Test]
    public async Task Test_WorksOnField_WithClass([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;
       
        public class DeclareType
        {
            [[|{{prefix}}MustInitialize{{suffix}}|]] public static string TestField;
        }
        """;

        await VerifyAnalyzerAsync(test);
    }

    [Test]
    public async Task Test_WorksOnField_WithInterface([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public interface IDeclareType
        {          
            [[|{{prefix}}MustInitialize{{suffix}}|]] public static string TestProp { get; set; }
        }        
        """;

        await VerifyAnalyzerAsync(test);
    }

    #endregion

    #region Property And Field

    [Test]
    public async Task Test_WorksOnPropertyAndField_WhenClass([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;

        public class DeclareType
        {            
            [[|{{prefix}}MustInitialize{{suffix}}|]] public static string TestProp { get; set; }
            [[|{{prefix}}MustInitialize{{suffix}}|]] public static string TestField;
        }        
        """;

        await VerifyAnalyzerAsync(test);
    }

    #endregion
}
