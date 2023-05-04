
namespace SequelPay.DotNetPowerExtensions;

/// <summary>
/// Specifies that the given Constructor is initializing the given members marked with <see cref="MustInitializeAttribute"/> and does not caller initialization
/// </summary>
[AttributeUsage(AttributeTargets.Constructor)]
public class InitializesAttribute : Attribute
{
    public InitializesAttribute(string member, params string[] members)
    {
        Members = new[] {member}.Concat(members).ToArray();
    }

    internal virtual string[] Members { get; set; }
}
