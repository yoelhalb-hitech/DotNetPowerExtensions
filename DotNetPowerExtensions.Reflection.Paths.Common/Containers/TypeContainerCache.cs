
namespace SequelPay.DotNetPowerExtensions.Reflection.Paths; // Needs to be in this namepsace because of the Outer class

internal partial class Outer<TTypeContainerCache>
{
    internal abstract class TypeContainerCache
    {
        internal static TTypeContainerCache ContainerCache = new TTypeContainerCache();

        internal Dictionary<string, PathContainer> StringContainerCache = new();

        internal PathContainer ArrayPathContainer = new TypePathContainer("[]") { IsPostFixName = true };

        protected abstract Dictionary<string, ITypeDetailInfo> SpecialTypes { get; }

        internal abstract string GetFullName(ITypeDetailInfo type); //  Will return similar to Reflection FullName but replaced for file class
        internal abstract ITypeDetailInfo GetTypeDetailInfo(string fullPath); // TODO... handle file class

        internal Dictionary<string, string> SpecialTypesLongName => SpecialTypes.ToDictionary(t => t.Key, t => GetFullName(t.Value));
        internal Dictionary<string, string> SpecialTypesShortName =>
                                    SpecialTypes.Select(s => (s.Key, GetFullName(s.Value)))
                                    .Concat(SpecialTypes.Select(s => (s.Key, s.Value.Name)))
                                    .ToDictionary(t => t.Item2, t => t.Key);


        protected static object lockObject = new object();
        public char NetNamespaceSeparator = '.';
        public char InnerClassSeparator = '+';

        private static PathContainer LockAndInsert(object lockObject, Func<PathContainer?> cond, Action action)
        {
            var val = cond();
            if (val is not null) return val;

            lock (lockObject)
            {
                val = cond();
                if (val is not null) return val;

                action();

                return cond()!;
            }
        }

        private static PathContainer? ValueOrDefault<T>(Dictionary<T, PathContainer> dict, T key) where T : notnull
                 => dict.TryGetValue(key, out var val) ? val : null;

        //  Expects similar to Reflection FullName but replaced for file class
        protected PathContainer InsertPathContainer(string path)
        {
            var splittedNameSpaces = path.Split(NetNamespaceSeparator).Reverse();
            var splittedInner = splittedNameSpaces.First().Split(InnerClassSeparator).Reverse();

            var typeName = splittedInner.First();
            var current = LockAndInsert(lockObject, () => ValueOrDefault(StringContainerCache, typeName),
                            () => StringContainerCache[typeName] = new TypePathContainer(typeName));

            foreach (var outer in splittedInner.Skip(1))
            {
                current = LockAndInsert(current, () => current.Outers.FirstOrDefault(o => o.Key == outer).Value,
                  () => current.Outers.Add(outer, new OuterPathContainer(outer, current)));
            }

            foreach (var ns in splittedNameSpaces.Skip(1))
            {
                current = LockAndInsert(current, () => current.Namespaces.FirstOrDefault(o => o.Key == ns).Value,
                    () => current.Namespaces.Add(ns, new NamespacePathContainer(ns, current)));
            }

            return current;
        }

        internal abstract void Ensure();
    }
}
