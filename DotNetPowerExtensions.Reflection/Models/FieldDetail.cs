
namespace SequelPay.DotNetPowerExtensions.Reflection;

public class FieldDetail : MemberDetail<FieldInfo, FieldDetail, IFieldDetail>, IFieldDetail
{
    internal FieldDetail() { }
    [Initialized] public override FieldDetail? ExplicitDetail { get => null; internal set => throw new NotSupportedException(); }

    // Note we do no set the MemberDetailType as there are a few possibilities
    public TypeDetailInfo FieldType => ReflectionInfo.FieldType.GetTypeDetailInfo();

    ITypeDetailInfo IFieldDetail.FieldType => FieldType;
}
