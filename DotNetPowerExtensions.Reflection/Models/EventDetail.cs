using SequelPay.DotNetPowerExtensions;

namespace DotNetPowerExtensions.Reflection.Models;

public class EventDetail : MemberDetail<EventInfo>
{
    internal EventDetail() { }

    [Initialized] public override MemberDetailTypes MemberDetailType { get => MemberDetailTypes.Event; internal set => throw new NotSupportedException(); }
    [MustInitialize] public FieldDetail? BackingField { get; internal set; }
    [MustInitialize] public MethodDetail AddMethod { get; internal set; }
    [MustInitialize] public MethodDetail RemoveMethod { get; internal set; }
    /// <summary>
    /// CAUTION: This might not work correctly for default interface implementations
    /// </summary>
    [MustInitialize] public EventDetail? OverridenEvent { get; internal set; }
}
