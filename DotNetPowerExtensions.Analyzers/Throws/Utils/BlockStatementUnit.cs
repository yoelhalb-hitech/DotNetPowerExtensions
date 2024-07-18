
using Microsoft.CodeAnalysis.FlowAnalysis;

namespace DotNetPowerExtensions.Analyzers.Throws.Utils;

public class BlockStatementUnit
{
    public BlockStatementUnit(BasicBlock block, IOperation[] operations)
    {
        Block = block;
        Operations = operations;
    }

    public BasicBlock Block { get; }
    public IOperation[] Operations { get; }
}
