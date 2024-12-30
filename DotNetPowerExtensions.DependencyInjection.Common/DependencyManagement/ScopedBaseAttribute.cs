
namespace SequelPay.DotNetPowerExtensions;

public class ScopedBaseAttribute : DependencyBaseAttribute
{
    public ScopedBaseAttribute() : base(DependencyType.Scoped) { }
}