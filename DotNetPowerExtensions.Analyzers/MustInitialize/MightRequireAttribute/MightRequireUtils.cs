using SequelPay.DotNetPowerExtensions;

namespace DotNetPowerExtensions.Analyzers.MustInitialize.MightRequireAttribute;

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

    public class MightRequiredInfo
    {
        public required ISymbol ContainingSymbol { get; set; }
        public required AttributeData Attribute { get; set; }
        public required string Name { get; set; }
        public required ITypeSymbol Type { get; set; }
    }

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
