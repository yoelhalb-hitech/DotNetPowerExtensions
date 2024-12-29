
namespace SequelPay.DotNetPowerExtensions;

/// <summary>
/// Specifies that the given Constructor is initializing all members marked with <see cref="MustInitializeAttribute"/> and does not caller initialization
/// </summary>
[AttributeUsage(AttributeTargets.Constructor)]
public class InitializesAllRequiredAttribute : Attribute
{
}
