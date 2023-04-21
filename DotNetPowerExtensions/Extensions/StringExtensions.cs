using System.Diagnostics.CodeAnalysis;

namespace DotNetPowerExtensions.Extensions;

public static class StringExtensions
{
    public static string? SubstringUntil(this string str, char charTill, bool last = false, bool include = false, bool nullIfNotFound = false)
    {
        var idx = last ? str.LastIndexOf(charTill) : str.IndexOf(charTill);
        if (idx < 0) return nullIfNotFound ? null : str;

        return str.Substring(0, idx + (include ? 1 : 0));
    }

    public static string? SubstringFrom(this string str, char charFrom, bool first = false, bool include = false, bool nullIfNotFound = false)
    {
        var idx = first ? str.IndexOf(charFrom) : str.LastIndexOf(charFrom);
        if (idx < 0) return nullIfNotFound ? null : str;

        return str.Substring(idx + (include ? 0 : 1));
    }

    public static string? SubstringUntil(this string str, string till, bool last = false, bool include = false, bool nullIfNotFound = false,
            StringComparison comparison = StringComparison.Ordinal)
    {
        var idx = last ? str.LastIndexOf(till, comparison) : str.IndexOf(till, comparison);
        if (idx < 0) return nullIfNotFound ? null : str;

        return str.Substring(0, idx + (include ? till.Length : 0));
    }

    public static string? SubstringFrom(this string str, string from, bool first = false, bool include = false, bool nullIfNotFound = false,
        StringComparison comparison = StringComparison.Ordinal)
    {
        var idx = first ? str.IndexOf(from, comparison) : str.LastIndexOf(from, comparison);
        if (idx < 0) return nullIfNotFound ? null : str;

        return str.Substring(idx + (include ? 0 : from.Length));
    }

    public static bool HasValue([NotNullWhen(true)] this string? str) => !string.IsNullOrWhiteSpace(str);
    public static bool CaseInsensitiveEquels(this string? str1, string? str2)
                            => string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);
#if NETSTANDARD2_1_OR_GREATER
    public static bool CaseInsensitiveContains(this string str1, string str2) => str1.Contains(str2, StringComparison.OrdinalIgnoreCase);
    public static bool CaseInsensitiveContains(this IEnumerable<string?> strs, string? str2)
                        => strs.Any(s => s.CaseInsensitiveEquels(str2));
#endif

    [return: NotNullIfNotNull("str")]
    [return: NotNullIfNotNull("str2")]
    public static string? Or(this string? str, string? str2) => str.HasValue() ? str : str2;

    public static string? NullIfNoValue(this string? str) => str.HasValue() ? str : null;
    public static string NonNull(this string? str) => str ?? "";

    public static string Join(this IEnumerable<string?> strs, string? joinStr) => string.Join(joinStr, strs);

}
