extern alias Workspaces;

using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System.Collections;
using System.Linq.Expressions;
using static System.Security.SecurityElement;

namespace DotNetPowerExtensions.Analyzers.Throws;

/// <summary>
/// Walker to follow assignemt paths to see which value might be assigned and eventually
/// </summary>
/// <remarks>
/// This is intendent to track invocations and await sources for exception tracking
/// While this might work in general data flow, it is not guaranteed
/// Specifically for example, we do not track here exceptions declared in the catch block and then rethrown etc.
/// We do it however in the <see cref="ExceptionOperationWalker"/> and <see cref="EmptyRethrowOperationWalker"/> which are geared for it
/// Similarly we also ignore most operators as they aren't really relevent
/// Also for captured variables and loops it should also always capture delegates and tasks so to track all inline invocations, alongside your the type you are looking for
/// </remarks>
public partial class AssignmentDataFlowOperationWalker : BackwardOperationWalker
{
    INamedTypeSymbol? enumerableSymbol { get; }
    INamedTypeSymbol? enumerableClassSymbol { get; }
    INamedTypeSymbol? expression1Symbol { get; }
    INamedTypeSymbol? lambdaSymbol { get; }
    INamedTypeSymbol? taskSymbol { get; }
    INamedTypeSymbol? taskFactorySymbol { get; }
    INamedTypeSymbol? taskFactory1Symbol { get; }

    private DataFlowResult Result { get; }
    public DataFlowContext Context { get; }

    public AssignmentDataFlowOperationWalker(DataFlowContext context)
    {
        Context = context;

        Result = new DataFlowResult(Context.StartOperation);

        Func<Type, INamedTypeSymbol?> func = t => context.Compilation.GetTypeByMetadataName(t.FullName!);
        enumerableSymbol = func(typeof(IEnumerable));
        enumerableClassSymbol = func(typeof(Enumerable));
        expression1Symbol = func(typeof(Expression<>));
        lambdaSymbol = func(typeof(LambdaExpression));
        taskSymbol = func(typeof(Task));
        taskFactorySymbol = func(typeof(TaskFactory));
        taskFactory1Symbol = func(typeof(TaskFactory<>));
    }

    public static DataFlowResult Analyze(DataFlowContext context)
    {
        var walker = new AssignmentDataFlowOperationWalker(context);
        walker.Visit(context.StartOperation);

        return walker.Result;
    }

    private void Handle(IOperation newOperation, IOperation? oldOperation) => Result.Handle(newOperation, oldOperation);

    private void Handle(ISymbol symbol, IOperation oldOperation) => Result.Handle(symbol, oldOperation);

    private void Handle(IEnumerable<ISymbol> symbols, IOperation oldOperation) => Result.Handle(symbols, oldOperation);

    private void Handle(IOperation newOperation, ISymbol oldSymbol, bool force) => Result.Handle(newOperation, oldSymbol, force);
    private void Handle(IOperation newOperation, IEnumerable<ISymbol> oldSymbols, bool force) => Result.Handle(newOperation, oldSymbols, force);

    private int counter = 0;
    public override void Visit(IOperation? operation)
    {
        if (operation is null) return;
        counter++;

        Result.Log($"""<{Escape(operation.GetType().Name)} Syntax="{Escape(operation.Syntax?.ToFullString().Replace("\r", "").Replace("\n", ""))}" Type="{Escape(operation.Type?.Name)}" Count="{counter}">""");

        var result = Context.StartingPointPredicate(operation);
        if(result.Item1)
        {
            if(!Result.StartingOperations.Contains(operation)) Result.StartingOperations.Add(operation);

            Handle(operation, result.Item2, true);
            Result.StartingSymbols.AddRange(result.Item2.Where(i => !Result.StartingSymbols.Contains(i, SymbolEqualityComparer.Default)));

            Result.Log($"""<MatchedStart />""");
        }

        var symbolsToCapture = Context.SymbolsToCapture?.Invoke(operation);
        if(symbolsToCapture?.Any() == true)
        {
            Handle(operation, result.Item2, true); // Note that we didn't add it to the starting point

            Result.Log($"""<MatchedSymbolToCapture />""");
        }

        if (Context.EndPointPredicate is not null && Context.EndPointPredicate(operation))
        {
            if(!Result.EndingOperations.Contains(operation)) Result.EndingOperations.Add(operation);

            Result.Log($"""<MatchedEnd />""");
        }

        base.Visit(operation);

        Result.Log($"""</{Escape(operation.GetType().Name)}>""");
    }

