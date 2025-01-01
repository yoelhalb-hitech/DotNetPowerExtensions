using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace DotNetPowerExtensions.Tests.DependencyInjectionExtensions;

internal static class Utils
{
    internal class FooBase { }
    internal interface IFoo { }

    private static Func<ServiceDescriptor, bool> GetPredicate(Type type, Type forType, ServiceLifetime lifetime)
                                => r => r.ImplementationType == type && r.ServiceType == forType && r.Lifetime == lifetime;

    public static Func<Type, Type, ServiceLifetime, bool> GetPredicate()
    {
        var mock = new Mock<IServiceCollection>();
        var list = new List<ServiceDescriptor>();

        mock
            .Setup(m => m.Add(It.IsAny<ServiceDescriptor>()))
            .Callback<ServiceDescriptor>(c => list.Add(c));
        mock
            .Setup(m => m.GetEnumerator())
            .Returns(() => list.GetEnumerator());

        SequelPay.DotNetPowerExtensions.DependencyInjectionExtensions.AddDependencies(mock.Object);

        return (type, forType, lifetime) => list.Any(GetPredicate(type, forType, lifetime));
    }

    public static bool HasOtherRegistration(Func<Type, Type, ServiceLifetime, bool> predicate, Type type, Type? forType = null, ServiceLifetime? exceptLifetime = null)
    {
        var result = false;

        if (exceptLifetime is null || exceptLifetime != ServiceLifetime.Transient) result |= predicate(type, forType ?? type, ServiceLifetime.Transient);
        if (exceptLifetime is null || exceptLifetime != ServiceLifetime.Scoped) result |= predicate(type, forType ?? type, ServiceLifetime.Scoped);
        if (exceptLifetime is null || exceptLifetime != ServiceLifetime.Singleton) result |= predicate(type, forType ?? type, ServiceLifetime.Singleton);

        return result;
    }
}
