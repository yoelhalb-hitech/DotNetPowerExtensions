using System.Text.RegularExpressions;

namespace SequelPay.DotNetPowerExtensions.Reflection.Core.Paths.Maskers;

internal class ParenthesisMasker : MaskerBase
{
    private static Regex regex = new Regex(@"\([^)]+\)");
    protected override Regex Regex => regex;

    protected override string ReplaceKey(int key) => $";${key};";
}
