using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using SequelPay.DotNetPowerExtensions.Reflection.Core.Models;
using SequelPay.DotNetPowerExtensions.Reflection.Core.Paths.Maskers;

namespace SequelPay.DotNetPowerExtensions.Reflection.Core.Paths;

internal partial class Outer<TTypeContainerCache>
{
    internal class MemberPathMatcher
    {
        internal static ConcurrentDictionary<ITypeDetailInfo, MemberPathMatcher> memberPathDict = new();
        internal static MemberPathMatcher GetMemberPathMatcher(ITypeDetailInfo type)
                => memberPathDict.GetOrAdd(type, t => new MemberPathMatcher(t));
        public MemberPathMatcher(ITypeDetailInfo type)
        {
            Type = type;
            var typeInfo = type;

            typeInfo.PropertyDetails.ToList().ForEach(pd => GetOrAdd(pd.Name).Add((type, pd)));
            typeInfo.EventDetails.ToList().ForEach(ed => GetOrAdd(ed.Name).Add((type, ed)));

            typeInfo.FieldDetails.ToList().ForEach(fd => GetOrAdd(fd.Name).Add((type, fd)));
            typeInfo.MethodDetails.ToList().ForEach(md => GetOrAdd(md.Name).Add((type, md)));

            typeInfo.ExplicitPropertyDetails.ToList().ForEach(pd => GetOrAdd(pd.Name).Add((pd.ExplicitDetail!.CurrentType, pd)));
            typeInfo.ExplicitEventDetails.ToList().ForEach(ed => GetOrAdd(ed.Name).Add((ed.ExplicitDetail!.CurrentType, ed)));
            typeInfo.ExplicitMethodDetails.ToList().ForEach(md => GetOrAdd(md.Name).Add((md.ExplicitDetail!.CurrentType, md)));

            if (type.IsGenericDefinition) genericStubs = type.GenericArguments;
        }

        private List<(ITypeDetailInfo, IMemberDetail)> GetOrAdd(string name)
        {
            if (!memberDict.ContainsKey(name)) memberDict[name] = new List<(ITypeDetailInfo, IMemberDetail)>();

            return memberDict[name];
        }

        public ITypeDetailInfo Type { get; }
        private Dictionary<string, List<(ITypeDetailInfo, IMemberDetail)>> memberDict = new();
        private ITypeDetailInfo[] genericStubs = { };

        const string MemberPrefix = "."; 
        const string ArgPrefix = "->";
        const string ExplicitDelimiter = ":";
        const string GenericCountDelimiter = "`";
        public string GetFullPath(IMethodDetail method) => GetFullPathInternal(method);
        public string GetFullPathInternal(IMethodDetail method)
        {
            var name = GetFullPathInternal((IMemberDetail<IMethodDetail>)method);

            name += !method.IsGeneric ? ""
                    : method.IsGenericDefinition ? GenericCountDelimiter + method.GenericArguments.Length
                        : $"<{method.GenericArguments.Select(a => new PathMatcher(genericStubs).GetFullPath(a)).Join(",")}>";

            var filtered = memberDict[method.Name].Where(m => m.Item1 == (method.ExplicitDetail?.CurrentType)
                                  && (m.Item2 as IMethodDetail)?.GenericArguments.Length == method.GenericArguments.Length);
            // If we are dealing with generic constructed we need the "(" as otherwise we might have issues parsing the recursive generic
            if (filtered.HasOnlyOne() && (!method.GenericArguments.Any() || method.IsGenericDefinition))
                return name;

            filtered = filtered.Where(m => (m.Item2 as IMethodDetail)?.Parameters.Length == method.Parameters.Length);
            if (filtered.HasOnlyOne())
                return name + $"(`{method.Parameters.Length})";

            var stubs = genericStubs;
            if (method.IsGenericDefinition) stubs = stubs.Concat(method.GenericArguments).ToArray();

            return name + "(" + method.Parameters
                               .Select(p => p.ParameterModifierType switch
                               {
                                   ParameterModifierTypes.None => "",
                                   ParameterModifierTypes.In => "in-",
                                   ParameterModifierTypes.Out => "out-",
                                   ParameterModifierTypes.Ref => "ref",
                                   _ => throw new ArgumentOutOfRangeException(p.ParameterModifierType.ToString()),
                               } + new PathMatcher(stubs).GetFullPath(p.ParameterType))
                    .Join(",") + ")";
        }

        public string GetMinimalPath(IMethodDetail method) => GetMinimalPathInternal(method);

