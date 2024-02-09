namespace SequelPay.DotNetPowerExtensions.Reflection.Core.Paths.Maskers;

internal interface IMasker
{
    string Mask(string str);
    IEnumerable<string> Unmask(IEnumerable<string> strs);
}
