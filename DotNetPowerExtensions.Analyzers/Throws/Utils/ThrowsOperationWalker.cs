extern alias Workspaces;

using DotNetPowerExtensions.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System;
using Workspaces::Microsoft.CodeAnalysis.Shared.Extensions;

namespace DotNetPowerExtensions.Analyzers.Throws;

public class ThrowsOperationWalker : CatchOperationWalker
{
    public ThrowsOperationWalker(Compilation compilation) : base(compilation)
    {
    }

    public override object? VisitConversion(IConversionOperation conversionOperation, (bool, List<ITypeSymbol>) argument)
    {
        if (argument.Item1 && conversionOperation.Type is not null) argument.Item2.Add(conversionOperation.Type);

        return base.VisitConversion(conversionOperation, argument);
    }

    public override object? DefaultVisit(IOperation operation, (bool, List<ITypeSymbol>) argument)
    {
        if (argument.Item1 && operation.Type is not null) argument.Item2.Add(operation.Type);

        return base.DefaultVisit(operation, argument);
    }

    public override object? VisitThrow(IThrowOperation throwOperation, (bool, List<ITypeSymbol>) argument)
    {
        // If the type of the throw expression is not Exception then the compiler does an implicit conversion to Exception so ignore it
        if (throwOperation.Exception is IConversionOperation conversion && conversion.IsImplicit)
        {
            if(conversion.Operand is ILiteralOperation literal && literal.ConstantValue.HasValue && literal.ConstantValue.Value is null)
            {
                argument.Item2.Add(compilation.GetTypeByMetadataName(typeof(NullReferenceException).FullName)!); // throw null literal throws NullReferenceException
            }
            else Visit(conversion.Operand, (true, argument.Item2)); // Default visit will go to the children only so use visit
        }
        else base.VisitThrow(throwOperation, (true, argument.Item2));

        return null;
    }

    public override object? VisitCatchClause(ICatchClauseOperation operation, (bool, List<ITypeSymbol>) argument)
    {
        base.VisitCatchClause(operation, argument);

        var hasEmptyRethrow = new EmptyRethrowOperationWalker().Visit(operation.Handler, null); // Caution: using operation itself won't work as it short circuits any catch clause
        if (hasEmptyRethrow) argument.Item2.Add(operation.ExceptionType);

        return null;
    }

    public override object? VisitConditional(IConditionalOperation operation, (bool, List<ITypeSymbol>) argument)
    {
        if (argument.Item1 && operation.Type is not null) argument.Item2.Add(operation.Type);

        DefaultVisit(operation.Condition, (false, argument.Item2)); // Even in throws context we don't care on the condition

        DefaultVisit(operation.WhenTrue, (argument));
        if (operation.WhenFalse is not null) DefaultVisit(operation.WhenFalse, (argument));

        return null;
    }

    public ITypeSymbol[] GetExceptionsForOperation(IOperation operation)
    {
        var args = new List<ITypeSymbol>();

        Visit(operation, (false, args));

        return args.Distinct(SymbolEqualityComparer.Default).OfType<ITypeSymbol>().ToArray();
    }
}
