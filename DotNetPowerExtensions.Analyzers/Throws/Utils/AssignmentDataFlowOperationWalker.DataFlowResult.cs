using Microsoft.CodeAnalysis.Operations;
using System.Collections.Concurrent;
using static System.Security.SecurityElement;

namespace DotNetPowerExtensions.Analyzers.Throws;

public partial class AssignmentDataFlowOperationWalker
{
    public partial class DataFlowResult
    {
        public DataFlowResult(IOperation startOperation)
        {
            StartOperation = startOperation;
        }

        private void Map(IBlockOperation outer, IBlockOperation inner)
        {
            if (BlockToOuterBlock.ContainsKey(inner)) return;

            BlockToInnerBlock[outer].Add(inner);
            BlockToOuterBlock[inner] = outer;
        }


        private void Flatten(IBlockOperation block)
        {
            if (BlockToLine.ContainsKey(block)) return;
            if (!BlockToInnerBlock.ContainsKey(block)) BlockToInnerBlock[block] = new List<IBlockOperation>();

            var lines = new List<IOperation>();
            foreach (var line in block.ChildOperations)
            {
                if(line is IBlockOperation blockOperation)
                {
                    Map(block, blockOperation);
                }
                else
                {
                    LineToBlock[line] = block;
                    lines.Add(line);
                }
            }
            BlockToLine[block] = lines;
        }

        private void Flatten(IOperation line)
        {
            if (LineToOperation.ContainsKey(line)) return;

            var outerBlock = LineToBlock[line];

            var operations = new List<IOperation>();
            foreach (var operation in line.ChildOperations)
            {
                var inner = FlatOperations(operation);

                inner.OfType<IBlockOperation>().ToList().ForEach(b => Map(outerBlock, b));

                foreach (var op in inner)
                {
                    operations.Add(op);
                    OperationToLine[op] = line;
                }
            }
            LineToOperation[line] = operations;

            IEnumerable<IOperation> FlatOperations(IOperation operation)
                => new[] { operation }.Union(operations.SelectMany(o => o.ChildOperations.SelectMany(FlatOperations)));
        }

        public Dictionary<IOperation, IOperation> OperationToLine { get; } = new Dictionary<IOperation, IOperation>();
        public Dictionary<IOperation, IBlockOperation> LineToBlock { get; } = new Dictionary<IOperation, IBlockOperation>();
        public Dictionary<IOperation, List<IOperation>> LineToOperation { get; } = new Dictionary<IOperation, List<IOperation>>();
        public Dictionary<IBlockOperation, List<IOperation>> BlockToLine { get; } = new Dictionary<IBlockOperation, List<IOperation>>();
        public Dictionary<IBlockOperation, List<IBlockOperation>> BlockToInnerBlock { get; } = new Dictionary<IBlockOperation, List<IBlockOperation>>();
        public Dictionary<IBlockOperation, IBlockOperation> BlockToOuterBlock { get; } = new Dictionary<IBlockOperation, IBlockOperation>();

        public IOperation StartOperation { get; }
        public List<IOperation> StartingOperations { get; } = new List<IOperation>();
        public List<ISymbol> StartingSymbols { get; } = new List<ISymbol>();
        public List<IOperation> EndingOperations { get; } = new List<IOperation>();
        public Dictionary<IOperation, List<IOperation>> FlowOperations { get; } = new Dictionary<IOperation, List<IOperation>>();
        public Dictionary<ISymbol, List<IOperation>> SymbolWithOperations { get; } = new Dictionary<ISymbol, List<IOperation>>(SymbolEqualityComparer.Default);
        public Dictionary<IOperation, List<ISymbol>> OperationsWithSymbols { get; } = new Dictionary<IOperation, List<ISymbol>>();
        private ConcurrentDictionary<IOperation, List<IOperation>> flowedStartFromOperation = new ConcurrentDictionary<IOperation, List<IOperation>>();
        private ConcurrentDictionary<IOperation, List<ISymbol>> flowedStartSymbolFromOperation = new ConcurrentDictionary<IOperation, List<ISymbol>>();

