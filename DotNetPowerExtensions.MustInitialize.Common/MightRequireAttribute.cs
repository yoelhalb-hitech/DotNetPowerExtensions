
namespace SequelPay.DotNetPowerExtensions;

/// <summary>
/// Specifies that an implementation (for an interface) or a subclass (for a base class) might require the specific properties to be initialized
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple = true)]
public class MightRequireAttribute : Attribute
{
	public MightRequireAttribute(string name, Type type)
	{
        Name = name;
        Type = type;
    }

    public string Name { get; }
    public Type Type { get; }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple = true)]
public class MightRequireAttribute<TType> : MightRequireAttribute
{
    public MightRequireAttribute(string name) : base(name, typeof(TType))
    {
    }
}

