using DotNetPowerExtensions.Analyzers.MustInitialize.MustInitializeAttribute;

namespace DotNetPowerExtensions.Analyzers.Tests.MustInitialize.MustInitializeAttribute;

internal sealed class SupressNullable_Tests : NullableAnalyzerVerifierBase<SuppressNullableAnalyzer>
{
    [Test]
    public async Task Test_Warns_WhenNoMustInitialize()
    {
        var code = """
            public class Test
            {
                public string {|CS8618:TestStr|} { get; set; }
            }
        """;
        await NullableVerifyAnalyzerAsync(code).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Warns_WhenOtherMustInitialize([ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class MustInitializeAttribute : System.Attribute {}
        public class Test
        {
            [MustInitialize{{suffix}}] public string {|CS8618:TestStr|} { get; set; }
        }
        """;

        await NullableVerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenMustInitialize([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class Test
        {
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField { get; set; }
        }
        """;

        await NullableVerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenMustInitialize_AndCtor([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class Test
        {
            public Test(string test){}
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField { get; set; }
        }
        """;

        await NullableVerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenMustInitialize_AndMultipleCtors([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public class Test
        {
            public Test(string test){}
            public Test(){}
            [{{prefix}}MustInitialize{{suffix}}] public string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public string TestField { get; set; }
        }
        """;

        await NullableVerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_Works_WithOthers([ValueSource(nameof(Prefixes))] string prefix, [ValueSource(nameof(Suffixes))] string suffix)
    {
        var test = $$"""
        public interface IDeclareType
        {
            string OtherMethod();
            string TestingOtherProp { get; }
            [{{prefix}}MustInitialize{{suffix}}] string TestProp { get; set; }
            string OtherMethod2();
        }
        public interface IOther
        {
            string TestOtherProp { get; set; }
            string TestThirdProp { get; set; }
        }
        public class DeclareTypeBase : IDeclareType, IOther
        {
            string IDeclareType.OtherMethod() => "Test";
            public string TestingOtherProp => "Test";
            [{{prefix}}MustInitialize{{suffix}}] public virtual string TestProp { get; set; }
            public string OtherMethod2() => "Test";
            string IOther.{|CS8618:TestOtherProp|} { get; set; }
            public string {|CS8618:TestOtherField|};
            public string {|CS8618:TestThirdProp|} { get; set; }
        }
        public class DeclareTypeSub : DeclareTypeBase
        {
            [{{prefix}}MustInitialize{{suffix}}] public override string TestProp { get; set; }
            [{{prefix}}MustInitialize{{suffix}}] public new string TestThirdProp { get; set; }
        }
        """;

        await NullableVerifyAnalyzerAsync(test).ConfigureAwait(false);
    }
}
