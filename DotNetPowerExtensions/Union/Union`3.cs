using System.Diagnostics.CodeAnalysis;

#pragma warning disable CA1716
namespace SequelPay.DotNetPowerExtensions;

public struct Union<TFirst, TSecond, TThird> : IUnion
    where TFirst : class 
    where TSecond : class
    where TThird : class
{
    [DisallowNull] public TFirst? First { get => Value as TFirst; set => Value = value ?? throw new ArgumentNullException(nameof(value), "Cannot set to null"); }
    [DisallowNull] public TSecond? Second { get => Value as TSecond; set => Value = value ?? throw new ArgumentNullException(nameof(value), "Cannot set to null"); }
    [DisallowNull] public TThird? Third { get => Value as TThird; set => Value = value ?? throw new ArgumentNullException(nameof(value), "Cannot set to null"); }

    internal object Value { get; set; }

    public T? As<T>() where T : class => Value as T; // TODO... Add analyzer that ensures the type is correct, can then suppress nulls

    public override bool Equals(object? obj)
    {
        return obj is Union<TFirst, TSecond, TThird> of && Value.Equals(of.Value);
    }

    public override int GetHashCode()
    {
        return -1937169412 + Value.GetHashCode();
    }

    public Union(TFirst value) => Value = value;
    public Union(TSecond value) => Value = value;
    public Union(TThird value) => Value = value;

    public static implicit operator TFirst?(Union<TFirst, TSecond, TThird> obj) => obj.First;
    public static implicit operator TSecond?(Union<TFirst, TSecond, TThird> obj) => obj.Second;
    public static implicit operator TThird?(Union<TFirst, TSecond, TThird> obj) => obj.Third;

    public static implicit operator Union<TFirst, TSecond, TThird>(TFirst obj) => new Union<TFirst, TSecond, TThird>(obj);
    public static implicit operator Union<TFirst, TSecond, TThird>(TSecond obj) => new Union<TFirst, TSecond, TThird>(obj);
    public static implicit operator Union<TFirst, TSecond, TThird>(TThird obj) => new Union<TFirst, TSecond, TThird>(obj);

    public static bool operator ==(Union<TFirst, TSecond, TThird> left, Union<TFirst, TSecond, TThird> right)
    {
        return left.Value == right.Value;
    }

    public static bool operator !=(Union<TFirst, TSecond, TThird> left, Union<TFirst, TSecond, TThird> right)
    {
        return left.Value != right.Value;
    }
}

#pragma warning restore CA1716