    public override void VisitMethodBodyOperation(IMethodBodyOperation operation)
    {
        var parameters = (operation.Syntax as MethodDeclarationSyntax)?.ParameterList.Parameters;
        foreach (var @default in parameters?.Select(p => p.Default).OfType<EqualsValueClauseSyntax>() ?? new EqualsValueClauseSyntax[] { })
        {
            var defaultOperation = Context.Compilation.GetSemanticModel(Context.Compilation.SyntaxTrees.First()).GetOperation(@default);
            if (defaultOperation is not null) Visit(defaultOperation);
        }

        base.VisitMethodBodyOperation(operation);
    }

    public override void VisitVariableInitializer(IVariableInitializerOperation operation)
    {
        Handle(operation, operation.Value);
        Handle(operation.Locals, operation);

        base.VisitVariableInitializer(operation);
    }

    public override void VisitConversion(IConversionOperation operation)
    {
        Handle(operation, operation.Operand);
        base.VisitConversion(operation);
    }

    public override void VisitSimpleAssignment(ISimpleAssignmentOperation operation)
    {
        Handle(operation, operation.Value);
        Handle(operation.Target, operation);
        //base.VisitSimpleAssignment(operation);
    }

    public override void VisitCoalesce(ICoalesceOperation operation)
    {
        Handle(operation, operation.Value);
        Handle(operation, operation.WhenNull);
        base.VisitCoalesce(operation);
    }

    public override void VisitCoalesceAssignment(ICoalesceAssignmentOperation operation)
    {
        Handle(operation, operation.Value);
        Handle(operation.Target, operation.Value);
        base.VisitCoalesceAssignment(operation);
    }

    public override void VisitConditional(IConditionalOperation operation)
    {
        Handle(operation, operation.WhenTrue);
        Handle(operation, operation.WhenFalse);
        base.VisitConditional(operation);
    }

    public override void VisitDeclarationPattern(IDeclarationPatternOperation operation)
    {
        if(operation.DeclaredSymbol is not null)
        {
            Handle(operation, operation.DeclaredSymbol!, false);
            Handle(operation.DeclaredSymbol, operation);
        }

        base.VisitDeclarationPattern(operation);
    }

    public override void VisitSwitchExpressionArm(ISwitchExpressionArmOperation operation)
    {
        Handle(operation, operation.Value);
        Handle(operation, operation.Pattern);
        Handle(operation, operation.Locals, false);
        base.VisitSwitchExpressionArm(operation);
    }

    public override void VisitSwitchExpression(ISwitchExpressionOperation operation)
    {
        operation.Arms.ToList().ForEach(a => Handle(operation, a));
        base.VisitSwitchExpression(operation);
    }

    public override void VisitExpressionStatement(IExpressionStatementOperation operation)
    {
        Handle(operation, operation.Operation);
        base.VisitExpressionStatement(operation);
    }

    public override void VisitArrayInitializer(IArrayInitializerOperation operation)
    {
        operation.ElementValues.ToList().ForEach(e => Handle(operation, e));
        base.VisitArrayInitializer(operation);
    }

    public override void VisitAwait(IAwaitOperation operation)
    {
        Handle(operation, operation.Operation);
        base.VisitAwait(operation);
    }

    public override void VisitConditionalAccess(IConditionalAccessOperation operation)
    {
        Handle(operation, operation.WhenNotNull);
        base.VisitConditionalAccess(operation);
    }

    public override void VisitDeclarationExpression(IDeclarationExpressionOperation operation)
    {
        Handle(operation, operation.Expression);
        base.VisitDeclarationExpression(operation);
    }
    public override void VisitDeconstructionAssignment(IDeconstructionAssignmentOperation operation)
    {
        Handle(operation.Target, operation.Value);
        Handle(operation, operation.Value);
        base.VisitDeconstructionAssignment(operation);
    }

