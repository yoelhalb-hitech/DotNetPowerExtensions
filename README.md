

<!-- PROJECT LOGO -->
<br />
<div align="center">
  <h1 align="center">Dot Net Power Extensions</h1>

  <p align="center">
    A set of extensions classes, helpers and methods, that extend and simplify the capabilities of the .Net framework.
  </p>
</div>

## Main Packages

As of version 5.0. the set of packages produced by this repository have been consolidated again. The following table summarizes this information:

| NuGet Package Name | Version | Summary |
|----------|:-------:|---------|
| SequelPay.DotNetPowerExtensions      | [![NuGet Version](https://img.shields.io/nuget/v/SequelPay.DotNetPowerExtensions)](https://www.nuget.org/packages/SequelPay.DotNetPowerExtensions) | Primary package for this repo, comprised of all smaller packages. Includs basic additions and extensions to the core .Net frameworks. [read more](DotNetPowerExtensions/README.md). |
| SequelPay.DotNetPowerExtensions.Reflection  | [![NuGet](https://img.shields.io/nuget/v/SequelPay.DotNetPowerExtensions.Reflection)](https://www.nuget.org/packages/SequelPay.DotNetPowerExtensions.Reflection) | Extensions and helpers for .Net Reflection, as well as the `TypeDetailInfo` system which simplifies many aspects of reflection. [Read more](DotNetPowerExtensions.Reflection/README.md). |
| SequelPay.DotNetPowerExtensions.RoslynExtensions         | [![NuGet](https://img.shields.io/nuget/v/SequelPay.DotNetPowerExtensions.RoslynExtensions)](https://www.nuget.org/packages/SequelPay.DotNetPowerExtensions.RoslynExtensions) | Extensions and helpers for the .Net Roslyn framework. |

## Individual Packages

All of these packages areautomatically installed with the main `SequelPay.DotNetPowerExtensions` package.

They can however be installed individually instead.

| NuGet Package Name | Version | Summary |
|----------|:-------:|---------|
| SequelPay.DotNetPowerExtensions.EnumerableExtensions      | [![NuGet Version](https://img.shields.io/nuget/v/SequelPay.DotNetPowerExtensions.EnumerableExtensions)](https://www.nuget.org/packages/SequelPay.DotNetPowerExtensions.EnumerableExtensions) | Helper extension methods for `IEnumerable`. |
| SequelPay.DotNetPowerExtensions.StringExtensions      | [![NuGet Version](https://img.shields.io/nuget/v/SequelPay.DotNetPowerExtensions.StringExtensions)](https://www.nuget.org/packages/SequelPay.DotNetPowerExtensions.StringExtensions) | Helper methods for C# strings. |
| SequelPay.DotNetPowerExtensions.Union         | [![NuGet](https://img.shields.io/nuget/v/SequelPay.DotNetPowerExtensions.Union)](https://www.nuget.org/packages/SequelPay.DotNetPowerExtensions.Union) | A Union type for .Net. [see features, demos and examples](DotNetPowerExtensions/README.md#1-union-classes). |
| SequelPay.DotNetPowerExtensions.NonDelegate         | [![NuGet](https://img.shields.io/nuget/v/SequelPay.DotNetPowerExtensions.NonDelegate)](https://www.nuget.org/packages/SequelPay.DotNetPowerExtensions.NonDelegate) | Attribute and analyzer to prevent assigning a method to a delegate/lambda (as well as passing it to a method). [see features, demos and examples](DotNetPowerExtensions/README.md#2-nondelegate).  |
| SequelPay.DotNetPowerExtensions.DependencyInjection         | [![NuGet](https://img.shields.io/nuget/v/SequelPay.DotNetPowerExtensions.DependencyInjection)](https://www.nuget.org/packages/SequelPay.DotNetPowerExtensions.DependencyInjection) | An attribute system with analyzers for DI use, so that the service class can be declared for DI with Attributes. <br /><br />Also adding the concept of `Local` services (which can be helpful for one time services or for services requiring intialization). <br /><br />[see features, demos and examples](DotNetPowerExtensions/README.md#3-dependency-attributes).   |
| SequelPay.DotNetPowerExtensions.MustInitialize         | [![NuGet](https://img.shields.io/nuget/v/SequelPay.DotNetPowerExtensions.MustInitialize)](https://www.nuget.org/packages/SequelPay.DotNetPowerExtensions.MustInitialize) | Attributes and analyzers to require intialization of a property field, this is similar to the modern C# `required` but more sophisticated and more flexible. <br /><br />(For example you can: <br />- Initialize it in a sub class and remove the requirment via `Initialized` <br />- suppress it which is useful for test code<br />- Use in DI with the DependencyInjection package `Local` system). <br /><br />[see features, demos and examples](DotNetPowerExtensions/README.md#4-mustinitialize).   |

## Common Packages

There are some common packages which should not be used directly by user code as they are just to assist the installable packages.
