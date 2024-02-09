using System.Text.RegularExpressions;

namespace SequelPay.DotNetPowerExtensions.Reflection.Core.Paths.Maskers;

internal abstract class MaskerBase : IMasker
{
    protected abstract Regex Regex { get; }
    protected Dictionary<int, string> replaceDict = new();

    protected abstract string ReplaceKey(int key);

    public string Mask(string str)
    {
        Match? match = null;

        // Remeber that generics can be nested so we start with the simplest ones and work our way from there

        for (var i = 0; (match = Regex.Match(str)).Success; i++)
        {
            str = str.Replace(match.Value, ReplaceKey(i));
            replaceDict[i] = match.Value;
        }

        return str;
    }

    public IEnumerable<string> Unmask(IEnumerable<string> strs)
    {
        // Remeber that generics can be nested so we start with the last one first as they might contain other nested replacements

        foreach (var entry in replaceDict.OrderByDescending(r => r.Key))
        {
            strs = strs.Select(s => s.Replace(ReplaceKey(entry.Key), entry.Value));
        }

        return strs;
    }
}
