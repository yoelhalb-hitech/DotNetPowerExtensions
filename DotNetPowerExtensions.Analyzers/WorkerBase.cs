using SequelPay.DotNetPowerExtensions.RoslynExtensions;

namespace SequelPay.DotNetPowerExtensions.Analyzers;

internal class WorkerBase
{
	public WorkerBase(SemanticModel semanticModel) : this(semanticModel.Compilation, semanticModel)
	{
    }

    public WorkerBase(Compilation compilation, SemanticModel semanticModel)
    {
        SemanticModel = semanticModel;
        Compilation = compilation;
    }

    public virtual SemanticModel SemanticModel { get; }
    public virtual Compilation Compilation { get; }

    public INamedTypeSymbol? GetTypeSymbol(Type type) => Compilation.GetTypeSymbol(type);

    public INamedTypeSymbol[] GetTypeSymbols(Type[] types) => types.Select(t => GetTypeSymbol(t)).OfType<INamedTypeSymbol>().ToArray();
}
