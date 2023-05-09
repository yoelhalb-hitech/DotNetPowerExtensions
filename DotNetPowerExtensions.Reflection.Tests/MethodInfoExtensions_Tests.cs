using DotNetPowerExtensions.Reflection;
using NUnit.Framework;
using System.Collections.Generic;

namespace DotNetPowerExtensions.Reflection.Tests;

public class MethodInfoExtensions_Tests
{
    [Test]
    [TestCase(MethodAttributes.Assembly, ExpectedResult = true)]
    [TestCase(MethodAttributes.FamORAssem, ExpectedResult = true)]
    [TestCase(MethodAttributes.FamANDAssem, ExpectedResult = false)]
    [TestCase(MethodAttributes.Family, ExpectedResult = false)]
    [TestCase(MethodAttributes.Public, ExpectedResult = false)]
    [TestCase(MethodAttributes.Static, ExpectedResult = false)]
    [TestCase(MethodAttributes.Private, ExpectedResult = false)]
    [TestCase(MethodAttributes.PrivateScope, ExpectedResult = false)]
    [TestCase(MethodAttributes.SpecialName, ExpectedResult = false)]
    public bool Test_IsInternal_Works_Correctly(MethodAttributes attributes)
    {
        var m = new Mock<MethodInfo>();
        m.SetupGet(f => f.Attributes).Returns(attributes);

        return m.Object.IsInternal();
    }

    [Test]
    [TestCase(MethodAttributes.Assembly, ExpectedResult = true)]
    [TestCase(MethodAttributes.FamORAssem, ExpectedResult = true)]
    [TestCase(MethodAttributes.FamANDAssem, ExpectedResult = false)]
    [TestCase(MethodAttributes.Family, ExpectedResult = false)]
    [TestCase(MethodAttributes.Public, ExpectedResult = true)]
    [TestCase(MethodAttributes.Static, ExpectedResult = false)]
    [TestCase(MethodAttributes.Private, ExpectedResult = false)]
    [TestCase(MethodAttributes.PrivateScope, ExpectedResult = false)]
    [TestCase(MethodAttributes.SpecialName, ExpectedResult = false)]
    public bool Test_IsPublicOrInternal_Works_Correctly(MethodAttributes attributes)
    {
        var m = new Mock<MethodInfo>();
        m.SetupGet(f => f.Attributes).Returns(attributes);

        return m.Object.IsPublicOrInternal();
    }

    private static void TestVoid() { }
    private static int TestNonVoid() { return 0; }

    [Test]
    public void Test_IsVoid_ReturnsTrueForVoid() => this.GetType().GetMethod("TestVoid", BindingFlagsExtensions.AllBindings)!.IsVoid().Should().BeTrue();

    [Test]
    public void Test_IsVoid_ReturnsFalseForNonVoid() => this.GetType().GetMethod("TestNonVoid", BindingFlagsExtensions.AllBindings)!.IsVoid().Should().BeFalse();

    abstract class TestClass
    {
        private void TestPrivate() { }
        protected virtual void TestProtected() { }
        public abstract int TestAbstract();
        public virtual int TestVirtual() => 0;
        public int TestNonVirtual() => 0;

    }

    class TestSub : TestClass
    {
        public override int TestAbstract() => 0;
        public override sealed int TestVirtual() => 0;

        public new virtual int TestNonVirtual() => 0;
    }

    interface TestIface
    {
        abstract int TestInterfaceAbstract();
        int TestInterface();
        public int TestPublicInterface();
        public virtual int TestPublicVirtualInterface() => 0;
    }

    [Test]
    [TestCase(typeof(TestClass), nameof(TestClass.TestAbstract), ExpectedResult = true)]
    [TestCase(typeof(TestClass), "TestPrivate", ExpectedResult = false)]
    [TestCase(typeof(TestClass), "TestProtected", ExpectedResult = true)]
    [TestCase(typeof(TestClass), nameof(TestClass.TestVirtual), ExpectedResult = true)]
    [TestCase(typeof(TestClass), nameof(TestClass.TestNonVirtual), ExpectedResult = false)]
    [TestCase(typeof(TestSub), nameof(TestSub.TestAbstract), ExpectedResult = true)]
    [TestCase(typeof(TestSub), nameof(TestSub.TestVirtual), ExpectedResult = false)]
    [TestCase(typeof(TestSub), nameof(TestSub.TestNonVirtual), ExpectedResult = true)]
    [TestCase(typeof(TestIface), nameof(TestIface.TestInterfaceAbstract), ExpectedResult = true)]
    [TestCase(typeof(TestIface), nameof(TestIface.TestInterface), ExpectedResult = true)]
    [TestCase(typeof(TestIface), nameof(TestIface.TestPublicInterface), ExpectedResult = true)]
    [TestCase(typeof(TestIface), nameof(TestIface.TestPublicVirtualInterface), ExpectedResult = true)]
    public bool Test_IsOverridable(Type type, string method) => type.GetMethod(method, BindingFlagsExtensions.AllBindings)!.IsOverridable();

