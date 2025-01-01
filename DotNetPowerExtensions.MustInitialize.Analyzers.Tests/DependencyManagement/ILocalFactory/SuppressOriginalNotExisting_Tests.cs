using DotNetPowerExtensions.Analyzers.Tests;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.CodeAnalysis.Testing;
using SequelPay.DotNetPowerExtensions.Analyzers.DependencyManagement.ILocalFactory.Analyzers;
using System.Reflection;
using Microsoft.CodeAnalysis.Diagnostics;
using System;

namespace DotNetPowerExtensions.MustInitialize.Analyzers.Tests.DependencyManagement.ILocalFactory;

internal class SuppressOriginalNotExisting_Tests : AnalyzerVerifierBase<SuppressOriginalNotExisting>
{
    [Test]
    public async Task Test_Works([ValueSource(nameof(Prefixes))] string prefix)
    {
        var test = $$"""
        public class DeclareType
        {
        }

        class Program { void Main() =>
            (null as {{prefix}}ILocalFactory<DeclareType>).Create(new
            {
                [|TestProp|] = 10,
                [|TestGeneralName|] = "",
                [|TestField|] = true
            }); }
        """;

        await VerifySuppressorAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_WarnsWhenOtherMightRequire([Values(nameof(Attribute), "")] string suffix)
    {
        var test = $$"""
        [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple=true)]
        public class MightRequireAttribute<TType> : System.Attribute { public MightRequireAttribute(string name){} }

        [MightRequire{{suffix}}<int>("TestMightRequire")]
        public class DeclareType
        {
        }

        class Program { void Main() =>
            (null as ILocalFactory<DeclareType>).Create(new
            {
                [|TestMightRequire|] = 10,
            }); }
        """;

        await VerifySuppressorAsync(test).ConfigureAwait(false);
    }


    [Test]
    public async Task Test_DoesNotWarnWhenHasMightRequire([ValueSource(nameof(Prefixes))] string prefix, [Values(nameof(Attribute), "")] string suffix)
    {
        var test = $$"""
        [{{prefix}}MightRequire{{suffix}}<int>("TestMightRequire")]
        public class DeclareType
        {
        }

        class Program { void Main() =>
            (null as ILocalFactory<DeclareType>).Create(new
            {
                TestMightRequire = 10,
            }); }
        """;

        await VerifySuppressorAsync(test).ConfigureAwait(false);
    }

    private class CSharpAnalyzerWithSuppressorTest: CSharpAnalyzerTest<SuppressOriginalNotExisting, NUnitVerifier>
    {
        protected override IEnumerable<DiagnosticAnalyzer> GetDiagnosticAnalyzers()
        {
            yield return new OriginalNotExisting();
            yield return new SuppressOriginalNotExisting();
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035:Do not use APIs banned for analyzers", Justification = "Just a test, and we depend on it")]
    public Task VerifySuppressorAsync(string source, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerWithSuppressorTest
        {
            TestCode = NamespacePart + source,
        };

        const string NamespaceString = $"SequelPay.{nameof(DotNetPowerExtensions)}";

        test.TestState.AdditionalReferences.Add(typeof(SuppressOriginalNotExisting).Assembly);
        test.TestState.AdditionalReferences.Add(typeof(OriginalNotExisting).Assembly);

        var assemblies = typeof(SuppressOriginalNotExisting).Assembly.GetReferencedAssemblies()
                            .Where(a => a.FullName?.StartsWith(NamespaceString, StringComparison.Ordinal) == true)
                        .Union(typeof(OriginalNotExisting).Assembly.GetReferencedAssemblies()
                            .Where(a => a.FullName?.StartsWith(NamespaceString, StringComparison.Ordinal) == true));

        foreach (var assembly in assemblies)
        {
            var asm = Assembly.Load(assembly.FullName);
            if (!string.IsNullOrWhiteSpace(asm.Location))
            {
                test.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(asm.Location));
            }
        }

        test.ExpectedDiagnostics.AddRange(expected);

        return test.RunAsync(CancellationToken.None);
    }
}
