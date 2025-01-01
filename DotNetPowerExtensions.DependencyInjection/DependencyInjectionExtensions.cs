using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

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

    public static void TryAddLocal<TService>(this IServiceCollection services) where TService : class
                => services.TryAddLocal(typeof(TService));
    public static void TryAddLocal(this IServiceCollection services, Type serviceType)
                    => services.TryAddLocal(serviceType, serviceType);

    public static void TryAddLocal(this IServiceCollection services, Type serviceType, Type implementationType)
    {
        if (services is null) throw new ArgumentNullException(nameof(services));
        if (serviceType is null) throw new ArgumentNullException(nameof(serviceType));
        if (implementationType is null) throw new ArgumentNullException(nameof(implementationType));

        var composedFor = typeof(ILocalFactory<>).MakeGenericType(serviceType);
        var composed = typeof(LocalFactory<>).MakeGenericType(implementationType);

        // We also need to add the serviceType, as the idea is the have the dependencies resolved, however we want the user to only use the LocaFactory but this will be done in an analyzer
        services.TryAddTransient(composedFor, composed);
        services.TryAddTransient(serviceType, implementationType);
    }

    public static IServiceCollection AddDependencies(this IServiceCollection services)
    {
        if (services is null) throw new ArgumentNullException(nameof(services));

        var types = AppDomain.CurrentDomain.GetAssemblies()
                         .SelectMany(a =>
                         {
                             try { return a.GetTypes(); } catch { return (Type[])[]; } // Sometimes it throws
                         })
                         .Where(t =>
                         {
                             try { return Attribute.IsDefined(t, typeof(DependencyAttribute), false); } catch { return false; }
                         });

        foreach (Type type in types!.OfType<Type>().Where(t => !t.IsInterface && !t.IsAbstract))
        {
            foreach (var attribute in type.GetCustomAttributes(false).OfType<DependencyAttribute>())
            {
                try
                {
                    if (attribute.DependencyType == DependencyType.None) continue;

                    Type implementingType = type!;
                    if (type.IsGenericTypeDefinition && attribute.Use is not null)
                    {
                        //// TODO... Maybe add analyzer for this
                        //if (!attribute.Use.IsGenericType || attribute.Use.IsGenericTypeDefinition
                        //        || attribute.Use.GetGenericTypeDefinition() != type) continue;

                        implementingType = attribute.Use!;
                    }

                    var forTypes = attribute.For;
                    //if (!forTypes.Any() && type.IsGenericTypeDefinition
                    //        && attribute.Use is not null && !type.IsAssignableFrom(attribute.Use)) continue; // TODO... Maybe add analyzer for this
                    //else if (!forTypes.Any()) forTypes = new[] { type };
                    if (!forTypes.Any()) forTypes = new[] { type };

                    foreach (var forType in forTypes.OfType<Type>())
                    {
                        try
                        {
                            // TODO... add analyzer in the callsite to not call a service that has multiple registrations unless keyed
                            //// TODO... Maybe add analyzer for this
                            //if (implementingType.IsGenericTypeDefinition != forType.IsGenericTypeDefinition) continue;
                            //if((!implementingType.IsGenericTypeDefinition && !forType.IsAssignableFrom(implementingType))) continue;

                            if (attribute.DependencyType == DependencyType.Scoped)
                                    services.AddScoped(forType, implementingType ?? forType);
                            else if (attribute.DependencyType == DependencyType.Transient)
                                    services.AddTransient(forType, implementingType ?? forType);
                            else if (attribute.DependencyType == DependencyType.Singleton)
                                    services.AddSingleton(forType, implementingType ?? forType);
                            else if (attribute.DependencyType == DependencyType.Local)
                                    services.AddLocal(forType, implementingType ?? forType);
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
