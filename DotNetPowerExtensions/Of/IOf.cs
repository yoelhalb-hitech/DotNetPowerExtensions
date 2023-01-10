#pragma warning disable CA1716

using DotNetPowerExtensions.AccessControl;

namespace DotNetPowerExtensions.Of;

internal interface IOf
{
    T? As<T>() where T : class;  // TODO... Add analyzer that ensures the type is correct, can then suppress nulls
}

#pragma warning restore CA1716
