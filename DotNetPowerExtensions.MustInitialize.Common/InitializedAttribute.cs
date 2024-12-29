
namespace SequelPay.DotNetPowerExtensions;

/// <summary>
/// Specifies that the given property/field has been initialized already in the current subclass and does not require caller initialization
/// </summary>
[AttributeUsage(AttributeTargets.Property|AttributeTargets.Field)]
public sealed class InitializedAttribute : Attribute
{
}
