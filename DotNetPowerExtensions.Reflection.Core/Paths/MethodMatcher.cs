using SequelPay.DotNetPowerExtensions.Reflection.Core.Models;
using System.Xml.Linq;

namespace SequelPay.DotNetPowerExtensions.Reflection.Core.Paths;

internal partial class Outer<TTypeContainerCache>
{
    internal class MethodMatcher : MethodMatcherBase<IMethodDetail>
    {
        public MethodMatcher(string name, IEnumerable<IMethodDetail> candidates, ITypeDetailInfo[] genericStubs)
                : base(name, candidates, genericStubs)
        {
        }

        private string? genericPart;
        protected string GenericPart => genericPart ?? (genericPart = GetGenericPart());

        private string GetGenericPart()
        {
            var genericPart = name.SubstringUntil("(")!; // Remember that the args can also use the '`' but have to be enclosed in `()`
                                                         // Remember that inside the generic we might have the '`' as well so first eliminate that part
            genericPart = genericPart.SubstringFrom("<", true, nullIfNotFound: true)?.SubstringUntil(">", true)
                                                                ?? genericPart.SubstringFrom("`", true, nullIfNotFound: true)
                                                                ?? "";
            return genericPart;
        }

        public override IEnumerable<IMethodDetail> GetCandidates()
        {
            if (GenericPart.HasValue()) // If it doesnt have value it can mean no generic but also that it relies on the args etc.
            {
                FilterByGenerics();
                if (candidates.HasOnlyOne()) return candidates.Take(1);
            }

            if (ParameterPart.HasValue())
            {
                FilterByParameters();
                if (candidates.HasOnlyOne()) return candidates.Take(1);
            }

            // We couldn't match directly, so let's try to assume that any missing info means to not use it       
            if (!genericPart.HasValue())
            {
                var nonGeneric = candidates.Where(m => !m.IsGeneric);
                if (nonGeneric.HasOnlyOne()) return nonGeneric.Take(1);
            }

            if (!ParameterPart.HasValue())
            {
                var nonArg = candidates.Where(m => !m.Parameters.Any());
                if (nonArg.HasOnlyOne()) return nonArg.Take(1);
            }

            if (!ParameterPart.HasValue() && !GenericPart.HasValue())
            {
                var nonArgAndGeneric = candidates.Where(m => !m.IsGeneric && !m.Parameters.Any());
                if (nonArgAndGeneric.HasOnlyOne()) return nonArgAndGeneric.Take(1);
            }

            return candidates.ToArray();
        }

        protected override IEnumerable<ITypeDetailInfo> GetGenericStubs(IMethodDetail method)
        {
            if (method.IsGenericDefinition) return genericStubs.Concat(method.GenericArguments);

            return genericStubs;
        }


        private void FilterByGenerics()
        {
            var args = FilterByArgs(GenericPart, m => m.GenericArguments.Length);

            if (!args.Any()) return;

            // We have to make a generic method, especially in order to check for agruments which might base on the substituted

            var argTypes = args.Select(a => new PathMatcher(genericStubs).Match(a)).OfType<ITypeDetailInfo>().ToArray();
            candidates = candidates.Select(m =>
            {
                try
                {
                    return m.GetConstructedGeneric(argTypes); // For constructed generic we return constructed generic, besides that for parameter matching we have to do it
                }
                catch { return null; }
            }).OfType<IMethodDetail>();

            if (candidates.Empty()) throw new FormatException($"{genericPart} do not match valid members");
        }
    }
}
