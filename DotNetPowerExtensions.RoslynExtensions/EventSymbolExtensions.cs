using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.ComponentModel;
using System.Security.AccessControl;

namespace SequelPay.DotNetPowerExtensions.RoslynExtensions;

public static class EventSymbolExtensions
{
    /// <summary>
    /// Returns the entire event override chain for an event
    /// </summary>
    /// <param name="evt">The event symbol for which we want to get the chain</param>
    /// <returns>A list of property symbols with the property closer to the passed property being returned first</returns>
    public static IEnumerable<IEventSymbol> GetEventOverrideChain(this IEventSymbol evt)
    {
        var e = evt;
        while (e.IsOverride && e.OverriddenEvent is not null)
        {
            e = e.OverriddenEvent;
            yield return e;
        }

        yield break;
    }

    /// <summary>
    /// Get the original event base decleration for a event (the one that does not have the <see langword="override" /> keyword)
    /// </summary>
    /// <param name="evt">The event symbol for which we want to get the base</param>
    /// <returns>The base event</returns>
    public static IEventSymbol GetBaseEvent(this IEventSymbol evt)
        => evt.GetEventOverrideChain().LastOrDefault() ?? evt;
}