    class TestPropClass
    {
        public int TestProp { get; set; }
    }
    class TestEmptyPropSub : TestPropClass
    {
    }

    [Test]
    public void Test_GetDeclaringProperty()
    {
        var propInfo = typeof(TestPropClass).GetProperty(nameof(TestPropClass.TestProp));

        var result = propInfo!.GetMethod!.GetDeclaringProperty();

        result.Should().BeSameAs(propInfo);
    }

    [Test]
    public void Test_GetDeclaringProperty_WorksCorrectly_WithReflectedClass()
    {
        var propInfo = typeof(TestPropClass).GetProperty(nameof(TestPropClass.TestProp));
        var result = propInfo!.GetMethod!.GetDeclaringProperty();

        var propSub = typeof(TestEmptyPropSub).GetProperty(nameof(TestPropClass.TestProp));
        var resultSub = propSub!.GetMethod!.GetDeclaringProperty();

        var result2 = propInfo!.GetMethod!.GetDeclaringProperty();

        resultSub.Should().BeSameAs(result);
        result2.Should().BeSameAs(result);
    }

    class TestEventClass
    {
        public event EventHandler? TestEvent;
    }
    class TestEmptyEventSub : TestEventClass
    {
    }

    [Test]
    public void Test_GetDeclaringEvent()
    {
        var eventInfo = typeof(TestEventClass).GetEvent(nameof(TestEventClass.TestEvent));

        var result = eventInfo!.AddMethod!.GetDeclaringEvent();

        result.Should().BeSameAs(eventInfo);
    }

    [Test]
    public void Test_GetDeclaringEvent_WorksCorrectly_WithReflectedClass()
    {
        var eventInfo = typeof(TestEventClass).GetEvent(nameof(TestEventClass.TestEvent));
        var result = eventInfo!.AddMethod!.GetDeclaringEvent();

        var eventSub = typeof(TestEmptyEventSub).GetEvent(nameof(TestEventClass.TestEvent));
        var resultSub = eventSub!.AddMethod!.GetDeclaringEvent();

        var result2 = eventInfo!.AddMethod!.GetDeclaringEvent();

        resultSub.Should().BeSameAs(result);
        result2.Should().BeSameAs(result);
    }

    class A
    {
        public virtual void Test() { }
        public virtual void Test<T>() { }
        public virtual void Test<T, T1>() { }
        public virtual void Test(int i) { }
        public virtual void Test<T>(int i) { }
        public virtual void Test<T, T1>(int i) { }
    }

    class B : A { } // Don't add here anything or the tests will fail
    class C : A { }

    [Test]
    public void Test_IsEqual_ReturnsTrue_WhenSubclassesEqual([Values(0,1,2,3,4,5)] int toSkip)
        => typeof(B).GetMethods().Skip(toSkip).First()
                    .IsEqual(typeof(C).GetMethods().Skip(toSkip).First())
                    .Should().BeTrue();

    [Test]
    public void Test_IsEqual_ReturnsTrue_WhenBaseAndSubclassEqual([Values(0, 1, 2, 3, 4, 5)] int toSkip)
        => typeof(A).GetMethods().Skip(toSkip).First()
                    .IsEqual(typeof(C).GetMethods().Skip(toSkip).First())
                    .Should().BeTrue();

    [Test]
    public void Test_IsEqual_ReturnsFalse_WhenSubclassesNotEqual([Values(0, 1, 2, 3, 4, 5)] int first, [Values(0, 1, 2, 3, 4, 5)] int second)
    {
        Assume.That(first != second);

        typeof(B).GetMethods().Skip(first).First()
            .IsEqual(typeof(C).GetMethods().Skip(second).First())
            .Should().BeFalse();
    }

    [Test]
    public void Test_IsEqual_ReturnsFalse_WhenBaseAndSublassNotEqual([Values(0, 1, 2, 3, 4, 5)] int first, [Values(0, 1, 2, 3, 4, 5)] int second)
    {
        Assume.That(first != second);

        typeof(A).GetMethods().Skip(first).First()
            .IsEqual(typeof(B).GetMethods().Skip(second).First()).Should().BeFalse();
    }


