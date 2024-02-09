using SequelPay.DotNetPowerExtensions;
using SequelPay.DotNetPowerExtensions.Reflection.Core.Models;
using SequelPay.DotNetPowerExtensions.Reflection.Core.Paths.Maskers;

namespace SequelPay.DotNetPowerExtensions.Reflection.Core.Paths;

internal partial class Outer<TTypeContainerCache>
{
    internal class PathProcessor
    {
        protected static TTypeContainerCache Cache = TypeContainerCache.ContainerCache;
        public PathProcessor() { }
        private Dictionary<string, ITypeDetailInfo> genericStubs = new();
        public PathProcessor(ITypeDetailInfo[] genericStubs) { this.genericStubs = genericStubs.ToDictionary(t => t.Name); }

        public ResultContainer GetPathContainer(string path)
        {
            var result = GetPathContainers(path);

            if (result.Length > 1) throw new AmbiguousMatchException($"`{path}` is ambiguous between `{result.Select(i => i.FullPath).Join("` and `")}`");

            return result.First();
        }

        internal ResultContainer[] GetPathContainers(string path)
        {
            if (!path.HasValue()) throw new ArgumentNullException(nameof(path), "Empty path, possibly you have an extra comma in a generic type");

            path = path.Trim();

            if (path.EndsWith("[]", StringComparison.Ordinal)) return HandleArray(path);

            if (Cache.SpecialTypesLongName.TryGetValue(path, out var longName)) path = longName;

            if (genericStubs.TryGetValue(path, out var stub))
                    return new[] { CreateResult(new GenericStubPathContainer(path, stub), null) };

            Cache.Ensure();

            // Remember that the generic part can also have namespaces and inner, so we have to temporary remove them before
            var genericMasker = new GenericMasker();
            var maskedPath = genericMasker.Mask(path);

            var splittedByNameSpaces = maskedPath.Split(Cache.NetNamespaceSeparator).Reverse();
            var splittedByOuter = splittedByNameSpaces.First().Split(Cache.InnerClassSeparator).Reverse();

            splittedByNameSpaces = genericMasker.Unmask(splittedByNameSpaces);
            splittedByOuter = genericMasker.Unmask(splittedByOuter);

            var firstResult = GetByTypeName(splittedByOuter.First());

            var outerResult = GetByOuterNames(splittedByOuter.Skip(1), firstResult);
            var nsResult = GetByNamespaceNames(splittedByNameSpaces.Skip(1), outerResult);

            return GetFull(nsResult); // This is so far taking the approach that 1) we don't need the type for minimal and that 2) we assume there not to be a collision later, this might be an oversimplification
        }

        private ResultContainer[] HandleArray(string path)
        {
            var innerPath = path.Substring(0, path.Length - 2);
            var inners = GetPathContainers(innerPath);

            return inners.Select(i => CreateResult(Cache.ArrayPathContainer, i, null)).ToArray();
        }

        protected virtual ResultContainer CreateResult(PathContainer underlying,
                                ResultContainer? parent, ResultContainer[]? genericArgs = null)
                    => new ResultContainer(underlying, parent, genericArgs);

        protected virtual ResultContainer[] GetFull(ResultContainer current)
            => (current.Underlying.Outers.Count + current.Underlying.Namespaces.Count) <= 0
                ? new[] { current }
                : current.Underlying.Outers.SelectMany(o => GetFull(CreateResult(o.Value, current)))
                        .Concat(current.Underlying.Namespaces.SelectMany(ns => GetFull(CreateResult(ns.Value, current))))
                        .ToArray();

        private (string name, ResultContainer[] genericArgs) ProcessGeneric(string name)
        {
            // File class outer part can start with <  and end with >
            var isGenericConstructed = name.EndsWith(">", StringComparison.Ordinal) && !name.StartsWith("<", StringComparison.Ordinal);
            if (!isGenericConstructed) return (name, new ResultContainer[] { });

            var actualNamePart = name.SubstringUntil('<');
            var genericPart = name.SubstringFrom("<", true)!.SubstringUntil(">", true)!;

            var args = SplitArgs(genericPart).Select(GetPathContainer).ToArray();

            return (actualNamePart + "`" + args.Length, args);
        }

        internal static IEnumerable<string> SplitArgs(string str)
        {
            // Remember that any path can also have inner generic parts which can also have commas so we remove them before splitting...
            var masker = new GenericMasker();
            var masked = masker.Mask(str);

            return masker.Unmask(masked.Split(','));
        }

        private ResultContainer GetByTypeName(string typeName)
        {
            var (actualNamePart, args) = ProcessGeneric(typeName);
            try
            {
                var current = Cache.StringContainerCache[actualNamePart];

                return CreateResult(current, null, args?.ToArray());
            }
            catch (KeyNotFoundException) { throw new KeyNotFoundException($"A type with name `{typeName}` was not found, please verify that the name is correct and that the type has been loaded"); }
        }

        protected virtual ResultContainer GetByOuterNames(IEnumerable<string> outerNames, ResultContainer parent)
            => !outerNames.Any() ? parent : GetByOuterNames(outerNames.Skip(1), GetByOuterName(outerNames.First(), parent));

        private ResultContainer GetByOuterName(string outerName, ResultContainer parent)
        {
            var (actualNamePart, args) = ProcessGeneric(outerName);

            try
            {
                var current = parent.Underlying.Outers[actualNamePart];

                return CreateResult(current, parent, args?.ToArray());
            }
            catch (KeyNotFoundException) { throw new FormatException($"{actualNamePart} is not an outer type of {parent.FullPath}, if it is a namespace then use `.` as the separator"); }
        }

        protected virtual ResultContainer GetByNamespaceNames(IEnumerable<string> nsNames, ResultContainer parent)
            => !nsNames.Any() ? parent : GetByNamespaceNames(nsNames.Skip(1), GetByNamespaceName(nsNames.First(), parent));

        private ResultContainer GetByNamespaceName(string nsName, ResultContainer parent)
        {
            try
            {
                var current = parent.Underlying.Namespaces[nsName];

                return CreateResult(current, parent, null);
            }
            catch (KeyNotFoundException) { throw new FormatException($"{nsName} is not a namespace of {parent.FullPath}, if it is an outer type then use `+` as the separator"); }
        }
    }
}
