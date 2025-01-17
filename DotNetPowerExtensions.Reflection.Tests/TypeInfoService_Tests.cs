﻿using System.Diagnostics.CodeAnalysis;
using static SequelPay.DotNetPowerExtensions.Reflection.Common.DeclarationTypes;
using static SequelPay.DotNetPowerExtensions.Reflection.Common.MemberDetailTypes;

namespace DotNetPowerExtensions.Reflection.Tests;
public class TypeInfoService_Tests
{
    [Test]
    public void Test_DoesNotThrowOnArray_BugRepro()
    {
        Assert.DoesNotThrow(() => typeof(string[]).GetTypeDetailInfo());
    }

    class SingleProp
    {
        public virtual int Prop { get; set; }
    }
    class SinglePropInherited : SingleProp { }
    class SinglePropOverriden : SinglePropInherited
    {
        public override int Prop { get; set; }
    }
    class SinglePropOverridenInherited : SinglePropOverriden { }
    class SinglePropShadowed : SinglePropOverridenInherited
    {
        public new virtual int Prop { get; set; }
    }
    class SinglePropShadowedInherited : SinglePropShadowed { }
    class SinglePropShadowedOverriden : SinglePropShadowedInherited
    {
        public override int Prop { get; set; }
    }
    class SinglePropShadowedOverridenInherited : SinglePropShadowedOverriden { }
    class SinglePropComplexType
    {
        public virtual SingleProp? Prop { get; set; }
    }
    class SinglePropShadowedSubType : SinglePropComplexType
    {
        public new virtual SinglePropOverriden? Prop { get; set; }
    }
    class SingleProp_UsingSubType
    {
        public virtual SinglePropOverriden? Prop { get; set; }
    }
    class SinglePropShadowed_UsingBaseType : SingleProp_UsingSubType
    {
        public new virtual SingleProp? Prop { get; set; }
    }
    [Test]
    [TestCase(typeof(SingleProp), Decleration, false, false, false)]
    [TestCase(typeof(SinglePropInherited), Decleration, true, false, false)]
    [TestCase(typeof(SinglePropOverriden), Override, false, false, true)]
    [TestCase(typeof(SinglePropOverridenInherited), Override, true, false, true)]
    [TestCase(typeof(SinglePropShadowed), Shadow, false, true, false)]
    [TestCase(typeof(SinglePropShadowedInherited), Shadow, true, true, false)]
    [TestCase(typeof(SinglePropShadowedOverriden), ShadowOverride, false, true, true)]
    [TestCase(typeof(SinglePropShadowedOverridenInherited), ShadowOverride, true, true, true)]
    [TestCase(typeof(SinglePropComplexType), Decleration, false, false, false)]
    [TestCase(typeof(SinglePropShadowedSubType), Shadow, false, true, false, typeof(SinglePropComplexType))]
    [TestCase(typeof(SingleProp_UsingSubType), Decleration, false, false, false)]
    [TestCase(typeof(SinglePropShadowed_UsingBaseType), Shadow, false, true, false, typeof(SingleProp_UsingSubType))]

    public void Test_GetTypeInfo_SingleClassProp(Type type, DeclarationTypes decl, bool inherited,
                                        bool isShadow, bool overriden, Type? shadowedType = null)
    {
        var result = new TypeInfoService(type).GetTypeInfo();

        result.MethodDetails.Should().BeEmpty();
        result.FieldDetails.Should().BeEmpty();
        result.EventDetails.Should().BeEmpty();
        result.PropertyDetails.Length.Should().Be(1);

        // Can't do .GetProperty() because of shadowed properties
        var props = type.GetProperties().Where(p => p.Name == nameof(SingleProp.Prop)).ToArray();
        var propInfo = props.FirstOrDefault(p => p.DeclaringType == type) ?? props.Single();
        var propDetails = result.PropertyDetails.First();

        Validate(propDetails, propInfo, decl, inherited, true);
        propDetails.BackingField.Should().NotBeNull();
        propDetails.Name.Should().Be(nameof(SingleProp.Prop));

        if (overriden)
        {
            propDetails.OverridenProperty.Should().NotBeNull();
            propDetails.OverridenProperty!.DeclarationType.Should().NotBe(Override).And.NotBe(ShadowOverride);
        }
        else { propDetails.OverridenProperty.Should().BeNull(); }

        Validate(propDetails.GetMethod, propInfo.GetMethod!, decl, inherited, true);
        Validate(propDetails.SetMethod, propInfo.SetMethod!, decl, inherited, true);

        propDetails.BasePrivateGetMethod.Should().BeNull();
        propDetails.BasePrivateSetMethod.Should().BeNull();

        result.ShadowedPropertyDetails.Length.Should().Be(isShadow ? 1 : 0);
        if (isShadow)
        {
            var shadow = result.ShadowedPropertyDetails.First();
            var typeToUse = shadowedType is not null ? type : typeof(SinglePropOverridenInherited);
            var declToUse = shadowedType is not null ? Decleration : Override;

            var originalProp = typeToUse.GetProperties()
                .First(p => p.Name == nameof(SingleProp.Prop) && (shadowedType is null || p.DeclaringType == shadowedType))!;

            Validate(shadow, originalProp, declToUse, true, shadowedType is not null);
            shadow.Name.Should().Be(nameof(SingleProp.Prop));
        }

        result.ShadowedMethodDetails.Should().BeEmpty();
    }

    class SingleMethod
    {
        public virtual void TestMethod() { }
    }
    class SingleGenericMethod
    {
        public virtual void TestMethod<T>(T t) { }
    }
    class SingleMethodInherited : SingleMethod { }
    class SingleGenericMethodInherited : SingleGenericMethod { }
    class SingleMethodOverriden : SingleMethodInherited
    {
        public override void TestMethod() { }
    }
    class SingleGenericMethodOverriden : SingleGenericMethodInherited
    {
        public override void TestMethod<T>(T t) { }
    }
    class SingleMethodOverridenInherited : SingleMethodOverriden { }
    class SingleGenericMethodOverridenInherited : SingleGenericMethodOverriden { }
    class SingleMethodShadowed : SingleMethodOverridenInherited
    {
        public new virtual void TestMethod() { }
    }
    class SingleGenericMethodShadowed : SingleGenericMethodOverridenInherited
    {
        public new virtual void TestMethod<T>(T t) { }
    }
    class SingleMethodShadowedInherited : SingleMethodShadowed { }
    class SingleGenericMethodShadowedInherited : SingleGenericMethodShadowed { }
    class SingleMethodShadowedOverriden : SingleMethodShadowedInherited
    {
        public override void TestMethod() { }
    }
    class SingleGenericMethodShadowedOverriden : SingleGenericMethodShadowedInherited
    {
        public override void TestMethod<T>(T t) { }
    }
    class SingleMethodShadowedOverridenInherited : SingleMethodShadowedOverriden { }
    class SingleGenericMethodShadowedOverridenInherited : SingleGenericMethodShadowedOverriden { }

