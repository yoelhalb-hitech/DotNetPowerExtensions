
namespace SequelPay.DotNetPowerExtensions;

public class TransientAttribute : DependencyAttribute
{
    public TransientAttribute(params Type[] types) : base(DependencyType.Transient, types) { }
}

public class TransientAttribute<T> : TransientAttribute
{
    public TransientAttribute() : base(typeof(T)) { }
}
public class TransientAttribute<T1, T2> : TransientAttribute
{
    public TransientAttribute() : base(typeof(T1), typeof(T2)) { }
}

public class TransientAttribute<T1, T2, T3> : TransientAttribute
{
    public TransientAttribute() : base(typeof(T1), typeof(T2), typeof(T3)) { }
}

public class TransientAttribute<T1, T2, T3, T4> : TransientAttribute
{
    public TransientAttribute() : base(typeof(T1), typeof(T2), typeof(T3), typeof(T4)) { }
}

public class TransientAttribute<T1, T2, T3, T4, T5> : TransientAttribute
{
    public TransientAttribute() : base(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5)) { }
}

public class TransientAttribute<T1, T2, T3, T4, T5, T6> : TransientAttribute
{
    public TransientAttribute() : base(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6)) { }
}

public class TransientAttribute<T1, T2, T3, T4, T5, T6, T7> : TransientAttribute
{
    public TransientAttribute() : base(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7)) { }
}

public class TransientAttribute<T1, T2, T3, T4, T5, T6, T7, T8> : TransientAttribute
{
    public TransientAttribute() : base(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8)) { }
}
