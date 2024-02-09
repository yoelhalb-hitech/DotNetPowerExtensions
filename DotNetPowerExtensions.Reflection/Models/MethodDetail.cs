using SequelPay.DotNetPowerExtensions;
using SequelPay.DotNetPowerExtensions.Reflection.Core.Models;

namespace SequelPay.DotNetPowerExtensions.Reflection.Models;

public class MethodDetail : MemberDetail<MethodInfo, MethodDetail, IMethodDetail>, IMethodDetail
{
    internal MethodDetail() { }

    [Initialized] public override MemberDetailTypes MemberDetailType { get => MemberDetailTypes.Method; internal set => throw new NotSupportedException(); }

    public Type ReturnType => ReflectionInfo.ReturnType;

    /// <summary>
    /// CAUTION: This might not work correctly for default interface implementations
    /// </summary>
    [MustInitialize] public MethodDetail? OverridenMethod { get; internal set; }

    private Type[]? argumentTypes;
    public Type[] ArgumentTypes
        => LazyInitializer.EnsureInitialized(ref argumentTypes,
            () => ReflectionInfo.GetParameters().Select(p => p.ParameterType).ToArray())!;

    private ParameterDetail[]? parameters;
    public ParameterDetail[] Parameters => LazyInitializer.EnsureInitialized(ref parameters, () =>
        ReflectionInfo.GetParameters().Select(p => new ParameterDetail
        { //We have to think about these stack overflow issues because of the circular dependecies
            Name = p.Name ?? "",
            ParameterType = p.ParameterType.GetTypeDetailInfo(),
            ParameterModifierType = p.IsIn ? ParameterModifierTypes.In :
                                                    p.IsOut ? ParameterModifierTypes.Out :
                                                    p.ParameterType.IsByRef ? ParameterModifierTypes.Ref : ParameterModifierTypes.None,
        }).ToArray())!;

    public bool IsGeneric => ReflectionInfo.IsGenericMethod;
    public bool IsGenericDefinition => ReflectionInfo.IsGenericMethodDefinition;
    public bool IsConstructedGeneric => IsGeneric && !IsGenericDefinition;

    private TypeDetailInfo[]? genericArguments;
    public TypeDetailInfo[] GenericArguments => LazyInitializer.EnsureInitialized(ref genericArguments, () =>
                                ReflectionInfo.GetGenericArguments().Select(a => a.GetTypeDetailInfo()).ToArray())!;

    [MustInitialize] public MethodDetail? GenericDefinition { get; internal set; }

    ITypeDetailInfo IMethodDetail.ReturnType => ReturnType.GetTypeDetailInfo();
    IMethodDetail? IMethodDetail.OverridenMethod => OverridenMethod;
    IParameterDetail[] IMethodBase<IMethodDetail>.Parameters => Parameters;
    ITypeDetailInfo[] IGenericBase<IMethodDetail>.GenericArguments => GenericArguments;
    IMethodDetail? IGenericBase<IMethodDetail>.GenericDefinition => GenericDefinition;
    public MethodDetail GetConstructedGeneric(TypeDetailInfo[] genericArgs)
    {
        var args = genericArgs.Cast<TypeDetailInfo>().ToArray();

        var methodInfo = ReflectionInfo.MakeGenericMethod(args.Select(a => a.Type).ToArray());
        return new MethodDetail
        {            
            OverridenMethod = OverridenMethod,
            ExplicitDetail = ExplicitDetail,
            GenericDefinition = this,
            Name = Name,
            ReflectionInfo = methodInfo,
            IsInherited = IsInherited,
            DeclarationType = DeclarationType,
            InReflectionForCurrentType = InReflectionForCurrentType,
        };
    }

    IMethodDetail IGenericBase<IMethodDetail>.GetConstructedGeneric(ITypeDetailInfo[] genericArgs)
        => GetConstructedGeneric(genericArgs.Cast<TypeDetailInfo>().ToArray());
}
