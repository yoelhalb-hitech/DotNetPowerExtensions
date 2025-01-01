using Microsoft.Extensions.DependencyInjection;
using SequelPay.DotNetPowerExtensions;
using static DotNetPowerExtensions.Tests.DependencyInjectionExtensions.ForSingleOther_Tests;
using static DotNetPowerExtensions.Tests.DependencyInjectionExtensions.Utils;

namespace DotNetPowerExtensions.Tests.DependencyInjectionExtensions;

internal sealed class Generic_Tests
{
#if NET5_0_OR_GREATER
    [Transient<IFoo>(Use = typeof(FooTransientGeneric<string>))]
#endif
    [Transient(typeof(IFoo), Use = typeof(FooTransientGeneric<string>))]
    internal sealed class FooTransientGeneric<T> : FooBase, IFoo { }

#if NET5_0_OR_GREATER
    [Scoped<IFoo>(Use = typeof(FooScopedGeneric<string>))]
#endif
    [Scoped(typeof(IFoo), Use = typeof(FooScopedGeneric<string>))]
    internal sealed class FooScopedGeneric<T> : FooBase, IFoo { }

#if NET5_0_OR_GREATER
    [Singleton<IFoo>(Use = typeof(FooSingletonGeneric<string>))]
#endif
    [Singleton(typeof(IFoo), Use = typeof(FooSingletonGeneric<string>))]
    internal sealed class FooSingletonGeneric<T> : FooBase, IFoo { }

#if NET5_0_OR_GREATER
    [Local<IFoo>(Use = typeof(FooLocalGeneric<string>))]
#endif
    [Local(typeof(IFoo), Use = typeof(FooLocalGeneric<string>))]
    internal sealed class FooLocalGeneric<T> : FooBase, IFoo { }

    [Test]
    [TestCase(typeof(FooTransientGeneric<>), ServiceLifetime.Transient)]
    [TestCase(typeof(FooScopedGeneric<>), ServiceLifetime.Scoped)]
    [TestCase(typeof(FooSingletonGeneric<>), ServiceLifetime.Singleton)]
    public void Test_RegistersFor(Type type,ServiceLifetime lifetime)
    {
        var predicate = Utils.GetPredicate();

        var fooClosed = type.MakeGenericType(typeof(string));
        var forIfoo = typeof(IFoo);

        predicate(fooClosed, forIfoo, lifetime).Should().BeTrue();

        Utils.HasOtherRegistration(predicate, type).Should().BeFalse();
        Utils.HasOtherRegistration(predicate, type, forIfoo).Should().BeFalse();

        Utils.HasOtherRegistration(predicate, fooClosed).Should().BeFalse();
        Utils.HasOtherRegistration(predicate, fooClosed, forIfoo, lifetime).Should().BeFalse();
    }


    [Test]
    public void Test_Singleton_RegistersILocalServiceFor()
    {
        var predicate = Utils.GetPredicate();

        var fooFactory = typeof(LocalFactory<FooLocalGeneric<string>>);
        var ifooIFactory = typeof(ILocalFactory<IFoo>);

        predicate(fooFactory, ifooIFactory, ServiceLifetime.Transient).Should().BeTrue();

        Utils.HasOtherRegistration(predicate, fooFactory).Should().BeFalse();
        Utils.HasOtherRegistration(predicate, fooFactory, ifooIFactory, ServiceLifetime.Transient).Should().BeFalse();

        var fooLocal = typeof(FooLocalGeneric<>);
        var fooClosed = typeof(FooLocalGeneric<string>);
        var forIfoo = typeof(IFoo);

        Utils.HasOtherRegistration(predicate, fooLocal).Should().BeFalse();

        predicate(fooClosed, forIfoo, ServiceLifetime.Transient).Should().BeTrue();
        Utils.HasOtherRegistration(predicate, fooClosed, fooClosed, ServiceLifetime.Transient).Should().BeFalse();
        Utils.HasOtherRegistration(predicate, fooLocal, fooClosed).Should().BeFalse();

        Utils.HasOtherRegistration(predicate, fooLocal, fooClosed).Should().BeFalse();
        Utils.HasOtherRegistration(predicate, fooLocal, forIfoo).Should().BeFalse();
        Utils.HasOtherRegistration(predicate, fooClosed, forIfoo, ServiceLifetime.Transient).Should().BeFalse();
    }
}
