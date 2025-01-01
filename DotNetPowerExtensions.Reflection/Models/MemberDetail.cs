
namespace SequelPay.DotNetPowerExtensions.Reflection;

public interface IMemberDetail<out T> : IMemberDetail where T : MemberInfo
{
    public T ReflectionInfo { get; }

    public Type? ExplicitInterface { get; }
    public T? ExplicitInterfaceReflectionInfo { get; }
}

public abstract class MemberDetail<T> : IMemberDetail<T> where T : MemberInfo
{
    internal MemberDetail() { }

    [MustInitialize] public virtual string Name { get; internal set; }
    [MustInitialize] public virtual T ReflectionInfo { get; internal set; }
    [MustInitialize] public virtual MemberDetailTypes MemberDetailType { get; internal set; }
    [MustInitialize] public virtual bool IsInherited { get; internal set; }
    [MustInitialize] public virtual DeclarationTypes DeclarationType { get; internal set; }
    [MustInitialize] public virtual bool InReflectionForCurrentType { get; internal set; }

    public virtual ITypeDetailInfo CurrentType => ReflectionInfo.ReflectedType!.GetTypeDetailInfo();
    public virtual ITypeDetailInfo DeclaringType
        => ReflectionInfo.DeclaringType == ReflectionInfo.ReflectedType ? CurrentType : ReflectionInfo.DeclaringType!.GetTypeDetailInfo();

    public bool IsExplicit => ExplicitInterface is not null;
    public abstract Type? ExplicitInterface { get; }
    public abstract T? ExplicitInterfaceReflectionInfo { get; }
}

public class MemberDetail<T, TDetail, TInterfaceDetail> : MemberDetail<T>, Common.IMemberDetail<TInterfaceDetail>
    where T : MemberInfo
    where TDetail : MemberDetail<T, TDetail, TInterfaceDetail>, TInterfaceDetail
    where TInterfaceDetail : Common.IMemberDetail<TInterfaceDetail>
{
    internal MemberDetail() { }

    [MustInitialize] public virtual TDetail? ExplicitDetail { get; internal set; }
    public override Type? ExplicitInterface => (ExplicitDetail?.DeclaringType as TypeDetailInfo)?.Type;
    public override T? ExplicitInterfaceReflectionInfo => (ExplicitDetail as IMemberDetail<T>)?.ReflectionInfo;

    TInterfaceDetail? Common.IMemberDetail<TInterfaceDetail>.ExplicitDetail => ExplicitDetail;
}
