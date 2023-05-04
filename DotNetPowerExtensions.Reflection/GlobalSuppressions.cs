// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Performance", "CA1825:Avoid zero-length array allocations", Justification = "Targeting also net 45")]
[assembly: SuppressMessage("Performance", "CA1845:Use span-based 'string.Concat'", Justification = "Targeting also net 45")]
