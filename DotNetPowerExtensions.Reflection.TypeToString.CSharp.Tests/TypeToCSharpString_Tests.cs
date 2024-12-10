
namespace Outer
{
    namespace Inner
    {
        class OuterGeneric<TOuter>
        {
            public class InnerGeneric<TInner> { }
            public class InnerTwoGeneric<TInner1, TInner2> { }
            public class InnerNonGeneric { }
        }
        class OuterNonGeneric
        {
            public class InnerGeneric<TInner> { }
            public class InnerNonGeneric { }
        }
    }
}

namespace DotNetPowerExtensions.Reflection.Tests
{
    using Outer.Inner;
    using SequelPay.DotNetPowerExtensions.Reflection;

    public class TypeToCSharpString_Tests
    {
        [Test]
        [TestCase(typeof(int), ExpectedResult = "int")]
        [TestCase(typeof(string), ExpectedResult = "string")]
        [TestCase(typeof(object), ExpectedResult = "object")]
        [TestCase(typeof(char), ExpectedResult = "char")]
        [TestCase(typeof(short), ExpectedResult = "short")]
        [TestCase(typeof(ushort), ExpectedResult = "ushort")]
        [TestCase(typeof(void), ExpectedResult = "void")]
        [TestCase(typeof(Array), ExpectedResult = "Array")]
        [TestCase(typeof(Task), ExpectedResult = "Task")]
        [TestCase(typeof(Task<>), ExpectedResult = "Task<TResult>")]
        [TestCase(typeof(Task<int>), ExpectedResult = "Task<int>")]
        [TestCase(typeof(OuterGeneric<>), ExpectedResult = "OuterGeneric<TOuter>")]
        [TestCase(typeof(OuterGeneric<Task>), ExpectedResult = "OuterGeneric<Task>")]
        [TestCase(typeof(OuterGeneric<Task<int>>), ExpectedResult = "OuterGeneric<Task<int>>")]
        [TestCase(typeof(OuterGeneric<>.InnerGeneric<>), ExpectedResult = "InnerGeneric<TInner>")]
        [TestCase(typeof(OuterGeneric<Task>.InnerGeneric<Array>), ExpectedResult = "InnerGeneric<Array>")]
        [TestCase(typeof(OuterGeneric<Task>.InnerGeneric<Task<string>>), ExpectedResult = "InnerGeneric<Task<string>>")]
        [TestCase(typeof(OuterGeneric<Task<Task<Array>>>.InnerGeneric<Task<Task<string>>>), ExpectedResult = "InnerGeneric<Task<Task<string>>>")]
        [TestCase(typeof(OuterGeneric<>.InnerTwoGeneric<,>), ExpectedResult = "InnerTwoGeneric<TInner1,TInner2>")]
        [TestCase(typeof(OuterGeneric<Task>.InnerTwoGeneric<Task,Task<float>>), ExpectedResult = "InnerTwoGeneric<Task,Task<float>>")]
        [TestCase(typeof(OuterGeneric<>.InnerNonGeneric), ExpectedResult = "InnerNonGeneric")]
        [TestCase(typeof(OuterNonGeneric), ExpectedResult = "OuterNonGeneric")]
        [TestCase(typeof(OuterNonGeneric.InnerGeneric<>), ExpectedResult = "InnerGeneric<TInner>")]
        [TestCase(typeof(OuterNonGeneric.InnerNonGeneric), ExpectedResult = "InnerNonGeneric")]
        [TestCase(typeof(ValueTuple<,>), ExpectedResult = "ValueTuple<T1,T2>")]
        [TestCase(typeof(ValueTuple<int,string>), ExpectedResult = "(int,string)")]
        public string ToGenericTypeString_Test_NonFullName_NonEmptyOnStub(Type type)
            => type.ToCSharpTypeString(false, false, null);

