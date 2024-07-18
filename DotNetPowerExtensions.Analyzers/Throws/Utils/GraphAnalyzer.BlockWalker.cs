using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Roslyn.Utilities;
using System.Collections.Generic;
using System.Diagnostics;

namespace DotNetPowerExtensions.Analyzers.Throws.Utils;

public partial class GraphAnalyzer
{
    private class BlockWalker
    {
        public BlockWalker(BlockStatementUnit blockStatement, GraphAnalyzer graphHandler, IOperation endOperation)
        {
            BlockStatement = blockStatement;
            GraphHandler = graphHandler;
            EndOperation = endOperation;
        }

        public BlockWalker(BasicBlock block, GraphAnalyzer graphHandler)
        {
            Block = block;
            GraphHandler = graphHandler;
        }

        public BlockStatementUnit? BlockStatement { get; }
        public BasicBlock? Block { get; }
        public GraphAnalyzer GraphHandler { get; }
        public IOperation? EndOperation { get; }

        private List<IOperation> startOperations = new List<IOperation>();
        private List<ISymbol> startSymbols = new List<ISymbol>();

        private Dictionary<BasicBlock, List<(List<ILocalSymbol>, List<CaptureId>)>> walkedBlocks = new ();
        public (List<IOperation>, List<ISymbol>) Walk(List<ILocalSymbol>? lastLocalSymbols = null, List<CaptureId>? lastCaptures = null)
        {
            var localSymbols = lastLocalSymbols ?? new List<ILocalSymbol>();
            var localCaptures = lastCaptures ?? new List<CaptureId>();

            if(BlockStatement is not null)
            {
                (localSymbols, localCaptures) = WalkCurrentStatement(BlockStatement);

                var blockUnit = GraphHandler.BlockUnitDict[BlockStatement.Block];
                var remainingStatements = blockUnit.BlockStatements.TakeWhile(s => s != BlockStatement);

                WalkStatementsAndPredecessors(remainingStatements, BlockStatement.Block, localSymbols, localCaptures);
            }
            else
            {
                WalkBlock(Block!, localSymbols, localCaptures);
            }

            return (startOperations, startSymbols);
        }

        private (List<ILocalSymbol>, List<CaptureId>) WalkCurrentStatement(BlockStatementUnit blockStatement)
        {
            Debug.Assert(EndOperation is not null);

            var localSymbols = new List<ILocalSymbol>();
            var captures = new List<CaptureId>();

            foreach (var operation in blockStatement.Operations)
            {
                var context = new AssignmentDataFlowOperationWalker.DataFlowContext(GraphHandler.Context.Compilation)
                {
                    StartingPointPredicate = o =>
                    (GraphHandler.Context.StartingPointPredicate(o).Item1 || o.Kind == OperationKind.FlowCapture
                        || o.Kind == OperationKind.FlowCaptureReference || o.Kind == OperationKind.LocalReference,
                        GraphHandler.Context.StartingPointPredicate(o).Item2),
                    StartOperation = operation,
                };
                var result = AssignmentDataFlowOperationWalker.Analyze(context);
                var flowResult = result.GetFlowedStartFromOperation(EndOperation!);
                var flowSymbols = result.GetFlowedStartSymbolFromOperation(EndOperation!);

                startOperations.AddRange(flowResult.Where(r => GraphHandler.Context.StartingPointPredicate(r).Item1).Distinct());
                startSymbols.AddRange(startOperations.SelectMany(o => GraphHandler.Context.StartingPointPredicate(o).Item2).Distinct(SymbolEqualityComparer.Default));
                startSymbols.AddRange(flowSymbols);

                localSymbols.AddRange(flowResult.OfType<ILocalReferenceOperation>().Select(l => l.Local));
                captures.AddRange(flowResult.OfType<IFlowCaptureOperation>().Select(c => c.Id));
                captures.AddRange(flowResult.OfType<IFlowCaptureReferenceOperation>().Select(c => c.Id));
            }

            return (localSymbols.Distinct(SymbolEqualityComparer.Default).OfType<ILocalSymbol>().ToList(), captures.Distinct().ToList());
        }

        private void WalkBlock(BasicBlock block, List<ILocalSymbol> lastLocalSymbols, List<CaptureId> lastCaptures)
        {
            if (block.Ordinal < GraphHandler.FirstStartOrdinal) return;

            if (!lastLocalSymbols.Any() && !lastCaptures.Any()) return; // We are no longer having a connection to an earlier statement so we are done

            if (walkedBlocks.ContainsKey(block)
                    && walkedBlocks[block].Any(v =>
                        v.Item1.Count == lastLocalSymbols.Count && v.Item1.All(i => lastLocalSymbols.Contains(i, SymbolEqualityComparer.Default)) &&
                        v.Item2.Count == lastCaptures.Count && v.Item2.All(c => lastCaptures.Contains(c))))
                return;

            if (!walkedBlocks.ContainsKey(block)) walkedBlocks[block] = new List<(List<ILocalSymbol>, List<CaptureId>)>();
            walkedBlocks[block].Add((lastLocalSymbols, lastCaptures));

            var statements = GraphHandler.BlockUnitDict[block].BlockStatements;

            WalkStatementsAndPredecessors(statements, block, lastLocalSymbols, lastCaptures);
        }

        private void WalkStatementsAndPredecessors(IEnumerable<BlockStatementUnit> statements, BasicBlock block, List<ILocalSymbol> localSymbols, List<CaptureId> localCaptures)
        {
            if (!localSymbols.Any() && !localCaptures.Any()) return; // We are no longer having a connection to an earlier statement so we are done

            foreach (var statement in statements.Reverse()) // Remember that we are going backwards
            {
                (localSymbols, localCaptures) = WalkBlockStatement(statement, localSymbols, localCaptures);

                if (!localSymbols.Any() && !localCaptures.Any()) return;
            }

            foreach (var predecessor in block.Predecessors)
            {
                WalkBlock(predecessor.Source, localSymbols, localCaptures);
            }
        }

        private (List<ILocalSymbol>, List<CaptureId>) WalkBlockStatement(BlockStatementUnit blockStatement, List<ILocalSymbol> lastLocalSymbols, List<CaptureId> lastCaptures)
        {
            var result = GraphHandler.BlockResultDict[blockStatement];
            startOperations.AddRange(result.ReadResult.StartingOperations);
            startSymbols.AddRange(result.ReadResult.StartingSymbols);

            var localSymbols = lastLocalSymbols
                                    .Except(result.WriteResult.LocalSymbols, SymbolEqualityComparer.Default)
                                    .Union(result.ReadResult.LocalSymbols, SymbolEqualityComparer.Default)
                                    .OfType<ILocalSymbol>()
                                    .ToList();

            var localCaptures = lastCaptures
                        .Except(result.WriteResult.FlowCaptures)
                        .Except(result.ReadResult.FlowCaptures) // Flow capture from read result is actually a write
                        .Union(result.WriteResult.FlowCaptureReferences)
                        .Union(result.ReadResult.FlowCaptureReferences) // Flow capture reference from write result is actually a read
                        .ToList();

            return (localSymbols, localCaptures);
        }
    }
}