        private List<string> logs = new List<string>();
        public void Log(string s) => logs.Add(s);

        public List<IOperation> GetFlowedStartFromOperation(IOperation operation)
            => flowedStartFromOperation.GetOrAdd(operation, op =>
            {
                var flowResult = new List<IOperation>();
                new ResultVisitor(this).Visit(op, (o, s) => { if (o is not null && StartingOperations.Contains(o)) flowResult.Add(o); });

                return flowResult;
            });

        public List<ISymbol> GetFlowedStartSymbolFromOperation(IOperation operation)
            => flowedStartSymbolFromOperation.GetOrAdd(operation, op =>
            {
                var flowResult = new List<ISymbol>();
                new ResultVisitor(this).Visit(op, (o, s) => { if (s is not null && StartingSymbols.Contains(s)) flowResult.Add(s); });

                return flowResult;
            });

        internal void Handle(IOperation newOperation, IOperation? oldOperation)
        {
            if (oldOperation is null) return;

            Log($"""<BindingOperation Operation="{Escape(newOperation.GetType().Name)}" OldOperation="{Escape(oldOperation.GetType().Name)}">""");

            if (StartingOperations.Contains(oldOperation) || OperationsWithSymbols.ContainsKey(oldOperation) || FlowOperations.ContainsKey(oldOperation))
            {
                Log($"""<BindingMatch />""");

                if (!FlowOperations.TryGetValue(newOperation, out var olds)) FlowOperations[newOperation] = olds = new List<IOperation>();

                if (!olds.Contains(oldOperation)) olds.Add(oldOperation);
            }

            Log($"""</BindingOperation>""");
        }

        internal void Handle(ISymbol symbol, IOperation oldOperation)
        {
            Log($"""<BindingSymbol Symbol="{Escape(symbol.GetType().Name)}" Name="{Escape(symbol.Name)}" OldOperation="{Escape(oldOperation.GetType().Name)}">""");

            if (StartingOperations.Contains(oldOperation) || OperationsWithSymbols.ContainsKey(oldOperation) || FlowOperations.ContainsKey(oldOperation))
            {
                Log($"""<BindingMatch />""");

                if (!SymbolWithOperations.TryGetValue(symbol, out var olds)) SymbolWithOperations[symbol] = olds = new List<IOperation>();

                if (!olds.Contains(oldOperation)) olds.Add(oldOperation);
            }

            Log($"""</BindingSymbol>""");
        }

        internal void Handle(IEnumerable<ISymbol> symbols, IOperation oldOperation) => symbols.ToList().ForEach(s => Handle(s, oldOperation));

        internal void Handle(IOperation newOperation, ISymbol oldSymbol, bool force)
        {
            Log($"""<BindingOperation Operation="{Escape(newOperation.GetType().Name)}" OldSymbol="{Escape(oldSymbol.GetType().Name)}"  Name="{Escape(oldSymbol.Name)}" Force="{force}">""");

            if (SymbolWithOperations.ContainsKey(oldSymbol) || StartingSymbols.Contains(oldSymbol, SymbolEqualityComparer.Default) || force)
            {
                Log($"""<BindingMatch />""");

                if (!OperationsWithSymbols.TryGetValue(newOperation, out var olds)) OperationsWithSymbols[newOperation] = olds = new List<ISymbol>();

                if (!olds.Contains(oldSymbol, SymbolEqualityComparer.Default)) olds.Add(oldSymbol);
            }

            Log($"""</BindingOperation>""");
        }

        internal void Handle(IOperation newOperation, IEnumerable<ISymbol> oldSymbols, bool force) => oldSymbols.ToList().ForEach(s => Handle(newOperation, s, force));
    }
}
