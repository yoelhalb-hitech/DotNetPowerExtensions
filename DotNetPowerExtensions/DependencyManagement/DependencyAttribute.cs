
namespace DotNetPowerExtensions.DependencyManagement;

#pragma warning disable CA1813 // Avoid unsealed attributes

// TODO... add analyzer to force subclasses to be services
[AttributeUsage(AttributeTargets.Class|AttributeTargets.Struct|AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
public abstract class DependencyAttribute : Attribute
{
    public DependencyAttribute(DependencyType dependencyType)
    {
        DependencyType = dependencyType;
    }

    public virtual DependencyType DependencyType { get; }

#pragma warning disable CA1716
    public virtual Type? For { get; set; }
#pragma warning restore CA1716
}

public class ScopedAttribute : DependencyAttribute
{
    public ScopedAttribute() : base(DependencyType.Scoped) {}
}

public class ScopedAttribute<T> : ScopedAttribute
{
    public ScopedAttribute() => For = typeof(T);
}

public class TransientAttribute : DependencyAttribute
{
    public TransientAttribute() : base(DependencyType.Transient) {}
}

public class TransientAttribute<T> : TransientAttribute
{
    public TransientAttribute() => For = typeof(T);
}

public class LocalAttribute : DependencyAttribute
{
    public LocalAttribute() : base(DependencyType.Local) {}
}

public class LocalAttribute<T> : LocalAttribute
{
    public LocalAttribute() => For = typeof(T);
}


public class SingletonAttribute : DependencyAttribute
{
    public SingletonAttribute() : base(DependencyType.Singleton) {}
}

public class SingletonAttribute<T> : SingletonAttribute
{
    public SingletonAttribute() => For = typeof(T);
}


public class NonDependencyAttribute : DependencyAttribute
{
    public NonDependencyAttribute() : base(DependencyType.None) {}
}

public class NonDependencyAttribute<T> : NonDependencyAttribute
{
    public NonDependencyAttribute() => For = typeof(T);
}