    public override void VisitInvocation(IInvocationOperation operation)
    {
        Context.InvocationHandlers.ForEach(h => h.Handle(operation, operation.TargetMethod, operation.TargetMethod.ReceiverType, Result));

        base.VisitInvocation(operation);
    }

    private bool IsAssignmentTarget(IOperation operation)
    {
        var parent = operation.Parent;
        var child = (IOperation)operation;
        while (parent?.Parent is not null && ((parent is IPropertyReferenceOperation prop && prop.Property.IsIndexer)
                                            || parent is IArrayElementReferenceOperation || parent is IConversionOperation))
        {
            child = parent;
            parent = parent.Parent;
        }

        return parent is IAssignmentOperation assign && assign.Target == child;
    }

    public override void VisitLocalReference(ILocalReferenceOperation operation)
    {
        if (operation.IsDeclaration || IsAssignmentTarget(operation)) Handle(operation.Local, operation);
        else Handle(operation, operation.Local, false);

        base.VisitLocalReference(operation);
    }

    public override void VisitArrayElementReference(IArrayElementReferenceOperation operation)
    {

        if (IsAssignmentTarget(operation))
        {
            Handle(operation.ArrayReference, operation);
            Visit(operation.ArrayReference); // We need to make sure that we visit the array reference after we bound it to the assignment
        }
        else Handle(operation, operation.ArrayReference);
        base.VisitArrayElementReference(operation);
    }

    public override void VisitArrayCreation(IArrayCreationOperation operation)
    {
        Handle(operation, operation.Initializer);
        base.VisitArrayCreation(operation);
    }

    public override void VisitDelegateCreation(IDelegateCreationOperation operation)
    {
        Handle(operation, operation.Target);
        base.VisitDelegateCreation(operation);
    }

    public override void VisitFieldReference(IFieldReferenceOperation operation)
    {
        if(operation.Parent is IAssignmentOperation) Handle(operation, operation.Field, false);
        else Handle(operation.Field, operation);
        base.VisitFieldReference(operation);
    }

    public override void VisitEventAssignment(IEventAssignmentOperation operation)
    {
        // TODO... ???
        base.VisitEventAssignment(operation);
    }

    public override void VisitEventReference(IEventReferenceOperation operation)
    {
        // TODO... ???
        base.VisitEventReference(operation);
    }

    public override void VisitListPattern(IListPatternOperation operation)
    {
        operation.Patterns.ToList().ForEach(p => Handle(operation, p));
        if(operation.DeclaredSymbol is not null) Handle(operation.DeclaredSymbol, operation);
        base.VisitListPattern(operation);
    }

    public override void VisitMemberInitializer(IMemberInitializerOperation operation)
    {
        Handle(operation.InitializedMember, operation.Initializer);
        base.VisitMemberInitializer(operation);
    }

    public override void VisitObjectOrCollectionInitializer(IObjectOrCollectionInitializerOperation operation)
    {
        operation.Initializers.ToList().ForEach(i => Handle(operation, i));
        base.VisitObjectOrCollectionInitializer(operation);
    }

    public override void VisitParameterReference(IParameterReferenceOperation operation)
    {
        if(operation.Parameter.RefKind != RefKind.Out) Handle(operation, operation.Parameter, false);
        if(new [] { RefKind.Out, RefKind.Ref }.Contains(operation.Parameter.RefKind)) Handle(operation.Parameter, operation);
        base.VisitParameterReference(operation);
    }

    public override void VisitMethodReference(IMethodReferenceOperation operation)
    {
        Handle(operation, operation.Method, false);
        base.VisitMethodReference(operation);
    }

    public override void VisitParenthesized(IParenthesizedOperation operation)
    {
        Handle(operation, operation.Operand);
        base.VisitParenthesized(operation);
    }

    public override void VisitPropertyReference(IPropertyReferenceOperation operation)
    {
        if (IsAssignmentTarget(operation))
        {
            if (operation.Property.IsIndexer)
            {
                Handle(operation.Instance!, operation);
                Visit(operation.Instance!); // Make sure to visit the instance afterwards so it will handle it correctly
            }
            else Handle(operation.Property, operation);
        }
        else
        {
            if (operation.Property.IsIndexer)
            {
                Visit(operation.Instance!);
                Handle(operation, operation.Instance!);
            }
            else Handle(operation, operation.Property, false);
        }
        base.VisitPropertyReference(operation);
    }

