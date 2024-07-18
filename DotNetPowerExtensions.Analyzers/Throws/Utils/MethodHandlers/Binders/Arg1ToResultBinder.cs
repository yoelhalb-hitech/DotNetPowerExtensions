using Microsoft.CodeAnalysis.Operations;
using static DotNetPowerExtensions.Analyzers.Throws.AssignmentDataFlowOperationWalker;

namespace DotNetPowerExtensions.Analyzers.Throws;

internal class Arg1ToResultBinder : IBinder
{
    public void Bind(IInvocationOperation invocation, IMethodSymbol? method, DataFlowResult dataFlowResult)
        => dataFlowResult.Handle(invocation, invocation.Arguments.FirstOrDefault());
}

