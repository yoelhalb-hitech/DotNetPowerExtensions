using DotNetPowerExtensions.AccessControl;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable CA1716
namespace DotNetPowerExtensions.Of;

public struct Of<TFirst, TSecond> : IOf
    where TFirst : class 
    where TSecond : class
{
    [DisallowNull] public TFirst? First { get => Value as TFirst; set => Value = value ?? throw new ArgumentNullException(nameof(value), "Cannot set to null"); }
    [DisallowNull] public TSecond? Second { get => Value as TSecond; set => Value = value ?? throw new ArgumentNullException(nameof(value), "Cannot set to null"); }

    internal object Value { get; set; }

    public T? As<T>() where T : class => Value as T; // TODO... Add analyzer that ensures the type is correct, can then suppress nulls

    public override bool Equals(object? obj)
    {
        return obj is Of<TFirst, TSecond> of && Value.Equals(of.Value);
    }

    public override int GetHashCode()
    {
        return -1937163414 + Value.GetHashCode();
    }

    public Of(TFirst value) => Value = value;
    public Of(TSecond value) => Value = value;

    public static implicit operator TFirst?(Of<TFirst, TSecond> obj) => obj.First;
    public static implicit operator TSecond?(Of<TFirst, TSecond> obj) => obj.Second;

    public static explicit operator Of<TFirst, TSecond>(TFirst obj) => new Of<TFirst, TSecond>(obj);
    public static explicit operator Of<TFirst, TSecond>(TSecond obj) => new Of<TFirst, TSecond>(obj);

    public static bool operator ==(Of<TFirst, TSecond> left, Of<TFirst, TSecond> right)
    {
        return left.Value == right.Value;
    }

    public static bool operator !=(Of<TFirst, TSecond> left, Of<TFirst, TSecond> right)
    {
        return left.Value != right.Value;
    }
}

#pragma warning restore CA1716