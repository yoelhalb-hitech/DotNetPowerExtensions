using Microsoft.CodeAnalysis.Operations;

namespace DotNetPowerExtensions.Analyzers.Throws;

public partial class AssignmentDataFlowOperationWalker
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public class DataFlowContext
    {
        public DataFlowContext(Compilation compilation)
        {
            Compilation = compilation;
            InvocationHandlers = new List<IInvocationHandler>() { new InvocationHandler(compilation) };
        }
        /// <summary>
        /// The compilation which we are getting the operations from
        /// </summary>
        public Compilation Compilation { get; }
        /// <summary>
        /// The operation which we should start the analysis from
        /// Typically this will be an <see cref="IMethodBodyOperation"/>
        /// </summary>
        [MustInitialize] public IOperation StartOperation { get; set; }
        /// <summary>
        /// A predicate that will determine if the provided <see cref="IOperation"/> is something that should be tracked
        /// An example in our case would be any method reference or inline function which we want to see if it is getting invoked
        /// Also returns a list of <see cref="ISymbol"/> that we want to track btu aren't directly an <see cref="IOperation"/>
        /// For example <see cref="IParameterSymbol"/> which don't have an associated <see cref="IOperation"/>
        /// </summary>
        [MustInitialize] public Func<IOperation, (bool, ISymbol[])> StartingPointPredicate { get; set; }

        /// <summary>
        /// A predicate that will determine if the provided <see cref="IOperation"/> is something that should be tracked for captures in inline functions or for loop variables
        /// </summary>
        [MustInitialize] public Func<IOperation, ISymbol[]>? SymbolsToCapture { get; set; }
        /// <summary>
        /// A predicate that will determine if the provided <see cref="IOperation"/> is the end point of what we want to analyze
        /// An example in our case would be a method invocation which we want to see which possible method might be invoked
        /// </summary>
        public Func<IOperation, bool>? EndPointPredicate { get; set; }
        /// <summary>
        /// A list of <see cref="IInvocationHandler"/> instances to control how invoctions should be bound
        /// </summary>
        public List<IInvocationHandler> InvocationHandlers { get; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
