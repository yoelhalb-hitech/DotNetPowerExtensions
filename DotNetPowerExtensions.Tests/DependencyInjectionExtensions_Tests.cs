using DotNetPowerExtensions.DependencyManagement;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Linq;

namespace DotNetPowerExtensions.Tests;

public class DependencyInjectionExtensions_Tests
{
    #region Test Classes

    [Transient]
    internal sealed class FooTransient { }

    [Scoped]
    internal sealed class FooScoped { }

    [Singleton]
    internal sealed class FooSingleton { }

    internal class FooBase { }

    [Transient(For = typeof(FooBase))]
    internal sealed class FooTransientForBase : FooBase { }
    
    [Scoped(For = typeof(FooBase))]
    internal sealed class FooScopedForBase : FooBase { }

    [Singleton(For = typeof(FooBase))]
    internal sealed class FooSingletonForBase : FooBase { }

    internal interface IFoo { }

    [Transient(For = typeof(IFoo))]
    internal sealed class FooTransientForInterface : IFoo { }

    [Scoped(For = typeof(IFoo))]
    internal sealed class FooScopedForInterface : IFoo { }

    [Singleton(For = typeof(IFoo))]
    internal sealed class FooSingletonForInterface : IFoo { }

    #endregion

    private static Func<ServiceDescriptor, bool> GetPredicate(Type type, Type forType, ServiceLifetime lifetime)
        => r => r.ImplementationType == type && r.ServiceType == forType && r.Lifetime == lifetime;

    [Test]
    [TestCase(typeof(FooTransient))]
    [TestCase(typeof(FooTransientForBase))]
    [TestCase(typeof(FooTransientForInterface))]
    public void Test_Transient_RegistersSelf(Type type)
    {
        var mock = new Mock<IServiceCollection>();
        var list = new List<ServiceDescriptor>();

        mock
            .Setup(m => m.Add(It.IsAny<ServiceDescriptor>()))
            .Callback<ServiceDescriptor>(c => list.Add(c));

        DependencyInjectionExtensions.AddDependencies(mock.Object);

        list.Any(GetPredicate(type, type, ServiceLifetime.Transient)).Should().BeTrue();
        list.Any(GetPredicate(type, type, ServiceLifetime.Scoped)).Should().BeFalse();
        list.Any(GetPredicate(type, type, ServiceLifetime.Singleton)).Should().BeFalse();
    }

    [Test] 
    [TestCase(typeof(FooScoped))]
    [TestCase(typeof(FooScopedForBase))]
    [TestCase(typeof(FooScopedForInterface))]
    public void Test_Scoped_RegistersSelf(Type type)
    {
        var mock = new Mock<IServiceCollection>();
        var list = new List<ServiceDescriptor>();

        mock
            .Setup(m => m.Add(It.IsAny<ServiceDescriptor>()))
            .Callback<ServiceDescriptor>(c => list.Add(c));

        DependencyInjectionExtensions.AddDependencies(mock.Object);

        list.Any(GetPredicate(type, type, ServiceLifetime.Transient)).Should().BeFalse();
        list.Any(GetPredicate(type, type, ServiceLifetime.Scoped)).Should().BeTrue();
        list.Any(GetPredicate(type, type, ServiceLifetime.Singleton)).Should().BeFalse();
    }

    [Test]
    [TestCase(typeof(FooSingleton))]
    [TestCase(typeof(FooSingletonForBase))]
    [TestCase(typeof(FooSingletonForInterface))]
    public void Test_Singleton_RegistersSelf(Type type)
    {
        var mock = new Mock<IServiceCollection>();

        var list = new List<ServiceDescriptor>();

        mock
            .Setup(m => m.Add(It.IsAny<ServiceDescriptor>()))
            .Callback<ServiceDescriptor>(c => list.Add(c));

        DependencyInjectionExtensions.AddDependencies(mock.Object);

        list.Any(GetPredicate(type, type, ServiceLifetime.Transient)).Should().BeFalse();
        list.Any(GetPredicate(type, type, ServiceLifetime.Scoped)).Should().BeFalse();
        list.Any(GetPredicate(type, type, ServiceLifetime.Singleton)).Should().BeTrue();
    }

    [Test]
    [TestCase(typeof(FooTransientForBase), typeof(FooBase))]
    [TestCase(typeof(FooTransientForInterface), typeof(IFoo))]
    public void Test_Transient_RegistersFor(Type type, Type forType)
    {
        var mock = new Mock<IServiceCollection>();

        var list = new List<ServiceDescriptor>();

        mock
            .Setup(m => m.Add(It.IsAny<ServiceDescriptor>()))
            .Callback<ServiceDescriptor>(c => list.Add(c));

        DependencyInjectionExtensions.AddDependencies(mock.Object);

        list.Any(GetPredicate(type, forType, ServiceLifetime.Transient)).Should().BeTrue();
        list.Any(GetPredicate(type, forType, ServiceLifetime.Scoped)).Should().BeFalse();
        list.Any(GetPredicate(type, forType, ServiceLifetime.Singleton)).Should().BeFalse();
    }

    [Test]
    [TestCase(typeof(FooScopedForBase), typeof(FooBase))]
    [TestCase(typeof(FooScopedForInterface), typeof(IFoo))]
    public void Test_Scoped_RegistersFor(Type type, Type forType)
    {
        var mock = new Mock<IServiceCollection>();

        var list = new List<ServiceDescriptor>();

        mock
            .Setup(m => m.Add(It.IsAny<ServiceDescriptor>()))
            .Callback<ServiceDescriptor>(c => list.Add(c));

        DependencyInjectionExtensions.AddDependencies(mock.Object);

        list.Any(GetPredicate(type, forType, ServiceLifetime.Transient)).Should().BeFalse();
        list.Any(GetPredicate(type, forType, ServiceLifetime.Scoped)).Should().BeTrue();
        list.Any(GetPredicate(type, forType, ServiceLifetime.Singleton)).Should().BeFalse();
    }

    [Test]
    [TestCase(typeof(FooSingletonForBase), typeof(FooBase))]
    [TestCase(typeof(FooSingletonForInterface), typeof(IFoo))]
    public void Test_Singleton_RegistersFor(Type type, Type forType)
    {
        var mock = new Mock<IServiceCollection>();

        var list = new List<ServiceDescriptor>();

        mock
            .Setup(m => m.Add(It.IsAny<ServiceDescriptor>()))
            .Callback<ServiceDescriptor>(c => list.Add(c));

        DependencyInjectionExtensions.AddDependencies(mock.Object);

        list.Any(GetPredicate(type, forType, ServiceLifetime.Transient)).Should().BeFalse();
        list.Any(GetPredicate(type, forType, ServiceLifetime.Scoped)).Should().BeFalse();
        list.Any(GetPredicate(type, forType, ServiceLifetime.Singleton)).Should().BeTrue();
    }
}