
namespace DotNetPowerExtensions.Analyzers.Throws;

internal class Arg1ToResultRule : MethodRuleWrapper
{
    public Arg1ToResultRule(Compilation compilation) : base(compilation)
    {
        From = ArgTypes.Arg1;
        To = ArgTypes.Result;
    }
}
