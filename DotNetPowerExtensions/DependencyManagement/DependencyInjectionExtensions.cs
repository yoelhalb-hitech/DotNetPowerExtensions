using Microsoft.Extensions.DependencyInjection;

namespace SequelPay.DotNetPowerExtensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddLocal<TService>(this IServiceCollection services) where TService : class
                    => services.AddLocal(typeof(TService));
    public static IServiceCollection AddLocal(this IServiceCollection services, Type serviceType)
                    => services.AddLocal(serviceType, serviceType);

    public static IServiceCollection AddLocal(this IServiceCollection services, Type serviceType, Type implementationType)
    {
        if (services is null) throw new ArgumentNullException(nameof(services));
        if (serviceType is null) throw new ArgumentNullException(nameof(serviceType));
        if (implementationType is null) throw new ArgumentNullException(nameof(implementationType));

        var composedFor = typeof(ILocalFactory<>).MakeGenericType(serviceType);
        var composed = typeof(LocalFactory<>).MakeGenericType(implementationType);

        // We also need to add the serviceType, as the idea is the have the dependencies resolved, however we want the user to only use the LocaFactory but this will be done in an analyzer
        return services.AddTransient(composedFor, composed).AddTransient(serviceType, implementationType);
    }

    public static IServiceCollection AddDependencies(this IServiceCollection services)
    {
        if (services is null) throw new ArgumentNullException(nameof(services));

        var types = AppDomain.CurrentDomain.GetAssemblies()
                         .SelectMany(a =>
                         {
                             try { return a.GetTypes(); } catch { return ArrayUtils.Empty<Type>(); } // Sometimes it throws
                         })
                         .Where(t =>
                         {
                             try { return Attribute.IsDefined(t, typeof(DependencyAttribute), false); } catch { return false; }
                         });

        foreach (var type in types.Where(t => !t.IsInterface && !t.IsAbstract))
        {
            foreach (var attribute in Attribute.GetCustomAttributes(type, typeof(DependencyAttribute), false).OfType<DependencyAttribute>())
            {
                try
                {
                    if (attribute.DependencyType == DependencyType.None) continue;

                    var implementingType = type.IsGenericTypeDefinition ? attribute.Use : type;
                    if (implementingType is null) continue; // TODO... Maybe add analyzer for it

                    var forTypes = attribute.For.Any() ? attribute.For : new[] { implementingType };

                    foreach (var forType in forTypes)
                    {
                        try
                        {
                            if (attribute.DependencyType == DependencyType.Scoped) services.AddScoped(forType, implementingType);
                            else if (attribute.DependencyType == DependencyType.Transient) services.AddTransient(forType, implementingType);
                            else if (attribute.DependencyType == DependencyType.Singleton) services.AddSingleton(forType, implementingType);
                            else if (attribute.DependencyType == DependencyType.Local) services.AddLocal(forType, implementingType);
                        }
                        catch { }  // TODO...
                    }
                }
                catch { } // TODO...
            }
        }

        return services;
    }
}
