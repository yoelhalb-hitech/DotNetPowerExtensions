
namespace DotNetPowerExtensions.Reflection.Tests;

public class TypeExtensions_Tests
{
    [Test]
    [TestCase(typeof(int), ExpectedResult = 0)]
    [TestCase(typeof(float), ExpectedResult = 0f)]
    [TestCase(typeof(decimal), ExpectedResult = 0)]
    [TestCase(typeof(string), ExpectedResult = null)]
    [TestCase(typeof(int[]), ExpectedResult = null)]
    [TestCase(typeof(List<int>), ExpectedResult = null)]
    public object? Test_GetDefault_Works(Type type)  => type.GetDefault();

    [Test]
    [TestCase(typeof(int), ExpectedResult = false)]
    [TestCase(typeof(float), ExpectedResult = false)]
    [TestCase(typeof(decimal), ExpectedResult = false)]
    [TestCase(typeof(string), ExpectedResult = true)]
    [TestCase(typeof(int[]), ExpectedResult = true)]
    [TestCase(typeof(List<int>), ExpectedResult = true)]
    public bool Test_IsNullAllowed_Works(Type type) => type.IsNullAllowed();

    [Test]
    [TestCase(typeof(int), ExpectedResult = false)]
    [TestCase(typeof(Binder), ExpectedResult = false)]
    [TestCase(typeof(int[]), ExpectedResult = true)]
    [TestCase(typeof(List<int>), ExpectedResult = true)]
    [TestCase(typeof(Task), ExpectedResult = false)]
    [TestCase(typeof(Task<int>), ExpectedResult = true)]
    [TestCase(typeof(Task<int[]>), ExpectedResult = true)]
    [TestCase(typeof(Task<int>[]), ExpectedResult = true)]
    [TestCase(typeof(Tuple<int, string>), ExpectedResult = true)]
    public bool Test_HasInnerType(Type type) => type.HasInnerType();

    [Test]
    [TestCase(typeof(int), ExpectedResult = new Type[] {})]
    [TestCase(typeof(Binder), ExpectedResult = new Type[] { })]
    [TestCase(typeof(int[]), ExpectedResult = new Type[] { typeof(int)})]
    [TestCase(typeof(List<int>), ExpectedResult = new Type[] { typeof(int) })]
    [TestCase(typeof(Task), ExpectedResult = new Type[] {})]
    [TestCase(typeof(Task<int>), ExpectedResult = new Type[] { typeof(int) })]
    [TestCase(typeof(Task<int[]>), ExpectedResult = new Type[] { typeof(int[]) })]
    [TestCase(typeof(Task<int>[]), ExpectedResult = new Type[] { typeof(Task<int>) })]
    [TestCase(typeof(Tuple<int, string>), ExpectedResult = new Type[] { typeof(int), typeof(string) })]
    [TestCase(typeof(Tuple<int, string>[]), ExpectedResult = new Type[] { typeof(Tuple<int, string>) })]
    [TestCase(typeof(Tuple<int[], string>), ExpectedResult = new Type[] { typeof(int[]), typeof(string) })]
    [TestCase(typeof(Tuple<int[], string>[]), ExpectedResult = new Type[] { typeof(Tuple<int[], string>) })]
    public Type[] Test_GetInnerType(Type type) => type.GetInnerTypes();

    interface ITestIface { }
    interface ITestIfaceSub : ITestIface { }

    class TestBase : ITestIface { }
    class TestSub : TestBase, ITestIfaceSub { }
    sealed class TestSubSub : TestSub { }


    [Test]
    [TestCase(typeof(TestBase), ExpectedResult = new Type[] {})]
    [TestCase(typeof(TestSub), ExpectedResult = new[] { typeof(TestBase) })]
    [TestCase(typeof(TestSubSub), ExpectedResult = new[] { typeof(TestBase), typeof(TestSub) })]
    public Type[] Test_GetBaseTypes(Type type) => type.GetBaseTypes();