    [Test]
    [TestCase(typeof(SingleMethod), Decleration, false, false, false)]
    [TestCase(typeof(SingleMethodInherited), Decleration, true, false, false)]
    [TestCase(typeof(SingleMethodOverriden), Override, false, false, true)]
    [TestCase(typeof(SingleMethodOverridenInherited), Override, true, false, true)]
    [TestCase(typeof(SingleMethodShadowed), Shadow, false, true, false)]
    [TestCase(typeof(SingleMethodShadowedInherited), Shadow, true, true, false)]
    [TestCase(typeof(SingleMethodShadowedOverriden), ShadowOverride, false, true, true)]
    [TestCase(typeof(SingleMethodShadowedOverridenInherited), ShadowOverride, true, true, true)]
    [TestCase(typeof(SingleGenericMethod), Decleration, false, false, false)]
    [TestCase(typeof(SingleGenericMethodInherited), Decleration, true, false, false)]
    [TestCase(typeof(SingleGenericMethodOverriden), Override, false, false, true)]
    [TestCase(typeof(SingleGenericMethodOverridenInherited), Override, true, false, true)]
    [TestCase(typeof(SingleGenericMethodShadowed), Shadow, false, true, false)]
    [TestCase(typeof(SingleGenericMethodShadowedInherited), Shadow, true, true, false)]
    [TestCase(typeof(SingleGenericMethodShadowedOverriden), ShadowOverride, false, true, true)]
    [TestCase(typeof(SingleGenericMethodShadowedOverridenInherited), ShadowOverride, true, true, true)]

    public void Test_GetTypeInfo_SingleClassMethod(Type type, DeclarationTypes decl, bool inherited,
                                        bool isShadow, bool overriden)
    {
        var result = new TypeInfoService(type).GetTypeInfo();

        result.PropertyDetails.Should().BeEmpty();
        result.FieldDetails.Should().BeEmpty();
        result.EventDetails.Should().BeEmpty();
        result.MethodDetails.Length.Should().Be(1);

        var isGenericMethod = result.MethodDetails.First().GenericArguments.Any();
        var methodInfo = type.GetMethods().First(m => m.Name.StartsWith(nameof(SingleMethod.TestMethod))
                                && (m.DeclaringType == type || m.DeclaringType == type.BaseType));
        var methodDetails = result.MethodDetails.First();

        Validate(methodDetails, methodInfo, decl, inherited, true);
        methodDetails.Name.Should().Be(nameof(SingleMethod.TestMethod));

        if (overriden)
        {
            methodDetails.OverridenMethod.Should().NotBeNull();
            methodDetails.OverridenMethod!.DeclarationType.Should().NotBe(Override).And.NotBe(ShadowOverride);
        }
        else { methodDetails.OverridenMethod.Should().BeNull(); }

        result.ShadowedMethodDetails.Length.Should().Be(isShadow ? 1 : 0);
        if (isShadow)
        {
            var shadow = result.ShadowedMethodDetails.First();
            var typeToCompare = !isGenericMethod ? typeof(SingleMethodOverriden) : typeof(SingleGenericMethodOverriden);
            Validate(shadow, type.GetMethods().First(m => m.Name.StartsWith(nameof(SingleMethod.TestMethod)) && m.DeclaringType == typeToCompare), Override, true, true);
            shadow.Name.Should().Be(nameof(SingleMethod.TestMethod));
        }

        result.ShadowedPropertyDetails.Should().BeEmpty();
    }

    [Test]
    [TestCase(typeof(SingleMethod), Decleration, false, false, false)]
    [TestCase(typeof(SingleMethodInherited), Decleration, true, false, false)]
    [TestCase(typeof(SingleMethodOverriden), Override, false, false, true)]
    [TestCase(typeof(SingleMethodOverridenInherited), Override, true, false, true)]
    [TestCase(typeof(SingleMethodShadowed), Shadow, false, true, false)]
    [TestCase(typeof(SingleMethodShadowedInherited), Shadow, true, true, false)]
    [TestCase(typeof(SingleMethodShadowedOverriden), ShadowOverride, false, true, true)]
    [TestCase(typeof(SingleMethodShadowedOverridenInherited), ShadowOverride, true, true, true)]
    [TestCase(typeof(SingleGenericMethod), Decleration, false, false, false)]
    [TestCase(typeof(SingleGenericMethodInherited), Decleration, true, false, false)]
    [TestCase(typeof(SingleGenericMethodOverriden), Override, false, false, true)]
    [TestCase(typeof(SingleGenericMethodOverridenInherited), Override, true, false, true)]
    [TestCase(typeof(SingleGenericMethodShadowed), Shadow, false, true, false)]
    [TestCase(typeof(SingleGenericMethodShadowedInherited), Shadow, true, true, false)]
    [TestCase(typeof(SingleGenericMethodShadowedOverriden), ShadowOverride, false, true, true)]
    [TestCase(typeof(SingleGenericMethodShadowedOverridenInherited), ShadowOverride, true, true, true)]