        public string GetMinimalPathInternal(IMethodDetail method)
        {
            var name = GetMinimalPathInternal((IMemberDetail<IMethodDetail>)method);

            if (method.IsConstructedGeneric)
                name +=
                    $"<{method.GenericArguments.Select(a => new PathMatcher(genericStubs).GetFullPath(a)).Join(",")}>";

            if (memberDict[method.Name].Count == 1) return name;

            var filtered = memberDict[method.Name].Where(m => m.Item1 == (method.ExplicitDetail?.CurrentType ?? Type))
                                                    .Select(m => m.Item2)
                                                    .OfType<IMethodDetail>();
            if (filtered.HasOnlyOne())
                return name;

            // Not doing this because when we are dealing with generic constructed we need the "(" as otherwise we might have issues parsing the recursive generic
            //if(method.GenericArguments.Any() && !method.IsGenericDefinition) // We anyway append the generic part so let's use it
            //{
            //    filtered = filtered.Where(m => (m.Item2 as IMethodDetail)?.GenericArguments.Length == method.GenericArguments.Length );
            //    if (!filtered.Skip(1).Any())
            //        return name;
            //}

            // If the arguments are different we can ignore the generic part (if not constructed) since we are minimal
            if (filtered.Where(m => m.Parameters.Length == method.Parameters.Length).HasOnlyOne())
                return name + (method.Parameters.Any() ? $"(`{method.Parameters.Length})" : "()");

            if (method.IsGenericDefinition) name += GenericCountDelimiter + method.GenericArguments.Length;
            // If we are dealing with generic constructed we need the "(" as otherwise we might have issues parsing the recursive generic
            filtered = filtered.Where(m => m.GenericArguments.Length == method.GenericArguments.Length);
            if (!filtered.Skip(1).Any() && (!method.GenericArguments.Any() || method.IsGenericDefinition))
                return name;

            filtered = filtered.Where(m => m.Parameters.Length == method.Parameters.Length);
            if (filtered.HasOnlyOne())
                return name + (method.Parameters.Any() ? $"(`{method.Parameters.Length})" : "()");

            var stubs = genericStubs;
            if (method.IsGenericDefinition) stubs = stubs.Concat(method.GenericArguments).ToArray();

            return name + "(" + method.Parameters
                .Select(p => GetParamaterPrefix(p) + new PathMatcher(stubs).GetMinimalPath(p.ParameterType))
                .Join(",") + ")";
        }

        private static string GetParamaterPrefix(IParameterDetail parameter)
            => parameter.ParameterModifierType switch
            {
                ParameterModifierTypes.None => "",
                ParameterModifierTypes.In => "in-",
                ParameterModifierTypes.Out => "out-",
                ParameterModifierTypes.Ref => "ref",
                _ => throw new ArgumentOutOfRangeException(parameter.ParameterModifierType.ToString()),
            };

        public string GetFullPath<TDetail>(IMemberDetail<TDetail> member) where TDetail : IMemberDetail<TDetail>
        {
            if (member is IMethodDetail method) return GetFullPath(method);

            return GetFullPathInternal(member);
        }
        private string GetFullPathInternal<TDetail>(IMemberDetail<TDetail> member) where TDetail : IMemberDetail<TDetail>
        {
            if (!member.IsExplicit) return MemberPrefix + member.Name;

            return MemberPrefix + ExplicitDelimiter + new PathMatcher(genericStubs).GetFullPath(member.ExplicitDetail!.CurrentType) + ExplicitDelimiter + member.Name;
        }

        public string GetMinimalPath<TDetail>(IMemberDetail<TDetail> member) where TDetail : IMemberDetail<TDetail>
        {
            if(member is IMethodDetail method) return GetMinimalPathInternal(method);

            return GetMinimalPathInternal(member);
        }

        private string GetMinimalPathInternal<TDetail>(IMemberDetail<TDetail> member) where TDetail : IMemberDetail<TDetail>
        {
            if (memberDict[member.Name].Count == 1 || !member.IsExplicit) return MemberPrefix + member.Name;

            return MemberPrefix + ExplicitDelimiter + new PathMatcher(genericStubs).GetMinimalPath(member.ExplicitDetail!.CurrentType) + ExplicitDelimiter + member.Name;
        }

        public IEnumerable<Union<IMemberDetail, IParameterDetail>> ParsePath(string path)
        {
            if (!path.HasValue()) throw new ArgumentNullException(nameof(path));


            var masker = new MultiMasker(new IMasker[] { new GenericMasker(), new ExplicitMasker(), new ParenthesisMasker() });

            var masked = masker.Mask(path);


            // Since we masked out all other possible '.' we can split directly on it
            var splitted = masked.Split(new[] { MemberPrefix }, StringSplitOptions.RemoveEmptyEntries); // Remove empty entries because if started with the . there will be an empty entry in the beginning
            return ParsePath(splitted.Select(s => masker.Unmask(s)));
        }

