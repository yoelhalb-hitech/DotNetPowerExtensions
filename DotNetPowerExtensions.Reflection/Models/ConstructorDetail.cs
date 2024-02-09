using SequelPay.DotNetPowerExtensions.Reflection.Core.Models;

namespace SequelPay.DotNetPowerExtensions.Reflection.Models;

public class ConstructorDetail : MemberDetail<ConstructorInfo, ConstructorDetail, IConstructorDetail>, IConstructorDetail
{
    [Initialized] public override string Name { get => ""; internal set => throw new NotSupportedException(); }
    [Initialized] public override MemberDetailTypes MemberDetailType { get => MemberDetailTypes.Constructor; internal set => throw new NotSupportedException(); }
    [Initialized] public override DeclarationTypes DeclarationType { get => DeclarationTypes.Decleration; internal set => throw new NotSupportedException(); }
    [Initialized] public override bool InReflectionForCurrentType { get => true; internal set => throw new NotSupportedException(); }
    [Initialized] public override bool IsInherited { get => false; internal set => throw new NotSupportedException(); }
    [Initialized] public override ConstructorDetail? ExplicitDetail { get => null; internal set => throw new NotSupportedException(); }

    private ParameterDetail[]? parameters;
    public ParameterDetail[] Parameters => LazyInitializer.EnsureInitialized(ref parameters, () =>
        ReflectionInfo.GetParameters().Select(p => new ParameterDetail
        {
            Name = p.Name ?? "",
            ParameterType = p.ParameterType.GetTypeDetailInfo(),
            ParameterModifierType = p.IsIn ? ParameterModifierTypes.In :
                                                p.IsOut ? ParameterModifierTypes.Out :
                                                p.ParameterType.IsByRef ? ParameterModifierTypes.Ref : ParameterModifierTypes.None,
        }).ToArray())!;

    IParameterDetail[] IMethodBase<IConstructorDetail>.Parameters => Parameters;
}
