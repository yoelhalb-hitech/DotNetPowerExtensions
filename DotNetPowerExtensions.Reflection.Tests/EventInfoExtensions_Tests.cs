
namespace DotNetPowerExtensions.Reflection.Tests;

public class EventInfoExtensions_Tests
{
    private event EventHandler? BothPrivate;
    public event EventHandler? BothPublic;

    [Test]
    [TestCase(nameof(BothPrivate), ExpectedResult = 2)]
    [TestCase(nameof(BothPublic), ExpectedResult = 2)]
    public int Test_GetAllMethods(string e) => this.GetType().GetEvent(e, BindingFlagsExtensions.AllBindings)!.GetAllMethods().Length;

    [Test]
    [TestCase(nameof(BothPrivate), ExpectedResult = true)]
    [TestCase(nameof(BothPublic), ExpectedResult = false)]
    public bool Test_GetIsPrivate(string e) => this.GetType().GetEvent(e, BindingFlagsExtensions.AllBindings)!.IsPrivate();

    abstract class TestClass
    {
        public abstract event EventHandler? TestAbstract;
        public virtual event EventHandler? TestVirtual;
        public event EventHandler? TestNonVirtual;
    }

    class TestSub : TestClass
    {
        public override event EventHandler? TestAbstract;
        public override sealed event EventHandler? TestVirtual;
        public new virtual event EventHandler? TestNonVirtual;
    }

    interface TestIface
    {
        abstract event EventHandler? TestInterfaceAbstract;
        event EventHandler? TestInterface;
        public event EventHandler? TestPublicInterface;
        public virtual event EventHandler? TestPublicVirtualInterface { add => throw new NotImplementedException(); remove => throw new NotImplementedException(); }
    }


    [Test]
    [TestCase(typeof(TestClass), nameof(TestClass.TestAbstract), ExpectedResult = true)]
    [TestCase(typeof(TestClass), nameof(TestClass.TestVirtual), ExpectedResult = false)]
    [TestCase(typeof(TestClass), nameof(TestClass.TestNonVirtual), ExpectedResult = false)]
    [TestCase(typeof(TestSub), nameof(TestSub.TestAbstract), ExpectedResult = false)]
    [TestCase(typeof(TestSub), nameof(TestSub.TestVirtual), ExpectedResult = false)]
    [TestCase(typeof(TestSub), nameof(TestSub.TestNonVirtual), ExpectedResult = false)]
    [TestCase(typeof(TestIface), nameof(TestIface.TestInterfaceAbstract), ExpectedResult = true)]
    [TestCase(typeof(TestIface), nameof(TestIface.TestInterface), ExpectedResult = true)]
    [TestCase(typeof(TestIface), nameof(TestIface.TestPublicInterface), ExpectedResult = true)]
    [TestCase(typeof(TestIface), nameof(TestIface.TestPublicVirtualInterface), ExpectedResult = false)]
    public bool Test_IsAbstract(Type type, string e) => type.GetEvent(e)!.IsAbstract();

    [Test]
    [TestCase(typeof(TestClass), nameof(TestClass.TestAbstract), ExpectedResult = true)]
    [TestCase(typeof(TestClass), nameof(TestClass.TestVirtual), ExpectedResult = true)]
    [TestCase(typeof(TestClass), nameof(TestClass.TestNonVirtual), ExpectedResult = false)]
    [TestCase(typeof(TestSub), nameof(TestSub.TestAbstract), ExpectedResult = true)]
    [TestCase(typeof(TestSub), nameof(TestSub.TestVirtual), ExpectedResult = false)]
    [TestCase(typeof(TestSub), nameof(TestSub.TestNonVirtual), ExpectedResult = true)]
    [TestCase(typeof(TestIface), nameof(TestIface.TestInterfaceAbstract), ExpectedResult = true)]
    [TestCase(typeof(TestIface), nameof(TestIface.TestInterface), ExpectedResult = true)]
    [TestCase(typeof(TestIface), nameof(TestIface.TestPublicInterface), ExpectedResult = true)]
    [TestCase(typeof(TestIface), nameof(TestIface.TestPublicVirtualInterface), ExpectedResult = true)]
    public bool Test_IsOverridable(Type type, string e) => type.GetEvent(e)!.IsOverridable();
}
