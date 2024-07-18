extern alias Workspaces;
using Microsoft.CodeAnalysis.Operations;
using Workspaces::Microsoft.CodeAnalysis.Shared.Extensions;
using static DotNetPowerExtensions.Analyzers.Throws.AssignmentDataFlowOperationWalker;

namespace DotNetPowerExtensions.Analyzers.Throws;


internal class Arg2SubResultToResultBinder : IBinder
{
    public void Bind(IInvocationOperation invocation, IMethodSymbol? method, DataFlowResult dataFlowResult)
    {
        if (invocation.Arguments.Count() < 2 || !invocation.Arguments.Skip(1).First().Parameter.IsDelegateType() // TODO... will this handle a method?
                                                        || !invocation.Arguments.Skip(1).First().Parameter!.Type.GetParameters().Any()) return;
        // TODO... bind but not to the general func param and also not to the entire delegate
    }
}

