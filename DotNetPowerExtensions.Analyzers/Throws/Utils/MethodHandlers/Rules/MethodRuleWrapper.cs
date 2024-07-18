using Microsoft.CodeAnalysis.Operations;
using static DotNetPowerExtensions.Analyzers.Throws.AssignmentDataFlowOperationWalker;

namespace DotNetPowerExtensions.Analyzers.Throws;

internal class MethodRuleWrapper : IMethodRule
{
    public MethodRuleWrapper(Compilation compilation)
    {
        Compilation = compilation;
    }
    public Type? Type { get; set; }
    public string? Method { get; set; }
    public ArgTypes From { get; set; }
    public ArgTypes To { get; set; }
    public ArgTypes? SubFrom { get; set; }
    public ArgTypes? SubTo { get; set; }
    public Compilation Compilation { get; }
    public Rule? Rule { get; set; }

    private INamedTypeSymbol? GetTypeSymbol(Type type) => Compilation.GetTypeByMetadataName(type.FullName!);
    internal Rule GetRule() => Rule ?? new Rule
    {
        Type = Type is null ? null : GetTypeSymbol(Type),
        Methods = Type is null || Method is null ? null : (GetTypeSymbol(Type)?.GetMembers(Method).OfType<IMethodSymbol>() ?? new IMethodSymbol[0]),
    };
    internal IBinder GetBinder()
    {
        if(SubTo is null && SubFrom is null)
        {
            if (From == ArgTypes.This && To == ArgTypes.Arg1) return new ThisToArg1Binder();
            if (From == ArgTypes.Arg1 && To == ArgTypes.Result) return new Arg1ToResultBinder();
            if (From == ArgTypes.Arg1 && To == ArgTypes.This) return new Arg1ToThisBinder();
            if (From == ArgTypes.Arg2 && To == ArgTypes.Result) return new Arg2ToResultBinder();
        }
        if (From == ArgTypes.Arg1 && To == ArgTypes.Arg2 && SubTo == ArgTypes.Arg1) return new Arg1ToArg2SubArg1Binder();
        if (From == ArgTypes.Arg2 && To == ArgTypes.Result && SubFrom == ArgTypes.Result) return new Arg2SubResultToResultBinder();
        throw new NotImplementedException();
    }

    public void Handle(IInvocationOperation invocation, IMethodSymbol? method, ITypeSymbol? type, DataFlowResult dataFlowResult)
    {
        var methodRule = new MethodRule
        {
            Rule = GetRule(),
            Binder = GetBinder(),
        };
        methodRule.Handle(invocation, method, type, dataFlowResult);
    }
}

