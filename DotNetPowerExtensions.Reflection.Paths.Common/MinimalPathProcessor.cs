
namespace SequelPay.DotNetPowerExtensions.Reflection.Paths;

internal partial class Outer<TTypeContainerCache>
{
    internal partial class MinimalPathProcessor : PathProcessor
    {

        public MinimalPathProcessor() { }
        public MinimalPathProcessor(ITypeDetailInfo[] genericStubs) : base(genericStubs) { }

        protected override ResultContainer CreateResult(PathContainer underlying,
                                        ResultContainer? parent, ResultContainer[]? genericArgs = null)
        {
            return new MinimalResultContainer(underlying, parent, genericArgs);
        }

        private static bool IsEnough(ResultContainer current)
            => (current.Underlying.Outers.Count + current.Underlying.Namespaces.Count) <= 1;

        protected override ResultContainer[] GetFull(ResultContainer current)
            => IsEnough(current) ? new[] { current } : base.GetFull(current);

        protected override ResultContainer GetByOuterNames(IEnumerable<string> outerNames, ResultContainer parent)
            => IsEnough(parent) ? parent : base.GetByOuterNames(outerNames, parent);


        protected override ResultContainer GetByNamespaceNames(IEnumerable<string> nsNames, ResultContainer parent)
            => IsEnough(parent) ? parent : base.GetByNamespaceNames(nsNames, parent);

    }
}
