using DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;

namespace DotNetPowerExtensions.Analyzers.Tests.MustInitialize.MustInitializeAttribute;

internal sealed class MustInitializeNotAllowedOnStatic_Tests : MustInitializeAnalyzerVerifierBase<MustInitializeNotSupportedOnStatic>
{
    [Test]
    public async Task Test_DoesNotWarn_WhenNoMustInitialize()
    {
        var test = $$"""
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

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenOtherMustInitialize([ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
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

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    #region Property

    [Test]
    public async Task Test_WorksOnProperty_WithClass([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class DeclareType
        {
            [[|{{prefix}}MustInitialize{{suffix}}|]] public static string TestProp { get; set; }
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_WorksOnProperty_WithInterface([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public interface IDeclareType
        {            
            [[|{{prefix}}MustInitialize{{suffix}}|]] public static string TestProp { get; set; }
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    #endregion

    #region Property And Field

    [Test]
    public async Task Test_WorksOnField_WithClass([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class DeclareType
        {
            [[|{{prefix}}MustInitialize{{suffix}}|]] public static string TestField;
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_WorksOnField_WithInterface([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public interface IDeclareType
        {          
            [[|{{prefix}}MustInitialize{{suffix}}|]] public static string TestProp { get; set; }
        }        
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    #endregion

    #region Property And Field

    [Test]
    public async Task Test_WorksOnPropertyAndField_WhenClass([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class DeclareType
        {            
            [[|{{prefix}}MustInitialize{{suffix}}|]] public static string TestProp { get; set; }
            [[|{{prefix}}MustInitialize{{suffix}}|]] public static string TestField;
        }        
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    #endregion
}
