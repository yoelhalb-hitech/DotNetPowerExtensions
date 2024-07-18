
namespace DotNetPowerExtensions.Analyzers.Throws;

internal class Arg2SubResultToResultRule : MethodRuleWrapper
{
    public Arg2SubResultToResultRule(Compilation compilation) : base(compilation)
    {
        From = ArgTypes.Arg2;
        To = ArgTypes.Result;
        SubFrom = ArgTypes.Result;
    }
}
