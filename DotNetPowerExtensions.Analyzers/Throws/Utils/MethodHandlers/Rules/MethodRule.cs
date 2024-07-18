using Microsoft.CodeAnalysis.Operations;
using static DotNetPowerExtensions.Analyzers.Throws.AssignmentDataFlowOperationWalker;

namespace DotNetPowerExtensions.Analyzers.Throws;

internal class MethodRule : IMethodRule
{
    public Rule Rule { get; set; } = default!;
    public IBinder Binder { get; set; } = default!;

    public void Handle(IInvocationOperation invocation, IMethodSymbol? method, ITypeSymbol? type, DataFlowResult dataFlowResult)
    {
        if (!Rule.IsMatch(invocation, method, type)) return;

        Binder.Bind(invocation, method, dataFlowResult);
    }
}
