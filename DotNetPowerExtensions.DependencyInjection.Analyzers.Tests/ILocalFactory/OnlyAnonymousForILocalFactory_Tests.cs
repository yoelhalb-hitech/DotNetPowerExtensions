
namespace DotNetPowerExtensions.Analyzers.Tests.DependencyManagement.ILocalFactory;

internal class OnlyAnonymousForILocalFactory_Tests : AnalyzerVerifierBase<OnlyAnonymousForILocalFactory>
{
    [Test]
    public async Task Test_DoesNotWarnWhenNoArgs()
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

        class Program { void Main() => (null as ILocalFactory<DeclareType>).Create(); }
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
            public string TestProp { get; set; }
            public AppDomain TestGeneralName { get; set; }
            public List<(string, int)> TestField;
        }

        class Program { void Main() => (null as ILocalFactory<DeclareType>).Create([|new DeclareType{}|]); }
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
            public string TestProp { get; set; }
            public AppDomain TestGeneralName { get; set; }
            public List<(string, int)> TestField;
        }

        class Program { void Main() => (null as ILocalFactory<DeclareType>).Create([|new DeclareType()|]); }
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
            public string TestProp { get; set; }
            public AppDomain TestGeneralName { get; set; }
            public List<(string, int)> TestField;
        }
        public class Other{}
        class Program { void Main() => (null as ILocalFactory<DeclareType>).Create([|new Other{}|]); }
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }
}
