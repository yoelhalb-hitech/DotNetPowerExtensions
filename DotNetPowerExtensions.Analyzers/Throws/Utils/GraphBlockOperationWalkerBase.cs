using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Roslyn.Utilities;

namespace DotNetPowerExtensions.Analyzers.Throws.Utils;

public partial class GraphBlockOperationWalkerBase : OperationWalker
{

    public class TempResult
    {
        public List<ILocalSymbol> LocalSymbols = new();
        public List<IPropertySymbol> PropertySymbols = new();
        public List<IFieldSymbol> FieldSymbols = new();
        public List<IMethodSymbol> MethodSymbols = new();
        public List<IParameterSymbol> ParameterSymbols = new();
        public List<CaptureId> FlowCaptures = new();
        public List<CaptureId> FlowCaptureReferences = new();

        public List<IOperation> StartingOperations = new();
        public List<ISymbol> StartingSymbols = new();
        public List<IOperation> EndingOperations = new();
    }

    public IOperation? StartOperation;
    public TempResult Result { get; } = new TempResult();

    public GraphAnalyzer.DataFlowContext Context { get; }
    public GraphBlockOperationWalkerBase(GraphAnalyzer.DataFlowContext context)
    {
        Context = context;
    }

    public override void Visit(IOperation? operation)
    {
        if (operation is null) return;

        if (StartOperation is null) StartOperation = operation;

        var result = Context.StartingPointPredicate(operation);
        if (result.Item1)
        {
            Result.StartingOperations.Add(operation);
            Result.StartingSymbols.AddRange(result.Item2.Where(i => !Result.StartingSymbols.Contains(i, SymbolEqualityComparer.Default)));
        }

        if (Context.EndPointPredicate(operation))
        {
            Result.EndingOperations.Add(operation);
        }

        base.Visit(operation);
    }

    public override void VisitLocalReference(ILocalReferenceOperation operation)
    {
        Result.LocalSymbols.Add(operation.Local);
        base.VisitLocalReference(operation);
    }

    public override void VisitPropertyReference(IPropertyReferenceOperation operation)
    {
        Result.PropertySymbols.Add(operation.Property);
        base.VisitPropertyReference(operation);
    }

    public override void VisitFieldReference(IFieldReferenceOperation operation)
    {
        Result.FieldSymbols.Add(operation.Field);
        base.VisitFieldReference(operation);
    }

    public override void VisitParameterReference(IParameterReferenceOperation operation)
    {
        Result.ParameterSymbols.Add(operation.Parameter);
        base.VisitParameterReference(operation);
    }

    public override void VisitFlowCapture(IFlowCaptureOperation operation)
    {
        Result.FlowCaptures.Add(operation.Id);
        base.VisitFlowCapture(operation);
    }

    public override void VisitFlowCaptureReference(IFlowCaptureReferenceOperation operation)
    {
        Result.FlowCaptureReferences.Add(operation.Id);
        base.VisitFlowCaptureReference(operation);
    }

    public override void VisitMethodReference(IMethodReferenceOperation operation)
    {
        Result.MethodSymbols.Add(operation.Method);
        base.VisitMethodReference(operation);
    }
}
