
using SequelPay.DotNetPowerExtensions.RoslynExtensions;

namespace DotNetPowerExtensions.RoslynExtensions;

public static class CompilationExtensions
{
    public static MetadataReference? GetMetadataReference(this Compilation compilation, IAssemblySymbol assemblySymbol)
        => compilation.References.FirstOrDefault(r => compilation.GetAssemblyOrModuleSymbol(r).IsEqualTo(assemblySymbol));

    public static IAssemblySymbol? GetAssemblySymbol(this Compilation compilation, string assemblyFullName)
         => compilation.References
                                .Select(compilation.GetAssemblyOrModuleSymbol).OfType<IAssemblySymbol>()
                                .FirstOrDefault(a => a.Identity.ToString() == assemblyFullName); // Assuming that .Net won't allow more than one assembly with the same full name

    public static INamedTypeSymbol? GetTypeSymbol(this Compilation compilation, string typeName, string assemblyFullName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var type = compilation.GetTypeByMetadataName(typeName);
        if (type is not null) return type; // GetTypeByMetadataName can return null if there are 2 types with the same name

        var assembly = compilation.GetAssemblySymbol(assemblyFullName);
        return assembly?.GetTypeByMetadataName(typeName);
    }

    public static INamedTypeSymbol? GetTypeSymbol(this Compilation compilation, Type reflectionType, CancellationToken cancellationToken = default)
        => compilation.GetTypeSymbol(reflectionType.FullName, reflectionType.Assembly.FullName, cancellationToken);
}
