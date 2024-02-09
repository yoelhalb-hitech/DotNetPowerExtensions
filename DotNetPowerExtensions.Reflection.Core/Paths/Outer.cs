
namespace SequelPay.DotNetPowerExtensions.Reflection.Core.Paths;

// Wrapper to save us of having to write out the generic constrains in each file...
internal partial class Outer<TTypeContainerCache>
    where TTypeContainerCache : Outer<TTypeContainerCache>.TypeContainerCache, new()
{
}
