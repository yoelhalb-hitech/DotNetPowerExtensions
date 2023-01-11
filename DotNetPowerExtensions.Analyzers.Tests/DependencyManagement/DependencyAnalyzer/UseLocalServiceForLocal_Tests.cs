using DotNetPowerExtensions.Analyzers.DependencyManagement.DependencyAttribute.Analyzers;
using DotNetPowerExtensions.DependencyManagement;

namespace DotNetPowerExtensions.Analyzers.Tests.DependencyManagement.DependencyAnalyzer;

internal class UseLocalServiceForLocal_Tests : AnalyzerVerifierBase<UseLocalServiceForLocal>
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1307:Specify StringComparison for clarity", Justification = "Older frameworks don't support it")]
    public static string[] Attributes => new string[] { nameof(SingletonAttribute), nameof(ScopedAttribute), nameof(TransientAttribute) }
                                                        .Select(n => n.Replace(nameof(Attribute), ""))
                                                        .ToArray();
    public static string[] Suffixes = { "", nameof(Attribute), "()", $"{nameof(Attribute)}()" };
    const string DependencyNamespaceString = $"{nameof(DotNetPowerExtensions)}.{nameof(DotNetPowerExtensions.DependencyManagement)}";
    public static string[] DepnedncyPrefixes = {"", DependencyNamespaceString + ".",
                                                                    $"global::{DependencyNamespaceString}." };

    [Test]
    public async Task Test_Works([ValueSource(nameof(DepnedncyPrefixes))] string dependencyPrefix, [ValueSource(nameof(Attributes))] string attribute,
                                                                                                            [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.DependencyManagement;
        [{{dependencyPrefix}}{{attribute}}{{suffix}}]
        public class TransientType
        {
            public TransientType([|LocalType t|]){}
            public string TestProp { get; set; }            
        }

        [{{dependencyPrefix}}Local{{suffix}}]
        public class LocalType {}
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1307:Specify StringComparison for clarity", Justification = "Older frameworks don't support it")]
    public async Task Test_Works_WithGenerics([ValueSource(nameof(DepnedncyPrefixes))] string dependencyPrefix,
                                                [ValueSource(nameof(Attributes))] string attribute, [ValueSource(nameof(Suffixes))] string suffix)
    {

        var genericSuffix = suffix.Contains("()") ? suffix.Replace("()", "<TransientType>()") : suffix + "<TransientType>";

        var test = $$"""
        using DotNetPowerExtensions.DependencyManagement;
        [{{dependencyPrefix}}{{attribute}}{{genericSuffix}}]
        public class TransientType
        {
            public TransientType([|LocalType t|]){}
            public string TestProp { get; set; }            
        }

        [{{dependencyPrefix}}Local{{suffix}}]
        public class LocalType {}
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenNoAttribute()
    {
        var test = $$"""        
        public class TransientType
        {
            public TransientType(LocalType t){}
            public string TestProp { get; set; }            
        }
        
        public class LocalType {}
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenOtherLocal([ValueSource(nameof(DepnedncyPrefixes))] string dependencyPrefix,
                                                [ValueSource(nameof(Attributes))] string attribute, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using DotNetPowerExtensions.DependencyManagement;
        [{{dependencyPrefix}}{{attribute}}{{suffix}}]
        public class TransientType
        {
            public TransientType(LocalType t){}
            public string TestProp { get; set; }            
        }
        
        public class LocalAttribute : System.Attribute {}
        [Local{{suffix}}]
        public class LocalType {}
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }
}
