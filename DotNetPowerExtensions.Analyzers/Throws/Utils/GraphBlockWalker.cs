using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static DotNetPowerExtensions.Analyzers.Throws.AssignmentDataFlowOperationWalker;

namespace DotNetPowerExtensions.Analyzers.Throws.Utils;

public class GraphBlockWalker : GraphBlockOperationWalkerBase
{
    public class FinalResult
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [MustInitialize] public BlockStatementUnit Block { get; set; }
        [MustInitialize] public TempResult ReadResult { get; set; }
        [MustInitialize] public TempResult WriteResult { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }

    public IList<GraphBlockOperationWalkerBase> assignmentWalkers = new List<GraphBlockOperationWalkerBase>();

    public BlockStatementUnit Block { get; }

    public GraphBlockWalker(GraphAnalyzer.DataFlowContext context, BlockStatementUnit block) : base(context)
    {
        Block = block;
    }

    public FinalResult Execute()
    {
        var operations = Block.Operations;
        foreach (var operation in operations)
        {
            Visit(operation);
        }

        var final = new FinalResult
        {
            Block = Block,
            ReadResult = new TempResult
            {
                StartingOperations = Result.StartingOperations.Distinct().ToList(),
                StartingSymbols = Result.StartingSymbols.Distinct(SymbolEqualityComparer.Default).ToList(),
                EndingOperations = Result.EndingOperations.Distinct().ToList(),
                FieldSymbols = Result.FieldSymbols.Distinct(SymbolEqualityComparer.Default).OfType<IFieldSymbol>().ToList(),
                ParameterSymbols = Result.ParameterSymbols.Distinct(SymbolEqualityComparer.Default).OfType<IParameterSymbol>().ToList(),
                PropertySymbols = Result.PropertySymbols.Distinct(SymbolEqualityComparer.Default).OfType<IPropertySymbol>().ToList(),
                MethodSymbols = Result.MethodSymbols.Distinct(SymbolEqualityComparer.Default).OfType<IMethodSymbol>().ToList(),
                FlowCaptures = Result.FlowCaptures.Distinct().ToList(),
                FlowCaptureReferences = Result.FlowCaptureReferences.Distinct().ToList(),
                LocalSymbols = Result.LocalSymbols.Distinct(SymbolEqualityComparer.Default).OfType<ILocalSymbol>().ToList(),
            },
            WriteResult = new TempResult
            {
                StartingOperations = assignmentWalkers.SelectMany(w => w.Result.StartingOperations).Distinct().ToList(),
                StartingSymbols = assignmentWalkers.SelectMany(w => w.Result.StartingSymbols).Distinct(SymbolEqualityComparer.Default).ToList(),
                EndingOperations = assignmentWalkers.SelectMany(w => w.Result.EndingOperations).Distinct().ToList(),
                FieldSymbols = assignmentWalkers.SelectMany(w => w.Result.FieldSymbols).Distinct(SymbolEqualityComparer.Default).OfType<IFieldSymbol>().ToList(),
                ParameterSymbols = assignmentWalkers.SelectMany(w => w.Result.ParameterSymbols).Distinct(SymbolEqualityComparer.Default).OfType<IParameterSymbol>().ToList(),
                PropertySymbols = assignmentWalkers.SelectMany(w => w.Result.PropertySymbols).Distinct(SymbolEqualityComparer.Default).OfType<IPropertySymbol>().ToList(),
                MethodSymbols = assignmentWalkers.SelectMany(w => w.Result.MethodSymbols).Distinct(SymbolEqualityComparer.Default).OfType<IMethodSymbol>().ToList(),
                FlowCaptures = assignmentWalkers.SelectMany(w => w.Result.FlowCaptures).Distinct().ToList(),
                FlowCaptureReferences = assignmentWalkers.SelectMany(w => w.Result.FlowCaptureReferences).Distinct().ToList(),
                LocalSymbols = assignmentWalkers.SelectMany(w => w.Result.LocalSymbols).Distinct(SymbolEqualityComparer.Default).OfType<ILocalSymbol>().ToList(),
            },
        };

        return final;
    }

    public override void VisitSimpleAssignment(ISimpleAssignmentOperation operation)
    {
        var walker = new GraphBlockOperationWalkerBase(Context);
        walker.Visit(operation.Target);
        assignmentWalkers.Add(walker);

        Visit(operation.Value);
    }
}
