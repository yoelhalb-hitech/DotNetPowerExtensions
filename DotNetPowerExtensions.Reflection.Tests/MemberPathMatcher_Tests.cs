using SequelPay.DotNetPowerExtensions.Reflection.Core.Models;
using SequelPay.DotNetPowerExtensions.Reflection.Core.Paths;

namespace DotNetPowerExtensions.Reflection.Tests;

internal class MemberPathMatcher_Tests
{
    interface Iface
    {
        string? TestProp { get; set; }
        event EventHandler? TestEvent;
        void Method1();
        void Method1(int i);
        int Method2(int i);
        int Method2(string i);
        T Method2<T>();
        T Method2<T>(T i);
        T2 Method2<T, T2>(T i);
        T2 Method2<T, T2>(T2 i);
        T2 Method2<T, T2, T3>(T2 i);
    }

    abstract class Inner : Iface
    {
        private string? TestProp { get; set; }
        string? Iface.TestProp { get; set; }
        protected readonly int TestField;
        public event EventHandler? TestEvent;
        public void Method1() { }
        public void Method1(int i) { }
        int Iface.Method2(int i) => default;
        protected abstract int Method2(int i);
        int Iface.Method2(string i) => default;
        protected abstract int Method2(string i);
        public abstract T Method2<T>();
        public abstract T Method2<T>(T i);
        public abstract T2 Method2<T, T2>(T i);
        public abstract T2 Method2<T, T2>(T2 i);
        public abstract T2 Method2<T, T2, T3>(T2 i);
    }

    const string minimalNS = $"{nameof(Tests)}";
    const string withMinimalNS = $"{minimalNS}.{nameof(MemberPathMatcher_Tests)}";
    const string withHalfNS = $"{nameof(Reflection)}.{withMinimalNS}";
    const string withFullNS = $"{nameof(DotNetPowerExtensions)}.{withHalfNS}";
    const string fullNS = $"{nameof(DotNetPowerExtensions)}.{nameof(Reflection)}.{minimalNS}";

