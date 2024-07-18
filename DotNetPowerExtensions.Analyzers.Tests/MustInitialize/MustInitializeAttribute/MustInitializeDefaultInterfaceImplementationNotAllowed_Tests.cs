using DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;

namespace DotNetPowerExtensions.Analyzers.Tests.MustInitialize.MustInitializeAttribute;

internal sealed class MustInitializeDefaultInterfaceImplementationNotAllowed_Tests
                            : AnalyzerVerifierBase<MustInitializeNotAllowedOnDefaultInterfaceImplementation>
{
    [Test]
    public async Task Test_DoesNotWarn_WhenNoMustInitialize()
    {
        var test = $$"""
        public interface IDeclareType
        {
            string TestProp { get => "";set => _ = "";}
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
            [MustInitialize{{suffix}}] string TestProp { get => ""; set => _ = ""; }
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public interface IDeclareType
        {
            [[|{{prefix}}MustInitialize{{suffix}}|]] string TestProp { get => ""; set => _ = ""; }
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_WithBody([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public interface IDeclareType
        {
            [[|{{prefix}}MustInitialize{{suffix}}|]] string TestProp { get { return ""; } set { _ = "";} }
        }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }
}
