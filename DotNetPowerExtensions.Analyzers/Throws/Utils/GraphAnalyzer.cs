using Microsoft.CodeAnalysis.FlowAnalysis;
using Roslyn.Utilities;

namespace DotNetPowerExtensions.Analyzers.Throws.Utils;

public partial class GraphAnalyzer
{
    public Dictionary<BlockStatementUnit, GraphBlockWalker.FinalResult> BlockResultDict { get; } = new();
    public Dictionary<BasicBlock, BlockUnit> BlockUnitDict { get; } = new();
    public Dictionary<IOperation, BlockStatementUnit> WriteEndOperations { get; } = new();
    public Dictionary<IOperation, BlockStatementUnit> ReadEndOperations { get; } = new();
    public Dictionary<BasicBlock, GraphBlockOperationWalkerBase.TempResult> ReturnInfos { get; } = new();
    public Dictionary<BasicBlock, GraphBlockOperationWalkerBase.TempResult> ThrowsInfo { get; } = new();
    public int FirstStartOrdinal;
    public BasicBlock? EndBlock { get; }
    public GraphAnalyzer.DataFlowContext Context { get; }

    public GraphAnalyzer(GraphAnalyzer.DataFlowContext context)
    {
        Context = context;
        FirstStartOrdinal = context.Graph.Blocks.Length + 1;

        foreach (var block in context.Graph.Blocks)
        {
            if(block.Kind == BasicBlockKind.Exit)
            {
                EndBlock = block;
                HandleExitBlock(block);
            }
            else
            {
                HandleBlock(block);
            }
        }
    }

    private void HandleExitBlock(BasicBlock block)
    {
        foreach (var predecessor in block.Predecessors)
        {
            var returnWalker = new GraphBlockOperationWalkerBase(Context);
            returnWalker.Visit(predecessor.Source.BranchValue);
            ReturnInfos.Add(predecessor.Source, returnWalker.Result);
        }
    }

    private void HandleThrowsBlock(BasicBlock block)
    {
        var returnWalker = new GraphBlockOperationWalkerBase(Context);
        returnWalker.Visit(block.BranchValue);
        ThrowsInfo.Add(block, returnWalker.Result);
    }

    private BlockUnit CreateBlockUnit(BasicBlock block)
    {
        BlockStatementUnit[] units;
        if(block.Operations.All(o => o.Kind == OperationKind.SimpleAssignment || o.Kind == OperationKind.ExpressionStatement))
        {
            units = block.Operations.Select(o => new BlockStatementUnit(block, new [] { o })).ToArray();
        }
        else
        {
            units = new[] { new BlockStatementUnit(block, block.Operations.ToArray() ) };
        }

        var blockUnit = new BlockUnit(block, units);
        BlockUnitDict[block] = blockUnit;

        return blockUnit;
    }

    private void HandleBlock(BasicBlock block)
    {
        var errorSemantics = new[] { ControlFlowBranchSemantics.Throw, ControlFlowBranchSemantics.Rethrow }.ToList();
        if(new [] { block.ConditionalSuccessor, block.FallThroughSuccessor }.Any(b => b is not null && errorSemantics.Contains(b.Semantics)))
        {
            HandleThrowsBlock(block);
            return;
        }

        var blockUnit = CreateBlockUnit(block);

        foreach (var statement in blockUnit.BlockStatements)
        {
            var walker = new GraphBlockWalker(Context, statement);
            var result = walker.Execute();

            BlockResultDict[statement] = result;
            // We reverse it because for each statement the operation visitor visit from right to left which is not what we would expect
            // For example `f()()` would have the second invocation before while `var a = f(); a();` would have it in correct order
            // While in general it is not relevent, it makes a difference in the unit tests where we test things based on the position
            result.WriteResult.EndingOperations.Distinct().Reverse().ToList().ForEach(o => WriteEndOperations[o] = statement);
            result.ReadResult.EndingOperations.Distinct().Reverse().ToList().ForEach(o => ReadEndOperations[o] = statement);

            if (result.ReadResult.StartingOperations.Any() && FirstStartOrdinal > block.Ordinal) // Don't really consider write result
                FirstStartOrdinal = block.Ordinal;
        }
    }

    public (List<IOperation>, List<ISymbol>) GetStartInfo(IOperation endOperation)
    {
        if (!WriteEndOperations.TryGetValue(endOperation, out var block) && !ReadEndOperations.TryGetValue(endOperation, out block))
                    throw new InvalidOperationException(nameof(endOperation));

        return new BlockWalker(block, this, endOperation).Walk();
    }

    public IEnumerable<(BasicBlock, List<IOperation>, List<ISymbol>)> GetStartInfoForReturn()
    {
        foreach (var line in ReturnInfos)
        {
            var (operations, symbols) = new BlockWalker(line.Key, this).Walk(line.Value.LocalSymbols, line.Value.FlowCaptureReferences);

            operations.AddRange(line.Value.StartingOperations);
            symbols.AddRange(line.Value.StartingSymbols);

            yield return (line.Key, operations.Distinct().ToList(), symbols.Distinct(SymbolEqualityComparer.Default).ToList());
        }
    }

    public IEnumerable<(BasicBlock, List<IOperation>, List<ISymbol>)> GetStartInfoForThrows()
    {
        foreach (var line in ThrowsInfo)
        {
            var (operations, symbols) = new BlockWalker(line.Key, this).Walk(line.Value.LocalSymbols, line.Value.FlowCaptureReferences);

            operations.AddRange(line.Value.StartingOperations);
            symbols.AddRange(line.Value.StartingSymbols);

            yield return (line.Key, operations.Distinct().ToList(), symbols.Distinct(SymbolEqualityComparer.Default).ToList());
        }
    }
}
