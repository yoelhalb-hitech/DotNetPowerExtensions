
namespace DotNetPowerExtensions.Reflection;

public static class FieldInfoExtensions
{
    /// <summary>
    /// If the original Field is the equivalent of <see langword="internal" /> or <see langword="internal protected" />
    /// </summary>
    /// <param name="fieldInfo"></param>
    /// <returns></returns>
    public static bool IsInternal(this FieldInfo fieldInfo) => fieldInfo.IsAssembly || fieldInfo.IsFamilyOrAssembly;

    public static bool IsPublicOrInternal(this FieldInfo fieldInfo) => fieldInfo.IsPublic || fieldInfo.IsInternal();
}
