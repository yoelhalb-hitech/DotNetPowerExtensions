
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace DotNetPowerExtensions.Tests.DependencyInjectionExtensions;

internal sealed class EmptyTypes_Tests
{
    [Transient]
    internal sealed class FooTransient { }

    [Scoped]
    internal sealed class FooScoped { }

    [Singleton]
    internal sealed class FooSingleton { }

    [Local]
    internal sealed class FooLocal { }

    [Test]
    [TestCase(typeof(FooTransient), ServiceLifetime.Transient)]
    [TestCase(typeof(FooScoped), ServiceLifetime.Scoped)]
    [TestCase(typeof(FooSingleton), ServiceLifetime.Singleton)]
    public void Test_RegistersSelf(Type type, ServiceLifetime lifetime)
    {
        var predicate = Utils.GetPredicate();

        predicate(type, type, lifetime).Should().BeTrue();

        Utils.HasOtherRegistration(predicate, type, type, lifetime).Should().BeFalse();
    }

    [Test]
    public void Test_Local_RegistersILocalServiceForSelf()
    {
        var predicate = Utils.GetPredicate();

        var type = typeof(FooLocal);
        Utils.HasOtherRegistration(predicate, type).Should().BeFalse();

        var constructed = typeof(LocalFactory<FooLocal>);
        var constructedFor = typeof(ILocalFactory<FooLocal>);

        predicate(constructed, constructedFor, ServiceLifetime.Transient).Should().BeTrue();

        Utils.HasOtherRegistration(predicate, constructed).Should().BeFalse();
        Utils.HasOtherRegistration(predicate, constructed, constructedFor, ServiceLifetime.Transient).Should().BeFalse();
    }
}
