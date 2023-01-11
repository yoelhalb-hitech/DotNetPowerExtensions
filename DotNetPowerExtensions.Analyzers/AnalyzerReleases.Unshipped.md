; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
DNPE0101 | Language | Warning | MustInitializeNotSupportedOnReadonly
DNPE0102 | Language | Warning | MustIinitializeAccessibilityNotLessThanConstructor
DNPE0103 | Language | Warning | MustIinitializeRequiredMemebers
DNPE0104 | Language | Warning | MustInitializeRequiredWhenImplementingInterface
DNPE0105 | Language | Warning | CannotUseBaseImplementationForMustInitialize
DNPE0106 | Language | Warning | MustInitializeNotSupportedOnStatic
DNPE0107 | Language | Warning | ExplicitImplementationNotAllowed
DNPE0108 | Language | Warning | MustInitializeNotAllowedOnExplicitImplementation
DNPE0109 | Language | Warning | MustInitializeNotAllowedOnDefaultInterfaceImplementation
DNPE0110 | Language | Warning | MustInitializeRequiredWhenOverriding
DNPE0111 | Language | Warning | DisallowHidingMustInitialize
DNPE0201 | Language | Warning | MustIinitializeRequiredMembersForLocalService
DNPE0202 | Language | Warning | OnlyAnonymousForRequiredMembersForLocalService
DNPE0203 | Language | Warning | TypeMismatchForLocalService
DNPE0204 | Language | Warning | UseLocalServiceForLocal
DNPE0205 | Language | Warning | MustInitializeShouldBeLocal
DNPE0301 | Language | Warning | NonDelegateShouldNotBeAssigned
DNPE0401 | Language | Warning | AsShouldBeAssignableType
