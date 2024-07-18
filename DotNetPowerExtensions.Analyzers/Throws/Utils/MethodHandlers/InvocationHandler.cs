
namespace DotNetPowerExtensions.Analyzers.Throws;

internal class InvocationHandler : InvocationHandlerBase
{
    public InvocationHandler(Compilation compilation) : base(compilation)
    {
    }

    public List<IMethodRule> ToGenericMethodRules => new List<IMethodRule>
    {
        new Arg1ToThisRule(Compilation){ Type = typeof(IList<>), Method = nameof(IList<object>.Add), },
        new Arg1ToResultRule(Compilation){ Type = typeof(Task), Method = nameof(Task.Run), },
        new Arg1ToResultRule(Compilation){ Type = typeof(TaskFactory), Method = nameof(TaskFactory.StartNew), },
        new Arg1ToThisRule(Compilation){ Type = typeof(List<>), Method = nameof(List<object>.Add), },
        new Arg1ToThisRule(Compilation){ Type = typeof(List<>), Method = nameof(List<object>.AddRange), },
        new Arg2ToResultRule(Compilation){ Type = typeof(Enumerable), Method = "Append", },
    };
    public List<IMethodRule> FromGenericMethodRules => new List<IMethodRule>
    {
        new Arg1ToResultRule(Compilation){ Type = typeof(Task), Method = nameof(Task.WhenAll), },
        new Arg1ToResultRule(Compilation){ Type = typeof(Task), Method = nameof(Task.WhenAny), },
        new Arg1ToResultRule(Compilation){ Type = typeof(Task), Method = nameof(Task.FromResult), },
        new Arg1ToResultRule(Compilation){ Type = typeof(Task<>), Method = nameof(Task<object>.Result), },
        new Arg1ToResultRule(Compilation){ Type = typeof(Enumerable), Method = nameof(Enumerable.DefaultIfEmpty), },
        new Arg1ToResultRule(Compilation){ Type = typeof(Enumerable), Method = nameof(Enumerable.First), },
        new Arg1ToResultRule(Compilation){ Type = typeof(Enumerable), Method = nameof(Enumerable.FirstOrDefault), },
        new Arg1ToResultRule(Compilation){ Type = typeof(Enumerable), Method = nameof(Enumerable.Single), },
        new Arg1ToResultRule(Compilation){ Type = typeof(Enumerable), Method = nameof(Enumerable.SingleOrDefault), },
        new Arg1ToResultRule(Compilation){ Type = typeof(Enumerable), Method = nameof(Enumerable.Last), },
        new Arg1ToResultRule(Compilation){ Type = typeof(Enumerable), Method = nameof(Enumerable.LastOrDefault), },
        new Arg1ToResultRule(Compilation){ Type = typeof(Enumerable), Method = nameof(Enumerable.ElementAt), },
        new Arg1ToResultRule(Compilation){ Type = typeof(Enumerable), Method = nameof(Enumerable.ElementAtOrDefault), },
        new Arg1ToResultRule(Compilation){ Type = typeof(Enumerable), Method = nameof(Enumerable.Max), },
        new Arg1ToResultRule(Compilation){ Type = typeof(Enumerable), Method = nameof(Enumerable.Min), },
        new Arg1ToResultRule(Compilation){ Type = typeof(List<>), Method = nameof(List<object>.Find), },
        new Arg1ToResultRule(Compilation){ Type = typeof(List<>), Method = nameof(List<object>.FindAll), },
        new Arg1ToResultRule(Compilation){ Type = typeof(List<>), Method = nameof(List<object>.FindIndex), },
        new Arg1ToResultRule(Compilation){ Type = typeof(List<>), Method = nameof(List<object>.FindLast), },
        new Arg1ToResultRule(Compilation){ Type = typeof(List<>), Method = nameof(List<object>.FindLastIndex), },

        new Arg1ToArg2SubArg1Rule(Compilation) { Type = typeof(Enumerable), Method = nameof(Enumerable.Where), },
        new Arg1ToArg2SubArg1Rule(Compilation) { Type = typeof(Enumerable), Method = nameof(Enumerable.Select), },
        new Arg1ToArg2SubArg1Rule(Compilation) { Type = typeof(Enumerable), Method = nameof(Enumerable.SelectMany), },
        new Arg1ToArg2SubArg1Rule(Compilation) { Type = typeof(Enumerable), Method = nameof(Enumerable.SkipWhile), },
        new Arg1ToArg2SubArg1Rule(Compilation) { Type = typeof(Enumerable), Method = nameof(Enumerable.TakeWhile), },
        new Arg1ToArg2SubArg1Rule(Compilation) { Type = typeof(Enumerable), Method = nameof(Enumerable.Any), },
        new Arg1ToArg2SubArg1Rule(Compilation) { Type = typeof(Enumerable), Method = nameof(Enumerable.All), },

        new Arg2SubResultToResultRule(Compilation) { Type = typeof(Enumerable), Method = nameof(Enumerable.Select), },
        new Arg2SubResultToResultRule(Compilation) { Type = typeof(Enumerable), Method = nameof(Enumerable.SelectMany), },
        new Arg2SubResultToResultRule(Compilation) { Type = typeof(Enumerable), Method = nameof(Enumerable.GroupBy), },
        new Arg2SubResultToResultRule(Compilation) { Type = typeof(Enumerable), Method = nameof(Enumerable.ToDictionary), },
        new Arg2SubResultToResultRule(Compilation) { Type = typeof(Enumerable), Method = nameof(Enumerable.ToLookup), },

        new Arg1ToArg2SubArg1Rule(Compilation) { Type = typeof(List<>), Method = nameof(List<object>.ForEach), },
        new Arg1ToArg2SubArg1Rule(Compilation) { Type = typeof(List<>), Method = nameof(List<object>.TrueForAll), },
    };
    public override List<IMethodRule> MethodRules => SameGenericMethodRules.Concat(FromGenericMethodRules).Concat(ToGenericMethodRules).ToList();
    public List<IMethodRule> SameGenericMethodRules => new List<IMethodRule>
    {
        new Arg1ToResultRule(Compilation){ Type = typeof(Enumerable), Method = nameof(Enumerable.ToList), },
        new Arg1ToResultRule(Compilation){ Type = typeof(Enumerable), Method = nameof(Enumerable.ToArray), },
        new Arg1ToResultRule(Compilation){ Type = typeof(Enumerable), Method = "ToHashSet", },
        new Arg1ToResultRule(Compilation){ Type = typeof(Enumerable), Method = nameof(Enumerable.ToDictionary), }, // We need to exclude if it has a value selector
        new Arg1ToResultRule(Compilation){ Type = typeof(Enumerable), Method = nameof(Enumerable.ToLookup), }, // We need to exclude if it has a value selector
        new Arg1ToResultRule(Compilation){ Type = typeof(Enumerable), Method = nameof(Enumerable.GroupBy), },
        new Arg1ToResultRule(Compilation){ Type = typeof(Enumerable), Method = nameof(Enumerable.AsEnumerable), },
        new Arg1ToResultRule(Compilation){ Type = typeof(Enumerable), Method = nameof(Enumerable.Union), },
        new Arg2ToResultRule(Compilation){ Type = typeof(Enumerable), Method = nameof(Enumerable.Union), },
        new Arg1ToResultRule(Compilation){ Type = typeof(Enumerable), Method = "UnionBy", },
        new Arg2ToResultRule(Compilation){ Type = typeof(Enumerable), Method = "UnionBy", },
        new Arg1ToResultRule(Compilation){ Type = typeof(Enumerable), Method = nameof(Enumerable.Concat), },
        new Arg2ToResultRule(Compilation){ Type = typeof(Enumerable), Method = nameof(Enumerable.Concat), },
        new Arg1ToResultRule(Compilation){ Type = typeof(Enumerable), Method = nameof(Enumerable.Except), },
        new Arg1ToResultRule(Compilation){ Type = typeof(Enumerable), Method = nameof(Enumerable.OfType), },
        new Arg1ToResultRule(Compilation){ Type = typeof(Enumerable), Method = nameof(Enumerable.Cast), },
        new Arg1ToResultRule(Compilation){ Type = typeof(Enumerable), Method = nameof(Enumerable.Distinct), },
        new Arg1ToResultRule(Compilation){ Type = typeof(Enumerable), Method = nameof(Enumerable.Skip), },
        new Arg1ToResultRule(Compilation){ Type = typeof(Enumerable), Method = nameof(Enumerable.SkipWhile), },
        new Arg1ToResultRule(Compilation){ Type = typeof(Enumerable), Method = nameof(Enumerable.Take), },
        new Arg1ToResultRule(Compilation){ Type = typeof(Enumerable), Method = nameof(Enumerable.TakeWhile), },
        new Arg1ToResultRule(Compilation){ Type = typeof(Enumerable), Method = nameof(Enumerable.Where), },
        new Arg1ToResultRule(Compilation){ Type = typeof(Enumerable), Method = nameof(Enumerable.Join), },
        new Arg2ToResultRule(Compilation){ Type = typeof(Enumerable), Method = nameof(Enumerable.Join), },
        new Arg1ToResultRule(Compilation){ Type = typeof(Enumerable), Method = nameof(Enumerable.OrderBy), },
        new Arg1ToResultRule(Compilation){ Type = typeof(Enumerable), Method = nameof(Enumerable.OrderByDescending), },
        new Arg1ToResultRule(Compilation){ Type = typeof(Enumerable), Method = nameof(Enumerable.ThenBy), },
        new Arg1ToResultRule(Compilation){ Type = typeof(Enumerable), Method = nameof(Enumerable.ThenByDescending), },
        new ThisToArg1Rule(Compilation){ Type = typeof(ICollection<>), Method = nameof(IList<object>.CopyTo), }, // TODO... ,ake sure it works on the implementation such as list
    };
}

