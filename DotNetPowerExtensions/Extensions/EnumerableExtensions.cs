
namespace SequelPay.DotNetPowerExtensions;

public static class EnumerableExtensions
{
    /// <summary>
    /// Determines whether a sequence <see cref="IEnumerable"/> is empty
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">The System.Collections.Generic.IEnumerable`1 to check for emptiness.</param>
    /// <returns>true if the source sequence does not contain any elements; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">source is null.</exception>
    public static bool Empty<TSource>(this IEnumerable<TSource> source) => !source.Any();

    /// <summary>
    /// Determines whether a sequence <see cref="IEnumerable"/> is either null or empty
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">The System.Collections.Generic.IEnumerable`1 to check for null or emptiness.</param>
    /// <returns>true if the source sequence is null or does not contain any elements; otherwise, false.</returns>
    public static bool NullOrEmpty<TSource>(this IEnumerable<TSource>? source) => source?.Any() != true;

    /// <summary>
    /// Determines whether a sequence <see cref="IEnumerable"/> has exactly one element
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">The System.Collections.Generic.IEnumerable`1 to check for elements.</param>
    /// <returns>true if the source sequence is not null and contains exactly one element; otherwise, false.</returns>
    public static bool HasOnlyOne<TSource>(this IEnumerable<TSource>? source) => source?.NullOrEmpty() != true && source?.Skip(1).Empty() == true;

}
