
namespace SequelPay.DotNetPowerExtensions.Reflection.Core.Paths; // Needs to be in this namepsace because of the Outer class

internal partial class Outer<TTypeContainerCache>
{
    internal class MinimalResultContainer : ResultContainer
    {
        public MinimalResultContainer(PathContainer underlying,
                ResultContainer? parent, ResultContainer[]? genericArgs = null)
            : base(underlying, parent, genericArgs)
        {
        }

        public override string FullPath =>
            TypeContainerCache.ContainerCache.SpecialTypesShortName.TryGetValue(base.FullPath, out var shortName)
                    ? shortName
                    : base.FullPath;
    }
}
