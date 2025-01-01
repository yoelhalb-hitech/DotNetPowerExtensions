
namespace SequelPay.DotNetPowerExtensions.Reflection.Common;

public interface IMemberDetail
{
    string Name { get; }
    MemberDetailTypes MemberDetailType { get; }
    bool IsInherited { get; }
    DeclarationTypes DeclarationType { get; }
    bool InReflectionForCurrentType { get; }
    ITypeDetailInfo CurrentType { get; }
    ITypeDetailInfo DeclaringType { get; }
    bool IsExplicit { get; }
}


public interface IMemberDetail<out TDetail> : IMemberDetail where TDetail : IMemberDetail<TDetail>
{
    TDetail? ExplicitDetail { get; }
}
