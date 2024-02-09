using SequelPay.DotNetPowerExtensions;
using SequelPay.DotNetPowerExtensions.Reflection.Core.Models;
using System.Text.RegularExpressions;

namespace SequelPay.DotNetPowerExtensions.Reflection.Models;

public class TypeDetailInfo : ITypeDetailInfo
{
	internal TypeDetailInfo(Type type)
    {
        Type = type;
        typeInfo = new Lazy<TypeInfoFactory.TypeInfo>(() => new TypeInfoFactory(Type).CreateTypeInfo());
        StaticConstructor = Type.GetConstructors(BindingFlagsExtensions.AllBindings)
                .Where(c => c.IsStatic && !c.IsConstructor).Select(c => new ConstructorDetail
                {
                    ReflectionInfo = c,
                }).FirstOrDefault();
    }
    public Type Type { get; }
    public ConstructorDetail? StaticConstructor { get; }
    private ConstructorDetail[]? constructorDetails;
    public ConstructorDetail[] ConstructorDetails => LazyInitializer.EnsureInitialized(ref constructorDetails, () =>
                Type.GetConstructors(BindingFlagsExtensions.AllBindings)
                .Where(c => c.IsConstructor && !c.IsStatic)
                .Select(c => new ConstructorDetail
                {
                    ReflectionInfo = c,
                }).ToArray())!;
    private Lazy<TypeInfoFactory.TypeInfo> typeInfo;
    public PropertyDetail[] PropertyDetails => typeInfo.Value.PropertyDetails;
    public MethodDetail[] MethodDetails => typeInfo.Value.MethodDetails;
    public EventDetail[] EventDetails => typeInfo.Value.EventDetails;
    public FieldDetail[] FieldDetails  => typeInfo.Value.FieldDetails;
    public PropertyDetail[] ShadowedPropertyDetails  => typeInfo.Value.ShadowedPropertyDetails;
    public MethodDetail[] ShadowedMethodDetails  => typeInfo.Value.ShadowedMethodDetails;
    public EventDetail[] ShadowedEventDetails  => typeInfo.Value.ShadowedEventDetails;
    public FieldDetail[] ShadowedFieldDetails  => typeInfo.Value.ShadowedFieldDetails;
    public PropertyDetail[] BasePrivatePropertyDetails  => typeInfo.Value.BasePrivatePropertyDetails;
    public MethodDetail[] BasePrivateMethodDetails  => typeInfo.Value.BasePrivateMethodDetails;
    public EventDetail[] BasePrivateEventDetails  => typeInfo.Value. BasePrivateEventDetails;
    public FieldDetail[] BasePrivateFieldDetails => typeInfo.Value.BasePrivateFieldDetails;
    /// <summary>
    /// Array of <see cref="PropertyDetail"/> for properties that must be used explictly, will include default interface implmentations
    /// </summary>
    public PropertyDetail[] ExplicitPropertyDetails => typeInfo.Value.ExplicitPropertyDetails;
    public MethodDetail[] ExplicitMethodDetails => typeInfo.Value.ExplicitMethodDetails;
    public EventDetail[] ExplicitEventDetails => typeInfo.Value.ExplicitEventDetails;

    private static Regex fileClassRegex = new Regex(@"(^|\.|\+)<([^>]+)>[^_]+__([^+]+)");
    private string? name;
    public string Name => LazyInitializer.EnsureInitialized(ref name,
            ()=> fileClassRegex.Replace(Type.Name, "$3").SubstringUntil('`')!)!; // Fix for file class as well as generic

    public string? fileName;
    public string? FileName => LazyInitializer.EnsureInitialized(ref fileName, () => fileClassRegex.Replace(Type.Name, "$2"));

    public string? Namepsace => Type.Namespace;

    public TypeDetailInfo? BaseType
        => Type.BaseType is null ? null : Type.BaseType.GetTypeDetailInfo();

    public TypeDetailInfo? OuterType
        => Type.DeclaringType is null ? null : Type.DeclaringType.GetTypeDetailInfo();

