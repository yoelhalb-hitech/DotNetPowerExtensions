
using Microsoft.CodeAnalysis.Operations;

namespace DotNetPowerExtensions.Analyzers.Throws;

/// <summary>
/// A <see cref="OperationWalker"/> that works each statement from inside out, suitable for assignment analysis
/// </summary>
public class BackwardOperationWalker : OperationWalker
{

    public override void Visit(IOperation? operation)
    {
        if (operation is not null)
        {
            var children = operation.ChildOperations.ToList();
            if (operation is IAssignmentOperation) children = children.Skip(1).ToList(); // Assignment has the first argument the reference

            // Since we are looking for symbols assigned we want the innermost first
            foreach (var child in children)
                Visit(child);
        }

        base.Visit(operation);

        if (operation is IAssignmentOperation) Visit(operation.ChildOperations.First());
    }
    public override void DefaultVisit(IOperation operation) { }
}
