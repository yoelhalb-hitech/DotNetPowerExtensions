using SequelPay.DotNetPowerExtensions;

namespace SequelPay.DotNetPowerExtensions.Analyzers.MustInitialize.MightRequireAttribute;

internal class MightRequireUtils
{

    public static Type[] Attributes =
    {
        typeof(SequelPay.DotNetPowerExtensions.MightRequireAttribute),
        typeof(SequelPay.DotNetPowerExtensions.MightRequireAttribute<>),
    };

    public static string[] Names => new[]
    {
        nameof(SequelPay.DotNetPowerExtensions.MightRequireAttribute),
        nameof(SequelPay.DotNetPowerExtensions.MightRequireAttribute).Replace(nameof(Attribute), "")
    };

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class MightRequiredInfo
    {
        public ISymbol ContainingSymbol { get; set; }
        public AttributeData Attribute { get; set; }
        public string Name { get; set; }
        public ITypeSymbol Type { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public static IEnumerable<MightRequiredInfo> GetMightRequiredInfos(ITypeSymbol symbol, INamedTypeSymbol[] mightRequireSymbols)
    {
        var symbols = new[] { symbol }.Concat(symbol.GetAllBaseTypes()).Concat(symbol.AllInterfaces).ToArray();
        var attributes = symbols.SelectMany(s => s.GetAttributes()
                                                    .Where(at => mightRequireSymbols.ContainsGeneric(at.AttributeClass))
                                                    .Where(at => !at.ConstructorArguments.First().IsNull
                                                            && (at.AttributeClass!.IsGenericType || !at.ConstructorArguments.Last().IsNull))
                                                    .Select(at => new MightRequiredInfo
                                                    {
                                                        ContainingSymbol = s,
                                                        Attribute = at,
                                                        Name = at.ConstructorArguments.First().Value!.ToString(),
                                                        Type = at.AttributeClass!.IsGenericType ?
                                                                    (ITypeSymbol)at.AttributeClass.TypeArguments.First()! :
                                                                    (ITypeSymbol)at.ConstructorArguments.Last().Value!
                                                    })).ToArray();
        return attributes;
    }
}
