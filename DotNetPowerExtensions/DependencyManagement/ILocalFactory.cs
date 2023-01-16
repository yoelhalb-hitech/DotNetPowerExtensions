
namespace SequelPay.DotNetPowerExtensions;

public interface ILocalFactory<out TClass>
{
    TClass? Create();
    [NonDelegate] TClass? Create(object arg);
}