        [Test]
        [TestCase(typeof(int), ExpectedResult = "int")]
        [TestCase(typeof(string), ExpectedResult = "string")]
        [TestCase(typeof(object), ExpectedResult = "object")]
        [TestCase(typeof(char), ExpectedResult = "char")]
        [TestCase(typeof(short), ExpectedResult = "short")]
        [TestCase(typeof(ushort), ExpectedResult = "ushort")]
        [TestCase(typeof(void), ExpectedResult = "void")]
        [TestCase(typeof(Array), ExpectedResult = "Array")]
        [TestCase(typeof(Task), ExpectedResult = "Task")]
        [TestCase(typeof(Task<>), ExpectedResult = "Task<>")]
        [TestCase(typeof(Task<int>), ExpectedResult = "Task<int>")]
        [TestCase(typeof(OuterGeneric<>), ExpectedResult = "OuterGeneric<>")]
        [TestCase(typeof(OuterGeneric<Task>), ExpectedResult = "OuterGeneric<Task>")]
        [TestCase(typeof(OuterGeneric<Task<int>>), ExpectedResult = "OuterGeneric<Task<int>>")]
        [TestCase(typeof(OuterGeneric<>.InnerGeneric<>), ExpectedResult = "InnerGeneric<>")]
        [TestCase(typeof(OuterGeneric<Task>.InnerGeneric<Array>), ExpectedResult = "InnerGeneric<Array>")]
        [TestCase(typeof(OuterGeneric<Task>.InnerGeneric<Task<string>>), ExpectedResult = "InnerGeneric<Task<string>>")]
        [TestCase(typeof(OuterGeneric<Task<Task<Array>>>.InnerGeneric<Task<Task<string>>>), ExpectedResult = "InnerGeneric<Task<Task<string>>>")]
        [TestCase(typeof(OuterGeneric<>.InnerTwoGeneric<,>), ExpectedResult = "InnerTwoGeneric<,>")]
        [TestCase(typeof(OuterGeneric<Task>.InnerTwoGeneric<Task, Task<float>>), ExpectedResult = "InnerTwoGeneric<Task,Task<float>>")]
        [TestCase(typeof(OuterGeneric<>.InnerNonGeneric), ExpectedResult = "InnerNonGeneric")]
        [TestCase(typeof(OuterNonGeneric), ExpectedResult = "OuterNonGeneric")]
        [TestCase(typeof(OuterNonGeneric.InnerGeneric<>), ExpectedResult = "InnerGeneric<>")]
        [TestCase(typeof(OuterNonGeneric.InnerNonGeneric), ExpectedResult = "InnerNonGeneric")]
        [TestCase(typeof(ValueTuple<,>), ExpectedResult = "ValueTuple<,>")]
        [TestCase(typeof(ValueTuple<int, string>), ExpectedResult = "(int,string)")]
        public string ToGenericTypeString_Test_NonFullName_EmptyOnStub(Type type)
            => type.ToCSharpTypeString(false, true, null);

        [Test]
        [TestCase(typeof(int), ExpectedResult = "System.Int32")]
        [TestCase(typeof(string), ExpectedResult = "System.String")]
        [TestCase(typeof(object), ExpectedResult = "System.Object")]
        [TestCase(typeof(char), ExpectedResult = "System.Char")]
        [TestCase(typeof(short), ExpectedResult = "System.Int16")]
        [TestCase(typeof(ushort), ExpectedResult = "System.UInt16")]
        [TestCase(typeof(Array), ExpectedResult = "System.Array")]
        [TestCase(typeof(Task), ExpectedResult = "System.Threading.Tasks.Task")]
        [TestCase(typeof(Task<>), ExpectedResult = "System.Threading.Tasks.Task<TResult>")]
        [TestCase(typeof(Task<int>), ExpectedResult = "System.Threading.Tasks.Task<System.Int32>")]
        [TestCase(typeof(OuterGeneric<>), ExpectedResult = "Outer.Inner.OuterGeneric<TOuter>")]
        [TestCase(typeof(OuterGeneric<Task>), ExpectedResult = "Outer.Inner.OuterGeneric<System.Threading.Tasks.Task>")]
        [TestCase(typeof(OuterGeneric<Task<int>>), ExpectedResult = "Outer.Inner.OuterGeneric<System.Threading.Tasks.Task<System.Int32>>")]
        [TestCase(typeof(OuterGeneric<>.InnerGeneric<>), ExpectedResult = "Outer.Inner.OuterGeneric<TOuter>.InnerGeneric<TInner>")]
        [TestCase(typeof(OuterGeneric<Task>.InnerGeneric<Array>), ExpectedResult = "Outer.Inner.OuterGeneric<System.Threading.Tasks.Task>.InnerGeneric<System.Array>")]
        [TestCase(typeof(OuterGeneric<Task>.InnerGeneric<Task<string>>), ExpectedResult = "Outer.Inner.OuterGeneric<System.Threading.Tasks.Task>.InnerGeneric<System.Threading.Tasks.Task<System.String>>")]
        [TestCase(typeof(OuterGeneric<Task<Task<Array>>>.InnerGeneric<Task<Task<string>>>), ExpectedResult = "Outer.Inner.OuterGeneric<System.Threading.Tasks.Task<System.Threading.Tasks.Task<System.Array>>>.InnerGeneric<System.Threading.Tasks.Task<System.Threading.Tasks.Task<System.String>>>")]
        [TestCase(typeof(OuterGeneric<>.InnerTwoGeneric<,>), ExpectedResult = "Outer.Inner.OuterGeneric<TOuter>.InnerTwoGeneric<TInner1,TInner2>")]
        [TestCase(typeof(OuterGeneric<Task>.InnerTwoGeneric<Task, Task<float>>), ExpectedResult = "Outer.Inner.OuterGeneric<System.Threading.Tasks.Task>.InnerTwoGeneric<System.Threading.Tasks.Task,System.Threading.Tasks.Task<System.Single>>")]
        [TestCase(typeof(OuterGeneric<>.InnerNonGeneric), ExpectedResult = "Outer.Inner.OuterGeneric<TOuter>.InnerNonGeneric")]
        [TestCase(typeof(OuterNonGeneric), ExpectedResult = "Outer.Inner.OuterNonGeneric")]
        [TestCase(typeof(OuterNonGeneric.InnerGeneric<>), ExpectedResult = "Outer.Inner.OuterNonGeneric.InnerGeneric<TInner>")]
        [TestCase(typeof(OuterNonGeneric.InnerNonGeneric), ExpectedResult = "Outer.Inner.OuterNonGeneric.InnerNonGeneric")]
        [TestCase(typeof(ValueTuple<,>), ExpectedResult = "System.ValueTuple<T1,T2>")]
        [TestCase(typeof(ValueTuple<int, string>), ExpectedResult = "System.ValueTuple<System.Int32,System.String>")]
        public string ToGenericTypeString_Test_FullName_NonEmptyOnStub(Type type)
            => type.ToCSharpTypeString(true, false, null);