    [Test]
    [TestCase(typeof(TestBase), ExpectedResult = new[] { typeof(ITestIface) })]
    [TestCase(typeof(TestSub), ExpectedResult = new[] { typeof(ITestIface), typeof(ITestIfaceSub), typeof(TestBase) })]
    [TestCase(typeof(TestSubSub), ExpectedResult = new[] { typeof(ITestIface), typeof(ITestIfaceSub), typeof(TestBase), typeof(TestSub) })]
    public Type[] Test_GetBasesAndInterfaces(Type type) => type.GetBasesAndInterfaces().ToArray();

    interface ITestGeneric<T> { }

    class TestGeneric<T> { }
    class TestNonGeneric: TestGeneric<int> { }

#pragma warning disable CA1812 // Class is an internal class that is apparently never instantiated. If so, remove the code from the assembly. If this class is intended to contain only static members, make it 'static' (Module in Visual Basic).
    sealed class TestGenericSub<T> : TestNonGeneric, ITestGeneric<T> { }
#pragma warning restore CA1812 // Class is an internal class that is apparently never instantiated. If so, remove the code from the assembly. If this class is intended to contain only static members, make it 'static' (Module in Visual Basic).


    [Test]
    [TestCase(typeof(ITestGeneric<>), ExpectedResult = new[] { typeof(ITestGeneric<>) } )]
    [TestCase(typeof(ITestGeneric<int>), ExpectedResult = new[] { typeof(ITestGeneric<>) } )]
    [TestCase(typeof(TestGeneric<>), ExpectedResult = new[] { typeof(TestGeneric<>) } )]
    [TestCase(typeof(TestGeneric<string>), ExpectedResult = new[] { typeof(TestGeneric<>) } )]
    [TestCase(typeof(TestNonGeneric), ExpectedResult = new[] { typeof(TestGeneric<>) } )]
    [TestCase(typeof(TestGenericSub<>), ExpectedResult = new[] { typeof(ITestGeneric<>), typeof(TestGeneric<>), typeof(TestGenericSub<>) } )]
    [TestCase(typeof(TestGenericSub<string>), ExpectedResult = new[] { typeof(ITestGeneric<>), typeof(TestGeneric<>), typeof(TestGenericSub<>) } )]
    public Type[] Test_GetAllGenericDefinitions(Type type) => type.GetAllGenericDefinitions().ToArray();

    [Test]
    [TestCase(typeof(string), ExpectedResult = false)]
    [TestCase(typeof(int), ExpectedResult = false)]
    [TestCase(typeof(Binder), ExpectedResult = false)]
    [TestCase(typeof(Action), ExpectedResult = true)]
    [TestCase(typeof(Action<>), ExpectedResult = true)]
    [TestCase(typeof(Action<int>), ExpectedResult = true)]
    [TestCase(typeof(Func<>), ExpectedResult = true)]
    [TestCase(typeof(Func<int>), ExpectedResult = true)]
    [TestCase(typeof(Func<string, int>), ExpectedResult = true)]
    public bool Test_IsDelegate(Type type) => type.IsDelegate();

#pragma warning disable CA1852 // Type can be sealed because it has no subtypes in its containing assembly and is not externally visible
    class TestClass
    {
        public static int TestMethod() => 1;
        internal static int TestMethod(int i) => 2;
        private static int TestMethod(string s) => 3;
        protected static int TestMethod(string s, int i) => 4;
    }
#pragma warning restore CA1852 // Type can be sealed because it has no subtypes in its containing assembly and is not externally visible

    [Test]
    [TestCase(new object[] { 0 }, ExpectedResult = 2)]
    [TestCase(new object[] { "" }, ExpectedResult = 3)]
    [TestCase(new object[] { "", 0 }, ExpectedResult = 4)]
    public int Test_InvokeMethod(object[]? args) => (int)typeof(TestClass).InvokeMethod(nameof(TestClass.TestMethod), null, args)!;
    [Test]
    public void Test_InvokeMethod_NoArgs() => typeof(TestClass).InvokeMethod(nameof(TestClass.TestMethod), null, null)!.Should().Be(1);

#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
    [Test]
    public void Test_InvokeMethod_ThrowsOnNullArgs()
        => Assert.Throws<ArgumentNullException>(() => typeof(TestClass).InvokeMethod(nameof(TestClass.TestMethod), null, new object?[] { null }));
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
}
