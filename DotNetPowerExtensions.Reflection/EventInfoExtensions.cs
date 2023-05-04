using System.Reflection;

namespace DotNetPowerExtensions.Reflection;

public static class EventInfoExtensions
{
    public static MethodInfo[] GetAllMethods(this EventInfo eventInfo) => new [] { eventInfo.AddMethod!, eventInfo.RemoveMethod! }.ToArray();

    public static bool IsPrivate(this EventInfo eventInfo) => eventInfo.GetAllMethods().All(m => m.IsPrivate);

    public static bool IsAbstract(this EventInfo eventInfo) => eventInfo.GetAllMethods().Any(m => m.IsAbstract);

    public static bool IsOverridable(this EventInfo eventInfo) => eventInfo.GetAllMethods().First().IsOverridable();
    public static bool IsExplicitImplementation(this EventInfo eventInfo)
     => eventInfo.Name.Contains('.') && eventInfo.GetAllMethods().Any(m => m.IsExplicitImplementation());
}
