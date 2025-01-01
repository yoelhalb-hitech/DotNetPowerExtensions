/* Unmerged change from project 'DotNetPowerExtensions.Reflection.Common (net45)'
Before:
namespace SequelPay.DotNetPowerExtensions.Reflection.Common.Models;
After:
using SequelPay;
using SequelPay.DotNetPowerExtensions;
using SequelPay.DotNetPowerExtensions.Reflection;
using SequelPay.DotNetPowerExtensions.Reflection.Common;
using SequelPay.DotNetPowerExtensions.Reflection.Common;
using SequelPay.DotNetPowerExtensions.Reflection.Common.Models;

namespace SequelPay.DotNetPowerExtensions.Reflection.Common;
*/
namespace SequelPay.DotNetPowerExtensions.Reflection.Common;

public interface IEventDetail : IMemberDetail<IEventDetail>
{
    ITypeDetailInfo EventHandlerType { get; }
    IFieldDetail? BackingField { get; }
    IMethodDetail AddMethod { get; }
    IMethodDetail RemoveMethod { get; }
    /// <summary>
    /// CAUTION: This might not work correctly for default interface implementations
    /// </summary>
    IEventDetail? OverridenEvent { get; }
}