        [Test]
        public void ToGenericTypeString_Test_FullName_NonEmptyOnStub_HandlesCorrectly_InvalidTypes()
            => typeof(void).ToCSharpTypeString(true, false, null).Should().Be("void");

        [Test]
        [TestCase(typeof(int), ExpectedResult = "System.Int32")]
        [TestCase(typeof(string), ExpectedResult = "System.String")]
        [TestCase(typeof(object), ExpectedResult = "System.Object")]
        [TestCase(typeof(char), ExpectedResult = "System.Char")]
        [TestCase(typeof(short), ExpectedResult = "System.Int16")]
        [TestCase(typeof(ushort), ExpectedResult = "System.UInt16")]
        [TestCase(typeof(Array), ExpectedResult = "System.Array")]
        [TestCase(typeof(Task), ExpectedResult = "System.Threading.Tasks.Task")]
        [TestCase(typeof(Task<>), ExpectedResult = "System.Threading.Tasks.Task<>")]
        [TestCase(typeof(Task<int>), ExpectedResult = "System.Threading.Tasks.Task<System.Int32>")]
        [TestCase(typeof(OuterGeneric<>), ExpectedResult = "Outer.Inner.OuterGeneric<>")]
        [TestCase(typeof(OuterGeneric<Task>), ExpectedResult = "Outer.Inner.OuterGeneric<System.Threading.Tasks.Task>")]
        [TestCase(typeof(OuterGeneric<Task<int>>), ExpectedResult = "Outer.Inner.OuterGeneric<System.Threading.Tasks.Task<System.Int32>>")]
        [TestCase(typeof(OuterGeneric<>.InnerGeneric<>), ExpectedResult = "Outer.Inner.OuterGeneric<>.InnerGeneric<>")]
        [TestCase(typeof(OuterGeneric<Task>.InnerGeneric<Array>), ExpectedResult = "Outer.Inner.OuterGeneric<System.Threading.Tasks.Task>.InnerGeneric<System.Array>")]
        [TestCase(typeof(OuterGeneric<Task>.InnerGeneric<Task<string>>), ExpectedResult = "Outer.Inner.OuterGeneric<System.Threading.Tasks.Task>.InnerGeneric<System.Threading.Tasks.Task<System.String>>")]
        [TestCase(typeof(OuterGeneric<Task<Task<Array>>>.InnerGeneric<Task<Task<string>>>), ExpectedResult = "Outer.Inner.OuterGeneric<System.Threading.Tasks.Task<System.Threading.Tasks.Task<System.Array>>>.InnerGeneric<System.Threading.Tasks.Task<System.Threading.Tasks.Task<System.String>>>")]
        [TestCase(typeof(OuterGeneric<>.InnerTwoGeneric<,>), ExpectedResult = "Outer.Inner.OuterGeneric<>.InnerTwoGeneric<,>")]
        [TestCase(typeof(OuterGeneric<Task>.InnerTwoGeneric<Task, Task<float>>), ExpectedResult = "Outer.Inner.OuterGeneric<System.Threading.Tasks.Task>.InnerTwoGeneric<System.Threading.Tasks.Task,System.Threading.Tasks.Task<System.Single>>")]
        [TestCase(typeof(OuterGeneric<>.InnerNonGeneric), ExpectedResult = "Outer.Inner.OuterGeneric<>.InnerNonGeneric")]
        [TestCase(typeof(OuterNonGeneric), ExpectedResult = "Outer.Inner.OuterNonGeneric")]
        [TestCase(typeof(OuterNonGeneric.InnerGeneric<>), ExpectedResult = "Outer.Inner.OuterNonGeneric.InnerGeneric<>")]
        [TestCase(typeof(OuterNonGeneric.InnerNonGeneric), ExpectedResult = "Outer.Inner.OuterNonGeneric.InnerNonGeneric")]
        [TestCase(typeof(ValueTuple<,>), ExpectedResult = "System.ValueTuple<,>")]
        [TestCase(typeof(ValueTuple<int, string>), ExpectedResult = "System.ValueTuple<System.Int32,System.String>")]
        public string ToGenericTypeString_Test_FullName_EmptyOnStub(Type type)
            => type.ToCSharpTypeString(true, true, null);

        [Test]
        public void ToGenericTypeString_Test_FullName_EmptyOnStub_HandlesCorrectly_InvalidTypes()
            => typeof(void).ToCSharpTypeString(true, true, null).Should().Be("void");
    }
}


