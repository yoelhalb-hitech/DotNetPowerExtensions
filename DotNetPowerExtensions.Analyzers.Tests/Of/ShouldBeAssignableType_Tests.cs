
using DotNetPowerExtensions.Analyzers.Of;
using NUnit.Framework.Internal;

namespace DotNetPowerExtensions.Analyzers.Tests.Of;

internal class ShouldBeAssignableType_Tests : AnalyzerVerifierBase<ShouldBeAssignableType>
{
    [Test]
    public async Task Test_Works()
    {
        var test = $$"""
        public class DeclareType1{}
        public class DeclareType2{}
        public class DeclareType3{}
        public class DeclareType4{}

        class Program 
        { 
            void Main() 
            { 
                _ = new DotNetPowerExtensions.Of.Of<DeclareType1, DeclareType2>(new DeclareType1()).[|As<DeclareType3>|](); 
                _ = new DotNetPowerExtensions.Of.Of<DeclareType1, DeclareType2, DeclareType3>(new DeclareType1()).[|As<DeclareType4>|]();            
            }
        }
        
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenSame()
    {
        var test = $$"""
        public class DeclareType1{}
        public class DeclareType2{} 

        class Program { void Main() => new DotNetPowerExtensions.Of.Of<DeclareType1, DeclareType2>(new DeclareType1()).As<DeclareType1>(); }
        
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenAssignable()
    {
        var test = $$"""
        public interface IDeclareType {}
        public class DeclareType1 : IDeclareType {}
        public class DeclareType2 : IDeclareType {}

        class Program { void Main() => new DotNetPowerExtensions.Of.Of<DeclareType1, DeclareType2>(new DeclareType1()).As<IDeclareType>(); }
        
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Test]
    public async Task Test_DoesNotWarn_WhenOther()
    {
        var test = $$"""        
        public class DeclareType1 {}
        public class DeclareType2 {}
        public class DeclareType3 {}
        public class Of<T1, T2> { public T As<T>(){ throw new System.NotImplementedException(); } }

        class Program { void Main() => new Of<DeclareType1, DeclareType2>().As<DeclareType3>(); }
        
        """;

        await VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }
}
