using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.ComponentModel;
using System.Security.AccessControl;

namespace SequelPay.DotNetPowerExtensions.RoslynExtensions;

public static class PropertySymbolExtensions
{
    /// <summary>
    /// Returns the entire property override chain for a property
    /// </summary>
    /// <param name="property">The property symbol for which we want to get the chain</param>
    /// <returns>A list of property symbols with the property closer to the passed property being returned first</returns>
    public static IEnumerable<IPropertySymbol> GetPropertyOverrideChain(this IPropertySymbol property)
    {
        var prop = property;
        while(prop.IsOverride && prop.OverriddenProperty is not null)
        {
            prop = prop.OverriddenProperty;
            yield return prop;
        }

        yield break;
    }

    /// <summary>
    /// Get the original property base decleration for a property (the one that does not have the <see langword="override" /> keyword)
    /// </summary>
    /// <param name="property">The property symbol for which we want to get the base</param>
    /// <returns>The base property</returns>
    public static IPropertySymbol GetBaseProperty(this IPropertySymbol property)
        => property.GetPropertyOverrideChain().LastOrDefault() ?? property;
}
