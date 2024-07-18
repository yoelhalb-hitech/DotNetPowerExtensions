using Microsoft.CodeAnalysis.Shared.Extensions;
using Mono.Cecil.Cil;
using Mono.Cecil;

namespace SequelPay.DotNetPowerExtensions.RoslynExtensions;

public static class MethodSymbolExtensions
{
    /// <summary>
    /// Returns the entire constructor chain for a constructor (not including the passed constructor or the constructor of <see cref="object"/>)
    /// </summary>
    /// <param name="method">The constructor symbol for which we want to get the chain</param>
    /// <param name="semanticModel">The <see cref="SemanticModel"/> that was used to obtain the constructor symbol</param>
    /// <param name="cancellationToken">The <see cref="SemanticModel"/> that was used to obtain the constructor symbol</param>
    /// <returns>A list of chained constructors symbols with the constructors closer to the passed constructor being returned first</returns>
    /// <exception cref="ArgumentException">The constructor is a static constructor</exception>
    /// <exception cref="ArgumentException">The symbol provided is not a constructor</exception>
    public static IEnumerable<IMethodSymbol> GetConstructorChain(this IMethodSymbol method, SemanticModel semanticModel,
                                                                                    CancellationToken cancellationToken = default)
    {
        if(method is null) throw new ArgumentNullException(nameof(method));
        if(semanticModel is null) throw new ArgumentNullException(nameof(semanticModel));

        if(method.IsStatic) throw new ArgumentException("Static constructor is not valid", nameof(method));
        if(method.Name != ".ctor") throw new ArgumentException("Not a constructor", nameof(method));

        if (method.GetContainerFullName() == typeof(object).FullName) yield break;

        cancellationToken.ThrowIfCancellationRequested();

        var currentMethod = method.GetTrivialChainCtor();

        if (currentMethod is null) yield break;
        else if (!currentMethod.IsEqualTo(method))
        {
            yield return currentMethod;
            foreach (var m in currentMethod.GetConstructorChain(semanticModel, cancellationToken)) yield return m;
            yield break;
        }

        if (method.IsImplicitlyDeclared) // An implicit declared ctor always calls the default base method...
        {
            var baseMethod = method.ContainingType.BaseType?.GetDefaultConstructor();
            if (baseMethod is null || baseMethod.GetContainerFullName() == typeof(object).FullName) yield break;

            yield return baseMethod;
            foreach (var m in baseMethod.GetConstructorChain(semanticModel, cancellationToken)) yield return m;
            yield break;
        }
        We need to handle the new implicit primary ctors
        var syntax = method.GetSyntax<ConstructorDeclarationSyntax>(cancellationToken);
        if(!syntax.Any())
        {
            foreach (var m in method.GetConstructorChainFromOtherAssembly(semanticModel.Compilation, cancellationToken)) yield return m;
            yield break;
        }

        var chained = syntax?.FirstOrDefault(s => s.Initializer is not null);

        var originSemanticModel = chained is null ? null : semanticModel.SyntaxTree == chained.SyntaxTree ? semanticModel : semanticModel.Compilation.GetSemanticModel(chained.SyntaxTree);
        IMethodSymbol? chainedSymbol = chained is null ? null : originSemanticModel.GetSymbolInfo(chained!.Initializer!, cancellationToken).Symbol as IMethodSymbol;

        if(chainedSymbol is null) chainedSymbol = method.ContainingType.BaseType?.SpecialType == SpecialType.System_Object ? null :
                                                                                    method.ContainingType.BaseType?.GetDefaultConstructor();

        if (chainedSymbol is null || chainedSymbol.GetContainerFullName() == typeof(object).FullName) yield break;

        yield return chainedSymbol;
        foreach (var m in chainedSymbol.GetConstructorChain(originSemanticModel ?? semanticModel, cancellationToken)) yield return m;
    }

