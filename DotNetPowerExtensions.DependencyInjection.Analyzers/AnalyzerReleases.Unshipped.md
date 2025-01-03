; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
DNPE0202 | Language | Warning | OnlyAnonymousForILocalFactory
DNPE0203 | Language | Warning | TypeMismatchForILocalFactory
DNPE0204 | Language | Warning | UseLocalServiceForLocal
DNPE0206 | Language | Warning | UseShouldOnlyBeForGeneric
DNPE0207 | Language | Warning | UseShouldOnlyBeCurrent
DNPE0208 | Language | Warning | GenericRequiresUse
DNPE0209 | Language | Warning | DependencyShouldNotBeAbstract
DNPE0210 | Language | Warning | ForTypeMustBeParent
DNPE0211 | Language | Warning | DependencyRequiredWhenBase
DNPE0212 | Language | Warning | DependencyTypeNotOnBase
DNPE0217 | Language | Warning | OriginalNotExisting
DNPE0219 | Language | Warning | LocalServiceIsNotForLocal
DNPE0220 | Language | Warning | UseLocalServiceOnlyInDependency
DNPE0221 | Language | Warning | UseServiceOnlyInDependency
DNPE0222 | Language | Warning | OnlyUseServiceInDependency
DNPE0223 | Language | Warning | UseTransientOnlyInTransient
DNPE0224 | Language | Warning | NoScopedInSingleton