    public void Test_GetTypeInfo_SingleClassMethod_WhenTypeDoesNotReturnBaseTypeMethods(Type type,
        DeclarationTypes decl, bool inherited, bool isShadow, bool overriden)
    {
        var dict = new Dictionary<MethodInfo, MethodInfo>();
        var mock = new Mock<Type>();
        mock.Setup(m => m.GetMethods(It.IsAny<BindingFlags>())).Returns<BindingFlags>(b =>
        type.GetMethods(b).Where(m => m.DeclaringType == type).Select(m =>
        {
            if(dict.TryGetValue(m, out MethodInfo? value)) return value;

            var mi = new Mock<MethodInfo>(MockBehavior.Strict);
            mi.Setup(m => m.Name).Returns(m.Name);
            mi.Setup(m => m.Attributes).Returns(m.Attributes);
            mi.Setup(m => m.GetParameters()).Returns(m.GetParameters());
            mi.Setup(m => m.IsGenericMethodDefinition).Returns(m.IsGenericMethodDefinition);
            mi.Setup(m => m.IsGenericMethod).Returns(m.IsGenericMethod);
            mi.Setup(m => m.GetGenericArguments()).Returns(m.GetGenericArguments());
            mi.Setup(m => m.ReturnParameter).Returns(m.ReturnParameter);
            mi.Setup(m => m.ReflectedType).Returns(mock.Object);
            mi.Setup(m => m.GetHashCode()).Returns(m.GetHashCode() * 12345);
            mi.Setup(m => m.Equals(It.IsAny<object>())).Returns<object>(obj => m.Equals(obj));
            if(m.DeclaringType == type) mi.Setup(m => m.DeclaringType).Returns(mock.Object);

            dict.Add(m, mi.Object);
            return mi.Object;
        }).ToArray());
        mock.Setup(m => m.BaseType).Returns(type.BaseType);

        var result = new TypeInfoService(mock.Object).GetTypeInfo();

        result.PropertyDetails.Should().BeEmpty();
        result.FieldDetails.Should().BeEmpty();
        result.EventDetails.Should().BeEmpty();
        result.MethodDetails.Length.Should().Be(1);

        var isGenericMethod = result.MethodDetails.First().GenericArguments.Any();

        var typeForMethod = inherited ? type.BaseType : type;
        var methodInfo = typeForMethod.GetMethods().First(m => m.Name.StartsWith(nameof(SingleMethod.TestMethod))
                                && (m.DeclaringType == type || m.DeclaringType == type.BaseType));
        var methodDetails = result.MethodDetails.First();

        // `InReflectionForCurrentType` should be false for inherited because we removed it
        Validate(methodDetails, dict.ContainsKey(methodInfo) ? dict[methodInfo] : methodInfo, decl, inherited, !inherited);
        methodDetails.Name.Should().Be(nameof(SingleMethod.TestMethod));

        if (overriden)
        {
            methodDetails.OverridenMethod.Should().NotBeNull();
            methodDetails.OverridenMethod!.DeclarationType.Should().NotBe(Override).And.NotBe(ShadowOverride);
        }
        else { methodDetails.OverridenMethod.Should().BeNull(); }

        result.ShadowedMethodDetails.Length.Should().Be(isShadow ? 1 : 0);
        if (isShadow)
        {
            var shadow = result.ShadowedMethodDetails.First();
            var typeToCompare = !isGenericMethod ? typeof(SingleMethodOverriden) : typeof(SingleGenericMethodOverriden);
            Validate(shadow, type.BaseType!.GetMethods().First(m => m.Name.StartsWith(nameof(SingleMethod.TestMethod)) && m.DeclaringType == typeToCompare), Override, true, false);
            shadow.Name.Should().Be(nameof(SingleMethod.TestMethod));
        }

        result.ShadowedPropertyDetails.Should().BeEmpty();
    }

    class SingleEvent
    {
        public virtual event EventHandler? Event;
    }
    class SingleEventInherited : SingleEvent { }
    class SingleEventOverriden : SingleEventInherited
    {
        public override event EventHandler? Event;
    }
    class SingleEventOverridenInherited : SingleEventOverriden { }
    class SingleEventShadowed : SingleEventOverridenInherited
    {
        public new virtual event EventHandler? Event;
    }
    class SingleEventShadowedInherited : SingleEventShadowed { }
    class SingleEventShadowedOverriden : SingleEventShadowedInherited
    {
        public override event EventHandler? Event;
    }
    class SingleEventShadowedOverridenInherited : SingleEventShadowedOverriden { }

    [Test]
    [TestCase(typeof(SingleEvent), Decleration, false, false, false)]
    [TestCase(typeof(SingleEventInherited), Decleration, true, false, false)]
    [TestCase(typeof(SingleEventOverriden), Override, false, false, true)]
    [TestCase(typeof(SingleEventOverridenInherited), Override, true, false, true)]
    [TestCase(typeof(SingleEventShadowed), Shadow, false, true, false)]
    [TestCase(typeof(SingleEventShadowedInherited), Shadow, true, true, false)]
    [TestCase(typeof(SingleEventShadowedOverriden), ShadowOverride, false, true, true)]
    [TestCase(typeof(SingleEventShadowedOverridenInherited), ShadowOverride, true, true, true)]

    public void Test_GetTypeInfo_SingleClassEvent(Type type, DeclarationTypes decl, bool inherited, bool isShadow, bool overriden)
    {
        var result = new TypeInfoService(type).GetTypeInfo();

        result.MethodDetails.Should().BeEmpty();
        result.FieldDetails.Should().BeEmpty();
        result.PropertyDetails.Should().BeEmpty();
        result.EventDetails.Length.Should().Be(1);

        var eventInfo = type.GetEvent(nameof(SingleEvent.Event))!;
        var eventDetails = result.EventDetails.First();

        Validate(eventDetails, eventInfo, decl, inherited, true);
        eventDetails.BackingField.Should().NotBeNull();
        eventDetails.Name.Should().Be(nameof(SingleEvent.Event));

        if (overriden)
        {
            eventDetails.OverridenEvent.Should().NotBeNull();
            eventDetails.OverridenEvent!.DeclarationType.Should().NotBe(Override).And.NotBe(ShadowOverride);
        }
        else { eventDetails.OverridenEvent.Should().BeNull(); }

        Validate(eventDetails.AddMethod, eventInfo.AddMethod!, decl, inherited, true);
        Validate(eventDetails.RemoveMethod, eventInfo.RemoveMethod!, decl, inherited, true);

        result.ShadowedEventDetails.Length.Should().Be(isShadow ? 1 : 0);
        if (isShadow)
        {
            var shadow = result.ShadowedEventDetails.First();
            Validate(shadow, typeof(SingleEventOverridenInherited).GetEvent(nameof(SingleEvent.Event))!, Override, true, false);
            shadow.Name.Should().Be(nameof(SingleEvent.Event));
        }

        result.ShadowedMethodDetails.Should().BeEmpty();
    }

