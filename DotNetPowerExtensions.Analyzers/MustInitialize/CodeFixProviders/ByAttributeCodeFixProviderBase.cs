using DotNetPowerExtensions.Analyzers.MustInitialize.Analyzers;

namespace DotNetPowerExtensions.Analyzers.MustInitialize.CodeFixProviders;

public abstract class ByAttributeCodeFixProviderBase<TAnalyzer, TNode> : MustInitializeCodeFixProviderBase<TAnalyzer, TNode>
                                            where TAnalyzer : ByAttributeAnalyzerBase, IMustInitializeAnalyzer
                                            where TNode : CSharpSyntaxNode
{
    protected abstract Type AttributeType { get; }
}
