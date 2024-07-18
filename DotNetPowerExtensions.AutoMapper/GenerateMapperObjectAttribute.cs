
namespace SequelPay.DotNetPowerExtensions.AutoMapper;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class GenerateMapperObjectAttribute : Attribute
{
    public string ObjectName { get; }
    public bool MapInternal { get; }

    public GenerateMapperObjectAttribute(string objectName, bool mapInternal = false)
    {
        ObjectName = objectName;
        MapInternal = mapInternal;
    }
}