
using System.ComponentModel;

namespace SequelPay.DotNetPowerExtensions;

#pragma warning disable CA1813 // Avoid unsealed attributes

// TODO... add analyzer to force subclasses to be services
[EditorBrowsable(EditorBrowsableState.Never)]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, Inherited = false)]
public abstract class DependencyBaseAttribute : Attribute
{
    public DependencyBaseAttribute(DependencyType dependencyType)
    {
        DependencyType = dependencyType;
    }

    public virtual DependencyType DependencyType { get; }
}

#pragma warning restore CA1813 // Avoid unsealed attributes
