using static SequelPay.DotNetPowerExtensions.Reflection.MethodInfoExtensions;

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
        int TestInterface();
#if NETCOREAPP
        abstract int TestInterfaceAbstract();
        public int TestPublicInterface();
        public virtual int TestPublicVirtualInterface() => 0;
#endif
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
    [TestCase(typeof(TestIface), nameof(TestIface.TestInterface), ExpectedResult = true)]
#if NETCOREAPP
    [TestCase(typeof(TestIface), nameof(TestIface.TestInterfaceAbstract), ExpectedResult = true)]
    [TestCase(typeof(TestIface), nameof(TestIface.TestPublicInterface), ExpectedResult = true)]
    [TestCase(typeof(TestIface), nameof(TestIface.TestPublicVirtualInterface), ExpectedResult = true)]
#endif
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

    class BaseClass
    {
        public virtual void Test() { }
        public virtual void Test<T>() { }
        public virtual void Test<T, T1>() { }
        public virtual void Test(int i) { }
        public virtual void Test<T>(int i) { }
        public virtual void Test<T>(T t) { }
        public virtual void Test<T, T1>(int i) { }
        public virtual void Test<T, T1>(T t) { }
        public virtual void Test<T, T1>(T1 t1) { }
        public virtual void Test<T, T1>(int i, string s) { }
        public virtual void Test<T, T1>(string s, int i) { }
        public virtual void Test<T, T1>(T t, T1 t1) { }
        public virtual void Test<T, T1>(T1 t1, T t) { }
    }

    class Inherited1 : BaseClass { } // Don't add here anything or the tests will fail
    class Inherited2 : BaseClass { }

    class Overriden : BaseClass
    {
        public override void Test() { }
        public override void Test<T>() { }
        public override void Test<T, T1>() { }
        public override void Test(int i) { }
        public override void Test<T>(int i) { }
        public override void Test<T>(T t) { }
        public override void Test<T, T1>(int i) { }
        public override void Test<T, T1>(T t) { }
        public override void Test<T, T1>(T1 t1) { }
        public override void Test<T, T1>(int i, string s) { }
        public override void Test<T, T1>(string s, int i) { }
        public override void Test<T, T1>(T t, T1 t1) { }
        public override void Test<T, T1>(T1 t1, T t) { }
    }

    class Shadowed : BaseClass
    {
        public new virtual void Test() { }
        public new virtual void Test<T>() { }
        public new virtual void Test<T, T1>() { }
        public new virtual void Test(int i) { }
        public new virtual void Test<T>(int i) { }
        public new virtual void Test<T>(T t) { }
        public new virtual void Test<T, T1>(int i) { }
        public new virtual void Test<T, T1>(T t) { }
        public new virtual void Test<T, T1>(T1 t1) { }
        public new virtual void Test<T, T1>(int i, string s) { }
        public new virtual void Test<T, T1>(string s, int i) { }
        public new virtual void Test<T, T1>(T t, T1 t1) { }
        public new virtual void Test<T, T1>(T1 t1, T t) { }
    }

    class OtherClass
    {
        public virtual void Test() { }
        public virtual void Test<T>() { }
        public virtual void Test<T1, T>() { }
        public virtual void Test(int i) { }
        public virtual void Test<T>(int i) { }
        public virtual void Test<T>(T t) { }
        public virtual void Test<T12, T34>(int i) { }
        public virtual void Test<T1, T>(T1 t) { }
        public virtual void Test<T2, T1>(T1 t1) { }
        public virtual void Test<T124, T144>(int i, string s) { }
        public virtual void Test<T22, T133>(string s, int i) { }
        public virtual void Test<T33, T111>(T33 t, T111 t1) { }
        public virtual void Test<T111, T33>(T33 t, T111 t1) { }
    }

    public static int[] ToSkip = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12];

    public static (Type, Type)[] TypesEqual =
    [
        (typeof(Inherited1), typeof(Inherited2)),
        (typeof(BaseClass), typeof(Inherited2)),
        (typeof(BaseClass), typeof(BaseClass)),
        (typeof(Inherited1), typeof(Inherited1)),
        (typeof(Overriden), typeof(Overriden)),
        (typeof(OtherClass), typeof(OtherClass)),
    ];

    public static (Type, Type)[] TypesSignatureEqual =
    [
        (typeof(BaseClass), typeof(Overriden)),
        (typeof(BaseClass), typeof(Shadowed)),
        (typeof(Inherited1), typeof(Shadowed)),
        (typeof(Inherited1), typeof(Overriden)),
        (typeof(OtherClass), typeof(BaseClass)),
        (typeof(OtherClass), typeof(Shadowed)),
        (typeof(OtherClass), typeof(Overriden)),
        (typeof(OtherClass), typeof(Inherited2)),
    ];

    public static (Type, Type)[] AllTypes = [.. TypesEqual, .. TypesSignatureEqual];

    private void VerifyEqual(MethodInfo method1, MethodInfo method2)
        => VerifyEqualInternal(method1, method2, true);

    private void VerifyNotEqual(MethodInfo method1, MethodInfo method2)
        => VerifyEqualInternal(method1, method2, false);

    private void VerifyEqualInternal(MethodInfo method1, MethodInfo method2, bool should)
    {
        method1.IsEqual(method2).Should().Be(should);

        var equalityComparer = new MethodEqualityComparer();
        equalityComparer.Equals(method1, method2).Should().Be(should);

        if(should)
            (equalityComparer.GetHashCode(method1) == equalityComparer.GetHashCode(method2)).Should().BeTrue();
    }

    private void VerifySigEqual(MethodInfo method1, MethodInfo method2)
        => VerifySigEqualInternal(method1, method2, true);

    private void VerifySigNotEqual(MethodInfo method1, MethodInfo method2)
        => VerifySigEqualInternal(method1, method2, false);

    private void VerifySigEqualInternal(MethodInfo method1, MethodInfo method2, bool should)
    {
        method1.IsSignatureEqual(method2).Should().Be(should);

        var equalitySigComparer = new MethodSignatureEqualityComparer();
        equalitySigComparer.Equals(method1, method2).Should().Be(should);

        if (should)
            (equalitySigComparer.GetHashCode(method1) == equalitySigComparer.GetHashCode(method2)).Should().BeTrue();
    }

    private MethodInfo GetMethod(Type type, int toSkip) => type.GetMethods().Skip(toSkip).First();
    private MethodInfo MakeGeneric<Type>(MethodInfo method)
    {
        var concreteArgs = Enumerable.Repeat(typeof(Type), method.GetGenericArguments().Length).ToArray();
        return method.MakeGenericMethod(concreteArgs);
    }

    [Test]
    public void Test_Equality_WhenEqual([ValueSource(nameof(ToSkip))] int toSkip,
                        [ValueSource(nameof(TypesEqual))] (Type type1, Type type2) types)
    {
        var method1 = GetMethod(types.type1, toSkip);
        var method2 = GetMethod(types.type2, toSkip);

        VerifyEqual(method1, method2);
        VerifySigEqual(method1, method2);
    }

    [Test]
    public void Test_Equality_WhenEqual_AndContructedSame([ValueSource(nameof(ToSkip))] int toSkip,
                    [ValueSource(nameof(TypesEqual))] (Type type1, Type type2) types)
    {
        var method1 = GetMethod(types.type1, toSkip);
        var method2 = GetMethod(types.type2, toSkip);

        Assume.That(method1.IsGenericMethodDefinition);

        var constructed1 = MakeGeneric<string>(method1);
        var constructed2 = MakeGeneric<string>(method2);

        VerifyEqual(constructed1, constructed2);
        VerifySigEqual(constructed1, constructed2);
    }

    [Test]
    public void Test_SignatureEqual_WhenOnlySignatureEqual([ValueSource(nameof(ToSkip))] int toSkip,
                    [ValueSource(nameof(TypesSignatureEqual))] (Type type1, Type type2) types)
    {
        var method1 = GetMethod(types.type1, toSkip);
        var method2 = GetMethod(types.type2, toSkip);

        VerifyNotEqual(method1, method2);
        VerifySigEqual(method1, method2);
    }

    [Test]
    public void Test_SignatureEqual_WhenOnlySignatureEqual_AndContructedSame(
                [ValueSource(nameof(ToSkip))] int toSkip,
                [ValueSource(nameof(TypesSignatureEqual))] (Type type1, Type type2) types)
    {
        var method1 = GetMethod(types.type1, toSkip);
        var method2 = GetMethod(types.type2, toSkip);

        Assume.That(method1.IsGenericMethodDefinition);

        var constructed1 = MakeGeneric<string>(method1);
        var constructed2 = MakeGeneric<string>(method2);

        VerifyNotEqual(constructed1, constructed2);
        VerifySigEqual(constructed1, constructed2);
    }

    [Test]
    public void Test_NotEqual_WhenSignatureNotEqual(
                [ValueSource(nameof(ToSkip))] int first,
                [ValueSource(nameof(ToSkip))] int second,
                [ValueSource(nameof(TypesSignatureEqual))] (Type type1, Type type2) types)
    {
        Assume.That(first != second);

        var method1 = GetMethod(types.type1, first);
        var method2 = GetMethod(types.type2, second);

        VerifyNotEqual(method1, method2);
        VerifySigNotEqual(method1, method2);
    }

    [Test]
    public void Test_NotEqual_WhenGenericDefinitionAndConstruction(
            [ValueSource(nameof(ToSkip))] int toSkip,
            [ValueSource(nameof(AllTypes))] (Type type1, Type type2) types)
    {
        var method1 = GetMethod(types.type1, toSkip);
        var method2 = GetMethod(types.type2, toSkip);

        Assume.That(method1.IsGenericMethodDefinition);

        var constructed = MakeGeneric<string>(method1);

        VerifyNotEqual(constructed, method2);
        VerifySigNotEqual(constructed, method2);
    }

    [Test]
    public void Test_NotEqual_WhenConstructedMethodsAreDifferent(
        [ValueSource(nameof(ToSkip))] int toSkip,
        [ValueSource(nameof(AllTypes))] (Type type1, Type type2) types)
    {
        var method1 = GetMethod(types.type1, toSkip);
        var method2 = GetMethod(types.type2, toSkip);

        Assume.That(method1.IsGenericMethodDefinition);

        var constructed1 = MakeGeneric<string>(method1);
        var constructed2 = MakeGeneric<int>(method2);

        VerifyNotEqual(constructed1, constructed2);
        VerifySigNotEqual(constructed1, constructed2);
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

#if NETCOREAPP
    interface GetInterfaceMethod_DefaultInterfaceImplementation : GetInterfaceMethod_IFace
    {
        void GetInterfaceMethod_IFace.Test() { }
    }
#endif
    class GetInterfaceMethod_ExplicitImplementation : GetInterfaceMethod_IFace
    {
        void GetInterfaceMethod_IFace.Test() { }
    }

    [Test]
#if NETCOREAPP
    [TestCase(typeof(GetInterfaceMethod_DefaultInterfaceImplementation), ExpectedResult = new Type[] { typeof(GetInterfaceMethod_IFace) })]
#endif
    [TestCase(typeof(GetInterfaceMethod_ExplicitImplementation), ExpectedResult = new Type[] { typeof(GetInterfaceMethod_IFace) })]
    public Type[] Test_GetInterfaceMethod_WorksWithExplicitImplementation(Type t)
        => t!.GetInterfaceMapForInterface(typeof(GetInterfaceMethod_IFace)).TargetMethods.First().GetInterfaceMethods()!.Select(m => m.DeclaringType!).ToArray();
}
