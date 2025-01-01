
namespace SequelPay.DotNetPowerExtensions.Reflection.Paths;

internal partial class Outer<TTypeContainerCache>
{
    internal abstract class MethodMatcherBase<TMethod>
            where TMethod : IMethodBase<TMethod>
    {
        protected readonly string name;
        protected IEnumerable<TMethod> candidates;
        protected readonly ITypeDetailInfo[] genericStubs;

        public MethodMatcherBase(string name, IEnumerable<TMethod> candidates, ITypeDetailInfo[] genericStubs)
        {
            this.name = name;
            this.candidates = candidates;
            this.genericStubs = genericStubs;
        }

        private string? parameterPart;
        protected string ParameterPart => parameterPart ?? (parameterPart = GetParameterPart());

        public abstract IEnumerable<TMethod> GetCandidates();

        protected string[] FilterByArgs(string argPart, Func<TMethod, int> filter)
        {
            var args = new string[] { };
            if (!argPart.HasValue()) return args; // If it doesnt have value it can mean no generic/argument but also that it relies on the args etc.

            var count = 0;
            if (!int.TryParse(argPart, out count))
            {
                // Remember that the argument part can also have inner generic parts which can also have commas so we remove them before splitting...
                args = PathProcessor.SplitArgs(argPart).ToArray();
                count = args.Length;
            }
            candidates = candidates.Where(m => filter(m) == count);

            if (candidates.Empty()) throw new FormatException($"{argPart} do not match valid members");

            return args;
        }

        protected abstract IEnumerable<ITypeDetailInfo> GetGenericStubs(TMethod method);

        protected void FilterByParameters()
        {
            var args = FilterByArgs(ParameterPart, m => m.Parameters.Length);

            if (args.Any() && !candidates.HasOnlyOne())
            {
                // TODO... args can have in-out-ref etc.
                candidates = candidates.Where(m =>
                {
                    var stubs = GetGenericStubs(m).ToArray();
                    var types = args.Select(a => new PathMatcher(stubs).Match(a));
                    return m.Parameters.Select(a => a.ParameterType).SequenceEqual(types);
                });
            }

            if (candidates.Empty()) throw new FormatException($"{parameterPart} do not match valid members");
        }

        protected string GetParameterPart()
        {
            var argPart = name.SubstringFrom("(", true, nullIfNotFound: true)?.SubstringUntil(")", true) ?? "";

            if (argPart.StartsWith("`", StringComparison.Ordinal) == true) argPart = argPart.SubstringFrom("`", true)!;

            return argPart;
        }
    }
}
