
namespace SequelPay.DotNetPowerExtensions.Reflection.Common;

public interface ITypeDetailInfo : IGenericBase<ITypeDetailInfo>
{
    string Name { get; }
    /// <summary>
    /// FileName if it is a file class, otherwise null
    /// </summary>
    string? FileName { get; }
    string? Namepsace { get; }
    ITypeDetailInfo? BaseType { get; }
    ITypeDetailInfo? OuterType { get; }
    ITypeDetailInfo[] Interfaces { get; }

    bool IsGenericParameter { get; }

    ITypeDetailInfo ToArrayType();

    IConstructorDetail[] ConstructorDetails { get; }
    IPropertyDetail[] PropertyDetails { get; }
    IMethodDetail[] MethodDetails { get; }
    IEventDetail[] EventDetails { get; }
    IFieldDetail[] FieldDetails { get; }

    IPropertyDetail[] ShadowedPropertyDetails { get; }
    IMethodDetail[] ShadowedMethodDetails { get; }
    IEventDetail[] ShadowedEventDetails { get; }
    IFieldDetail[] ShadowedFieldDetails { get; }

    IPropertyDetail[] BasePrivatePropertyDetails { get; }
    IMethodDetail[] BasePrivateMethodDetails { get; }
    IEventDetail[] BasePrivateEventDetails { get; }
    IFieldDetail[] BasePrivateFieldDetails { get; }

    /// <summary>
    /// Array of <see cref="IPropertyDetail"/> for properties that must be used explictly, will include default interface implmentations
    /// </summary>
    IPropertyDetail[] ExplicitPropertyDetails { get; }
    IMethodDetail[] ExplicitMethodDetails { get; }
    IEventDetail[] ExplicitEventDetails { get; }
}
