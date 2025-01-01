
namespace SequelPay.DotNetPowerExtensions.Reflection.Paths.Maskers;

internal interface IMasker
{
    string Mask(string str);
    IEnumerable<string> Unmask(IEnumerable<string> strs);
}
