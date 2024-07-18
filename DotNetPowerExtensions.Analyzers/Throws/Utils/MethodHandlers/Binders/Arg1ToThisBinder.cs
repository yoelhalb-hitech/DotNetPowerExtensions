using Microsoft.CodeAnalysis.Operations;
using static DotNetPowerExtensions.Analyzers.Throws.AssignmentDataFlowOperationWalker;

namespace DotNetPowerExtensions.Analyzers.Throws;

internal class Arg1ToThisBinder : IBinder
{
    public void Bind(IInvocationOperation invocation, IMethodSymbol? method, DataFlowResult dataFlowResult)
    {
        if (invocation.Instance is null) return;
        dataFlowResult.Handle(invocation.Instance, invocation.Arguments.FirstOrDefault());
    }
}