    interface IExplicitImplementation
    {
        int TestProp { get; set; }
        event EventHandler? TestEvent;
        void TestMethod();
    }
    class ClassExplicitImplementation : IExplicitImplementation
    {
        int IExplicitImplementation.TestProp { get; set; }
        event EventHandler? IExplicitImplementation.TestEvent { add => throw new NotImplementedException(); remove => throw new NotImplementedException(); }
        void IExplicitImplementation.TestMethod() { }
    }
    [Test]
    public void Test_GetTypeInfo_ExplicitImplementation()
    {
        var result = new TypeInfoService(typeof(ClassExplicitImplementation)).GetTypeInfo();

        result.PropertyDetails.Should().BeEmpty();
        result.ShadowedPropertyDetails.Should().BeEmpty();
        result.EventDetails.Should().BeEmpty();
        result.ShadowedEventDetails.Should().BeEmpty();
        result.FieldDetails.Should().BeEmpty();
        result.ShadowedFieldDetails.Should().BeEmpty();
        result.MethodDetails.Should().BeEmpty();
        result.ShadowedMethodDetails.Should().BeEmpty();

        result.ExplicitPropertyDetails.Length.Should().Be(1);
        var prop = result.ExplicitPropertyDetails.First();
        prop.Name.Should().Be(nameof(IExplicitImplementation.TestProp));
        prop.IsExplicit.Should().BeTrue();
        prop.ExplicitInterface.Should().Be(typeof(IExplicitImplementation));
        prop.DeclarationType.Should().Be(DeclarationTypes.ExplicitImplementation);

        result.ExplicitEventDetails.Length.Should().Be(1);
        var evt = result.ExplicitEventDetails.First();
        evt.Name.Should().Be(nameof(IExplicitImplementation.TestEvent));
        evt.IsExplicit.Should().BeTrue();
        evt.ExplicitInterface.Should().Be(typeof(IExplicitImplementation));
        evt.DeclarationType.Should().Be(DeclarationTypes.ExplicitImplementation);

        result.ExplicitMethodDetails.Length.Should().Be(1);
        var method = result.ExplicitMethodDetails.First();
        method.Name.Should().Be(nameof(IExplicitImplementation.TestMethod));
        method.IsExplicit.Should().BeTrue();
        method.ExplicitInterface.Should().Be(typeof(IExplicitImplementation));
        method.DeclarationType.Should().Be(DeclarationTypes.ExplicitImplementation);
    }
#if NETCOREAPP

    interface IDefaultImplementation
    {
        public int TestProp { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public event EventHandler? TestEvent { add => throw new NotImplementedException(); remove => throw new NotImplementedException(); }
        public void TestMethod() { }
    }
    interface IDefaultFirstOverride : IDefaultImplementation
    {
        int IDefaultImplementation.TestProp { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        event EventHandler? IDefaultImplementation.TestEvent { add => throw new NotImplementedException(); remove => throw new NotImplementedException(); }
        void IDefaultImplementation.TestMethod() { }
    }
    interface IDefaultOverride : IDefaultFirstOverride
    {
        int IDefaultImplementation.TestProp { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        event EventHandler? IDefaultImplementation.TestEvent { add => throw new NotImplementedException(); remove => throw new NotImplementedException(); }
        void IDefaultImplementation.TestMethod() { }
    }
    class ClassDefaultImplementation : IDefaultImplementation
    {
    }
    class ClassDefaultOverride : IDefaultOverride
    {
    }
    [Test]
    [TestCase(typeof(IDefaultImplementation))]
    [TestCase(typeof(ClassDefaultImplementation))]
    public void Test_GetTypeInfo_DefaultInterfaceImplementation(Type type)
    {
        var result = new TypeInfoService(type).GetTypeInfo();

        result.PropertyDetails.Should().BeEmpty();
        result.ShadowedPropertyDetails.Should().BeEmpty();
        result.EventDetails.Should().BeEmpty();
        result.ShadowedEventDetails.Should().BeEmpty();
        result.FieldDetails.Should().BeEmpty();
        result.ShadowedFieldDetails.Should().BeEmpty();
        result.MethodDetails.Should().BeEmpty();
        result.ShadowedMethodDetails.Should().BeEmpty();

        result.ExplicitPropertyDetails.Length.Should().Be(1);
        var prop = result.ExplicitPropertyDetails.First();
        prop.Name.Should().Be(nameof(IDefaultImplementation.TestProp));
        prop.IsExplicit.Should().BeTrue();
        prop.ExplicitInterface.Should().Be(typeof(IDefaultImplementation));
        prop.DeclarationType.Should().Be(Decleration);

        result.ExplicitEventDetails.Length.Should().Be(1);
        var evt = result.ExplicitEventDetails.First();
        evt.Name.Should().Be(nameof(IDefaultImplementation.TestEvent));
        evt.IsExplicit.Should().BeTrue();
        evt.ExplicitInterface.Should().Be(typeof(IDefaultImplementation));
        evt.DeclarationType.Should().Be(Decleration);

        result.ExplicitMethodDetails.Length.Should().Be(1);
        var method = result.ExplicitMethodDetails.First();
        method.Name.Should().Be(nameof(IDefaultImplementation.TestMethod));
        method.IsExplicit.Should().BeTrue();
        method.ExplicitInterface.Should().Be(typeof(IDefaultImplementation));
        method.DeclarationType.Should().Be(Decleration);
    }

    [Test]
    [TestCase(typeof(IDefaultOverride))]
    [TestCase(typeof(ClassDefaultOverride))]
    public void Test_GetTypeInfo_DefaultOverride(Type type)
    {
        var result = new TypeInfoService(type).GetTypeInfo();

        result.PropertyDetails.Should().BeEmpty();
        result.ShadowedPropertyDetails.Should().BeEmpty();
        result.FieldDetails.Should().BeEmpty();
        result.ShadowedFieldDetails.Should().BeEmpty();
        result.MethodDetails.Should().BeEmpty();
        result.ShadowedMethodDetails.Should().BeEmpty();

        result.ExplicitPropertyDetails.Length.Should().Be(1);
        var prop = result.ExplicitPropertyDetails.First();
        prop.Name.Should().Be(nameof(IDefaultImplementation.TestProp));
        prop.ReflectionInfo.DeclaringType.Should().Be(typeof(IDefaultOverride));
        prop.IsExplicit.Should().BeTrue();
        prop.ExplicitInterface.Should().Be(typeof(IDefaultImplementation));
        prop.DeclarationType.Should().Be(DeclarationTypes.ExplicitImplementation);

        result.ExplicitMethodDetails.Length.Should().Be(1);
        var method = result.ExplicitMethodDetails.First();
        method.Name.Should().Be(nameof(IDefaultImplementation.TestMethod));
        method.ReflectionInfo.DeclaringType.Should().Be(typeof(IDefaultOverride));
        method.IsExplicit.Should().BeTrue();
        method.ExplicitInterface.Should().Be(typeof(IDefaultImplementation));
        method.DeclarationType.Should().Be(DeclarationTypes.ExplicitImplementation);
    }
#endif
    interface INonDefaultImplementation
    {
        int TestProp { get; set; }
        event EventHandler? TestEvent;
        void TestMethod();
    }
    class ClassNonExplicitImplementation : INonDefaultImplementation
    {
        public virtual int TestProp { get; set; }
        public virtual event EventHandler? TestEvent { add => throw new NotImplementedException(); remove => throw new NotImplementedException(); }
        public virtual void TestMethod() { }
    }

