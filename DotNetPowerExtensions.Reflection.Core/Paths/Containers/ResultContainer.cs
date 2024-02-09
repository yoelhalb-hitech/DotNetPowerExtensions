using SequelPay.DotNetPowerExtensions.Reflection.Core.Models;

namespace SequelPay.DotNetPowerExtensions.Reflection.Core.Paths; // Needs to be in this namepsace because of the Outer class

internal partial class Outer<TTypeContainerCache>
{
    public class ResultContainer
    {
        public ResultContainer(PathContainer underlying, ResultContainer? parent, ResultContainer[]? genericArgs = null)
        {
            Underlying = underlying;
            Parent = parent;
            GenericArgs = genericArgs;
        }
        public ResultContainer[]? GenericArgs { get; private set; }
        private IEnumerable<ResultContainer> AllGenericArgs => (Parent?.AllGenericArgs ?? new ResultContainer[] { })
                                                                            .Concat(GenericArgs ?? new ResultContainer[] { });
        public PathContainer Underlying { get; }
        public ResultContainer? Parent { get; }
        private string Name => GenericArgs?.Any() != true ? Underlying.Name : Underlying.Name.SubstringUntil('`')!;
        private string GenericArgsString => GenericArgs?.Any() != true ? "" : $"<{GenericArgs.Select(a => a.FullPath).Join(",")}>";
        public virtual string FullPath =>
            !Underlying.IsPostFixName ?
                Name + GenericArgsString + Underlying.Splitter + Parent?.FullPath :
                Parent?.FullPath + Underlying.Splitter + Name + GenericArgsString;

        public virtual ITypeDetailInfo? UnderlyingType =>
            Name == "[]" ? (Parent?.ConstructedType ?? Parent?.UnderlyingType)?.ToArrayType() : Underlying.Type;
        public virtual ITypeDetailInfo? ConstructedType =>
            AllGenericArgs?.Any() != true ? null : UnderlyingType?
                    .GetConstructedGeneric(AllGenericArgs
                                                .Select(a => a.ConstructedType ?? a.UnderlyingType)
                                                .OfType<ITypeDetailInfo>().ToArray());
    }
}
