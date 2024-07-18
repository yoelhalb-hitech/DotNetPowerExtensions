using Microsoft.CodeAnalysis.Operations;

namespace DotNetPowerExtensions.Analyzers.Throws;

internal class Rule
{
    public virtual INamedTypeSymbol? Type { get; set; }
    public virtual IEnumerable<IMethodSymbol>? Methods { get; set; }
    public virtual bool IsMatchingType(ITypeSymbol? type) => Type is null ? type is null :
                                                        (type.IsEqualTo(Type) || type is INamedTypeSymbol namedType && namedType.IsGenericEqualOrSubOf(Type));
    public virtual bool IsMatchingMethod(IMethodSymbol? method) => Methods?.ContainsSymbol(method) ?? false;
    public virtual Func<IInvocationOperation, bool>? IsMatchingInvocation { get; set; }


    public virtual bool IsMatch(IInvocationOperation invocation, IMethodSymbol? method, ITypeSymbol? type)
    {
        if (invocation is null) throw new ArgumentNullException(nameof(invocation));

        if (!IsMatchingType(type ?? method?.ReceiverType ?? invocation?.TargetMethod?.ReceiverType)) return false;

        if (!IsMatchingMethod(method ?? invocation?.TargetMethod)) return false;

        if (IsMatchingInvocation?.Invoke(invocation!) == false) return false;

        return true;
    }
}

