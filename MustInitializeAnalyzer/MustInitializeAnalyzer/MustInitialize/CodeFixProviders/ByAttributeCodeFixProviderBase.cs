using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using DotNetPowerExtensionsAnalyzer.MustInitialize.Analyzers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetPowerExtensionsAnalyzer.MustInitialize.CodeFixProviders;

public abstract class ByAttributeCodeFixProviderBase<TAnalyzer, TNode> : MustInitializeCodeFixProviderBase<TAnalyzer, TNode>
                                            where TAnalyzer : ByAttributeAnalyzerBase, IMustInitializeAnalyzer
                                            where TNode : CSharpSyntaxNode
{
    protected abstract Type AttributeType { get; }
}
