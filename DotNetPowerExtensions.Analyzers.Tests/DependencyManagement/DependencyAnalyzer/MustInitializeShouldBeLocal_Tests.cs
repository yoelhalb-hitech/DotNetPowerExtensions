using DotNetPowerExtensions.Analyzers.DependencyManagement.DependencyAttribute.Analyzers;
using DotNetPowerExtensions.Analyzers.Tests.MustInitialize;
using DotNetPowerExtensions.DependencyManagement;

namespace DotNetPowerExtensions.Analyzers.Tests.DependencyManagement.DependencyAnalyzer;

internal class MustInitializeShouldBeLocal_Tests : MustInitializeAnalyzerVerifierBase<MustInitializeShouldBeLocal>
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1307:Specify StringComparison for clarity", Justification = "Older frameworks don't support it")]
    public static string[] Attributes => new string[] { nameof(SingletonAttribute), nameof(ScopedAttribute), nameof(TransientAttribute) }
                                                            .Select(n => n.Replace(nameof(Attribute), ""))
                                                            .ToArray();
    const string DependencyNamespaceString = $"{nameof(DotNetPowerExtensions)}.{nameof(DotNetPowerExtensions.DependencyManagement)}";
    public static string[] DepnedncyPrefixes = {"", DependencyNamespaceString + ".",
                                                                    $"global::{DependencyNamespaceString}." };

    [Test]
    public async Task Test_Works([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix,
                [ValueSource(nameof(DepnedncyPrefixes))] string dependencyPrefix, [ValueSource(nameof(Attributes))] string attribute)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;
        using DotNetPowerExtensions.DependencyManagement;
        [[|{{dependencyPrefix}}{{attribute}}{{suffix}}|]]
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
            [ValueSource(nameof(DepnedncyPrefixes))] string dependencyPrefix, [ValueSource(nameof(Attributes))] string attribute)
    {
        var genericSuffix = suffix.Contains("()") ? suffix.Replace("()", "<TransientType>()") : suffix + "<TransientType>";

        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;
        using DotNetPowerExtensions.DependencyManagement;
        [[|{{dependencyPrefix}}{{attribute}}{{genericSuffix}}|]]
        public class TransientType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
        }
  
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_WithField([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix,
            [ValueSource(nameof(DepnedncyPrefixes))] string dependencyPrefix, [ValueSource(nameof(Attributes))] string attribute)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;
        using DotNetPowerExtensions.DependencyManagement;
        [[|{{dependencyPrefix}}{{attribute}}{{suffix}}|]]
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
        [ValueSource(nameof(DepnedncyPrefixes))] string dependencyPrefix, [ValueSource(nameof(Attributes))] string attribute)
    {
        var genericSuffix = suffix.Contains("()") ? suffix.Replace("()", "<TransientType>()") : suffix + "<TransientType>";

        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;
        using DotNetPowerExtensions.DependencyManagement;
        [[|{{dependencyPrefix}}{{attribute}}{{genericSuffix}}|]]
        public class TransientType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestField;
        }
  
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnForLocal([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix,
                                                                                        [ValueSource(nameof(DepnedncyPrefixes))] string dependencyPrefix)
    {
        var test = $$"""
        using DotNetPowerExtensions.MustInitialize;
        using DotNetPowerExtensions.DependencyManagement;
        [{{dependencyPrefix}}Local{{suffix}}]
        public class TransientType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
        }
  
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenNoMustInitializeAttribute([ValueSource(nameof(Suffixes))] string suffix,
            [ValueSource(nameof(DepnedncyPrefixes))] string dependencyPrefix, [ValueSource(nameof(Attributes))] string attribute)
    {
        var test = $$"""        
        using DotNetPowerExtensions.DependencyManagement;
        [{{dependencyPrefix}}{{attribute}}{{suffix}}]
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
        using DotNetPowerExtensions.MustInitialize;       
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
        using DotNetPowerExtensions.DependencyManagement;
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
        using DotNetPowerExtensions.MustInitialize;
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
