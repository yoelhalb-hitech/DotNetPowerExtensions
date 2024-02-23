// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Test Method Style")]
[assembly: SuppressMessage("Design", "CA1812:Class is never instantiated", Justification = "Needed for testing")]
[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Not for public consumption")]
[assembly: SuppressMessage("Design", "CA1852:Class can be sealed", Justification = "Needed for testing")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Needed for testing")]
[assembly: SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Needed for testing")]
[assembly: SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Needed for testing")]
[assembly: SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible", Justification = "Needed for testing")]
[assembly: SuppressMessage("Globalization", "CA1307:Specify StringComparison for clarity", Justification = ".Net Framework doesn't support it")]