    [Test]
    public void Test_IsEqual_ReturnsFalse_WhenGenericDefinitionAndConstructed([Values(0, 1, 2, 3, 4, 5)] int toSkip)
    {
        var method = typeof(B).GetMethods().Skip(toSkip).First();

        Assume.That(method.IsGenericMethodDefinition);

        typeof(A).GetMethods().Skip(toSkip).First() // Use a different reflected type as otherwise it will short circut to the built in comparison
            .IsEqual(method.MakeGenericMethod(method.GetGenericArguments().Select(s => typeof(int)).ToArray()))
            .Should().BeFalse();
    }

    [Test]
    public void Test_IsEqual_ReturnsFalse_WhenConstructedMethodsAreDifferent([Values(0, 1, 2, 3, 4, 5)] int toSkip)
    {
        var method = typeof(B).GetMethods().Skip(toSkip).First();

        Assume.That(method.IsGenericMethodDefinition);

        // Use a different reflected type as otherwise it will short circut to the built in comparison
        typeof(A).GetMethods().Skip(toSkip).First().MakeGenericMethod(method.GetGenericArguments().Select(s => typeof(string)).ToArray())
            .IsEqual(method.MakeGenericMethod(method.GetGenericArguments().Select(s => typeof(int)).ToArray())).Should().BeFalse();
    }

    class A1
    {
        public virtual void Testing() { }
    }

    class B1 : A1
    {
        public override void Testing() { }
    }
    class C1 : A1
    {
        public override void Testing() { }
    }
    [Test]
    public void Test_IsEqual_ReturnsFalse_WhenSubclassesOverridden()
            => typeof(B1).GetMethod(nameof(A1.Testing))!
                .IsEqual(typeof(C1).GetMethod(nameof(A1.Testing))!)
                .Should().BeFalse();

    [Test]
    public void Test_IsEqual_ReturnsFalse_WhenBaseAndOverridenSubclass()
        => typeof(A1).GetMethod(nameof(A1.Testing))!
            .IsEqual(typeof(C1).GetMethod(nameof(A1.Testing))!)
            .Should().BeFalse();

    interface GetInterfaceMethod_IFace
    {
        public void Test();
    }
    interface GetInterfaceMethod_IFace2
    {
        public void Test();
    }
    class GetInterfaceMethod_BaseNoImplementation
    {
        public virtual void Test() { }
    }
    class GetInterfaceMethod_SubImplementation : GetInterfaceMethod_BaseNoImplementation, GetInterfaceMethod_IFace {}
    class GetInterfaceMethod_SubSecondImplementation : GetInterfaceMethod_SubImplementation, GetInterfaceMethod_IFace2 {}
    class GetInterfaceMethod_SubOverride : GetInterfaceMethod_SubSecondImplementation
    {
        public override void Test() { }
    }

    [Test]
    [TestCase(typeof(GetInterfaceMethod_BaseNoImplementation), ExpectedResult = new Type[] {})]
    [TestCase(typeof(GetInterfaceMethod_SubImplementation), ExpectedResult = new Type[] { typeof(GetInterfaceMethod_IFace) })]
    [TestCase(typeof(GetInterfaceMethod_SubSecondImplementation), ExpectedResult = new Type[] { typeof(GetInterfaceMethod_IFace), typeof(GetInterfaceMethod_IFace2) })]
    [TestCase(typeof(GetInterfaceMethod_SubOverride), ExpectedResult = new Type[] { typeof(GetInterfaceMethod_IFace), typeof(GetInterfaceMethod_IFace2) })]
    public Type[] Test_GetInterfaceMethod(Type t)
        => t!.GetMethod(nameof(GetInterfaceMethod_IFace.Test))!.GetInterfaceMethods()!.Select(m => m.DeclaringType!).ToArray();

    interface GetInterfaceMethod_DefaultInterfaceImplementation : GetInterfaceMethod_IFace
    {
        void GetInterfaceMethod_IFace.Test() { }
    }
    class GetInterfaceMethod_ExplicitImplementation : GetInterfaceMethod_IFace
    {
        void GetInterfaceMethod_IFace.Test() { }
    }
    [Test]
    [TestCase(typeof(GetInterfaceMethod_DefaultInterfaceImplementation), ExpectedResult = new Type[] { typeof(GetInterfaceMethod_IFace) })]
    [TestCase(typeof(GetInterfaceMethod_ExplicitImplementation), ExpectedResult = new Type[] { typeof(GetInterfaceMethod_IFace) })]
    public Type[] Test_GetInterfaceMethod_WorksWithExplicitImplementation(Type t)
        => t!.GetInterfaceMapForInterface(typeof(GetInterfaceMethod_IFace)).TargetMethods.First().GetInterfaceMethods()!.Select(m => m.DeclaringType!).ToArray();
}
