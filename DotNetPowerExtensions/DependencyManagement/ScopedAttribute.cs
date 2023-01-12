
namespace DotNetPowerExtensions;

public class ScopedAttribute : DependencyAttribute
{
    public ScopedAttribute(params Type[] types) : base(DependencyType.Scoped, types) { }
}

public class ScopedAttribute<T> : ScopedAttribute
{
    public ScopedAttribute() : base(typeof(T)) { }
}
public class ScopedAttribute<T1, T2> : ScopedAttribute
{
    public ScopedAttribute() : base(typeof(T1), typeof(T2)) { }
}

public class ScopedAttribute<T1, T2, T3> : ScopedAttribute
{
    public ScopedAttribute() : base(typeof(T1), typeof(T2), typeof(T3)) { }
}

public class ScopedAttribute<T1, T2, T3, T4> : ScopedAttribute
{
    public ScopedAttribute() : base(typeof(T1), typeof(T2), typeof(T3), typeof(T4)) { }
}

public class ScopedAttribute<T1, T2, T3, T4, T5> : ScopedAttribute
{
    public ScopedAttribute() : base(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5)) { }
}

public class ScopedAttribute<T1, T2, T3, T4, T5, T6> : ScopedAttribute
{
    public ScopedAttribute() : base(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6)) { }
}

public class ScopedAttribute<T1, T2, T3, T4, T5, T6, T7> : ScopedAttribute
{
    public ScopedAttribute() : base(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7)) { }
}

public class ScopedAttribute<T1, T2, T3, T4, T5, T6, T7, T8> : ScopedAttribute
{
    public ScopedAttribute() : base(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8)) { }
}
