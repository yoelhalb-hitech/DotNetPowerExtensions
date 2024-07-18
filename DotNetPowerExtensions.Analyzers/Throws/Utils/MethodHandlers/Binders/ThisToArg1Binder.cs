using Microsoft.CodeAnalysis.Operations;
using static DotNetPowerExtensions.Analyzers.Throws.AssignmentDataFlowOperationWalker;

namespace DotNetPowerExtensions.Analyzers.Throws;

internal class ThisToArg1Binder : IBinder
{
    public void Bind(IInvocationOperation invocation, IMethodSymbol? method, DataFlowResult dataFlowResult)
    {
        if (!invocation.Arguments.Any()) return;
        dataFlowResult.Handle(invocation.Arguments.First(), invocation.Instance);
    }
}
