using Microsoft.Extensions.DependencyInjection;
using Moq;
using SequelPay.DotNetPowerExtensions;
using static DotNetPowerExtensions.Tests.DependencyInjectionExtensions.Utils;

namespace DotNetPowerExtensions.Tests.DependencyInjectionExtensions;

internal sealed class ForMultiple_Tests
{
    [Transient]
#if NET5_0_OR_GREATER
    [Transient<FooBase>]
    [Transient<IFoo>]
#endif
    [Transient(typeof(FooBase))]
    [Transient(typeof(IFoo))]
    internal sealed class TestMultipleAttributesDoesNotThrow : FooBase, IFoo { }

#if NET5_0_OR_GREATER
    [Transient<FooTransientForMultiple, FooBase, IFoo>]
#endif
    [Transient(typeof(FooTransientForMultiple), typeof(FooBase), typeof(IFoo))]
    internal sealed class FooTransientForMultiple : FooBase, IFoo { }

#if NET5_0_OR_GREATER
    [Scoped<FooScopedForMultiple, FooBase, IFoo>]
#endif
    [Scoped(typeof(FooScopedForMultiple), typeof(FooBase), typeof(IFoo))]
    internal sealed class FooScopedForMultiple : FooBase, IFoo { }

#if NET5_0_OR_GREATER
    [Singleton<FooSingletonForMultiple, FooBase, IFoo>]
#endif
    [Singleton(typeof(FooSingletonForMultiple), typeof(FooBase), typeof(IFoo))]
    internal sealed class FooSingletonForMultiple : FooBase, IFoo { }

#if NET5_0_OR_GREATER
    [Local<FooLocalForMultiple, FooBase, IFoo>]
#endif
    [Local(typeof(FooLocalForMultiple), typeof(FooBase), typeof(IFoo))]
    internal sealed class FooLocalForMultiple : FooBase, IFoo { }

    [Test]
    [TestCase(typeof(FooTransientForMultiple), ServiceLifetime.Transient)]
    [TestCase(typeof(FooScopedForMultiple), ServiceLifetime.Scoped)]
    [TestCase(typeof(FooSingletonForMultiple), ServiceLifetime.Singleton)]
    public void Test_RegistersForMultiple(Type type, ServiceLifetime lifetime)
    {
        var predicate = Utils.GetPredicate();

        var forBase = typeof(FooBase);
        var forIfoo = typeof(IFoo);

        predicate(type, type, lifetime).Should().BeTrue();
        predicate(type, forBase, lifetime).Should().BeTrue();
        predicate(type, forIfoo, lifetime).Should().BeTrue();

        Utils.HasOtherRegistration(predicate, type, type, lifetime).Should().BeFalse();
        Utils.HasOtherRegistration(predicate, type, forBase, lifetime).Should().BeFalse();
        Utils.HasOtherRegistration(predicate, type, forIfoo, lifetime).Should().BeFalse();
    }

    [Test]
    public void Test_Singleton_RegistersILocalServiceForMultiple()
    {
        var predicate = Utils.GetPredicate();

        var originalType = typeof(FooLocalForMultiple);

        var type = typeof(LocalFactory<FooLocalForMultiple>);
        var forType1 = typeof(ILocalFactory<FooLocalForMultiple>);
        var forType2 = typeof(ILocalFactory<FooBase>);
        var forType3 = typeof(ILocalFactory<IFoo>);

        predicate(type, forType1, ServiceLifetime.Transient).Should().BeTrue();
        predicate(type, forType2, ServiceLifetime.Transient).Should().BeTrue();
        predicate(type, forType3, ServiceLifetime.Transient).Should().BeTrue();

        predicate(originalType, originalType, ServiceLifetime.Transient).Should().BeTrue();

        Utils.HasOtherRegistration(predicate, originalType, originalType, ServiceLifetime.Transient).Should().BeFalse();

        Utils.HasOtherRegistration(predicate, originalType, forType1).Should().BeFalse();
        Utils.HasOtherRegistration(predicate, originalType, forType2).Should().BeFalse();
        Utils.HasOtherRegistration(predicate, originalType, forType3).Should().BeFalse();

        Utils.HasOtherRegistration(predicate, type, originalType, ServiceLifetime.Transient).Should().BeFalse();
        Utils.HasOtherRegistration(predicate, type, originalType, ServiceLifetime.Transient).Should().BeFalse();
        Utils.HasOtherRegistration(predicate, type, originalType, ServiceLifetime.Transient).Should().BeFalse();
    }
}
