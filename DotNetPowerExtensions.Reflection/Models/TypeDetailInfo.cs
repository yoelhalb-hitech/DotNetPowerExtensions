using SequelPay.DotNetPowerExtensions;

namespace DotNetPowerExtensions.Reflection.Models;

public class TypeDetailInfo
{
	internal TypeDetailInfo() {}
    [MustInitialize] public Type Type { get; internal set; }
    [MustInitialize] public PropertyDetail[] PropertyDetails { get; internal set; }
    [MustInitialize] public MethodDetail[] MethodDetails { get; internal set; }
    [MustInitialize] public EventDetail[] EventDetails { get; internal set; }
    [MustInitialize] public FieldDetail[] FieldDetails { get; internal set; }
    [MustInitialize] public PropertyDetail[] ShadowedPropertyDetails { get; internal set; }
    [MustInitialize] public MethodDetail[] ShadowedMethodDetails { get; internal set; }
    [MustInitialize] public EventDetail[] ShadowedEventDetails { get; internal set; }
    [MustInitialize] public FieldDetail[] ShadowedFieldDetails { get; internal set; }
    [MustInitialize] public PropertyDetail[] BasePrivatePropertyDetails { get; internal set; }
    [MustInitialize] public MethodDetail[] BasePrivateMethodDetails { get; internal set; }
    [MustInitialize] public EventDetail[] BasePrivateEventDetails { get; internal set; }
    [MustInitialize] public FieldDetail[] BasePrivateFieldDetails { get; internal set; }
    /// <summary>
    /// Array of <see cref="PropertyDetail"/> for properties that must be used explictly, will include default interface implmentations
    /// </summary>
    [MustInitialize] public PropertyDetail[] ExplicitPropertyDetails { get; internal set; }
    [MustInitialize] public MethodDetail[] ExplicitMethodDetails { get; internal set; }
    [MustInitialize] public EventDetail[] ExplicitEventDetails { get; internal set; }
}
