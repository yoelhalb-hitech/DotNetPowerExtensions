
namespace SequelPay.DotNetPowerExtensions.Reflection;

public static class CSharpTypeExtensions
{
    public static string ToCSharpTypeString(this Type type, bool fullName, bool emptyForStub, Type[]? genericArgs)
        => new TypeToCSharpString().ToGenericTypeString(type, fullName, emptyForStub, genericArgs);
}

internal class TypeToCSharpString : TypeToStringBase
{
    protected override string ArraySymbol => "[]";

    protected override string GenericStartSymbol => "<";

    protected override string ValueTupleStartSymbol => "(";

    protected override string ValueTupleEndSymbol => ")";

    protected override string GenericEndSymbol => ">";

    protected override string GenericSeparatorSymbol => ",";

    protected override string NamespaceClassSeparatorSymbol => ".";

    protected override string NestedClassSeparatorSymbol => ".";

    public override string? HandleCustomName(Type t)
        => t.FullName switch
        {
            nameof(System) + ".Void" => "void",
            nameof(System) + "." + nameof(Int32) => "int",
            nameof(System) + "." + nameof(UInt32) => "uint",
            nameof(System) + "." + nameof(Char) => "char",
            nameof(System) + "." + nameof(String) => "string",
            nameof(System) + "." + nameof(Single) => "float",
            nameof(System) + "." + nameof(Double) => "double",
            nameof(System) + "." + nameof(Decimal) => "decimal",
            nameof(System) + "." + nameof(Byte) => "byte",
            nameof(System) + "." + nameof(SByte) => "sbyte",
            nameof(System) + "." + nameof(Int16) => "short",
            nameof(System) + "." + nameof(UInt16) => "ushort",
            nameof(System) + "." + nameof(Int64) => "long",
            nameof(System) + "." + nameof(UInt64) => "ulong",
            nameof(System) + "." + nameof(Object) => "object",
            nameof(System) + "." + nameof(IntPtr) => "nint",
            nameof(System) + "." + nameof(UIntPtr) => "nuint",
            _ => null,
        };

    public override string? HandleInvalidName(Type t)
        => t == typeof(void) ? "void" : null;
}
