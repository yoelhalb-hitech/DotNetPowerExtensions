
namespace DotNetPowerExtensions.Reflection.Tests;

public class PropertyInfoExtensions_Tests
{
    private int BothPrivate { get; set; }
    internal int GetPrivate { private get; set; }
    internal int SetPrivate { get; private set; }
    public int BothPublic { get; set; }
    internal int GetOnly { get; }
    private int GetOnlyPrivate { get; }
    public int GetOnlyPublic { get; }
    internal int SetOnly { set => throw new NotImplementedException(); }
    private int SetOnlyPrivate { set => throw new NotImplementedException(); }
#pragma warning disable CA1044 // Properties should not be write only
    public int SetOnlyPublic { set => throw new NotImplementedException(); }
#pragma warning restore CA1044 // Properties should not be write only

    [Test]
    [TestCase(nameof(BothPrivate), ExpectedResult = 2)]
    [TestCase(nameof(GetPrivate), ExpectedResult = 2)]
    [TestCase(nameof(SetPrivate), ExpectedResult = 2)]
    [TestCase(nameof(BothPublic), ExpectedResult = 2)]
    [TestCase(nameof(GetOnly), ExpectedResult = 1)]
    [TestCase(nameof(GetOnlyPrivate), ExpectedResult = 1)]
    [TestCase(nameof(GetOnlyPublic), ExpectedResult = 1)]
    [TestCase(nameof(SetOnly), ExpectedResult = 1)]
    [TestCase(nameof(SetOnlyPrivate), ExpectedResult = 1)]
    [TestCase(nameof(SetOnlyPublic), ExpectedResult = 1)]
    public int Test_GetAllMethods(string prop) => this.GetType().GetProperty(prop, BindingFlagsExtensions.AllBindings)!.GetAllMethods().Length;

    [Test]
    [TestCase(nameof(BothPrivate), ExpectedResult = true)]
    [TestCase(nameof(GetPrivate), ExpectedResult = false)]
    [TestCase(nameof(SetPrivate), ExpectedResult = false)]
    [TestCase(nameof(BothPublic), ExpectedResult = false)]
    [TestCase(nameof(GetOnly), ExpectedResult = false)]
    [TestCase(nameof(GetOnlyPrivate), ExpectedResult = true)]
    [TestCase(nameof(GetOnlyPublic), ExpectedResult = false)]
    [TestCase(nameof(SetOnly), ExpectedResult = false)]
    [TestCase(nameof(SetOnlyPrivate), ExpectedResult = true)]
    [TestCase(nameof(SetOnlyPublic), ExpectedResult = false)]
    public bool Test_GetIsPrivate(string prop) => this.GetType().GetProperty(prop, BindingFlagsExtensions.AllBindings)!.IsPrivate();

    abstract class TestClass
    {
        public abstract int TestAbstract { get; set; }
        public virtual int TestVirtual { get; set; }
        public int TestNonVirtual { get; set; }
    }

    class TestSub : TestClass
    {
        public override int TestAbstract { get; set; }
        public override sealed int TestVirtual { get; set; }

        public new virtual int TestNonVirtual { get; set; }
    }

    interface TestIface
    {
        abstract int TestInterfaceAbstractProp { get; set; }
        int TestInterfaceProp { get; set; }
        public int TestPublicInterfaceProp { get; set; }
        public virtual int TestPublicVirtualInterfaceProp { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }


    [Test]
    [TestCase(typeof(TestClass), nameof(TestClass.TestAbstract), ExpectedResult = true)]
    [TestCase(typeof(TestClass), nameof(TestClass.TestVirtual), ExpectedResult = false)]
    [TestCase(typeof(TestClass), nameof(TestClass.TestNonVirtual), ExpectedResult = false)]
    [TestCase(typeof(TestSub), nameof(TestSub.TestAbstract), ExpectedResult = false)]
    [TestCase(typeof(TestSub), nameof(TestSub.TestVirtual), ExpectedResult = false)]
    [TestCase(typeof(TestSub), nameof(TestSub.TestNonVirtual), ExpectedResult = false)]
    [TestCase(typeof(TestIface), nameof(TestIface.TestInterfaceAbstractProp), ExpectedResult = true)]
    [TestCase(typeof(TestIface), nameof(TestIface.TestInterfaceProp), ExpectedResult = true)]
    [TestCase(typeof(TestIface), nameof(TestIface.TestPublicInterfaceProp), ExpectedResult = true)]
    [TestCase(typeof(TestIface), nameof(TestIface.TestPublicVirtualInterfaceProp), ExpectedResult = false)]
    public bool Test_IsAbstract(Type type, string prop) => type.GetProperty(prop)!.IsAbstract();

    [Test]
    [TestCase(typeof(TestClass), nameof(TestClass.TestAbstract), ExpectedResult = true)]
    [TestCase(typeof(TestClass), nameof(TestClass.TestVirtual), ExpectedResult = true)]
    [TestCase(typeof(TestClass), nameof(TestClass.TestNonVirtual), ExpectedResult = false)]
    [TestCase(typeof(TestSub), nameof(TestSub.TestAbstract), ExpectedResult = true)]
    [TestCase(typeof(TestSub), nameof(TestSub.TestVirtual), ExpectedResult = false)]
    [TestCase(typeof(TestSub), nameof(TestSub.TestNonVirtual), ExpectedResult = true)]
    [TestCase(typeof(TestIface), nameof(TestIface.TestInterfaceAbstractProp), ExpectedResult = true)]
    [TestCase(typeof(TestIface), nameof(TestIface.TestInterfaceProp), ExpectedResult = true)]
    [TestCase(typeof(TestIface), nameof(TestIface.TestPublicInterfaceProp), ExpectedResult = true)]
    [TestCase(typeof(TestIface), nameof(TestIface.TestPublicVirtualInterfaceProp), ExpectedResult = true)]
    public bool Test_IsOverridable(Type type, string prop) => type.GetProperty(prop)!.IsOverridable();
}
