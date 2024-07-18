
namespace SequelPay.DotNetPowerExtensions.AutoMapper;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
// Custom attribute to mark classes that should be automatically mapped
public class AutoMapAttribute : Attribute
{
    public Type TargetClass { get; }
    public bool MapInternal { get; }

    public AutoMapAttribute(Type targetClass, bool mapInternal = false)
    {
        TargetClass = targetClass;
        MapInternal = mapInternal;
    }
}