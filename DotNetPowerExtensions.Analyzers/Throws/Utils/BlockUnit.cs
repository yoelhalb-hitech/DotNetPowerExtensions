
using Microsoft.CodeAnalysis.FlowAnalysis;

namespace DotNetPowerExtensions.Analyzers.Throws.Utils;

public class BlockUnit
{
    public BlockUnit(BasicBlock block, BlockStatementUnit[] blockStatements)
    {
        Block = block;
        BlockStatements = blockStatements;
    }

    public BasicBlock Block { get; }
    public BlockStatementUnit[] BlockStatements { get; }
}
