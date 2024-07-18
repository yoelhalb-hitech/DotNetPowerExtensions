using Microsoft.CodeAnalysis.Operations;
using static DotNetPowerExtensions.Analyzers.Throws.AssignmentDataFlowOperationWalker;

namespace DotNetPowerExtensions.Analyzers.Throws;

internal abstract class InvocationHandlerBase : IInvocationHandler
{
    public InvocationHandlerBase(Compilation compilation)
    {
        Compilation = compilation;
    }

    protected INamedTypeSymbol? GetTypeSymbol(Type type) => Compilation.GetTypeByMetadataName(type.FullName!);
    public Compilation Compilation { get; }

    public abstract List<IMethodRule> MethodRules { get; }

    public virtual void Handle(IInvocationOperation invocation, IMethodSymbol? method, ITypeSymbol? type, DataFlowResult dataFlowResult)
    {
        MethodRules.ForEach(rule => rule.Handle(invocation, method, type, dataFlowResult));
    }
}

