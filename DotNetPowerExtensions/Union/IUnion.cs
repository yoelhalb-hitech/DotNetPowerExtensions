#pragma warning disable CA1716

namespace SequelPay.DotNetPowerExtensions;

internal interface IUnion
{
    T? As<T>() where T : class;  // TODO... Add analyzer that ensures the type is correct, can then suppress nulls
}

#pragma warning restore CA1716