    class ClassNonExplicitReimplementation : ClassNonExplicitImplementation, INonDefaultImplementation
    {
        public override int TestProp { get; set; }
        public override event EventHandler? TestEvent { add => throw new NotImplementedException(); remove => throw new NotImplementedException(); }
        public override void TestMethod() { }
    }

    class ClassExplicitImplementation_OnNonExplicitReimplementation : ClassNonExplicitReimplementation, INonDefaultImplementation
    {
        int INonDefaultImplementation.TestProp { get; set; }
        event EventHandler? INonDefaultImplementation.TestEvent { add => throw new NotImplementedException(); remove => throw new NotImplementedException(); }
        void INonDefaultImplementation.TestMethod() { }
    }

    class ClassExplicitImplementation_OnNonExplicitReimplementationSub : ClassExplicitImplementation_OnNonExplicitReimplementation, INonDefaultImplementation
    {
    }

    class ClassOverrideNonImplmentation_OnExplicitImplementation : ClassExplicitImplementation_OnNonExplicitReimplementationSub
    {
        public override int TestProp { get; set; }
        public override event EventHandler? TestEvent { add => throw new NotImplementedException(); remove => throw new NotImplementedException(); }
        public override void TestMethod() { }
    }

    class ClassOverrideNonImplmentation_OnExplicitImplementation_Sub : ClassOverrideNonImplmentation_OnExplicitImplementation
    {
    }

    class Class_Reimplmentation_Nothing_On_OverrideNonImplmentation_OnExplicitImplementation : ClassOverrideNonImplmentation_OnExplicitImplementation, INonDefaultImplementation
    {
    }

    class Class_Reimplmentation_Nothing_On_OverrideNonImplmentation_OnExplicitImplementation_Sub : Class_Reimplmentation_Nothing_On_OverrideNonImplmentation_OnExplicitImplementation
    {
    }

    class Class_ReimplmentationImplicit_OnExplicitImplementation : Class_Reimplmentation_Nothing_On_OverrideNonImplmentation_OnExplicitImplementation, INonDefaultImplementation
    {
        public override int TestProp { get; set; }
        public override event EventHandler? TestEvent { add => throw new NotImplementedException(); remove => throw new NotImplementedException(); }
        public override void TestMethod() { }
    }

    class Class_ReimplmentationImplicit_OnExplicitImplementation_Sub : Class_ReimplmentationImplicit_OnExplicitImplementation
    { }

    class Class_ReimplmentationNothing_OnReimplmentationImplicit_OnExplicitImplementation : Class_ReimplmentationImplicit_OnExplicitImplementation_Sub, INonDefaultImplementation
    { }

    class Class_ReimplmentationNothing_OnReimplmentationImplicit_OnExplicitImplementation_Sub : Class_ReimplmentationNothing_OnReimplmentationImplicit_OnExplicitImplementation
    { }

    [Test]
    [TestCase(typeof(ClassNonExplicitImplementation))]
    [TestCase(typeof(ClassNonExplicitReimplementation))]
    [TestCase(typeof(Class_ReimplmentationImplicit_OnExplicitImplementation))]
    [TestCase(typeof(Class_ReimplmentationImplicit_OnExplicitImplementation_Sub))]
    [TestCase(typeof(Class_ReimplmentationNothing_OnReimplmentationImplicit_OnExplicitImplementation))]
    [TestCase(typeof(Class_ReimplmentationNothing_OnReimplmentationImplicit_OnExplicitImplementation_Sub))]
    public void Test_GetTypeInfo_NonExplicitImplementation(Type type)
    {
        var result = new TypeInfoService(type).GetTypeInfo();

        result.MethodDetails.Length.Should().Be(1);
        result.FieldDetails.Should().BeEmpty();
        result.PropertyDetails.Length.Should().Be(1);
        result.EventDetails.Length.Should().Be(1);

        result.ShadowedPropertyDetails.Should().BeEmpty();
        result.ShadowedEventDetails.Should().BeEmpty();
        result.ShadowedFieldDetails.Should().BeEmpty();
        result.ShadowedMethodDetails.Should().BeEmpty();

        result.ExplicitPropertyDetails.Should().BeEmpty();
        result.ExplicitEventDetails.Should().BeEmpty();
        result.ExplicitMethodDetails.Should().BeEmpty();
    }

    [Test]
    [TestCase(typeof(ClassExplicitImplementation_OnNonExplicitReimplementation))]
    [TestCase(typeof(ClassExplicitImplementation_OnNonExplicitReimplementationSub))]
    [TestCase(typeof(Class_Reimplmentation_Nothing_On_OverrideNonImplmentation_OnExplicitImplementation))]
    [TestCase(typeof(Class_Reimplmentation_Nothing_On_OverrideNonImplmentation_OnExplicitImplementation_Sub))]
    public void Test_GetTypeInfo_ExplicitReimplementation_OnNonExplicitImplementation(Type type)
    {
        var result = new TypeInfoService(type).GetTypeInfo();

        result.MethodDetails.Length.Should().Be(1);
        result.FieldDetails.Should().BeEmpty();
        result.PropertyDetails.Length.Should().Be(1);
        result.EventDetails.Length.Should().Be(1);

        result.ShadowedPropertyDetails.Should().BeEmpty();
        result.ShadowedEventDetails.Should().BeEmpty();
        result.ShadowedFieldDetails.Should().BeEmpty();
        result.ShadowedMethodDetails.Should().BeEmpty();

        result.ExplicitMethodDetails.Length.Should().Be(1);
        result.ExplicitPropertyDetails.Length.Should().Be(1);
        result.ExplicitEventDetails.Length.Should().Be(1);
    }


