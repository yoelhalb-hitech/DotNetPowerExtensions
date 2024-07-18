extern alias Workspaces;

using DotNetPowerExtensions.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System;
using Workspaces::Microsoft.CodeAnalysis.Shared.Extensions;

namespace DotNetPowerExtensions.Analyzers.Throws;

public class ExceptionOperationWalker : ThrowsOperationWalker
{
    private readonly PossibleExceptionTracker exceptionTracker;

    public ExceptionOperationWalker(Compilation compilation, PossibleExceptionTracker exceptionTracker) : base(compilation)
    {
        this.exceptionTracker = exceptionTracker;
    }

    public override object? Visit(IOperation? operation, (bool, List<ITypeSymbol>) argument)
    {
        if(operation is not null)
        {
            var result = exceptionTracker.GetPossibleExceptions(operation);
            if(result is not null) argument.Item2.AddRange(result);
        }
        return base.Visit(operation, argument);
    }

    public override object? VisitPropertyReference(IPropertyReferenceOperation refOperation, (bool, List<ITypeSymbol>) argument)
    {
        argument.Item2.AddRange(exceptionTracker.GetSymbolExceptions(refOperation.Property));
        return base.VisitPropertyReference(refOperation, argument);
    }

    public override object? VisitEventReference(IEventReferenceOperation refOperation, (bool, List<ITypeSymbol>) argument)
    {
        argument.Item2.AddRange(exceptionTracker.GetSymbolExceptions(refOperation.Event));
        return base.VisitEventReference(refOperation, argument);
    }
}
