
namespace DotNetPowerExtensions;

#pragma warning disable CA1813 // Avoid unsealed attributes

// TODO... add analyzer to force subclasses to be services
[AttributeUsage(AttributeTargets.Class|AttributeTargets.Struct|AttributeTargets.Interface, AllowMultiple = true /* Because of the generic `Use` */, Inherited = false)]
public abstract class DependencyAttribute : Attribute
{
    public DependencyAttribute(DependencyType dependencyType, params Type[] types)
    {
        DependencyType = dependencyType;
        For = types;
    }

    public virtual DependencyType DependencyType { get; }

#pragma warning disable CA1716
    internal virtual Type[] For { get; set; }
    public virtual Type? Use { get; set; }
#pragma warning restore CA1716
}


public class NonDependencyAttribute : DependencyAttribute
{
    public NonDependencyAttribute(params Type[] types) : base(DependencyType.None, types) {}
}

public class NonDependencyAttribute<T> : NonDependencyAttribute
{
    public NonDependencyAttribute() : base(typeof(T)) {}
}


#pragma warning restore CA1813 // Avoid unsealed attributes
