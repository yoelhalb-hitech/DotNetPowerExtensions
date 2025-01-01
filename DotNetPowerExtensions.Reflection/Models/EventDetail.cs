
namespace SequelPay.DotNetPowerExtensions.Reflection;

public class EventDetail : MemberDetail<EventInfo, EventDetail, IEventDetail>, IEventDetail
{
    internal EventDetail() { }

    public ITypeDetailInfo EventHandlerType => ReflectionInfo.EventHandlerType!.GetTypeDetailInfo();

    [Initialized] public override MemberDetailTypes MemberDetailType { get => MemberDetailTypes.Event; internal set => throw new NotSupportedException(); }

    [MustInitialize] public FieldDetail? BackingField { get; internal set; }
    [MustInitialize] public MethodDetail AddMethod { get; internal set; }
    [MustInitialize] public MethodDetail RemoveMethod { get; internal set; }
    /// <summary>
    /// CAUTION: This might not work correctly for default interface implementations
    /// </summary>
    [MustInitialize] public EventDetail? OverridenEvent { get; internal set; }

    ITypeDetailInfo IEventDetail.EventHandlerType => EventHandlerType;
    IFieldDetail? IEventDetail.BackingField => BackingField;
    IMethodDetail IEventDetail.AddMethod => AddMethod;
    IMethodDetail IEventDetail.RemoveMethod => RemoveMethod;
    IEventDetail? IEventDetail.OverridenEvent => OverridenEvent;
}