    public override void VisitPropertySubpattern(IPropertySubpatternOperation operation)
    {
        // TODO... Do we need this???
        base.VisitPropertySubpattern(operation);
    }

    public override void VisitRaiseEvent(IRaiseEventOperation operation)
    {
        // TODO....
        base.VisitRaiseEvent(operation);
    }

    public override void VisitReturn(IReturnOperation operation)
    {
        Handle(operation, operation.ReturnedValue);
        base.VisitReturn(operation);
    }

    public override void VisitAnonymousFunction(IAnonymousFunctionOperation operation)
    {
        // TODO... Is this right??
        Handle(operation, operation.Symbol, false);
        Handle(operation.Symbol, operation);
        base.VisitAnonymousFunction(operation);
    }

    public override void VisitVariableDeclaration(IVariableDeclarationOperation operation)
    {
        if (operation.Initializer is not null) // Do it first in case we deal with a collection initializer
        {
            Visit(operation.Initializer);
            Handle(operation, operation.Initializer);
            operation.Declarators.ToList().ForEach(d => Handle(d, operation.Initializer));
            operation.Declarators.ToList().ForEach(d => Handle(d.Symbol, operation.Initializer));
        }
        operation.Declarators.ToList().ForEach(d => Handle(operation, d));

        base.VisitVariableDeclaration(operation);
    }

    public override void VisitVariableDeclarator(IVariableDeclaratorOperation operation)
    {
        if (operation.Initializer is not null) // Do it first in case we deal with a collection initializer
        {
            Visit(operation.Initializer);
            Handle(operation, operation.Initializer);
            Handle(operation.Symbol, operation.Initializer);
        }
        Handle(operation, operation.Symbol, false);
        Handle(operation.Symbol, operation);

        base.VisitVariableDeclarator(operation);
    }

    public override void VisitVariableDeclarationGroup(IVariableDeclarationGroupOperation operation)
    {
        operation.Declarations.ToList().ForEach(d => Handle(operation, d));
        base.VisitVariableDeclarationGroup(operation);
    }

    public override void VisitWith(IWithOperation operation)
    {
        Handle(operation, operation.Operand);
        Handle(operation, operation.Initializer);
        base.VisitWith(operation);
    }

    public override void VisitIsPattern(IIsPatternOperation operation)
    {
        Handle(operation, operation.Value);
        base.VisitIsPattern(operation);
    }

    public override void VisitIsType(IIsTypeOperation operation)
    {
        Handle(operation, operation.ValueOperand);
        base.VisitIsType(operation);
    }

    public override void VisitObjectCreation(IObjectCreationOperation operation)
    {
        if(operation.Constructor?.ContainingType.TypeKind == TypeKind.Delegate)
        {
            operation.Arguments.ToList().ForEach(a => Handle(operation, a));
        }
        if(operation.Initializer is not null)
        {
            Handle(operation, operation.Initializer);
        }
        base.VisitObjectCreation(operation);
    }

    public override void VisitArgument(IArgumentOperation operation)
    {
        if(operation.Parameter?.RefKind != RefKind.Out)
        {
            if(operation.Parameter is not null) Handle(operation, operation.Parameter, false);
            Handle(operation, operation.Value);
        }
        if (new RefKind?[] { RefKind.Out, RefKind.Ref }.Contains(operation.Parameter?.RefKind))
        {
            // Handle(operation.Parameter!, operation); Don't use it as this will add the invocation parameter symbol and so all subsequent invocations will become related
            Handle(operation.Value, operation);
        }
        base.VisitArgument(operation);
    }

    public override void VisitImplicitIndexerReference(IImplicitIndexerReferenceOperation operation)
    {
        base.VisitImplicitIndexerReference(operation);
    }

    public override void VisitThrow(IThrowOperation operation)
    {
        Handle(operation, operation.Exception);
        base.VisitThrow(operation);
    }
}
