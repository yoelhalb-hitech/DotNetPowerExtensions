using DotNetPowerExtensions.AccessControl;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable CA1716
namespace DotNetPowerExtensions.Of;

public struct Of<TFirst, TSecond, TThird> : IOf
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
        return obj is Of<TFirst, TSecond, TThird> of && Value.Equals(of.Value);
    }

    public override int GetHashCode()
    {
        return -1937169412 + Value.GetHashCode();
    }

    public Of(TFirst value) => Value = value;
    public Of(TSecond value) => Value = value;
    public Of(TThird value) => Value = value;

    public static implicit operator TFirst?(Of<TFirst, TSecond, TThird> obj) => obj.First;
    public static implicit operator TSecond?(Of<TFirst, TSecond, TThird> obj) => obj.Second;
    public static implicit operator TThird?(Of<TFirst, TSecond, TThird> obj) => obj.Third;

    public static implicit operator Of<TFirst, TSecond, TThird>(TFirst obj) => new Of<TFirst, TSecond, TThird>(obj);
    public static implicit operator Of<TFirst, TSecond, TThird>(TSecond obj) => new Of<TFirst, TSecond, TThird>(obj);
    public static implicit operator Of<TFirst, TSecond, TThird>(TThird obj) => new Of<TFirst, TSecond, TThird>(obj);

    public static bool operator ==(Of<TFirst, TSecond, TThird> left, Of<TFirst, TSecond, TThird> right)
    {
        return left.Value == right.Value;
    }

    public static bool operator !=(Of<TFirst, TSecond, TThird> left, Of<TFirst, TSecond, TThird> right)
    {
        return left.Value != right.Value;
    }
}

#pragma warning restore CA1716