    class SingleFieldClass
    {
        public int TestField = 10;
    }
    sealed class SingleFieldClassInherited : SingleFieldClass { }
    class SingleFieldClassShadowed : SingleFieldClass
    {
        public new int TestField = 20;
    }
    sealed class SingleFieldClassShadowedInherited : SingleFieldClassShadowed { }
    [Test]
    [TestCase(typeof(SingleFieldClass), typeof(SingleFieldClass), false, false)]
    [TestCase(typeof(SingleFieldClassInherited), typeof(SingleFieldClass), false, true)]
    [TestCase(typeof(SingleFieldClassShadowed), typeof(SingleFieldClassShadowed), true, false)]
    [TestCase(typeof(SingleFieldClassShadowedInherited), typeof(SingleFieldClassShadowed), true, true)]
    public void Test_GetTypeInfo_SingleField(Type type, Type declaringType, bool hasShadow, bool isInherited)
    {
        var result = new TypeInfoService(type).GetTypeInfo();

        result.PropertyDetails.Should().BeEmpty();
        result.ShadowedPropertyDetails.Should().BeEmpty();
        result.MethodDetails.Should().BeEmpty();
        result.ShadowedMethodDetails.Should().BeEmpty();

        result.FieldDetails.Should().NotBeEmpty();
        result.ShadowedFieldDetails.Length.Should().Be(hasShadow ? 1 : 0);
        if (hasShadow) result.ShadowedFieldDetails.First().ReflectionInfo.DeclaringType.Should().Be(typeof(SingleFieldClass));

        var field = result.FieldDetails.First();
        field.Name.Should().Be(nameof(SingleFieldClass.TestField));
        field.ReflectionInfo.DeclaringType.Should().Be(declaringType);
        field.DeclarationType.Should().Be(hasShadow ? DeclarationTypes.Shadow : DeclarationTypes.Decleration);
        field.IsInherited.Should().Be(isInherited);
    }

    private static void Validate<TDetail, TInfo>(TDetail? detail, TInfo reflectionInfo, DeclarationTypes declaration,
                    bool inherited, bool inReflection, MemberDetailTypes? detailType = null)
    where TDetail : MemberDetail<TInfo>
    where TInfo : MemberInfo
    {
        detail.Should().NotBeNull();
        detail!.ReflectionInfo.Should().BeSameAs(reflectionInfo);
        detail.DeclarationType.Should().Be(declaration);
        detail.MemberDetailType.Should().Be(detailType ?? detail switch
        {
            PropertyDetail => Property,
            MethodDetail => Method,
            EventDetail => Event,
            FieldDetail => Field,
            _ => throw new ArgumentOutOfRangeException(nameof(detailType)),
        });
        detail.InReflectionForCurrentType.Should().Be(inReflection);
        detail.IsInherited.Should().Be(inherited);
    }

    interface IComplexExample
    {
        int Prop10 { get; }
        int Prop11 { get; }
#if NETCOREAPP
        void TestMethod1() { }
#endif
        int TestMethod<T>();
        int TestMethod13();
        int TestMethod13(int i);
        int TestMethod13<T>();
    }
    class ComplexExample : IComplexExample
    {
        public int Field1;
        [SuppressMessage("Performance", "CA1823:Avoid unused private fields", Justification = "used via Reflection")]
        private readonly string? Field2;
        internal List<string>? Field3;
        public int Prop1 { get; }
        public int Prop2 { get; }
        public string? Prop3 { get; set; }
        internal virtual string? Prop4 { get; set; }

        int IComplexExample.Prop10 => throw new NotImplementedException();

        int IComplexExample.Prop11 => throw new NotImplementedException();

        public void TestMethod() { }
        public virtual int TestMethod<T>() => 10;
        public void TestMethod(int i) { }

        public void TestMethod1() { }
        public virtual int TestMethod1<T>() => 10;
        public void TestMethod1(int i) { }
        public virtual string TestMethod1(int i, string s) => "tt";

