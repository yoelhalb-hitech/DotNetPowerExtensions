
using Microsoft.CodeAnalysis.Operations;

namespace DotNetPowerExtensions.Analyzers.Throws;

public class PossibleExceptionTracker
{
    private readonly Compilation compilation;
    private readonly SemanticModel semanticModel;

    public PossibleExceptionTracker(Compilation compilation) : this(compilation, compilation.GetSemanticModel(compilation.SyntaxTrees.First())) { }
    public PossibleExceptionTracker(Compilation compilation, SemanticModel semanticModel)
	{
        this.compilation = compilation;
        this.semanticModel = semanticModel;
    }

    private IEnumerable<INamedTypeSymbol> GetDocCommentExceptions(ISymbol symbol)
    {
        var exceptions = ThrowsUtils.GetDocCommentExceptions(symbol, compilation);
        return exceptions.Select(e => e.Item2).OfType<INamedTypeSymbol>();
    }

    public IEnumerable<INamedTypeSymbol> GetSymbolExceptions(ISymbol symbol)
        => GetDocCommentExceptions(symbol).Concat(ThrowsUtils.GetThrowsExceptions(symbol, compilation)).Distinct(SymbolEqualityComparer.Default).OfType<INamedTypeSymbol>();

    public IEnumerable<INamedTypeSymbol>? GetPossibleExceptions(IOperation operation)
    {
        throw new NotImplementedException();
    }

    private Func<IOperation, (bool, ISymbol[])> parameterInStartingPoint => o => (o is IMethodBodyOperation,
                                        ((o as IMethodBodyOperation)?.Syntax as MethodDeclarationSyntax)?.ParameterList.Parameters
                                                .Select(p => semanticModel.GetDeclaredSymbol(p))
                                                .OfType<IPropertySymbol>()
                                                .Where(p => p.RefKind != RefKind.Out && GetSymbolExceptions(p).Any())
                                                .ToArray()
                                             ?? Array.Empty<ISymbol>());

    private Func<IOperation, (bool, ISymbol[])> argumentOutStartingPoint => o => (o is IArgumentOperation argument &&
                                                                        argument.Parameter is not null && argument.Parameter.RefKind != RefKind.In
                                                                            && GetSymbolExceptions(argument.Parameter).Any(),
                                                                Array.Empty<ISymbol>());

    // TODO we need a better way for return values on properties/fields that can throw
    private Func<IOperation, (bool, ISymbol[])> propertyStartingPoint => o => (o is IPropertyReferenceOperation prop
                                                                                    && GetSymbolExceptions(prop.Property).Any(),
                                                                        Array.Empty<ISymbol>());

    private Func<IOperation, (bool, ISymbol[])> fieldStartingPoint => o => (o is IFieldReferenceOperation field
                                                                                && GetSymbolExceptions(field.Field).Any(),
                                                                    Array.Empty<ISymbol>());

    // TODO... handle return and return attribute...
    private Func<IOperation, (bool, ISymbol[])> methodStartingPoint => o => (o is IMethodReferenceOperation method
                                                                                && GetSymbolExceptions(method.Method).Any(),
                                                                    Array.Empty<ISymbol>());

    private Func<IOperation, bool> parameterOutEndpoint => o => o is IParameterReferenceOperation parameter && parameter.Parameter.RefKind != RefKind.In;
    private Func<IOperation, bool> argumentInEndpoint => o => o is IArgumentOperation argument &&
                                                                        argument.Parameter is not null && argument.Parameter.RefKind != RefKind.In;
    private Func<IOperation, bool> fieldOutEndpoint => o => o is IFieldReferenceOperation;
    private Func<IOperation, bool> propertyOutEndpoint => o => o is IParameterReferenceOperation;
    private Func<IOperation, bool> returnEndpoint => o => o is IReturnOperation;
    private Func<IOperation, bool> invocationEndpoint => o => o is IInvocationOperation;
    private Func<IOperation, bool> awaitEndpoint => o => o is IAwaitOperation;


    public void Analyze()
    {
        // TODO... we only need to capture local if there is a try/catch block or an inline or local function
    }
}
