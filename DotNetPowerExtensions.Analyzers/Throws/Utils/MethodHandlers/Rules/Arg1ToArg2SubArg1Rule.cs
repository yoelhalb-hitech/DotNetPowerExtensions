
namespace DotNetPowerExtensions.Analyzers.Throws;

internal class Arg1ToArg2SubArg1Rule : MethodRuleWrapper
{
    public Arg1ToArg2SubArg1Rule(Compilation compilation) : base(compilation)
    {
        From = ArgTypes.Arg1;
        To = ArgTypes.Arg2;
        SubTo = ArgTypes.Arg1;
    }
}
