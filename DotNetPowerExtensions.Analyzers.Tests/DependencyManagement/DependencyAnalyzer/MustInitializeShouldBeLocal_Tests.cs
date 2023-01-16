using DotNetPowerExtensions.Analyzers.DependencyManagement.DependencyAttribute.Analyzers;
using SequelPay.DotNetPowerExtensions;

namespace DotNetPowerExtensions.Analyzers.Tests.DependencyManagement.DependencyAnalyzer;

internal class MustInitializeShouldBeLocal_Tests : AnalyzerVerifierBase<MustInitializeShouldBeLocal>
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1307:Specify StringComparison for clarity", Justification = "Older frameworks don't support it")]
    public static string[] Attributes => new string[] { nameof(SingletonAttribute), nameof(ScopedAttribute), nameof(TransientAttribute) }
                                                            .Select(n => n.Replace(nameof(Attribute), ""))
                                                            .ToArray();

    [Test]
    public async Task Test_Works([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix,
                                                                                            [ValueSource(nameof(Attributes))] string attribute)
    {
        var test = $$"""
        [[|{{prefix}}{{attribute}}{{suffix}}|]]
        public class TransientType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
        }
  
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1307:Specify StringComparison for clarity", Justification = "Older frameworks don't support it")]
    public async Task Test_Works_WithGeneric([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix,
                                                                                                [ValueSource(nameof(Attributes))] string attribute)
    {
        var genericSuffix = suffix.Contains("()") ? suffix.Replace("()", "<TransientType>()") : suffix + "<TransientType>";

        var test = $$"""
        [[|{{prefix}}{{attribute}}{{genericSuffix}}|]]
        public class TransientType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
        }
  
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_WithField([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix,
                                                                                                         [ValueSource(nameof(Attributes))] string attribute)
    {
        var test = $$"""
        [[|{{prefix}}{{attribute}}{{suffix}}|]]
        public class TransientType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
        }
  
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1307:Specify StringComparison for clarity", Justification = "Older frameworks don't support it")]
    public async Task Test_Works_WithField_AndGeneric([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix,
                                                                                                        [ValueSource(nameof(Attributes))] string attribute)
    {
        var genericSuffix = suffix.Contains("()") ? suffix.Replace("()", "<TransientType>()") : suffix + "<TransientType>";

        var test = $$"""
        [[|{{prefix}}{{attribute}}{{genericSuffix}}|]]
        public class TransientType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
        }
  
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnForLocal([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        [{{prefix}}Local{{suffix}}]
        public class TransientType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
        }
  
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenNoMustInitializeAttribute([ValueSource(nameof(Prefixes))] string prefix,
                                        [ValueSource(nameof(Suffixes))] string suffix, [ValueSource(nameof(Attributes))] string attribute)
    {
        var test = $$"""
        [{{prefix}}{{attribute}}{{suffix}}]
        public class TransientType
        {
            public string TestProp { get; set; }
        }
  
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenNoAttribute([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class TransientType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
        }
  
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnForOtherMustInitialize([ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        class MustInitializeAttribute : System.Attribute {}
        [Transient]
        public class TransientType
        {
            [MustInitialize{{suffix}}] public string TestProp { get; set; }
        }
  
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnForOtherAttribute([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix,
                                                                        [ValueSource(nameof(Attributes))] string attribute)
    {
        var test = $$"""
        class {{attribute}}Attribute : System.Attribute {}
        [{{attribute}}{{suffix}}]
        public class TransientType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
        }
  
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }
}
