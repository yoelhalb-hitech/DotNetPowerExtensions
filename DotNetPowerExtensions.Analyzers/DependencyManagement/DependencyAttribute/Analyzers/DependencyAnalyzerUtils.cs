
using SequelPay.DotNetPowerExtensions;

namespace DotNetPowerExtensions.Analyzers.DependencyManagement.DependencyAttribute.Analyzers;

internal static class DependencyAnalyzerUtils
{
    public static Type[] BaseAttributes =
    {
        typeof(LocalBaseAttribute),
        typeof(SingletonBaseAttribute),
        typeof(ScopedBaseAttribute),
        typeof(TransientBaseAttribute),
    };

    public static string[] NonLocalAttributeNames =
{
        nameof(SingletonAttribute),
        nameof(ScopedAttribute),
        nameof(TransientAttribute),
    };
    //CAUTION: The test framework for whatever reason has a compile issue if the following is before the decleration of the referenced props
    public static string[] DependencyAttributeNames = NonLocalAttributeNames.Concat(new[] { nameof(LocalAttribute) }).ToArray();

    public static Type[] LocalAttributes =
    {
        typeof(LocalAttribute),
        typeof(LocalAttribute<>),
        typeof(LocalAttribute<,>),
        typeof(LocalAttribute<,,>),
        typeof(LocalAttribute<,,,>),
        typeof(LocalAttribute<,,,,>),
        typeof(LocalAttribute<,,,,,>),
        typeof(LocalAttribute<,,,,,,>),
        typeof(LocalAttribute<,,,,,,,>),
    };

    public static Type[] NonLocalAttributes =
    {
        typeof(SingletonAttribute),
        typeof(SingletonAttribute<>),
        typeof(SingletonAttribute<,>),
        typeof(SingletonAttribute<,,>),
        typeof(SingletonAttribute<,,,>),
        typeof(SingletonAttribute<,,,,>),
        typeof(SingletonAttribute<,,,,,>),
        typeof(SingletonAttribute<,,,,,,>),
        typeof(SingletonAttribute<,,,,,,,>),
        typeof(ScopedAttribute),
        typeof(ScopedAttribute<>),
        typeof(ScopedAttribute<,>),
        typeof(ScopedAttribute<,,>),
        typeof(ScopedAttribute<,,,>),
        typeof(ScopedAttribute<,,,,>),
        typeof(ScopedAttribute<,,,,,>),
        typeof(ScopedAttribute<,,,,,,>),
        typeof(ScopedAttribute<,,,,,,,>),
        typeof(TransientAttribute),
        typeof(TransientAttribute<>),
        typeof(TransientAttribute<,>),
        typeof(TransientAttribute<,,>),
        typeof(TransientAttribute<,,,>),
        typeof(TransientAttribute<,,,,>),
        typeof(TransientAttribute<,,,,,>),
        typeof(TransientAttribute<,,,,,,>),
        typeof(TransientAttribute<,,,,,,,>),
    };
    //CAUTION: The test framework for whatever reason has a compile issue if the following is before the decleration of the referenced props
    public static Type[] AllDependencies = LocalAttributes.Concat(NonLocalAttributes).ToArray();
}