        int IComplexExample.TestMethod13() => throw new NotImplementedException();
        int IComplexExample.TestMethod13(int i) => throw new NotImplementedException();
        int IComplexExample.TestMethod13<T>() => throw new NotImplementedException();
        public event EventHandler? e1;
        public virtual event EventHandler? e2;
    }
    [Test]
    public void Test_GetTypeInfo_ComplexExample()
    {
        var result = new TypeInfoService(typeof(ComplexExample)).GetTypeInfo();

        result.PropertyDetails.Length.Should().Be(4);
        new[] { nameof(ComplexExample.Prop1), nameof(ComplexExample.Prop2), nameof(ComplexExample.Prop3), nameof(ComplexExample.Prop4) }
            .All(p => result.PropertyDetails.Any(pd => pd.Name == p && typeof(ComplexExample).GetProperty(p, BindingFlagsExtensions.AllBindings) == pd.ReflectionInfo))
            .Should().BeTrue();
        result.PropertyDetails.All(pd => pd.DeclarationType == DeclarationTypes.Decleration).Should().BeTrue();
        result.PropertyDetails.All(pd => !pd.IsInherited).Should().BeTrue();

        result.MethodDetails.Length.Should().Be(7);
        result.MethodDetails.All(pd => pd.DeclarationType == DeclarationTypes.Decleration).Should().BeTrue();
        result.MethodDetails.All(pd => !pd.IsInherited).Should().BeTrue();
        result.MethodDetails.Count(md => md.Name == nameof(ComplexExample.TestMethod) && md.ReflectionInfo.Name == nameof(ComplexExample.TestMethod))
                                                                                                                .Should().Be(3);
        result.MethodDetails.Any(md => md.Name == nameof(ComplexExample.TestMethod) && !md.GenericArguments.Any() && !md.ArgumentTypes.Any()
                && !md.ReflectionInfo.IsGenericMethod && !md.ReflectionInfo.GetParameters().Any())
            .Should().BeTrue();

#pragma warning disable NullConditionalAssertion // Code Smell, Justification conditional is inside expression
        result.MethodDetails.Any(md => md.Name == nameof(ComplexExample.TestMethod) && md.GenericArguments.FirstOrDefault()?.Name == "T" && !md.ArgumentTypes.Any()
                && md.ReflectionInfo.IsGenericMethodDefinition && !md.ReflectionInfo.GetParameters().Any())
            .Should().BeTrue();
        result.MethodDetails.Any(md => md.Name == nameof(ComplexExample.TestMethod) && !md.GenericArguments.Any() && md.ArgumentTypes.FirstOrDefault() == typeof(int)
                && !md.ReflectionInfo.IsGenericMethod && md.ReflectionInfo.GetParameters().FirstOrDefault()?.ParameterType == typeof(int))
            .Should().BeTrue();
#pragma warning restore NullConditionalAssertion // Code Smell

        result.MethodDetails.Count(md => md.Name == nameof(ComplexExample.TestMethod1) && md.ReflectionInfo.Name == nameof(ComplexExample.TestMethod1))
                                                                                                                .Should().Be(4);
        result.MethodDetails.Any(md => md.Name == nameof(ComplexExample.TestMethod1) && !md.GenericArguments.Any() && !md.ArgumentTypes.Any()
                        && !md.ReflectionInfo.IsGenericMethod && !md.ReflectionInfo.GetParameters().Any())
                .Should().BeTrue();

#pragma warning disable NullConditionalAssertion // Code Smell, Justification conditional is inside expression
        result.MethodDetails.Any(md => md.Name == nameof(ComplexExample.TestMethod1) && md.GenericArguments.FirstOrDefault()?.Name == "T" && !md.ArgumentTypes.Any()
                        && md.ReflectionInfo.IsGenericMethod && !md.ReflectionInfo.GetParameters().Any())
                .Should().BeTrue();
        result.MethodDetails.Any(md => md.Name == nameof(ComplexExample.TestMethod1) && !md.GenericArguments.Any() && md.ArgumentTypes.FirstOrDefault() == typeof(int)
                        && !md.ReflectionInfo.IsGenericMethod && md.ReflectionInfo.GetParameters().FirstOrDefault()?.ParameterType == typeof(int))
                .Should().BeTrue();
        result.MethodDetails.Any(md => md.Name == nameof(ComplexExample.TestMethod1) && !md.GenericArguments.Any()
            && md.ArgumentTypes.FirstOrDefault() == typeof(int) && md.ArgumentTypes.LastOrDefault() == typeof(string)
            && !md.ReflectionInfo.IsGenericMethod
            && md.ReflectionInfo.GetParameters().FirstOrDefault()?.ParameterType == typeof(int)
                                        && md.ReflectionInfo.GetParameters().LastOrDefault()?.ParameterType == typeof(string))
                .Should().BeTrue();
#pragma warning restore NullConditionalAssertion // Code Smell

        result.FieldDetails.Length.Should().Be(3);
        result.FieldDetails.All(pd => pd.DeclarationType == DeclarationTypes.Decleration).Should().BeTrue();
        result.FieldDetails.All(pd => !pd.IsInherited).Should().BeTrue();
        new[] { nameof(ComplexExample.Field1), "Field2", nameof(ComplexExample.Field3) }
                .All(p => result.FieldDetails.Any(pd => pd.Name == p && typeof(ComplexExample).GetField(p, BindingFlagsExtensions.AllBindings) == pd.ReflectionInfo))
                .Should().BeTrue();

        result.EventDetails.Length.Should().Be(2);
        result.EventDetails.All(pd => pd.DeclarationType == DeclarationTypes.Decleration).Should().BeTrue();
        result.EventDetails.All(pd => !pd.IsInherited).Should().BeTrue();
        new[] { nameof(ComplexExample.e1), nameof(ComplexExample.e2) }
            .All(p => result.EventDetails.Any(pd => pd.Name == p && typeof(ComplexExample).GetEvent(p, BindingFlagsExtensions.AllBindings) == pd.ReflectionInfo))
            .Should().BeTrue();

        result.ShadowedPropertyDetails.Should().BeEmpty();
        result.ShadowedMethodDetails.Should().BeEmpty();
        result.ShadowedEventDetails.Should().BeEmpty();
        result.ShadowedFieldDetails.Should().BeEmpty();

        result.ExplicitPropertyDetails.Length.Should().Be(2);
        result.ExplicitPropertyDetails.All(pd => pd.DeclarationType == DeclarationTypes.ExplicitImplementation).Should().BeTrue();
        result.ExplicitPropertyDetails.All(pd => !pd.IsInherited).Should().BeTrue();
        result.ExplicitPropertyDetails.All(pd => pd.IsExplicit).Should().BeTrue();
        result.ExplicitPropertyDetails.All(pd => pd.ExplicitInterface == typeof(IComplexExample)).Should().BeTrue();
        result.ExplicitPropertyDetails.All(pd => pd.ExplicitInterfaceReflectionInfo is not null).Should().BeTrue();
        result.ExplicitPropertyDetails.All(pd => pd.ExplicitInterfaceReflectionInfo!.ReflectedType == typeof(IComplexExample)).Should().BeTrue();
        new[] { nameof(IComplexExample.Prop10), nameof(IComplexExample.Prop10) }
            .All(p => result.ExplicitPropertyDetails
                    .Any(pd => pd.Name == p
                            && typeof(ComplexExample).GetProperty(typeof(IComplexExample).FullName!.Replace("+", ".") + "." + p, BindingFlagsExtensions.AllBindings) == pd.ReflectionInfo))
            .Should().BeTrue();

        result.ExplicitMethodDetails.Length.Should().Be(3);
        result.ExplicitMethodDetails.All(md => md.DeclarationType == DeclarationTypes.ExplicitImplementation).Should().BeTrue();
        result.ExplicitMethodDetails.All(md => !md.IsInherited).Should().BeTrue();
        result.ExplicitMethodDetails.All(md => md.IsExplicit).Should().BeTrue();
        result.ExplicitMethodDetails.All(md => md.ExplicitInterface == typeof(IComplexExample)).Should().BeTrue();
        result.ExplicitMethodDetails.All(md => md.ExplicitInterfaceReflectionInfo is not null).Should().BeTrue();
        result.ExplicitMethodDetails.All(md => md.ExplicitInterfaceReflectionInfo!.ReflectedType == typeof(IComplexExample)).Should().BeTrue();

        result.ExplicitMethodDetails
            .All(md => typeof(ComplexExample).GetMethods(BindingFlagsExtensions.AllBindings).Contains(md.ReflectionInfo))
        .Should().BeTrue();

        const string ifaceMethodName = nameof(IComplexExample.TestMethod13);

        result.ExplicitMethodDetails
            .All(md => md.Name == ifaceMethodName
                && md.ReflectionInfo.Name == typeof(IComplexExample).FullName!.Replace("+", ".") + "." + ifaceMethodName
                && md.ExplicitInterfaceReflectionInfo!.Name == ifaceMethodName)
        .Should().BeTrue();

        result.ExplicitMethodDetails.Any(md => !md.GenericArguments.Any() && !md.ArgumentTypes.Any()
                 && !md.ReflectionInfo.IsGenericMethod && !md.ReflectionInfo.GetParameters().Any()
                 && !md.ExplicitInterfaceReflectionInfo!.IsGenericMethod && !md.ExplicitInterfaceReflectionInfo.GetParameters().Any())
            .Should().BeTrue();
#pragma warning disable NullConditionalAssertion // Code Smell, Justification conditional is inside expression
        result.ExplicitMethodDetails.Any(md =>
                md.GenericArguments.FirstOrDefault()?.Name == "T" && !md.ArgumentTypes.Any()
                && md.ReflectionInfo.IsGenericMethod && !md.ReflectionInfo.GetParameters().Any()
                && md.ExplicitInterfaceReflectionInfo!.IsGenericMethod && !md.ExplicitInterfaceReflectionInfo.GetParameters().Any())
            .Should().BeTrue();
        result.ExplicitMethodDetails.Any(md =>
                !md.GenericArguments.Any() && md.ArgumentTypes.FirstOrDefault() == typeof(int)
                && !md.ReflectionInfo.IsGenericMethod && md.ReflectionInfo.GetParameters().FirstOrDefault()?.ParameterType == typeof(int)
                && !md.ExplicitInterfaceReflectionInfo!.IsGenericMethod && md.ExplicitInterfaceReflectionInfo.GetParameters().FirstOrDefault()?.ParameterType == typeof(int))
            .Should().BeTrue();
#pragma warning restore NullConditionalAssertion // Code Smell

        result.ExplicitEventDetails.Should().BeEmpty();
    }

