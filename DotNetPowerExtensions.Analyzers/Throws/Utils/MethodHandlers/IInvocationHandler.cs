using Microsoft.CodeAnalysis.Operations;
using static DotNetPowerExtensions.Analyzers.Throws.AssignmentDataFlowOperationWalker;

namespace DotNetPowerExtensions.Analyzers.Throws;

public interface IInvocationHandler
{
    void Handle(IInvocationOperation invocation, IMethodSymbol? method, ITypeSymbol? type, DataFlowResult dataFlowResult);
}
