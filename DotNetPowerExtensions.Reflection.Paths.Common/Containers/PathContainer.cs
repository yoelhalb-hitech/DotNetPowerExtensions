
namespace SequelPay.DotNetPowerExtensions.Reflection.Paths; // Needs to be in this namepsace because of the Outer class

internal partial class Outer<TTypeContainerCache>
{
    public abstract class PathContainer
    {
        public PathContainer(string name, PathContainer? parent)
        {
            Name = name;
            Parent = parent;
        }

        public string Name { get; private set; }
        public abstract string Splitter { get; }
        public string FullPath => (Parent?.FullPath.HasValue() == true ? Parent?.FullPath + Splitter : "") + Name;
        public Dictionary<string, NamespacePathContainer> Namespaces { get; set; } = new Dictionary<string, NamespacePathContainer>();
        public Dictionary<string, OuterPathContainer> Outers { get; set; } = new Dictionary<string, OuterPathContainer>();
        public virtual ITypeDetailInfo? Type => (Outers.Count + Namespaces.Count) switch
        {
            1 => Outers.FirstOrDefault().Value?.Type ?? Namespaces.First().Value?.Type,
            0 => TypeContainerCache.ContainerCache.GetTypeDetailInfo(FullPath),
            _ => throw new AmbiguousMatchException(Outers.Keys.Concat(Namespaces.Keys).Join(",")),
        };
        public PathContainer? Parent { get; private set; }
        public bool IsPostFixName { get; set; }
    }
}
