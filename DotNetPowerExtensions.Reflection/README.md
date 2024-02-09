# DotNetPowerExtensions.Reflection

## What is the purpose of DotNetPowerExtensions.Reflection
- To add Reflection tools and shortcuts currently not available in .Net

### 1. General Reflection Extensions

General extension methods for `MemberInfo` and subclasses

#### 1.1 `FieldInfo` extensions

- `IsInternal`: Retruns whether the `FieldInfo` has the equivalent of the C# `internal` keyword (or `internal protected`)
- `IsPublicOrInternal`: Retruns whether the `FieldInfo` has the equivalent of the C# `public` keyowrd OR the `internal` keyword (or `internal protected`)

#### 1.2 `PropertyInfo` extensions

- `GetAllMethods`: Returns the `get` and `set` accessors (whichever are available) as an array, even the non public ones, equivalent to `.GetAccessors(true)`
- `IsPrivate`: Returns whether the given property is marked with the equivalent of the C# `private` keyword
- `IsAbstract`: Returns whether the given property is marked with the equivalent of the C# `abstract` keyword
- `IsOverridable`: Returns whether the given property is overridable
- `GetWritablePropertyInfo`: Returns the given `PropertyInfo` that has a set method, if the current class does not have a set method, but a base has a private setter then it will return it
- `HasGetAndSet`: Returns whether the property has both a get method and a set method, optionally taking `includeBasePrivate` to return true if the base has a private set method (note that it currently ignores base private `get` methods)
- `IsExplicitImplementation`: Returns whether the given `PropertyInfo` represents an explicit interface implmentation

#### 1.3 `EventInfo` extensions

- `GetAllMethods`: Returns the `add` and `remove` accessors as an array
- `IsPrivate`: Returns whether the given event is marked with the equivalent of the C# `private` keyword
- `IsAbstract`: Returns whether the given event is marked with the equivalent of the C# `abstract` keyword
- `IsOverridable`: Returns whether the given event is overridable
- `IsExplicitImplementation`: Returns whether the given `EventInfo` represents an explicit interface implmentation

#### 1.4 `MethodInfo` extensions

- `IsInternal`: Retruns whether the `MethodInfo` has the equivalent of the C# `internal` keyword (or `internal protected`)
- `IsPublicOrInternal`: Retruns whether the `MethodInfo` has the equivalent of the C# `public` keyowrd OR the `internal` keyword (or `internal protected`)
- `IsOverridable`: Returns whether the given method is overridable
- `IsVoid`: Returns whether the given method retruns void
- `GetDeclaringProperty`: If the given method is an accessor of a property then it will return the `PropertyInfo` for the property
- `GetDeclaringEvent`: If the given method is an accessor of an event then it will return the `EventInfo` for the event
- `IsExplicitImplementation`: Returns whether the given `MethodInfo` represents an explicit interface implmentation
- `IsEqual`: Returns whether a given `MethodInfo` is the same method in code as another `MethodInfo` instance regardless of which class the `MethodInfo` has been obtained from (unlike the built in equailty tests which only retruns true if the `MethodInfo` was obtained from the same `Type` object and ignroes the base class)
- `IsSignatureEqual`: Returns whether a given `MethodInfo` has an equal signature to another `MethodInfo` regardless if they are from the same type or not
- `GetInterfaceMethods`: Retruns all interface methods that the given `MethodInfo` implements (either explictly or implicitly)

#### 1.5 `Type` extensions
- `GetDefault`: Returns the default value for the given type
- `IsNullAllowed`: Returns whether the type allows `null`
- `HasInnerType`: Retruns whether the type has an inner type such as an array or generic type
- `GetInnerTypes`: Rertuns the inner types if there are
- `GetBaseTypes`: Gets all base types for the given `Type`, exluding the given `Type` itself
- `GetBasesAndInterfaces`: Gets all base types for the given `Type`, as well as all interfaces that the `Type` implements (directly or indirectly via a base class), exluding the given `Type` itself
- `GetAllGenericDefinitions`: Gets all open generic type definitions `Type` objects for the given `Type` and it's base types or interfaces
- `IsDelegate`: Returns whether the type represents a deleagte
- `InvokeMethod`: A convenience method to invoke a nongeneric method without having to separately obtain the method first, this only works if all arguments are non null
- `GetInterfaceMapForInterface`: Returns the `InterfaceMap` for the given `Type` and the given interface, similar to the built in `GetInterfaceMap` method but also works if the given `Type` is an interface on it's own, useful for default interface implementations

### 2. Usefull Addition
- `MethodInfoExtensions.MethodEqualityComparer`: An `IEqualityComparer` that considers two `MethodInfo` instances equal if they rerpesent the same method in code regardless of which class the `MethodInfo` has been obtained from
- `BindingFlagsExtensions.AllBindings`: A `BindingFlags` variant that represetns both `Static` and `Instance` regardless if they are `Public` or `NonPublic`

### 3. `TypeDetailInfo` instance for a `Type` via 
You can get a `TypeDetailInfo` instance for a `Type` via the `GetTypeDetailInfo()` extension method

`TypeDetailInfo` encapsulates all information on a type in an organized manner, including:
- Info about the type, such as:
	- `Name`: The actual readable name (unlike the framework which for `file class` mangles the name, or for generic)
	- `FileName`: For `file class`
	- `Namespace`: If it has one
	- `Generic info`: Such as Generic parmaeters, the original geneirc definition if there is one
	- `BaseType`: The immediate base type if there is one
	- `Interfaces`: All implemented interfaces
- All Constructors
- All non shadowed properties/methods/events/fields (unlike the framework whcih deosn't distinguish between shadwoed and non shadowed methods)
- All shadowed properties/methods/events/fields (unlike the frameowrk which returns only methods)
- All base private properties/methods/events/fields
- All explicit implementations of properties/methods/events, including default interface implementaions

For each member we show the following
   - `Name`: The actual readable name (unlike the framework which for explicit interfaces shows a mangled name)
   - `ReflectionInfo`: The framwork `MemberInfo`/`FieldInfo`/`PropertyInfo`/`EventInfo`/`MethodInfo`/`ConsttructorInfo` for the member
   - `IsExplicit`: If this is an explicit interface implementation
   - `ExplicitInterface`: Contains a reference to the implmented interface `Type` object if it is an explicit implementation, null otheriwse
   - `ExplicitDetail`: A `FieldDetail`/`PropertyDetail`/`EventDetail`/`MethodDetail` for the original explicit member (if this is an explicit implmenetation)
   - `InReflectionForCurrentType`: If you can find it via refelection on the `Type` object for the current subclass
   - `IsInherited`: If the member was declared in the base class and not overriden in the current class
   - `DeclarationType`: Returns whether this an original decleration, an override, a shadow, or an override of a shadow
   
 For any Property/Method/Event that is overriden we also include a reference to the original membe in the `OverridenProperty`/`OverridenMethod`/`OverridenEvent` respectfully
 
 Also for Properties we include any base private get or set methods that are not available in the current subclass
