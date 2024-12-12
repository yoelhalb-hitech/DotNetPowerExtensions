using System.Diagnostics.CodeAnalysis;

namespace SequelPay.DotNetPowerExtensions;

public struct Union<TFirst, TSecond> : IUnion
    where TFirst : class
    where TSecond : class
{
    [DisallowNull] public TFirst? First { get => Value as TFirst; set => Value = value ?? throw new ArgumentNullException(nameof(value), "Cannot set to null"); }
    [DisallowNull] public TSecond? Second { get => Value as TSecond; set => Value = value ?? throw new ArgumentNullException(nameof(value), "Cannot set to null"); }

    internal object Value { get; set; }

    public T? As<T>() where T : class => Value as T; // TODO... Add analyzer that ensures the type is correct, can then suppress nulls

    public override bool Equals(object? obj)
    {
        return obj is Union<TFirst, TSecond> of && Value.Equals(of.Value);
    }

    public override int GetHashCode()
    {
        return -1937163414 + Value.GetHashCode();
    }

    public Union(TFirst value) => Value = value;
    public Union(TSecond value) => Value = value;

    public static implicit operator TFirst?(Union<TFirst, TSecond> obj) => obj.First;
    public static implicit operator TSecond?(Union<TFirst, TSecond> obj) => obj.Second;

    public static explicit operator Union<TFirst, TSecond>(TFirst obj) => new Union<TFirst, TSecond>(obj);
    public static explicit operator Union<TFirst, TSecond>(TSecond obj) => new Union<TFirst, TSecond>(obj);

    public static bool operator ==(Union<TFirst, TSecond> left, Union<TFirst, TSecond> right)
    {
        return left.Value == right.Value;
    }

    public static bool operator !=(Union<TFirst, TSecond> left, Union<TFirst, TSecond> right)
    {
        return left.Value != right.Value;
    }
}
