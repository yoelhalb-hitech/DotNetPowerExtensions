
namespace DotNetPowerExtensions.Analyzers.Throws;

internal class Arg1ToThisRule : MethodRuleWrapper
{
    public Arg1ToThisRule(Compilation compilation) : base(compilation)
    {
        From = ArgTypes.Arg1;
        To = ArgTypes.This;
    }
}
