
namespace SequelPay.DotNetPowerExtensions.Reflection.Paths;

internal partial class Outer<TTypeContainerCache>
{
    internal class ConstrcutorMatcher : MethodMatcherBase<IConstructorDetail>
    {
        public ConstrcutorMatcher(string name, IEnumerable<IConstructorDetail> candidates, ITypeDetailInfo[] genericStubs)
                : base(name, candidates, genericStubs)
        {
        }

        protected override IEnumerable<ITypeDetailInfo> GetGenericStubs(IConstructorDetail method) => genericStubs;

        public override IEnumerable<IConstructorDetail> GetCandidates()
        {
            FilterByParameters();
            return candidates;
        }
    }
}

