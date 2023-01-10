
﻿using DotNetPowerExtensions.Polyfill;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetPowerExtensions.DependencyManagement
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddDependencies(this IServiceCollection services)
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                             .SelectMany(a =>
                             {
                                 try
                                 {
                                     return a.GetTypes();
                                 }
                                 catch { return ArrayUtils.Empty<Type>(); } // Sometimes it throws
                             })
                             .Where(t =>
                             {
                                 try
                                 {
                                     return Attribute.IsDefined(t, typeof(DependencyAttribute), false);
                                 }
                                 catch {  return false; }
                             });

            foreach (var type in types.Where(t => !t.IsInterface && !t.IsAbstract && !t.IsGenericTypeDefinition))
            {
                var attribute = Attribute.GetCustomAttribute(type, typeof(DependencyAttribute), false) as DependencyAttribute;
                if (attribute is null || attribute.DependencyType == DependencyType.None) continue;

                if (attribute.DependencyType == DependencyType.Scoped) services.AddScoped(type);
                if (attribute.DependencyType == DependencyType.Transient) services.AddTransient(type);
                if (attribute.DependencyType == DependencyType.Singleton) services.AddSingleton(type);

                if(attribute.For is not null && attribute.For != type)
                {
                    if (attribute.DependencyType == DependencyType.Scoped) services.AddScoped(attribute.For, type);
                    if (attribute.DependencyType == DependencyType.Transient) services.AddTransient(attribute.For, type);
                    if (attribute.DependencyType == DependencyType.Singleton) services.AddSingleton(attribute.For, type);
                }
            }

            return services;
        }
    }
}
