
namespace SequelPay.DotNetPowerExtensions;

public class LocalBaseAttribute : DependencyBaseAttribute
{
    public LocalBaseAttribute() : base(DependencyType.Local) { }
}
