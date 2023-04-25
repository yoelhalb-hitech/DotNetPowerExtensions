
namespace SequelPay.DotNetPowerExtensions;

public class SingletonBaseAttribute : DependencyBaseAttribute
{
    public SingletonBaseAttribute() : base(DependencyType.Singleton) { }
}
