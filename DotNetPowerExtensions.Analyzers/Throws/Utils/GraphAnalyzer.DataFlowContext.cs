using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace DotNetPowerExtensions.Analyzers.Throws.Utils;

public partial class GraphAnalyzer
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public class DataFlowContext
    {
        /// <summary>
        /// The compilation which we are getting the operations from
        /// </summary>
        [MustInitialize] public Compilation Compilation { get; set; }
        /// <summary>
        /// The operation which we should start the anlysis from
        /// Typically this will be an <see cref="IMethodBodyOperation"/>
        /// </summary>
        [MustInitialize] public IOperation StartOperation { get; set; }
        /// <summary>
        /// The <see cref="ControlFlowGraph"/> for the <see cref="StartOperation"/>
        /// </summary>
        [MustInitialize] public ControlFlowGraph Graph { get; set; }
        /// <summary>
        /// A predicate that will determine if the provided <see cref="IOperation"/> is something that should be tracked
        /// An example in our case would be any method reference or inline function which we want to see if it is getting invoked
        /// Also returns a list of <see cref="ISymbol"/> that we want to track but aren't directly an <see cref="IOperation"/>
        /// </summary>
        [MustInitialize] public Func<IOperation, (bool, ISymbol[])> StartingPointPredicate { get; set; }

        /// <summary>
        /// A predicate that will determine if the provided <see cref="IOperation"/> is the end point of what we want to analyze
        /// An example in our case would be a method invocation which we want to see which possible method might be invoked
        /// </summary>
        public Func<IOperation, bool> EndPointPredicate { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }
}
