using DotNetPowerExtensions.Analyzers.DependencyManagement.LocalService.Analyzers;

namespace DotNetPowerExtensions.Analyzers.Tests.DependencyManagement.LocalService;

internal class OnlyAnonymousForRequiredMembersForLocalService_Tests : AnalyzerVerifierBase<OnlyAnonymousForRequiredMembersForLocalService>
{
    [Test]
    public async Task Test_DoesNotWarnWhenNoAttribute()
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

        class Program { void Main() => new LocalService<DeclareType>(null).Get(); }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarnWhenOtherMustInitialize([ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using System;
        using System.Collections.Generic;
        public class MustInitializeAttribute : Attribute {}
        public class DeclareType
        {
            [MustInitialize{{suffix}}] public string TestProp { get; set; }
            [MustInitialize{{suffix}}] public AppDomain TestGeneralName { get; set; }
            [MustInitialize{{suffix}}] public List<(string, int)> TestField;
        }

        class Program { void Main() => new LocalService<DeclareType>(null).Get(); }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using System;
        using System.Collections.Generic;
        public class DeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public AppDomain TestGeneralName { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public List<(string, int)> TestField;
        }

        class Program { void Main() => new LocalService<DeclareType>(null).Get([|new DeclareType{}|]); }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_WithNoInitializer([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using System;
        using System.Collections.Generic;
        public class DeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public AppDomain TestGeneralName { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public List<(string, int)> TestField;
        }

        class Program { void Main() => new LocalService<DeclareType>(null).Get([|new DeclareType()|]); }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_WhenOtherClass([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        using System;
        using System.Collections.Generic;
        public class DeclareType
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public AppDomain TestGeneralName { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public List<(string, int)> TestField;
        }
        public class Other{}
        class Program { void Main() => new LocalService<DeclareType>(null).Get([|new Other{}|]); }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }
}
