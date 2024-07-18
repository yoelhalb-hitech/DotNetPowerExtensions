
namespace DotNetPowerExtensions.Analyzers.Throws;

internal class ThisToArg1Rule : MethodRuleWrapper
{
    public ThisToArg1Rule(Compilation compilation) : base(compilation)
    {
        From = ArgTypes.This;
        To = ArgTypes.Arg1;
    }
}