    class BaseType
    {
        public BaseType? Prop { get; set; }
    }
    class SubType : BaseType
    {
        public new SubType? Prop { get; set; }
    }
    class SubTypeSub : SubType { }
    class SubSubType : SubTypeSub
    {
        public new SubSubType? Prop { get; set; }
    }
    class SubSubTypeSub : SubSubType { }

    [Test]
    public void Test_HandlesCorrectly_Shadow_WhenDifferentType_BugRepro()
    {
        var typeInfo = typeof(SubType).GetTypeDetailInfo();

        var props = typeInfo.PropertyDetails;
        props.Should().NotBeNull();
        props.Length.Should().Be(1);

        props.First().PropertyType.Should().Be(typeof(SubType));
        props.First().DeclarationType.Should().Be(Shadow);

        var shadowed = typeInfo.ShadowedPropertyDetails;
        shadowed.Should().NotBeNull();
        shadowed.Length.Should().Be(1);

        shadowed.First().PropertyType.Should().Be(typeof(BaseType));
        shadowed.First().DeclarationType.Should().Be(Decleration);

        typeInfo = typeof(SubTypeSub).GetTypeDetailInfo();

        props = typeInfo.PropertyDetails;
        props.Should().NotBeNull();
        props.Length.Should().Be(1);

        props.First().PropertyType.Should().Be(typeof(SubType));
        props.First().DeclarationType.Should().Be(Shadow);

        shadowed = typeInfo.ShadowedPropertyDetails;
        shadowed.Should().NotBeNull();
        shadowed.Length.Should().Be(1);

        shadowed.First().PropertyType.Should().Be(typeof(BaseType));
        shadowed.First().DeclarationType.Should().Be(Decleration);

        typeInfo = typeof(SubSubType).GetTypeDetailInfo();

        props = typeInfo.PropertyDetails;
        props.Should().NotBeNull();
        props.Length.Should().Be(1);

        props.First().PropertyType.Should().Be(typeof(SubSubType));
        props.First().DeclarationType.Should().Be(Shadow);

        shadowed = typeInfo.ShadowedPropertyDetails;
        shadowed.Should().NotBeNull();
        shadowed.Length.Should().Be(2);

        typeInfo = typeof(SubSubTypeSub).GetTypeDetailInfo();

        props = typeInfo.PropertyDetails;
        props.Should().NotBeNull();
        props.Length.Should().Be(1);

        props.First().PropertyType.Should().Be(typeof(SubSubType));
        props.First().DeclarationType.Should().Be(Shadow);

        shadowed = typeInfo.ShadowedPropertyDetails;
        shadowed.Should().NotBeNull();
        shadowed.Length.Should().Be(2);
    }

    //[Test]
    //[TestCase(typeof(object))]
    //[TestCase(typeof(List<string>))]
    //[TestCase(typeof(string[]))]
    //[TestCase(typeof(List<string>[]))]
    //[TestCase(typeof(Dictionary<int, List<string>>))]
    //[TestCase(typeof(Dictionary<int[], List<string>[]>[]))]
    //[TestCase(typeof(Type))]
    //[TestCase(typeof(TypeDetailInfo))]
    //[TestCase(typeof(ICollection<TypeDetailInfo>))]
    //[TestCase(typeof(ICollection<>))]
    //[TestCase(typeof(Dictionary<,>))]
    //[TestCase(typeof(Union<,>))]
    //[TestCase(typeof(Union<,,>))]
    //[TestCase(typeof(Union<List<string>, Process, IQueryable<MustInitializeAttribute>>))]
    //public Task Test_Types(Type t)
    //{
    //    var settings = new VerifySettings();
    //    settings.AddExtraSettings(s => s.ContractResolver = new WritablePropertiesOnlyResolver());
    //    //settings.AddExtraSettings(s => s.DefaultValueHandling = DefaultValueHandling.Include);
    //    //static Task<CompareResult> CompareImages(
    //    //    Stream received,
    //    //    Stream verified,
    //    //    IReadOnlyDictionary<string, object> context)
    //    //{
    //    //    // Fake comparison
    //    //    if (received.Length == verified.Length)
    //    //    {
    //    //        return Task.FromResult(CompareResult.Equal);
    //    //    }

    //    //    var result = CompareResult.NotEqual();
    //    //    return Task.FromResult(result);
    //    //};
    //    //settings.UseStreamComparer(CompareImages);
    //    //settings.IgnoreMembers<TypeDetailInfo>(_ => _.ConstructorDetails, _=> _.PropertyDetails.);
    //    return Verify(t.GetTypeDetailInfo(), settings);

    //        //_ => _.ExplicitDetail,
    //        //_ => _.IsGeneric,
    //        //_ => _.IsGenericDefinition,
    //        //_ => _.GenericDefinition)   ;
    //}

    //// From https://stackoverflow.com/a/34112601/640195
    //class WritablePropertiesOnlyResolver : DefaultContractResolver
    //{
    //    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    //    {
    //        var property = base.CreateProperty(member, memberSerialization);
    //        property.ShouldSerialize = _ => ShouldSerialize(member);
    //        return property;
    //    }

    //    internal static bool ShouldSerialize(MemberInfo memberInfo)
    //    {
    //        var propertyInfo = memberInfo as PropertyInfo;
    //        if (propertyInfo == null)
    //        {
    //            return false;
    //        }
    //        if (new[] { typeof(TypeDetailInfo), typeof(ITypeDetailInfo), typeof(TypeDetailInfo[]), typeof(ITypeDetailInfo[]) }.Contains(propertyInfo.PropertyType)) return false;
    //        return true;
    //        //if (propertyInfo.SetMethod != null)
    //        //{
    //        //    return true;
    //        //}



    //        //var getMethod = propertyInfo.GetMethod;
    //        //return Attribute.GetCustomAttribute(getMethod, typeof(CompilerGeneratedAttribute)) != null;
    //    }
    //}
    // TODO... add override and shadow tests
}
