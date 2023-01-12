
namespace DotNetPowerExtensions;

public class SingletonAttribute : DependencyAttribute
{
    public SingletonAttribute(params Type[] types) : base(DependencyType.Singleton, types) { }
}

public class SingletonAttribute<T> : SingletonAttribute
{
    public SingletonAttribute() : base(typeof(T)) { }
}
public class SingletonAttribute<T1, T2> : SingletonAttribute
{
    public SingletonAttribute() : base(typeof(T1), typeof(T2)) { }
}

public class SingletonAttribute<T1, T2, T3> : SingletonAttribute
{
    public SingletonAttribute() : base(typeof(T1), typeof(T2), typeof(T3)) { }
}

public class SingletonAttribute<T1, T2, T3, T4> : SingletonAttribute
{
    public SingletonAttribute() : base(typeof(T1), typeof(T2), typeof(T3), typeof(T4)) { }
}

public class SingletonAttribute<T1, T2, T3, T4, T5> : SingletonAttribute
{
    public SingletonAttribute() : base(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5)) { }
}

public class SingletonAttribute<T1, T2, T3, T4, T5, T6> : SingletonAttribute
{
    public SingletonAttribute() : base(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6)) { }
}

public class SingletonAttribute<T1, T2, T3, T4, T5, T6, T7> : SingletonAttribute
{
    public SingletonAttribute() : base(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7)) { }
}

public class SingletonAttribute<T1, T2, T3, T4, T5, T6, T7, T8> : SingletonAttribute
{
    public SingletonAttribute() : base(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8)) { }
}
