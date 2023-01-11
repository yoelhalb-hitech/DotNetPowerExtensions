#pragma warning disable CA1716

namespace DotNetPowerExtensions;

internal interface IOf
{
    T? As<T>() where T : class;  // TODO... Add analyzer that ensures the type is correct, can then suppress nulls
}

#pragma warning restore CA1716
