namespace DotNetPowerExtensions.Analyzers.Throws;

public partial class AssignmentDataFlowOperationWalker
{
    public partial class DataFlowResult
    {
        private class ResultVisitor
        {
            private readonly DataFlowResult result;
            private readonly List<IOperation> operations = new List<IOperation>();
            private readonly List<ISymbol> symbols = new List<ISymbol>();

            public ResultVisitor(DataFlowResult result)
            {
                this.result = result;
            }

            public void Visit(IOperation operation, Action<IOperation?, ISymbol?> action)
            {
                if (result.FlowOperations.TryGetValue(operation, out var associatedOperations))
                    foreach (var associatedOperation in associatedOperations)
                    {
                        if (operations.Contains(associatedOperation)) continue; // We have done it already...
                        operations.Add(associatedOperation);

                        action(associatedOperation, null);
                        Visit(associatedOperation, action);
                    }


                if (result.OperationsWithSymbols.TryGetValue(operation, out var associatedSymbols))
                    foreach (var associatedSymbol in associatedSymbols)
                    {
                        if (symbols.Contains(associatedSymbol, SymbolEqualityComparer.Default)) continue; // We have done it already...

                        symbols.Add(associatedSymbol);

                        action(null, associatedSymbol);
                        Visit(associatedSymbol, action);
                    }
            }

            public void Visit(ISymbol symbol, Action<IOperation?, ISymbol?> action)
            {
                if (result.SymbolWithOperations.TryGetValue(symbol, out var associatedOperations))
                    foreach (var associatedOperation in associatedOperations)
                    {
                        if (operations.Contains(associatedOperation)) continue; // We have done it already...
                        operations.Add(associatedOperation);

                        action(associatedOperation, null);
                        Visit(associatedOperation, action);
                    }
            }
        }
    }
}
