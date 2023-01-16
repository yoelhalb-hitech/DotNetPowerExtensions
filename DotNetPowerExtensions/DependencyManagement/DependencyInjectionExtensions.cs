using Microsoft.Extensions.DependencyInjection;

namespace SequelPay.DotNetPowerExtensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddDependencies(this IServiceCollection services)
    {
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
            var attribute = Attribute.GetCustomAttribute(type, typeof(DependencyAttribute), false) as DependencyAttribute;
            if (attribute is null || attribute.DependencyType == DependencyType.None) continue;

            var implementingType = type.IsGenericTypeDefinition ? attribute.Use : type;
            if(implementingType is null) continue; // TODO... Maybe add analyzer for it

            var forTypes = attribute.For.Any() ? attribute.For : new[] { implementingType };

            foreach (var forType in forTypes)
            {
                if (attribute.DependencyType == DependencyType.Scoped) services.AddScoped(forType, implementingType);
                else if (attribute.DependencyType == DependencyType.Transient) services.AddTransient(forType, implementingType);
                else if (attribute.DependencyType == DependencyType.Singleton) services.AddSingleton(forType, implementingType);
                else if (attribute.DependencyType == DependencyType.Local)
                {
                    var composedFor = typeof(ILocalFactory<>).MakeGenericType(forType);
                    var composed = typeof(LocalFactory<>).MakeGenericType(implementingType);

                    services.AddTransient(composedFor, composed);
                }
            }         
        }

        return services;
    }
}
