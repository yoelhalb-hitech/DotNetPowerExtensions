
namespace SequelPay.DotNetPowerExtensions.Reflection;

public static class PropertyInfoExtensions
{
    public static MethodInfo[] GetAllMethods(this PropertyInfo property) => property.GetAccessors(true);

    public static bool IsPrivate(this PropertyInfo propertyInfo) => propertyInfo.GetAllMethods().All(m => m.IsPrivate);

    public static bool IsAbstract(this PropertyInfo property) => property.GetAllMethods().Any(m => m.IsAbstract);

    public static bool IsOverridable(this PropertyInfo property) => property.GetAllMethods().First().IsOverridable();

    public static PropertyInfo? GetWritablePropertyInfo(this PropertyInfo property)
    {
        if (property.SetMethod is not null) return property;

        // It is still possible that the original decleration of the property has a private set

        // `prop.DeclaringType` or `prop.GetMethod.DeclaringType` will only return the class it was overridden in
        //      but we cannot just traverse the graph and just see if the property exists,
        //          as it might have been shadowed, and we are dealing with the shadow

        var declaringType = property.GetMethod!.GetBaseDefinition().DeclaringType;
        if (property.ReflectedType == declaringType && !property.IsExplicitImplementation()) return null;

        if (property.ReflectedType == declaringType && property.IsExplicitImplementation())
        {
            var methods = property.GetMethod.GetInterfaceMethods().Where(m => m.DeclaringType != declaringType);
            var props = methods.Select(m => m.GetDeclaringProperty()).Where(p => p!.SetMethod is not null).ToArray();

            return props.FirstOrDefault(p => !p!.IsExplicitImplementation()) ?? props.FirstOrDefault();
        }

        var declaringProperty = declaringType!.GetProperty(property.Name, BindingFlagsExtensions.AllBindings);

        return declaringProperty?.SetMethod is not null ? declaringProperty : null;
    }

    public static bool HasGetAndSet(this PropertyInfo property, bool includeBasePrivate)
        => property.GetMethod is not null
                && (property.SetMethod is not null || (includeBasePrivate && property.GetWritablePropertyInfo() is not null));

    public static bool IsExplicitImplementation(this PropertyInfo property)
        => property.Name.Contains('.') && property.GetAllMethods().Any(m => m.IsExplicitImplementation());
}
