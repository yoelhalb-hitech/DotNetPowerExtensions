
namespace SequelPay.DotNetPowerExtensions;

/// <summary>
/// Specifies that the given Constructor is initializing all properties and fields marked with <see cref="MustInitializeAttribute"/> and does not caller initialization
/// </summary>
[AttributeUsage(AttributeTargets.Constructor)]
public class InitializesAttribute : Attribute
{
    public InitializesAttribute(string member, params string[] members)
    {
    }
}
