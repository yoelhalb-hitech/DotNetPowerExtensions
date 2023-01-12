
namespace DotNetPowerExtensions;

public class LocalAttribute : DependencyAttribute
{
    public LocalAttribute(params Type[] types) : base(DependencyType.Local, types) { }
}

public class LocalAttribute<T> : LocalAttribute
{
    public LocalAttribute() : base(typeof(T)) { }
}
public class LocalAttribute<T1, T2> : LocalAttribute
{
    public LocalAttribute() : base(typeof(T1), typeof(T2)) { }
}

public class LocalAttribute<T1, T2, T3> : LocalAttribute
{
    public LocalAttribute() : base(typeof(T1), typeof(T2), typeof(T3)) { }
}

public class LocalAttribute<T1, T2, T3, T4> : LocalAttribute
{
    public LocalAttribute() : base(typeof(T1), typeof(T2), typeof(T3), typeof(T4)) { }
}

public class LocalAttribute<T1, T2, T3, T4, T5> : LocalAttribute
{
    public LocalAttribute() : base(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5)) { }
}

public class LocalAttribute<T1, T2, T3, T4, T5, T6> : LocalAttribute
{
    public LocalAttribute() : base(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6)) { }
}

public class LocalAttribute<T1, T2, T3, T4, T5, T6, T7> : LocalAttribute
{
    public LocalAttribute() : base(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7)) { }
}

public class LocalAttribute<T1, T2, T3, T4, T5, T6, T7, T8> : LocalAttribute
{
    public LocalAttribute() : base(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8)) { }
}