        public IEnumerable<Union<IMemberDetail, IParameterDetail>> ParsePath(IEnumerable<string> splittedPaths)
        {
            if (!splittedPaths.Any()) return new Union<IMemberDetail, IParameterDetail>[0];

            string[] splitByArgs = splittedPaths.First().Split(new[] { ArgPrefix }, StringSplitOptions.None); // We need the emtpy entries as it represents the name which is empty for the ctor
            var result = ParseMember(splitByArgs.First(), splitByArgs.Skip(1).FirstOrDefault());

            var nextType = result.Last().Second?.ParameterType ?? result.Last().First switch
            {
                IFieldDetail f => f.FieldType,
                IPropertyDetail p => p.PropertyType,
                IEventDetail e => e.EventHandlerType,
                IMethodDetail m => m.ReturnType,
                IConstructorDetail => Type,
                _ => throw new ArgumentOutOfRangeException(result.Last().First?.ToString()),
            };
            var pathMatcher = GetMemberPathMatcher(nextType);

            var nextArgs = splitByArgs.Skip(2).Concat(splittedPaths.Skip(1));
            return result.Concat(pathMatcher.ParsePath(nextArgs));
        }

        public IEnumerable<Union<IMemberDetail, IParameterDetail>> ParseMember(string name, string? arg)
        {
            var candidates = GetByName(name);

            if (candidates.Empty()) throw new FormatException($"{name} does not transalate to a valid member, ensure that name is correct and the explicit type string is valid and enclosed in `:`");

            if (!arg.HasValue())
            {
                if (!candidates.HasOnlyOne()) throw new AmbiguousMatchException(name);
                return new Union<IMemberDetail, IParameterDetail>[] { new(candidates.First()) };
            }

            // If multiple and there is an arg then try to match the arg
            var methods = candidates.OfType<IConstructorDetail>().Select(m => new Union<IConstructorDetail, IMethodDetail>(m))
                                    .Concat(candidates.OfType<IMethodDetail>().Select(m => new Union<IConstructorDetail, IMethodDetail>(m)));
            if (!methods.Any()) throw new InvalidOperationException($"argument `{arg}` expected on `{name}` but `{name}` is not a method");

            methods = methods.Where(m => (m.First?.Parameters ?? m.Second!.Parameters)
                                    .Any(p => GetParamaterPrefix(p) + p.Name == arg));

            if (methods.Empty()) throw new InvalidOperationException($"argument `{arg}` expected on `{name}` was not found");
            if (!methods.HasOnlyOne()) throw new AmbiguousMatchException(name);

            var match = methods.First();
            var matchParameters = match.First?.Parameters ?? match.Second!.Parameters;
            return new Union<IMemberDetail, IParameterDetail>[]
            {
                new (match.As<IMemberDetail>()!),
                new (matchParameters.First(p => GetParamaterPrefix(p) + p.Name == arg))
            };
        }

        private IEnumerable<IMemberDetail> HandleCtor(string name)
        {
            var ctors = Type.ConstructorDetails;
            if (ctors.Length == 1) return ctors;

            return new ConstrcutorMatcher(name, ctors, genericStubs).GetCandidates();
        }

        private IEnumerable<IMemberDetail> GetByName(string name)
        {
            var namePart = name;
            // Cannot use SubstringFrom(MemberPrefix) as it might not have in the beginning but in the middle...
            if (namePart.StartsWith(MemberPrefix)) namePart = namePart.Substring(1, namePart.Length - 1);            

            namePart = namePart.SubstringUntil(GenericCountDelimiter)!.SubstringUntil("<")!.SubstringUntil("(")!;
            namePart = namePart.SubstringFrom(ExplicitDelimiter);

            if (!namePart.HasValue()) return HandleCtor(name);

            var line = memberDict[namePart];
            if (line.HasOnlyOne()) return new[] { line.First().Item2 };

            // If there is more than one we always have to specify explicit if it is there
            var explicitTypes = GetExplicitTypes(name);
            
            var filtered = line.Where(l => explicitTypes.Contains(l.Item1)).Select(l => l.Item2);

            if (filtered.Empty()) throw new FormatException($"{namePart} is not a valid member, ensure that name is correct and the explicit type string is valid and enclosed in `:`");
            else if (filtered.HasOnlyOne()) return filtered.Take(1);

            return new MethodMatcher(name, filtered.OfType<IMethodDetail>(), genericStubs).GetCandidates();
        }

        private ITypeDetailInfo[] GetExplicitTypes(string name)
        {
            var explicitStr = name;
            // Cannot use SubstringFrom(MemberPrefix) as it might not have in the beginning but in the middle...
            if (explicitStr.StartsWith(MemberPrefix)) explicitStr = explicitStr.Substring(1, explicitStr.Length - 1);

            if (!explicitStr.StartsWith(ExplicitDelimiter)) explicitStr = "";

            explicitStr = explicitStr.SubstringFrom(ExplicitDelimiter, true, false, true)?.SubstringUntil(ExplicitDelimiter); // Assuming it is well formed

            if (!explicitStr.HasValue()) return new[] { Type };

            var matches = new PathProcessor(genericStubs).GetPathContainers(explicitStr); // Allowing multiple if narrowed down later
            if (matches.Empty()) throw new FormatException($"{explicitStr} does not transalate to a valid type");

            // We need the actual consturcted type, as we might have implemented the same interface with multiple args
            return matches.Select(m => m.ConstructedType ?? m.UnderlyingType).OfType<ITypeDetailInfo>().ToArray();
        }
    }
}
