﻿using DotNetPowerExtensions.Reflection.Models;
using System.ComponentModel.DataAnnotations;
using static DotNetPowerExtensions.Reflection.Models.MemberDetailTypes;
using static DotNetPowerExtensions.Reflection.Models.DeclarationTypes;

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

    [Test]
    [TestCase(typeof(SingleProp), Decleration, false, false, false)]
    [TestCase(typeof(SinglePropInherited), Decleration, true, false, false)]
    [TestCase(typeof(SinglePropOverriden), Override, false, false, true)]
    [TestCase(typeof(SinglePropOverridenInherited), Override, true, false, true)]
    [TestCase(typeof(SinglePropShadowed), Shadow, false, true, false)]
    [TestCase(typeof(SinglePropShadowedInherited), Shadow, true, true, false)]
    [TestCase(typeof(SinglePropShadowedOverriden), ShadowOverride, false, true, true)]
    [TestCase(typeof(SinglePropShadowedOverridenInherited), ShadowOverride, true, true, true)]

    public void Test_GetTypeInfo_SingleClassProp(Type type, DeclarationTypes decl, bool inherited, bool isShadow, bool overriden)
    {
        var result = new TypeInfoService(type).GetTypeInfo();

        result.MethodDetails.Should().BeEmpty();
        result.FieldDetails.Should().BeEmpty();
        result.EventDetails.Should().BeEmpty();
        result.PropertyDetails.Length.Should().Be(1);

        var propInfo = type.GetProperty(nameof(SingleProp.Prop))!;
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
            Validate(shadow, typeof(SinglePropOverridenInherited).GetProperty(nameof(SingleProp.Prop))!, Override, true, true);
            shadow.Name.Should().Be(nameof(SingleProp.Prop));
        }

        result.ShadowedMethodDetails.Should().BeEmpty();
    }

    class SingleMethod
    {
        public virtual void TestMethod() { }
    }
    class SingleMethodInherited : SingleMethod { }
    class SingleMethodOverriden : SingleMethodInherited
    {
        public override void TestMethod() { }
    }
    class SingleMethodOverridenInherited : SingleMethodOverriden { }
    class SingleMethodShadowed : SingleMethodOverridenInherited
    {
        public new virtual void TestMethod() { }
    }
    class SingleMethodShadowedInherited : SingleMethodShadowed { }
    class SingleMethodShadowedOverriden : SingleMethodShadowedInherited
    {
        public override void TestMethod() { }
    }
    class SingleMethodShadowedOverridenInherited : SingleMethodShadowedOverriden { }

    [Test]
    [TestCase(typeof(SingleMethod), Decleration, false, false, false)]
    [TestCase(typeof(SingleMethodInherited), Decleration, true, false, false)]
    [TestCase(typeof(SingleMethodOverriden), Override, false, false, true)]
    [TestCase(typeof(SingleMethodOverridenInherited), Override, true, false, true)]
    [TestCase(typeof(SingleMethodShadowed), Shadow, false, true, false)]
    [TestCase(typeof(SingleMethodShadowedInherited), Shadow, true, true, false)]
    [TestCase(typeof(SingleMethodShadowedOverriden), ShadowOverride, false, true, true)]
    [TestCase(typeof(SingleMethodShadowedOverridenInherited), ShadowOverride, true, true, true)]

    public void Test_GetTypeInfo_SingleClassMethod(Type type, DeclarationTypes decl, bool inherited, bool isShadow, bool overriden)
    {
        var result = new TypeInfoService(type).GetTypeInfo();

        result.PropertyDetails.Should().BeEmpty();
        result.FieldDetails.Should().BeEmpty();
        result.EventDetails.Should().BeEmpty();
        result.MethodDetails.Length.Should().Be(1);

        var methodInfo = type.GetMethod(nameof(SingleMethod.TestMethod))!;
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
            Validate(shadow, typeof(SingleMethodOverridenInherited).GetMethod(nameof(SingleMethod.TestMethod))!, Override, true, true);
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
            Validate(shadow, typeof(SingleEventOverridenInherited).GetEvent(nameof(SingleEvent.Event))!, Override, true, true);
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
    [TestCase(typeof(IDefaultImplementation) )]
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

    interface INonDefaultImplementation
    {
        int TestProp { get; set; }
        event EventHandler? TestEvent;
        void TestMethod();
    }
    class ClassNonExplicitImplementation : IDefaultImplementation
    {
        public virtual int TestProp { get; set; }
        public virtual event EventHandler? TestEvent { add => throw new NotImplementedException(); remove => throw new NotImplementedException(); }
        public virtual void TestMethod() { }
    }

    class ClassNonExplicitReimplementation : ClassNonExplicitImplementation, IDefaultImplementation
    {
        public override int TestProp { get; set; }
        public override event EventHandler? TestEvent { add => throw new NotImplementedException(); remove => throw new NotImplementedException(); }
        public override void TestMethod() { }
    }

    [Test]
    [TestCase(typeof(ClassNonExplicitImplementation))]
    [TestCase(typeof(ClassNonExplicitReimplementation))]
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
        if(hasShadow) result.ShadowedFieldDetails.First().ReflectionInfo.DeclaringType.Should().Be(typeof(SingleFieldClass));

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
        void TestMethod1() { }
        int TestMethod<T>();
        int TestMethod13();
    }
    class ComplexExample : IComplexExample
    {
        public int Field1;
        private readonly string? Field2;
        internal List<string>? Field3;
        public int Prop1 { get; }
        public int Prop2 { get; }
        public string Prop3 { get; set; }
        internal virtual string Prop4 { get; set; }

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
        result.MethodDetails.Any(md => md.Name == nameof(ComplexExample.TestMethod) && md.GenericArguments.FirstOrDefault()?.Name == "T" && !md.ArgumentTypes.Any()
                && md.ReflectionInfo.IsGenericMethodDefinition && !md.ReflectionInfo.GetParameters().Any())
            .Should().BeTrue();
        result.MethodDetails.Any(md => md.Name == nameof(ComplexExample.TestMethod) && !md.GenericArguments.Any() && md.ArgumentTypes.FirstOrDefault() == typeof(int)
                && !md.ReflectionInfo.IsGenericMethod && md.ReflectionInfo.GetParameters().FirstOrDefault()?.ParameterType == typeof(int))
            .Should().BeTrue();

        result.MethodDetails.Count(md => md.Name == nameof(ComplexExample.TestMethod1) && md.ReflectionInfo.Name == nameof(ComplexExample.TestMethod1))
                                                                                                                .Should().Be(4);
        result.MethodDetails.Any(md => md.Name == nameof(ComplexExample.TestMethod1) && !md.GenericArguments.Any() && !md.ArgumentTypes.Any()
                        && !md.ReflectionInfo.IsGenericMethod && !md.ReflectionInfo.GetParameters().Any())
                .Should().BeTrue();
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
        new[] { nameof(IComplexExample.Prop10), nameof(IComplexExample.Prop10) }
            .All(p => result.ExplicitPropertyDetails
                    .Any(pd => pd.Name == p
                            && typeof(ComplexExample).GetProperty(typeof(IComplexExample).FullName!.Replace("+", ".") +"." + p, BindingFlagsExtensions.AllBindings) == pd.ReflectionInfo))
            .Should().BeTrue();

        result.ExplicitMethodDetails.Length.Should().Be(1);
        result.ExplicitMethodDetails.All(pd => pd.DeclarationType == DeclarationTypes.ExplicitImplementation).Should().BeTrue();
        result.ExplicitMethodDetails.All(pd => !pd.IsInherited).Should().BeTrue();
        result.ExplicitMethodDetails.All(pd => pd.IsExplicit).Should().BeTrue();
        result.ExplicitMethodDetails.All(pd => pd.ExplicitInterface == typeof(IComplexExample)).Should().BeTrue();
        new[] { nameof(IComplexExample.TestMethod13) }
            .All(p => result.ExplicitMethodDetails
                    .Any(pd => pd.Name == p
                            && typeof(ComplexExample).GetMethod(typeof(IComplexExample).FullName!.Replace("+", ".") + "." + p, BindingFlagsExtensions.AllBindings) == pd.ReflectionInfo))
            .Should().BeTrue();

        result.ExplicitEventDetails.Should().BeEmpty();
    }

    // TODO... add override and shadow tests
}
