
namespace SequelPay.DotNetPowerExtensions.Reflection.Core.Models;

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
