using DotNetPowerExtensions.RoslynExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SequelPay.DotNetPowerExtensions.RoslynExtensions;

internal static class CecilExtensions
{
    public static string GetCecilTypeName(this ITypeSymbol type) => type.GetFullName().Replace("+", "/");//Cecil uses `/` for inner types
    public static string GetTypeName(this TypeReference type) => type.FullName.Replace("/", "+");

    public static ModuleDefinition? GetModuleDefinition(this Compilation compilation, IAssemblySymbol assembly,
                                                                                CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var reference = compilation.GetMetadataReference(assembly);

        if (reference?.Display is null) return null;

        try
        {
            if (!File.Exists(reference.Display)) return null;
        }
        catch { return null; }

        return ModuleDefinition.ReadModule(reference.Display, new ReaderParameters() { InMemory = true }); // This way it is not blocking
    }

    public static bool IsEqual(this TypeReference typeReference, ITypeSymbol typeSymbol)
               => typeReference.GetTypeName() == typeSymbol.GetFullName() && typeReference.Scope?.Name == typeSymbol.ContainingAssembly?.Name; // Also ensure that the types come from the same assembly and not just have identical names

    public static bool IsEqual(this TypeReference[] typeReferences, ITypeSymbol[] typeSymbols)
            => typeReferences.Length == typeSymbols.Length && Enumerable.Range(0, typeReferences.Length).All(i => typeReferences[i].IsEqual(typeSymbols[i]));

    public static ITypeSymbol? ToTypeSymbol(this TypeReference typeReference, Compilation compilation,
                                                                                CancellationToken cancellationToken = default)
    {
        var roslynTypeName = typeReference.GetTypeName();

        var assemblyName = typeReference.Module.Assembly.Name.FullName;
        return compilation.GetTypeSymbol(roslynTypeName, assemblyName);
    }

    // TODO... if we are to make this public we have to test the methods under all conditions
    //      when 1) the class is generic and the method is not 2) the method is genetic and the class is not 3) when both are generic 4) when none is
    // So far we only use it for ctors which are not generic...
    internal static IMethodSymbol? ToMethodSymbol(this MethodReference methodReference, Compilation compilation,
                                                                                CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var type = methodReference.DeclaringType.ToTypeSymbol(compilation, cancellationToken);

        var methods = type?.GetMembers(methodReference.Name).OfType<IMethodSymbol>()
            .Where(m => m.GetParameters().Length == methodReference.Parameters.Count && m.TypeParameters.Length == methodReference.GenericParameters.Count);

        var parameterTypes = methodReference.Parameters.Select(p => p.ParameterType).ToArray();
        return methods?.FirstOrDefault(m => parameterTypes.IsEqual(m.Parameters.Select(p => p.Type).ToArray()));
    }

    // TODO... if we are to make this public we have to test the methods under all conditions
    //      when 1) the class is generic and the method is not 2) the method is genetic and the class is not 3) when both are generic 4) when none is
    // So far we only use it for ctors which are not generic...
    internal static MethodDefinition? ToMethodDefinition(this ModuleDefinition module, IMethodSymbol methodSymbol,
                                                                                CancellationToken cancellationToken = default)
    {
        if(!methodSymbol.OriginalDefinition.IsEqualTo(methodSymbol))
                throw new ArgumentException("Only original method symbols are valid", nameof(methodSymbol));

        cancellationToken.ThrowIfCancellationRequested();

        var type = module.ToTypeDefinition(methodSymbol.ContainingType, cancellationToken);

        var methods = type?.Methods.Where(m => m.Name == methodSymbol.MetadataName && m.Parameters.Count == methodSymbol.Parameters.Length
                                                                            && m.GenericParameters.Count == methodSymbol.TypeParameters.Length);

        var parameterTypes = methodSymbol.Parameters.Select(p => p.Type).ToArray();
        return methods? .FirstOrDefault(m => m.Parameters.Select(p => p.ParameterType).ToArray().IsEqual(parameterTypes));
    }


    public static TypeDefinition? ToTypeDefinition(this ModuleDefinition module, ITypeSymbol type, CancellationToken cancellationToken = default)
    {
        if (!type.IsEqualTo(type.OriginalDefinition)) throw new ArgumentException("Only non generic or open generic types are valid", nameof(type));

        cancellationToken.ThrowIfCancellationRequested();

        var outerType = type;

        var containers = new List<ITypeSymbol>();
        while (outerType.ContainingType is not null)
        {
            if (!outerType.IsEqualTo(type)) containers.Add(outerType);
            outerType = outerType.ContainingType;
        }

        containers.Reverse(); // We need it from outer in instead of now as it is inner out

        var cecilName = outerType.GetCecilTypeName();
        var outerCecil = module.Types.FirstOrDefault(t => t.FullName == cecilName);

        var cecilType = outerCecil;
        foreach (var container in containers) cecilType = cecilType?.NestedTypes.FirstOrDefault(t => t.Name == container.MetadataName);

        if (!outerType.IsEqualTo(type)) cecilType = cecilType?.NestedTypes.FirstOrDefault(t => t.Name == type.MetadataName);

        return cecilType;
    }
}
