
namespace DotNetPowerExtensions.Analyzers.Tests.DependencyManagement.ILocalFactory;

internal class TypeMismatchForILocalFactory_Tests: AnalyzerVerifierBase<TypeMismatchForILocalFactory>
{
    [Test]
    public async Task Test_HasCorrectMessage([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using System;
        using System.Collections.Generic;
        public class DeclareType
        {
            public string TestProp { get; set; }
            public AppDomain TestGeneralName { get; set; }
            public List<(string, int)> TestField;
        }

        class Program { void Main() =>
            (null as ILocalFactory<DeclareType>).Create(new
            {
                TestProp = 10,
                TestGeneralName = "",
                TestField = true
            }); }
        """;

        await VerifyAnalyzerAsync(test, new DiagnosticResult("DNPE0203", DiagnosticSeverity.Warning)
                                            .WithSpan(14, 20, 14, 22)
                                            .WithMessage("Type of Member 'TestProp' should be 'string'"),
                                        new DiagnosticResult("DNPE0203", DiagnosticSeverity.Warning)
                                            .WithSpan(15, 27, 15, 29)
                                            .WithMessage("Type of Member 'TestGeneralName' should be 'AppDomain'"),
                                        new DiagnosticResult("DNPE0203", DiagnosticSeverity.Warning)
                                            .WithSpan(16, 21, 16, 25)
                                            .WithMessage("Type of Member 'TestField' should be 'List<(string, int)>'"))
            .ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using System;
        using System.Collections.Generic;
        public class DeclareType
        {
            public string TestProp { get; set; }
            public AppDomain TestGeneralName { get; set; }
            public List<(string, int)> TestField;
        }

        class Program { void Main() =>
            (null as ILocalFactory<DeclareType>).Create(new
            {
                TestProp = [|10|],
                TestGeneralName = [|""|],
                TestField = [|true|]
            }); }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_WithSubClass([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using System;
        using System.Collections.Generic;
        public class DeclareType
        {
            public string TestProp { get; set; }
            public AppDomain TestGeneralName { get; set; }
            public List<(string, int)> TestField;
        }
        public class Subclass : DeclareType{}

        class Program { void Main() =>
            (null as ILocalFactory<Subclass>).Create(new
            {
                TestProp = [|10|],
                TestGeneralName = [|""|],
                TestField = [|true|]
            }); }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Subclass_HasCorrectMessage_OnlyOnceForEach([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using System;
        using System.Collections.Generic;
        public class DeclareType
        {
            public virtual string TestProp { get; set; }
            public AppDomain TestGeneralName { get; set; }
            public List<(string, int)> TestField;
        }
        public class Subclass : DeclareType
        {
            public override string TestProp { get; set; }
        }

        class Program { void Main() =>
            (null as ILocalFactory<Subclass>).Create(new
            {
                TestProp = 10,
                TestGeneralName = "",
                TestField = true
            }); }
        """;

        await VerifyAnalyzerAsync(test, new DiagnosticResult("DNPE0203", DiagnosticSeverity.Warning)
                                            .WithSpan(18, 20, 18, 22)
                                            .WithMessage("Type of Member 'TestProp' should be 'string'"),
                                        new DiagnosticResult("DNPE0203", DiagnosticSeverity.Warning)
                                            .WithSpan(19, 27, 19, 29)
                                            .WithMessage("Type of Member 'TestGeneralName' should be 'AppDomain'"),
                                        new DiagnosticResult("DNPE0203", DiagnosticSeverity.Warning)
                                            .WithSpan(20, 21, 20, 25)
                                            .WithMessage("Type of Member 'TestField' should be 'List<(string, int)>'"))
            .ConfigureAwait(false);
    }
}
