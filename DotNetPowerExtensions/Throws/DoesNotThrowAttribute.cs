
namespace SequelPay.DotNetPowerExtensions;

/// <summary>
/// Specifies that the given property/method ensures that no exceptions are propagating
/// </summary>
[AttributeUsage(AttributeTargets.Property|AttributeTargets.Method|AttributeTargets.Event|AttributeTargets.Class|AttributeTargets.Struct|AttributeTargets.Assembly)]
public sealed class DoesNotThrowAttribute : Attribute
{
	public DoesNotThrowAttribute()
	{
    }

	public bool AllowIfSpecified { get; set; }
	public bool IgnorePrivate { get; set; }
	public bool IgnoreProtected { get; set; }
	public bool IgnoreInternal { get; set; }
}
