extern alias Workspaces;

using DotNetPowerExtensions.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System;
using Workspaces::Microsoft.CodeAnalysis.Shared.Extensions;

namespace DotNetPowerExtensions.Analyzers.Throws;

public class CatchOperationWalker : OperationWalker<(bool, List<ITypeSymbol>)>
{
    protected readonly Compilation compilation;

    public CatchOperationWalker(Compilation compilation)
    {
        this.compilation = compilation;
    }

    public override object? VisitTry(ITryOperation tryOperation, (bool, List<ITypeSymbol>) argument)
    {
        var childExceptions = new List<ITypeSymbol>();

        Visit(tryOperation.Body, (false, childExceptions)); // DefaultVisit will also capture catch which is not what we want

        var handledExcpetions = tryOperation.Catches.Where(c => c.Filter is null).Select(c => c.ExceptionType).ToArray();

        argument.Item2.AddRange(childExceptions.Where(c => handledExcpetions.All(e => !c.InheritsFromOrEquals(e))));

        tryOperation.Catches.ToList().ForEach(c => Visit(c, argument));
        Visit(tryOperation.Finally, argument);

        return null;
    }

    public override object? VisitDelegateCreation(IDelegateCreationOperation operation, (bool, List<ITypeSymbol>) argument)
        => null; // Ignore inline methods

    public override object? VisitAnonymousFunction(IAnonymousFunctionOperation operation, (bool, List<ITypeSymbol>) argument)
        => null; // Ignore inline methods

    public override object? VisitFlowAnonymousFunction(IFlowAnonymousFunctionOperation operation, (bool, List<ITypeSymbol>) argument)
        => null; // Ignore lambda expressions


    public override object? VisitLocalFunction(ILocalFunctionOperation operation, (bool, List<ITypeSymbol>) argument)
        => null; // Ignore local function
}
