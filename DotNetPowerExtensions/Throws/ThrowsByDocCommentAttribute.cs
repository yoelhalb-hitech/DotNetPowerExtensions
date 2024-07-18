
namespace SequelPay.DotNetPowerExtensions;

/// <summary>
/// Specifies that the given property/method is describing all exception that it might throw (or propagate) in the doc comments
/// </summary>
[AttributeUsage(AttributeTargets.Property|AttributeTargets.Method|AttributeTargets.Event)]
public sealed class ThrowsByDocCommentAttribute : Attribute
{
	public ThrowsByDocCommentAttribute()
	{
    }
}
