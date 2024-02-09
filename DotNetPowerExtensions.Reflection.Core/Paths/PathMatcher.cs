using SequelPay.DotNetPowerExtensions;
using SequelPay.DotNetPowerExtensions.Reflection.Core.Models;

namespace SequelPay.DotNetPowerExtensions.Reflection.Core.Paths;

/********************************************
 * There can be ambiguaity in the folowing situations:
 * 1) Two classes with the exact same namespace and name and number of generic arguments
 * 2) Two files classes with the exact same namespace and name and number of generic arguments and file name
********************************************/

internal partial class Outer<TTypeContainerCache>
{
    internal class PathMatcher
    {
        private ITypeDetailInfo[] genericStubs = new ITypeDetailInfo[] { };
        public PathMatcher() { }
        public PathMatcher(ITypeDetailInfo[] genericStubs) { this.genericStubs = genericStubs; }

        public ITypeDetailInfo? Match(string path)
        {
            if (path.EndsWith("[]", StringComparison.Ordinal)) return Match(path.SubstringUntil('[', true)!)?.ToArrayType();

            var result = new PathProcessor(genericStubs).GetPathContainer(path);
            return result.ConstructedType ?? result.UnderlyingType;
        }
        public string GetFullPath(string path)
            => new PathProcessor(genericStubs).GetPathContainer(path).FullPath;

        public string GetFullPath(ITypeDetailInfo type)
        {
            if (type.IsConstructedGeneric) return GetConstructedGenericPath(type, GetFullPath);
            else if (type.IsGenericParameter) return type.Name;

            return TypeContainerCache.ContainerCache.GetFullName(type);
        }

        public string GetMinimalPath(string path)
            => new MinimalPathProcessor(genericStubs).GetPathContainer(path).FullPath;

        public string GetMinimalPath(ITypeDetailInfo type)
        {
            if (type.IsConstructedGeneric) return GetConstructedGenericPath(type, GetMinimalPath);
            else if(type.IsGenericParameter) return type.Name;

            var longPath = TypeContainerCache.ContainerCache.GetFullName(type);
            return GetMinimalPath(longPath);
        }

        private static string GetConstructedGenericPath(ITypeDetailInfo type, Func<ITypeDetailInfo, string> func)
        {
            if (!type.IsConstructedGeneric) throw new ArgumentException("Should not arrive here");

            var path = "";
            var args = type.GenericArguments; // Notice that GenericTypeArguments is only there for the actual type, not for any outer type

            // We need to figure out where the generic belongs as they will be reported for all inner as well
            for (var t = type; t is not null && t.IsGeneric; t = t.OuterType) // IsGenericType is true as long as an outer is generic
            {
                var outerArgs = t.OuterType?.IsGeneric == true ? t.OuterType!.GenericArguments.Length : 0;
                var currentArgs = args.Skip(outerArgs);
                args = args.Take(outerArgs).ToArray();

                var argsPath = !currentArgs.Any() ? "" : "<" + currentArgs.Select(func).Join(",") + ">";

                var currentPath = t.OuterType is null || !t.OuterType.IsGeneric ? func(t.GenericDefinition ?? t) : t.Name;

                path = currentPath.SubstringUntil('`') + argsPath +
                        (path.HasValue() ? TypeContainerCache.ContainerCache.InnerClassSeparator + path : "");
            }

            return path;
        }
    }
}
