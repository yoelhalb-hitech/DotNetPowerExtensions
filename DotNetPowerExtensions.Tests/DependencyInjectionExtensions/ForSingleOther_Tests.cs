using Microsoft.Extensions.DependencyInjection;
using Moq;
using SequelPay.DotNetPowerExtensions;
using static DotNetPowerExtensions.Tests.DependencyInjectionExtensions.Utils;

namespace DotNetPowerExtensions.Tests.DependencyInjectionExtensions;

internal sealed class ForSingleOther_Tests
{
    [Transient(typeof(FooBase))]
    internal sealed class FooTransientForBase : FooBase { }

    [Scoped(typeof(FooBase))]
    internal sealed class FooScopedForBase : FooBase { }

    [Singleton(typeof(FooBase))]
    internal sealed class FooSingletonForBase : FooBase { }

    [Local(typeof(FooBase))]
    internal sealed class FooLocalForBase : FooBase { }


    [Transient(typeof(IFoo))]
    internal sealed class FooTransientForInterface : IFoo { }

    [Scoped(typeof(IFoo))]
    internal sealed class FooScopedForInterface : IFoo { }

    [Singleton(typeof(IFoo))]
    internal sealed class FooSingletonForInterface : IFoo { }

    [Local(typeof(IFoo))]
    internal sealed class FooLocalForInterface : IFoo { }

    [Test]
    [TestCase(typeof(FooTransientForBase), typeof(FooBase), ServiceLifetime.Transient)]
    [TestCase(typeof(FooTransientForInterface), typeof(IFoo), ServiceLifetime.Transient)]
    [TestCase(typeof(FooScopedForBase), typeof(FooBase), ServiceLifetime.Scoped)]
    [TestCase(typeof(FooScopedForInterface), typeof(IFoo), ServiceLifetime.Scoped)]
    [TestCase(typeof(FooSingletonForBase), typeof(FooBase), ServiceLifetime.Singleton)]
    [TestCase(typeof(FooSingletonForInterface), typeof(IFoo), ServiceLifetime.Singleton)]
    public void Test_RegistersFor(Type type, Type forType, ServiceLifetime lifetime)
    {
        var predicate = Utils.GetPredicate();

        predicate(type, forType, lifetime).Should().BeTrue();

        Utils.HasOtherRegistration(predicate, type).Should().BeFalse();
        Utils.HasOtherRegistration(predicate, type, forType, lifetime).Should().BeFalse();
    }


    [Test]
    [TestCase(typeof(FooLocalForBase), typeof(FooBase))]
    [TestCase(typeof(FooLocalForInterface), typeof(IFoo))]
    public void Test_Singleton_RegistersILocalServiceFor(Type type, Type forType)
    {
        var predicate = Utils.GetPredicate();

        Utils.HasOtherRegistration(predicate, type).Should().BeFalse();
        Utils.HasOtherRegistration(predicate, type, forType).Should().BeFalse();


        var constructed = typeof(LocalFactory<>).MakeGenericType(type);
        var constructedFor = typeof(ILocalFactory<>).MakeGenericType(forType);

        predicate(constructed, constructedFor, ServiceLifetime.Transient).Should().BeTrue();

        Utils.HasOtherRegistration(predicate, constructed).Should().BeFalse();
        Utils.HasOtherRegistration(predicate, constructed, constructedFor, ServiceLifetime.Transient).Should().BeFalse();
    }
}
