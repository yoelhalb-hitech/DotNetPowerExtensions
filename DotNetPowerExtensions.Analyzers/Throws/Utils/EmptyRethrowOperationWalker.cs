using Microsoft.CodeAnalysis.Operations;

namespace DotNetPowerExtensions.Analyzers.Throws;

/// <summary>
/// Walker to determine if there is an empty throw statement in a catch clause for rethrow
/// </summary>
public class EmptyRethrowOperationWalker : OperationVisitor<object?, bool>
{
    public override bool DefaultVisit(IOperation operation, object? _)
        => operation.ChildOperations.Any(o => Visit(o, null));

    public override bool VisitCatchClause(ICatchClauseOperation operation, object? _)
        => false; // In inner catch clause we cannot rethrow as it will mean the inner one

    public override bool VisitThrow(IThrowOperation operation, object? _)
        => operation.Exception is null;

    public override bool VisitMethodBodyOperation(IMethodBodyOperation operation, object? _)
        => false;
}
