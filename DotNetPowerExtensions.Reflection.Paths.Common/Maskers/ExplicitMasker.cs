using System.Text.RegularExpressions;

namespace SequelPay.DotNetPowerExtensions.Reflection.Paths.Maskers;

internal class ExplicitMasker : MaskerBase
{
    private static Regex regex = new Regex(":[^:]+:");
    protected override Regex Regex => regex;

    protected override string ReplaceKey(int key) => $"#${key}#";
}
