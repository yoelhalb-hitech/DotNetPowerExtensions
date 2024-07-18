
namespace SequelPay.DotNetPowerExtensions;

/// <summary>
/// Specifies that the given property/method is only throwing (or propagating) the exceptions specified
/// </summary>
[AttributeUsage(AttributeTargets.Property|AttributeTargets.Method|AttributeTargets.Event|AttributeTargets.Parameter|AttributeTargets.ReturnValue)]
public class ThrowsAttribute : Attribute
{
	public ThrowsAttribute(params Type[] types)
	{
        ExceptionTypes = types;
    }

    public Type[]? ExceptionTypes { get; }
}

public class ThrowsAttribute<T> : ThrowsAttribute
{
    public ThrowsAttribute() : base(typeof(T)){}
}
public class ThrowsAttribute<T1, T2> : ThrowsAttribute
{
    public ThrowsAttribute() : base(typeof(T1), typeof(T2)) { }
}

public class ThrowsAttribute<T1, T2, T3> : ThrowsAttribute
{
    public ThrowsAttribute() : base(typeof(T1), typeof(T2), typeof(T3)) { }
}

public class ThrowsAttribute<T1, T2, T3, T4> : ThrowsAttribute
{
    public ThrowsAttribute() : base(typeof(T1), typeof(T2), typeof(T3), typeof(T4)) { }
}

public class ThrowsAttribute<T1, T2, T3, T4, T5> : ThrowsAttribute
{
    public ThrowsAttribute() : base(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5)) { }
}

public class ThrowsAttribute<T1, T2, T3, T4, T5, T6> : ThrowsAttribute
{
    public ThrowsAttribute() : base(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6)) { }
}

public class ThrowsAttribute<T1, T2, T3, T4, T5, T6, T7> : ThrowsAttribute
{
    public ThrowsAttribute() : base(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7)) { }
}

public class ThrowsAttribute<T1, T2, T3, T4, T5, T6, T7, T8> : ThrowsAttribute
{
    public ThrowsAttribute() : base(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8)) { }
}