    private static IMethodSymbol? GetTrivialChainCtor(this IMethodSymbol method)
    {
        if (method.GetContainerFullName() == typeof(object).FullName) return null;
        var containingType = method.ContainingType;

        if (containingType.GetConstructors(false).Count() == 1) // Single ctor method which obviously doens't call this()...
        {
            if (containingType.IsValueType || containingType.BaseType is null || containingType.BaseType.GetFullName() == typeof(object).FullName) return null;

            if (containingType.BaseType.GetConstructors(false).Count() == 1) return containingType.BaseType.GetConstructors(false).First();
        }

        return method;
    }

    private static IMethodSymbol? GetChainedMethod(this IMethodSymbol method, ModuleDefinition module, Compilation compilation,
                                                                                CancellationToken cancellationToken = default)
    {
        var cecilMethod = module?.ToMethodDefinition(method, cancellationToken);

        var initializer = cecilMethod?.Body.Instructions.FirstOrDefault(i => i.OpCode.Code == Code.Call).Operand as MethodReference;

        return initializer?.Name != ".ctor" ? null : initializer.ToMethodSymbol(compilation, cancellationToken);

    }

    private static IEnumerable<IMethodSymbol> GetChainedMethodsInModule(this ModuleDefinition module, IMethodSymbol method, Compilation compilation,
                                                                            CancellationToken cancellationToken = default)
    {
        var objType = compilation.GetTypeByMetadataName(typeof(object).FullName); // Not using .GetTypeSymbol as the runtime core assembly might be different.., assuming that there won't be two System.Object in a compilation...

        var currentMethod = method.OriginalDefinition;

        while (currentMethod is not null && currentMethod.ContainingAssembly.IsEqualTo(method.ContainingAssembly))
        {
            var roslynMethod = currentMethod.GetChainedMethod(module, compilation, cancellationToken);

            if (roslynMethod is null
                || roslynMethod.ContainingType.IsEqualTo(objType)
                || roslynMethod.IsEqualTo(currentMethod)
                || (!roslynMethod.ContainingType.IsEqualTo(method.ContainingType)
                            && !roslynMethod.ContainingType.IsEqualTo(method.ContainingType.BaseType)
                            && !roslynMethod.ContainingType.IsEqualTo(method.ContainingType.BaseType?.OriginalDefinition)
            )) yield break;
            else yield return roslynMethod;

            currentMethod = roslynMethod;
        }

    }

    private static IEnumerable<IMethodSymbol> GetConstructorChainFromOtherAssembly(this IMethodSymbol method, Compilation compilation,
                                                                                CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var currentMethod = method.GetTrivialChainCtor();

        if (currentMethod is null) yield break;
        else if (!currentMethod.IsEqualTo(method))
        {
            yield return currentMethod;
        }
        else
        {
            var module = compilation.GetModuleDefinition(method.ContainingAssembly, cancellationToken);
            if (module is null) yield break;

            var found = false;
            foreach (var chainedMethod in module.GetChainedMethodsInModule(method, compilation, cancellationToken))
            {
                found = true;
                currentMethod = chainedMethod;
                yield return chainedMethod;
            }
            if (!found) yield break;
        }

        foreach (var m in currentMethod.GetConstructorChainFromOtherAssembly(compilation, cancellationToken)) yield return m;
    }

    /// <summary>
    /// Returns the entire method override chain for a method
    /// </summary>
    /// <param name="method">The method symbol for which we want to get the chain</param>
    /// <returns>A list of method symbols with the method closer to the passed property being returned first</returns>
    public static IEnumerable<IMethodSymbol> GetMethodOverrideChain(this IMethodSymbol method)
    {
        var m = method;
        while (m.IsOverride && m.OverriddenMethod is not null)
        {
            m = m.OverriddenMethod;
            yield return m;
        }

        yield break;
    }

    /// <summary>
    /// Get the original method base decleration for a method (the one that does not have the <see langword="override" /> keyword)
    /// </summary>
    /// <param name="method">The method symbol for which we want to get the base</param>
    /// <returns>The base method</returns>
    public static IMethodSymbol GetBaseMethod(this IMethodSymbol method)
        => method.GetMethodOverrideChain().LastOrDefault() ?? method;
}
