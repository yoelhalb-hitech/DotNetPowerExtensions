using SequelPay.DotNetPowerExtensions.Analyzers.Union;

namespace DotNetPowerExtensions.Analyzers.Tests.Union;

internal sealed class SupressNullable_Tests : NullableAnalyzerVerifierBase<SuppressNullableAnalyzer>
{
    [Test]
    public async Task Test_Warns_WhenOther()
    {
        var test = $$"""
            public class DeclareType1 {}
            public class DeclareType2 {}
            public class DeclareType3 {}
            public class Union<T1, T2> { public T? As<T>(){ throw new System.NotImplementedException(); } }

            class Program { void Main(){ DeclareType3 d = {|CS8600:new Union<DeclareType1, DeclareType2>().As<DeclareType3>()|}; } }

        """;

        await NullableVerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenAs()
    {
        var test = $$"""
        public class DeclareType1{}
        public class DeclareType2{}

        class Program { void Main(){ DeclareType1 d =  new Union<DeclareType1, DeclareType2>(new DeclareType1()).As<DeclareType1>(); } }

        """;

        await NullableVerifyAnalyzerAsync(test).ConfigureAwait(false);
    }
}
