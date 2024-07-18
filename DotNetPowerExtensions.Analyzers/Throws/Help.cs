using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using SequelPay.DotNetPowerExtensions.RoslynExtensions;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.CSharp.Extensions;

namespace DotNetPowerExtensions.Analyzers.Throws;

public class Help
{
    public Help(Compilation compilation)
    {
        this.compilation = compilation;
    }
    Type[] symbolsTypes = new[]
    {
                    typeof(Expression<>),
                    typeof(IAsyncResult),
    };
    INamedTypeSymbol[] metadatas => symbolsTypes.Select(t => compilation.GetTypeByMetadataName(t.FullName)).OfType<INamedTypeSymbol>().ToArray();
    Func<ITypeSymbol, bool> isInvokable => t => t.TypeKind == TypeKind.Delegate ||
        (t is INamedTypeSymbol named
            && metadatas.ContainsGeneric(named)
            && (!named.IsGenericType || (named.TypeArguments.Length == 1 && named.TypeArguments.First().TypeKind == TypeKind.Delegate)));

    Func<ITypeSymbol, Func<ITypeSymbol, bool>, bool> isArrayOf => (t, func) => func(t) ||
    t.AllInterfaces.Any(i => i.IsGenericEqual(compilation.GetTypeByMetadataName(typeof(IEnumerable<>).FullName)) && func(i.TypeArguments.First()));

    Func<ITypeSymbol, bool> isInvokableOrArray => t => isArrayOf(t, isInvokable);
    Func<IMethodBodyOperation, ISymbol[]> getInvokableCandidates => o => getCandidates(o, isInvokable);
    Func<IOperation, ISymbol[], bool> isAssigningToCandidate => (o, s) => o is IAssignmentOperation assignment
        && (assignment.Target is ILocalReferenceOperation local && s.Contains(local.Local, SymbolEqualityComparer.Default)
    || assignment.Target is IParameterReferenceOperation parameter && s.Contains(parameter.Parameter, SymbolEqualityComparer.Default))
    || (o is IVariableInitializerOperation init && init.Locals.Any(l => s.Contains(l, SymbolEqualityComparer.Default)))
    || (o is IParameterInitializerOperation paramInit && s.Contains(paramInit.Parameter, SymbolEqualityComparer.Default));

    Func<IOperation, ISymbol[], bool> isAssigningFrom => (o, s) => o is IAssignmentOperation assignment
    && (assignment.Target is ILocalReferenceOperation local && s.Contains(local.Local, SymbolEqualityComparer.Default)
|| assignment.Target is IParameterReferenceOperation parameter && s.Contains(parameter.Parameter, SymbolEqualityComparer.Default))
|| (o is IVariableInitializerOperation init && init.Locals.Any(l => s.Contains(l, SymbolEqualityComparer.Default)))
|| (o is IParameterInitializerOperation paramInit && s.Contains(paramInit.Parameter, SymbolEqualityComparer.Default));


    Type[] assignableTypes = new[]
{
                    typeof(Expression),
                    typeof(LambdaExpression),
                    typeof(object),
    };
    INamedTypeSymbol[] assignableMetadatas => symbolsTypes.Select(t => compilation.GetTypeByMetadataName(t.FullName)).OfType<INamedTypeSymbol>().ToArray();
    Func<ITypeSymbol, bool> isAssignable => t => assignableMetadatas.Contains(t, SymbolEqualityComparer.Default);
    Func<ITypeSymbol, bool> isAssignableOrArray => t => isArrayOf(t, isAssignable);
    Func<IMethodBodyOperation, ISymbol[]> getAssignableCandidates => o => getCandidates(o, isAssignable);

    Func<IMethodBodyOperation, Func<ITypeSymbol, bool>, ISymbol[]> getParameterCandidates => (o, func) => (o.Syntax as MethodDeclarationSyntax)?.ParameterList.Parameters
        .Select(p => compilation.GetSemanticModel(compilation.SyntaxTrees.First()).GetDeclaredSymbol(p))
    .OfType<IParameterSymbol>()
     .Where(p => p.RefKind != RefKind.Out && func(p.Type))
    .OfType<ISymbol>().ToArray() ?? new ISymbol[] { };

    public Func<IOperation, Func<ITypeSymbol, bool>, bool> isIncoming => (o, func) => o is IMethodReferenceOperation ||
        o is IAnonymousFunctionOperation ||
        //(o is ILocalReferenceOperation localRef && localRef. is ILocalFunctionOperation) ||
        // TODO... also a method invocation taking a delegate and returning a task
        (o is IPropertyReferenceOperation prop && isPropertyCandidate(prop, func)) ||
        (o is IFieldReferenceOperation fi && isFieldCandidate(fi, func)) ||
        (o is IArgumentOperation arg && func(arg.Parameter.Type) && arg.Parameter.RefKind != RefKind.In) ||
        (o is IParameterReferenceOperation p && isParamaterCandidate(p, func)); // TODO... also ref and out and return values from called function

    Func<IOperation, Func<ITypeSymbol, bool>, bool> isOutgoing => (o, func) => o is IInvocationOperation ||
        (o is IAwaitOperation wait && func(wait.Type))  || // TODO... there are others that can be such as Task.Wait() Task.Result() Task.WaitAny() Task.WaitAll()
        (o is IPropertyReferenceOperation prop && isPropertyCandidate(prop, func)) ||
        (o is IFieldReferenceOperation fi && isFieldCandidate(fi, func)) ||
        (o is IParameterReferenceOperation p && p.Parameter.RefKind != RefKind.In && func(p.Parameter.Type)) ||
        (o is IReturnOperation returnOperation && func(returnOperation.Type));

    Func<IPropertyReferenceOperation, Func<ITypeSymbol, bool>, bool> isPropertyCandidate => (o, func) => func(o.Property.Type);
    Func<IFieldReferenceOperation, Func<ITypeSymbol, bool>, bool> isFieldCandidate => (o, func) => func(o.Field.Type);
    Func<IParameterReferenceOperation, Func<ITypeSymbol, bool>, bool> isParamaterCandidate => (o, func) => o.Parameter.RefKind != RefKind.Out && func(o.Parameter.Type);

    Func<IMethodBodyOperation, Func<ITypeSymbol, bool>, ISymbol[]> getCandidates => (o, func) => o.BlockBody?.Locals.Where(l => func(l.Type)).OfType<ISymbol>()
                .Concat((o.Syntax as MethodDeclarationSyntax)?.ParameterList.Parameters
            .Select(p => compilation.GetSemanticModel(compilation.SyntaxTrees.First()).GetDeclaredSymbol(p))
        .OfType<IParameterSymbol>()
         .Where(p => p.RefKind != RefKind.Out && func(p.Type))
        .OfType<ISymbol>() ?? new ISymbol[] {})
    .ToArray() ?? new ISymbol[] { };

    private readonly Compilation compilation;
}
