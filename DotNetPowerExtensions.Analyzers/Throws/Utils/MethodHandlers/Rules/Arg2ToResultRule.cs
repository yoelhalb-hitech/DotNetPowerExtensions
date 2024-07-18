
namespace DotNetPowerExtensions.Analyzers.Throws;

internal class Arg2ToResultRule : MethodRuleWrapper
{
    public Arg2ToResultRule(Compilation compilation) : base(compilation)
    {
        From = ArgTypes.Arg2;
        To = ArgTypes.Result;
    }
}
