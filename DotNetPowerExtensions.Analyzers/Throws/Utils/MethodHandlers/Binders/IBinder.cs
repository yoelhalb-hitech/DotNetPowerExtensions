using Microsoft.CodeAnalysis.Operations;
using static DotNetPowerExtensions.Analyzers.Throws.AssignmentDataFlowOperationWalker;

namespace DotNetPowerExtensions.Analyzers.Throws;

internal interface IBinder
{
    void Bind(IInvocationOperation invocation, IMethodSymbol? method, DataFlowResult dataFlowResult);
}
