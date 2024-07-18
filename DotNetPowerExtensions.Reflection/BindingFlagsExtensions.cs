
namespace DotNetPowerExtensions.Reflection;

public static class BindingFlagsExtensions
{
    public static BindingFlags AllBindings => BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
}