    private TypeDetailInfo[]? interfaces;
    public TypeDetailInfo[] Interfaces => LazyInitializer.EnsureInitialized(ref interfaces,
                () => Type.GetInterfaces().Select(i => i.GetTypeDetailInfo()).ToArray())!;

    private TypeDetailInfo[]? genericArguments;
    public TypeDetailInfo[] GenericArguments
        => LazyInitializer.EnsureInitialized(ref genericArguments,
                () => Type.GetGenericArguments().Select(a => a.GetTypeDetailInfo()).ToArray())!;

    public bool IsGeneric => Type.IsGenericType;

    public bool IsGenericDefinition => Type.IsGenericTypeDefinition;
    public bool IsGenericParameter => Type.IsGenericParameter;

    public bool IsConstructedGeneric => IsGeneric && !Type.IsGenericParameter && !IsGenericDefinition;

    private TypeDetailInfo? genericDefinition;
    public TypeDetailInfo? GenericDefinition
        => !IsGeneric || IsGenericDefinition ? null :
                LazyInitializer.EnsureInitialized(ref genericDefinition, () => Type.GetGenericTypeDefinition().GetTypeDetailInfo());

    public TypeDetailInfo GetConstructedGeneric(TypeDetailInfo[] genericArgs)
    {
        var args = genericArgs.Cast<TypeDetailInfo>().ToArray();

        var type = Type.MakeGenericType(args.Select(a => a.Type).ToArray());
        return type.GetTypeDetailInfo();
    }

    public TypeDetailInfo ToArrayType()
    {
        var type = Type.MakeArrayType();
        return type.GetTypeDetailInfo();
    }

    ITypeDetailInfo? ITypeDetailInfo.BaseType => BaseType;
    ITypeDetailInfo? ITypeDetailInfo.OuterType => OuterType;
    ITypeDetailInfo[] ITypeDetailInfo.Interfaces => Interfaces;
    IConstructorDetail[] ITypeDetailInfo.ConstructorDetails => ConstructorDetails;
    IPropertyDetail[] ITypeDetailInfo.PropertyDetails => PropertyDetails;
    IMethodDetail[] ITypeDetailInfo.MethodDetails => MethodDetails;
    IEventDetail[] ITypeDetailInfo.EventDetails => EventDetails;
    IFieldDetail[] ITypeDetailInfo.FieldDetails => FieldDetails;
    IPropertyDetail[] ITypeDetailInfo.ShadowedPropertyDetails => ShadowedPropertyDetails;
    IMethodDetail[] ITypeDetailInfo.ShadowedMethodDetails => ShadowedMethodDetails;
    IEventDetail[] ITypeDetailInfo.ShadowedEventDetails => ShadowedEventDetails;
    IFieldDetail[] ITypeDetailInfo.ShadowedFieldDetails => ShadowedFieldDetails;
    IPropertyDetail[] ITypeDetailInfo.BasePrivatePropertyDetails => BasePrivatePropertyDetails;
    IMethodDetail[] ITypeDetailInfo.BasePrivateMethodDetails => BasePrivateMethodDetails;
    IEventDetail[] ITypeDetailInfo.BasePrivateEventDetails => BasePrivateEventDetails;
    IFieldDetail[] ITypeDetailInfo.BasePrivateFieldDetails => BasePrivateFieldDetails;
    IPropertyDetail[] ITypeDetailInfo.ExplicitPropertyDetails => ExplicitPropertyDetails;
    IMethodDetail[] ITypeDetailInfo.ExplicitMethodDetails => ExplicitMethodDetails;
    IEventDetail[] ITypeDetailInfo.ExplicitEventDetails => ExplicitEventDetails;
    ITypeDetailInfo[] IGenericBase<ITypeDetailInfo>.GenericArguments => GenericArguments;
    ITypeDetailInfo? IGenericBase<ITypeDetailInfo>.GenericDefinition => GenericDefinition;


    ITypeDetailInfo ITypeDetailInfo.ToArrayType() => ToArrayType();

    public ITypeDetailInfo GetConstructedGeneric(ITypeDetailInfo[] genericArgs)
        => GetConstructedGeneric(genericArgs.Cast<TypeDetailInfo>().ToArray());
}