    [Test]
    [TestCase(typeof(Iface), new string[]
    {
        $".{nameof(Iface.Method1)}()",
        $".{nameof(Iface.Method1)}(`1)",
        $".{nameof(Iface.Method2)}(int)",
        $".{nameof(Iface.Method2)}(string)",
        $".{nameof(Iface.Method2)}()",
        $".{nameof(Iface.Method2)}`1(`1)",
        $".{nameof(Iface.Method2)}`2(T)",
        $".{nameof(Iface.Method2)}`2(T2)",
        $".{nameof(Iface.Method2)}`3",
        $".{nameof(Iface.TestProp)}",
        $".{nameof(Iface.TestEvent)}",
    })]
    [TestCase(typeof(Inner), new string[]
    {
        $".{nameof(Iface.TestProp)}",
        $".:{nameof(Iface)}:{nameof(Iface.TestProp)}",
        $".TestField",
        $".{nameof(Iface.TestEvent)}",
        $".{nameof(Inner.Method1)}()",
        $".{nameof(Inner.Method1)}(`1)",
        $".{nameof(Inner.Method2)}(int)",
        $".:{nameof(Iface)}:{nameof(Inner.Method2)}(int)",
        $".{nameof(Inner.Method2)}(string)",
        $".:{nameof(Iface)}:{nameof(Inner.Method2)}(string)",
        $".{nameof(Inner.Method2)}()",
        $".{nameof(Inner.Method2)}`1(`1)",
        $".{nameof(Inner.Method2)}`2(T)",
        $".{nameof(Inner.Method2)}`2(T2)",
        $".{nameof(Inner.Method2)}`3",
    })]
    public void Test_GetMinimalPath(Type type, string[] expected)
    {
        var td = type.GetTypeDetailInfo() as ITypeDetailInfo;

        var matcher = Outer<TypeContainerCache>.MemberPathMatcher.GetMemberPathMatcher(td);

        var result = td.MethodDetails.Select(md => matcher.GetMinimalPath(md))
                .Concat(td.PropertyDetails.Select(pd => matcher.GetMinimalPath(pd)))
                .Concat(td.FieldDetails.Select(fd => matcher.GetMinimalPath(fd)))
                .Concat(td.EventDetails.Select(ed => matcher.GetMinimalPath(ed)))
                .Concat(td.ExplicitMethodDetails.Select(emd => matcher.GetMinimalPath(emd)))
                .Concat(td.ExplicitPropertyDetails.Select(epd => matcher.GetMinimalPath(epd)))
                .Concat(td.ExplicitEventDetails.Select(eed => matcher.GetMinimalPath(eed)))
                .ToArray();

        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    [TestCase(typeof(Iface), new string[]
    {
        $".{nameof(Iface.Method1)}()",
        $".{nameof(Iface.Method1)}(System.Int32)",
        $".{nameof(Iface.Method2)}(System.Int32)",
        $".{nameof(Iface.Method2)}(System.String)",
        $".{nameof(Iface.Method2)}`1()",
        $".{nameof(Iface.Method2)}`1(T)",
        $".{nameof(Iface.Method2)}`2(T)",
        $".{nameof(Iface.Method2)}`2(T2)",
        $".{nameof(Iface.Method2)}`3(T2)",
        $".{nameof(Iface.TestProp)}",
        $".{nameof(Iface.TestEvent)}",
    })]
    [TestCase(typeof(Inner), new string[]
    {
        $".{nameof(Iface.TestProp)}",
        $".:{withFullNS}+{nameof(Iface)}:" + nameof(Iface.TestProp),
        $".TestField",
        $".{nameof(Iface.TestEvent)}",
        $".{nameof(Inner.Method1)}()",
        $".{nameof(Inner.Method1)}(System.Int32)",
        $".{nameof(Inner.Method2)}(System.Int32)",
        $".:{withFullNS}+{nameof(Iface)}:{nameof(Inner.Method2)}(System.Int32)",
        $".{nameof(Inner.Method2)}(System.String)",
        $".:{withFullNS}+{nameof(Iface)}:{nameof(Inner.Method2)}(System.String)",
        $".{nameof(Inner.Method2)}`1()",
        $".{nameof(Inner.Method2)}`1(T)",
        $".{nameof(Inner.Method2)}`2(T)",
        $".{nameof(Inner.Method2)}`2(T2)",
        $".{nameof(Inner.Method2)}`3(T2)",
    })]
    public void Test_GetFullPath(Type type, string[] expected)
    {
        var td = type.GetTypeDetailInfo() as ITypeDetailInfo;

        var matcher = Outer<TypeContainerCache>.MemberPathMatcher.GetMemberPathMatcher(td);

        var result = td.MethodDetails.Select(md => matcher.GetFullPath(md))
                .Concat(td.PropertyDetails.Select(pd => matcher.GetFullPath(pd)))
                .Concat(td.FieldDetails.Select(fd => matcher.GetFullPath(fd)))
                .Concat(td.EventDetails.Select(ed => matcher.GetFullPath(ed)))
                .Concat(td.ExplicitMethodDetails.Select(emd => matcher.GetFullPath(emd)))
                .Concat(td.ExplicitPropertyDetails.Select(epd => matcher.GetFullPath(epd)))
                .Concat(td.ExplicitEventDetails.Select(eed => matcher.GetFullPath(eed)))
                .ToArray();

        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    [TestCase(typeof(Iface), $".{nameof(Iface.Method1)}()", ExpectedResult = $".{nameof(Iface.Method1)}()")]
    [TestCase(typeof(Iface), $".{nameof(Iface.Method1)}(System.Int32)", ExpectedResult = $".{nameof(Iface.Method1)}(`1)")]
    [TestCase(typeof(Iface), $".{nameof(Iface.Method2)}(System.Int32)", ExpectedResult = $".{nameof(Iface.Method2)}(int)")]
    [TestCase(typeof(Iface), $".{nameof(Iface.Method2)}(System.String)", ExpectedResult = $".{nameof(Iface.Method2)}(string)")]
    [TestCase(typeof(Iface), $".{nameof(Iface.Method2)}`1()", ExpectedResult = $".{nameof(Iface.Method2)}()")]
    [TestCase(typeof(Iface), $".{nameof(Iface.Method2)}`1(T)", ExpectedResult = $".{nameof(Iface.Method2)}`1(`1)")]
    [TestCase(typeof(Iface), $".{nameof(Iface.Method2)}`2(T)", ExpectedResult = $".{nameof(Iface.Method2)}`2(T)")]
    [TestCase(typeof(Iface), $".{nameof(Iface.Method2)}`2(T2)", ExpectedResult = $".{nameof(Iface.Method2)}`2(T2)")]
    [TestCase(typeof(Iface), $".{nameof(Iface.Method2)}`3(T2)", ExpectedResult = $".{nameof(Iface.Method2)}`3")]
    [TestCase(typeof(Iface), $".{nameof(Iface.TestProp)}", ExpectedResult = $".{nameof(Iface.TestProp)}")]
    [TestCase(typeof(Iface), $".{nameof(Iface.TestEvent)}", ExpectedResult = $".{nameof(Iface.TestEvent)}")]
    [TestCase(typeof(Inner), $".{nameof(Iface.TestProp)}", ExpectedResult = $".{nameof(Iface.TestProp)}")]
    [TestCase(typeof(Inner), $".:{withFullNS}+{nameof(Iface)}:" + nameof(Iface.TestProp), ExpectedResult = $".:{nameof(Iface)}:{nameof(Iface.TestProp)}")]
    [TestCase(typeof(Inner), $".TestField", ExpectedResult = $".TestField")]
    [TestCase(typeof(Inner), $".{nameof(Iface.TestEvent)}", ExpectedResult = $".{nameof(Iface.TestEvent)}")]
    [TestCase(typeof(Inner), $".{nameof(Inner.Method1)}()", ExpectedResult = $".{nameof(Inner.Method1)}()")]
    [TestCase(typeof(Inner), $".{nameof(Inner.Method1)}(System.Int32)", ExpectedResult = $".{nameof(Inner.Method1)}(`1)")]
    [TestCase(typeof(Inner), $".{nameof(Inner.Method2)}(System.Int32)", ExpectedResult = $".{nameof(Inner.Method2)}(int)")]
    [TestCase(typeof(Inner), $".:{withFullNS}+{nameof(Iface)}:{nameof(Inner.Method2)}(System.Int32)", ExpectedResult = $".:{nameof(Iface)}:{nameof(Inner.Method2)}(int)")]
    [TestCase(typeof(Inner), $".{nameof(Inner.Method2)}(System.String)", ExpectedResult = $".{nameof(Inner.Method2)}(string)")]
    [TestCase(typeof(Inner), $".:{withFullNS}+{nameof(Iface)}:{nameof(Inner.Method2)}(System.String)", ExpectedResult = $".:{nameof(Iface)}:{nameof(Inner.Method2)}(string)")]
    [TestCase(typeof(Inner), $".{nameof(Inner.Method2)}`1()", ExpectedResult = $".{nameof(Inner.Method2)}()")]
    [TestCase(typeof(Inner), $".{nameof(Inner.Method2)}`1(T)", ExpectedResult = $".{nameof(Inner.Method2)}`1(`1)")]
    [TestCase(typeof(Inner), $".{nameof(Inner.Method2)}`2(T)", ExpectedResult = $".{nameof(Inner.Method2)}`2(T)")]
    [TestCase(typeof(Inner), $".{nameof(Inner.Method2)}`2(T2)", ExpectedResult = $".{nameof(Inner.Method2)}`2(T2)")]
    [TestCase(typeof(Inner), $".{nameof(Inner.Method2)}`3(T2)", ExpectedResult = $".{nameof(Inner.Method2)}`3")]

    //TODO...
    //Test on multiple steps and long paths, possibly including multiple generics and multiple invocations and explicit in the middle
    //Test on args
    //We need to handle the generic concretee args and return It and test it
    //Test when mutiple explicit interfaces have the same name but not the same method name as well as the same method name
    public string Test_ParsePath_Full_ToMinimal(Type type, string fullPath)
    {
        var td = type.GetTypeDetailInfo() as ITypeDetailInfo;

        var matcher = Outer<TypeContainerCache>.MemberPathMatcher.GetMemberPathMatcher(td);

        var parsed = matcher.ParsePath(fullPath);

        return parsed.First().First switch
        {
            IMethodDetail method => matcher.GetMinimalPath(method),
            IPropertyDetail prop => matcher.GetMinimalPath(prop),
            IEventDetail evt => matcher.GetMinimalPath(evt),
            IFieldDetail field => matcher.GetMinimalPath(field),
            _ => throw new InvalidCastException(),
        };
    }

    [Test]
    [TestCase(typeof(Iface), $".{nameof(Iface.Method1)}()", ExpectedResult = $".{nameof(Iface.Method1)}()")]
    [TestCase(typeof(Iface), $".{nameof(Iface.Method1)}(`1)", ExpectedResult = $".{nameof(Iface.Method1)}(System.Int32)")]
    [TestCase(typeof(Iface), $".{nameof(Iface.Method2)}(int)", ExpectedResult = $".{nameof(Iface.Method2)}(System.Int32)")]
    [TestCase(typeof(Iface), $".{nameof(Iface.Method2)}(string)", ExpectedResult = $".{nameof(Iface.Method2)}(System.String)")]
    [TestCase(typeof(Iface), $".{nameof(Iface.Method2)}()", ExpectedResult = $".{nameof(Iface.Method2)}`1()")]
    [TestCase(typeof(Iface), $".{nameof(Iface.Method2)}`1(`1)", ExpectedResult = $".{nameof(Iface.Method2)}`1(T)")]
    [TestCase(typeof(Iface), $".{nameof(Iface.Method2)}`2(T)", ExpectedResult = $".{nameof(Iface.Method2)}`2(T)")]
    [TestCase(typeof(Iface), $".{nameof(Iface.Method2)}`2(T2)", ExpectedResult = $".{nameof(Iface.Method2)}`2(T2)")]
    [TestCase(typeof(Iface), $".{nameof(Iface.Method2)}`3", ExpectedResult = $".{nameof(Iface.Method2)}`3(T2)")]
    [TestCase(typeof(Iface), $".{nameof(Iface.TestProp)}", ExpectedResult = $".{nameof(Iface.TestProp)}")]
    [TestCase(typeof(Iface), $".{nameof(Iface.TestEvent)}", ExpectedResult = $".{nameof(Iface.TestEvent)}")]
    [TestCase(typeof(Inner), $".{nameof(Iface.TestProp)}", ExpectedResult = $".{nameof(Iface.TestProp)}")]
    [TestCase(typeof(Inner), $".:{nameof(Iface)}:{nameof(Iface.TestProp)}", ExpectedResult = $".:{withFullNS}+{nameof(Iface)}:" + nameof(Iface.TestProp))]
    [TestCase(typeof(Inner), $".TestField", ExpectedResult = $".TestField")]
    [TestCase(typeof(Inner), $".{nameof(Iface.TestEvent)}", ExpectedResult = $".{nameof(Iface.TestEvent)}")]
    [TestCase(typeof(Inner), $".{nameof(Inner.Method1)}()", ExpectedResult = $".{nameof(Inner.Method1)}()")]
    [TestCase(typeof(Inner), $".{nameof(Inner.Method1)}(`1)", ExpectedResult = $".{nameof(Inner.Method1)}(System.Int32)")]
    [TestCase(typeof(Inner), $".{nameof(Inner.Method2)}(int)", ExpectedResult = $".{nameof(Inner.Method2)}(System.Int32)")]
    [TestCase(typeof(Inner), $".:{nameof(Iface)}:{nameof(Inner.Method2)}(int)", ExpectedResult = $".:{withFullNS}+{nameof(Iface)}:{nameof(Inner.Method2)}(System.Int32)")]
    [TestCase(typeof(Inner), $".{nameof(Inner.Method2)}(string)", ExpectedResult = $".{nameof(Inner.Method2)}(System.String)")]
    [TestCase(typeof(Inner), $".:{nameof(Iface)}:{nameof(Inner.Method2)}(string)", ExpectedResult = $".:{withFullNS}+{nameof(Iface)}:{nameof(Inner.Method2)}(System.String)")]
    [TestCase(typeof(Inner), $".{nameof(Inner.Method2)}()", ExpectedResult = $".{nameof(Inner.Method2)}`1()")]
    [TestCase(typeof(Inner), $".{nameof(Inner.Method2)}`1(`1)", ExpectedResult = $".{nameof(Inner.Method2)}`1(T)")]
    [TestCase(typeof(Inner), $".{nameof(Inner.Method2)}`2(T)", ExpectedResult = $".{nameof(Inner.Method2)}`2(T)")]
    [TestCase(typeof(Inner), $".{nameof(Inner.Method2)}`2(T2)", ExpectedResult = $".{nameof(Inner.Method2)}`2(T2)")]
    [TestCase(typeof(Inner), $".{nameof(Inner.Method2)}`3", ExpectedResult = $".{nameof(Inner.Method2)}`3(T2)")]
    public string Test_ParsePath_Minimal_ToFull(Type type, string fullPath)
    {
        var td = type.GetTypeDetailInfo() as ITypeDetailInfo;

        var matcher = Outer<TypeContainerCache>.MemberPathMatcher.GetMemberPathMatcher(td);

        var parsed = matcher.ParsePath(fullPath);

        return parsed.First().First switch
        {
            IMethodDetail method => matcher.GetFullPath(method),
            IPropertyDetail prop => matcher.GetFullPath(prop),
            IEventDetail evt => matcher.GetFullPath(evt),
            IFieldDetail field => matcher.GetFullPath(field),
            _ => throw new InvalidCastException(),
        };
    }
}
