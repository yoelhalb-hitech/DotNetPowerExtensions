
namespace SequelPay.DotNetPowerExtensions;

public class TransientBaseAttribute : DependencyBaseAttribute
{
    public TransientBaseAttribute() : base(DependencyType.Transient) { }
}
