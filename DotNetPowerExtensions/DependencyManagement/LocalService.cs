using DotNetPowerExtensions.AccessControl;
using DotNetPowerExtensions.MustInitialize;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DotNetPowerExtensions.DependencyManagement;

public class LocalService<TClass>
{
    private readonly IServiceProvider serviceProvider;

    public LocalService(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public TClass? Get()
    {
        var obj = (TClass?)serviceProvider.GetService(typeof(TClass));

        return obj;
    }

    [NonDelegate]
    public TClass? Get(object arg)
    {
        var obj = Get();
        if (obj is null) return obj;

        var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        var type = obj.GetType(); // Not doing typeof(TClass) in case this is a subclass and it has more Required props it should also be included

        var classProps = type.GetProperties(bindingFlags)
                            .Where(p => p.SetMethod is not null && Attribute.IsDefined(p, typeof(MustInitializeAttribute), false))
                            .ToDictionary(p => p.Name);

        var classFields = type.GetFields(bindingFlags)
                            .Where(f => Attribute.IsDefined(f, typeof(MustInitializeAttribute), false))
                            .ToDictionary(f => f.Name);

        foreach (var prop in arg.GetType().GetProperties(bindingFlags))
        {
            if (classProps.TryGetValue(prop.Name, out var propInfo)) propInfo.SetValue(obj, prop.GetValue(arg));
            else if (classFields.TryGetValue(prop.Name, out var fieldInfo)) fieldInfo.SetValue(obj, prop.GetValue(arg));
        }

        foreach (var field in arg.GetType().GetFields(bindingFlags))
        {
            if (classProps.TryGetValue(field.Name, out var propInfo)) propInfo.SetValue(obj, field.GetValue(arg));
            else if (classFields.TryGetValue(field.Name, out var fieldInfo)) fieldInfo.SetValue(obj, field.GetValue(arg));
        }

        return obj;
    }
}
