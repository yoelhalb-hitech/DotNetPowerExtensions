using System.Collections.Concurrent;

namespace SequelPay.DotNetPowerExtensions.Reflection;

internal abstract class TypeToStringBase
{
    // TODO... how to handle correctly `file` classes without name collision?
    protected static bool IsGenericType(Type type) => type.IsGenericType
        //Non generic class in a generic class has `IsGenericType` true and the same generic arguments as the parent
        && (!type.IsNested || type.DeclaringType!.GetGenericArguments().Length != type.GetGenericArguments().Length);

    protected static Type[] GetCurrentGenericArguments(Type type, Type[]? genericArgs)
                                                        => (genericArgs ?? type.GetGenericArguments()) // GenericTypeArguments only works for constructed type
                                                                                                        // Nested type also has the declaring generic types and they are even different than the outer ones so comparison won't help
                                                                .Skip(type.DeclaringType?.GetGenericArguments().Length ?? 0) // But they appear to be in order
                                                                .ToArray();
    protected static bool IsGenericArgumentStub(Type type) => type.IsGenericParameter;

    protected static bool IsArray(Type type) => type.IsArray;
    protected static string GetTypeName(Type type) => type.Name.Contains('`') ? type.Name.Substring(0, type.Name.IndexOf('`')) : type.Name;

    protected abstract string ArraySymbol { get; }
    protected abstract string GenericStartSymbol { get; }
    protected abstract string ValueTupleStartSymbol { get; }
    protected abstract string ValueTupleEndSymbol { get; }
    protected abstract string GenericEndSymbol { get; }
    protected abstract string GenericSeparatorSymbol { get; }
    protected abstract string NamespaceClassSeparatorSymbol { get; }
    protected abstract string NestedClassSeparatorSymbol { get; }

    private static ConcurrentDictionary<Type, string> typeNameCache = new ConcurrentDictionary<Type, string>();
    private static ConcurrentDictionary<Type, string> typeFullNameCache = new ConcurrentDictionary<Type, string>();

    public string GetTypeNameString(Type type) => typeNameCache.GetOrAdd(type, t => ToGenericTypeString(t, false, false, null));
    public string GetTypeFullNameString(Type type) => typeFullNameCache.GetOrAdd(type, t => ToGenericTypeString(t, true, false, null));

    public abstract string? HandleCustomName(Type t);
    internal protected string ToGenericTypeString(Type type, bool fullName, bool emptyForStub, Type[]? genericArgs)
    {
        if (type is null) throw new ArgumentNullException(nameof(type));
        // Note: We don't use FullName as it is much more complicated

        if (IsGenericArgumentStub(type)) return type.Name;
        else if (IsArray(type)) return ToGenericTypeString(type.GetElementType()!, fullName, emptyForStub, genericArgs) + ArraySymbol;

        if(!fullName)
        {
            var custom = HandleCustomName(type);
            if (custom is not null) return custom;
        }

        var currentGenericArgs = GetCurrentGenericArguments(type, genericArgs);
        var genericPart = string.Join(GenericSeparatorSymbol, currentGenericArgs
                                        .Select(a => emptyForStub && a.IsGenericParameter ? "" : ToGenericTypeString(a, fullName, emptyForStub, null)));

        // Value Tuple symbol does not allow stubs
        // Using the FullName to ensure we are not dealing with soomething with a similar name
        if (type.FullName?.StartsWith("System.ValueTuple`", StringComparison.Ordinal) == true && !fullName && !type.IsGenericTypeDefinition)
            return ValueTupleStartSymbol + genericPart + ValueTupleEndSymbol;

        var name = IsGenericType(type) ? type.Name.Substring(0, type.Name.IndexOf('`')) + GenericStartSymbol + genericPart + GenericEndSymbol : type.Name;

        if (fullName && type.DeclaringType is not null)
        {
            var allGenericArgs = (genericArgs ?? type.GetGenericArguments());  // Only the actual type has the constructed generic args (not declaring type) so we have to pass it around
            var newGenericArgs = allGenericArgs?.Take(allGenericArgs.Length - currentGenericArgs.Length).ToArray(); // The outer are the first
            return ToGenericTypeString(type.DeclaringType, fullName, emptyForStub, newGenericArgs) + NestedClassSeparatorSymbol + name;
        }
        else if (fullName) return type.Namespace + NamespaceClassSeparatorSymbol + name;

        return name;
    }
}
