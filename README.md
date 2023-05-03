# DotNetPowerExtensions And Analyzer

## What is the purpose of DotNetPowerExtensions and Analyzer?
- To add functionality and diagnostics currently not available in .Net

### 1. Union<> Classes
#### Motivation
Sometimes we want to return a one of two possible values from a method, and of course it is not typesafe to return `object`, while returning a Tuple or other solutions might be too cumbersome

It is therefore useful to have a low ceremony typesafe struct Union<> taking 2 or 3 type arguments

##### Example Code
    
    using DotNetPowerExtensions;

    public interface IReturnType { string Name { get; set; }}
    public class ReturnType1 : IReturnType { public string Name { get; set; }}
    public class ReturnType2 : IReturnType { public string Name { get; set; }}

    public Union<ReturnType1, RertunType2> TestMethod()
                => new Union<ReturnType1, RertunType2>(new ReturnType1 { Name = "Test" });

    var retValue = TestMethod();
    var n1 = retValue.First?.Name ?? retValue.Second!.Name; // n1 == "Test"
    var n2 = retValue.As<IReturnType>().Name; // n2 == "Test"

### 2. NonDelegate
Decorate a method with `[NonDelegate]` in order to prevent it from being used as a callback and/or saved in a variable/property/argument.
This is useful for Analyzers that rely on compile time static analysis (such as the `ILocalFactory.Get()` method, see below).

### 3. Dependency Attributes
You can now decorate your DI services with the an attribtue describing the service type, and insert all such classes in the DI at once,

##### Example Code
    
    // [Singleton] // For a Singleton service
    // [Scoped] // For a Scoped service
    [Transient] // For a transient service
    public class TestClass{}
    
And in your DI setup code, just have the following (where `services` is an `IServiceCollection` instance):

    services.AddDependencies(); // That's it

#### 3.1 For Base or Interface
If you want to register for a base or interface then just specify the desired types as parameters for the ctor (or the generic type in C# 11).

##### Example Code
    
    public interface ITestClass {}

    // Declaring service
    // [Singleton(typeof(ITestClass))] // Or in C# 11 [Singleton<ITestClass>] // For a Singleton service
    // [Scoped(typeof(ITestClass))] // Or in C# 11 [Scoped<ITestClass>] // For a Scoped service
    // [Local(typeof(ITestClass))] // Or in C# 11 [Local<ITestClass>] // For a Local service, see below
    [Transient(typeof(ITestClass))] // Or in C# 11 [Transient<ITestClass>] // For a transient service
    public class TestClass : ITestClass {}

    // Using service
    // [Singleton] // For a Singleton service
    // [Scoped] // For a Scoped service
    // [Local] // For a Local service, see below
    [Transient] // For a transient service
    public class TestUserClass
    {
        public TestUserClass(ITestClass testClass) {}
    }

#### 3.2 Generic Classes
You can register a closed generic version for an open generic by using the `Use` property

##### Example Code
    
    public interface ITestClass {}

    // Declaring service
    // [Singleton(typeof(ITestClass), Use=typeof(TestClass<string>))] // Or in C# 11 [Singleton<ITestClass>(Use=typeof(TestClass<string>))] // For a Singleton service
    // [Scoped(typeof(ITestClass), Use=typeof(TestClass<string>))] // Or in C# 11 [Scoped<ITestClass>(Use=typeof(TestClass<string>)] // For a Scoped service
    // [Local(typeof(ITestClass), Use=typeof(TestClass<string>))] // Or in C# 11 [Local<ITestClass>(Use=typeof(TestClass<string>)] // For a Local service, see below
    [Transient(typeof(ITestClass), Use=typeof(TestClass<string>))] // Or in C# 11 [Transient<ITestClass>(Use=typeof(TestClass<string>)] // For a transient service
    public class TestClass<T> : ITestClass {}

    // Using service
    // [Singleton] // For a Singleton service
    // [Scoped] // For a Scoped service
    // [Local] // For a Local service, see below
    [Transient] // For a transient service
    public class TestUserClass
    {
        public TestUserClass(ITestClass testClass) 
        {
            Assert.Equals(testClass.GetType(), typeof(TestClass<string>));
        }
    }

#### 3.3 ILocalFactory
Many times we just want an object to be local to a specific function instead of having an object for the entire lifetime of the object.

We can use for that `ILocalFactory<>` which is like a factory class and decorate the service with `Local`.

##### Example Code    
    
    // Declaring service
    [Local]
    public class TestClass : IDisposable { public void Dispose(){} }

    // Using service
    // [Singleton] // For a Singleton service
    // [Scoped] // For a Scoped service
    // [Local] // For a Local service
    [Transient] // For a transient service
    public class TestUserClass
    {
        private ILocalFactory<TestClass> testClassFactory;
        public TestUserClass(ILocalFactory<TestClass> testClassFactory)
        {
            this.testClassFactory = testClassFactory;
        }

        public void SomeMethod()
        {
            using var testClass = testClassFactory.Get();
            // Do something
        }
    }

#### 3.4 Base Attributes to Force Subclass Implementation

Sometimes we want all subclasses to be required to register their implementations for the base interface/class.
In this case we can decorate the base/interface with one of the Base attributes (see example), which will then warn for any subclass that doesn't register.

##### Example Code
    
    // Declaring service base/interface, the following interfaces will require the implementors to register for the base/interface
    // [SingletonBase]
    // [ScopedBase]
    // [LocalBase]
    [TransientBase]
    public interface ITestClass {}

    // Declaring service
    // [Singleton(typeof(ITestClass))] // Or in C# 11 [Singleton<ITestClass>] // For a Singleton service
    // [Scoped(typeof(ITestClass))] // Or in C# 11 [Scoped<ITestClass>] // For a Scoped service
    // [Local(typeof(ITestClass))] // Or in C# 11 [Local<ITestClass>] // For a Local service, see below
    [Transient(typeof(ITestClass))] // Or in C# 11 [Transient<ITestClass>] // For a transient service
    public class TestClass : ITestClass {}

### 5. Requires subclasses to register
To require that all subclasses (or interface implmentations) register for the current class/interface we can use one of the `Base` attributes.

##### Example Code

    // Base/interface
    // [SingletonBase] // For a Singleton service
    // [ScopedBase] // For a Scoped service
    // [LocalBase] // For a Local service, see below
    [TransientBase] // For a transient service
    public interface ITestClass {}

    // Implementing service
    // [Singleton(typeof(ITestClass))] // Or in C# 11 [Singleton<ITestClass>] // For a Singleton service
    // [Scoped(typeof(ITestClass))] // Or in C# 11 [Scoped<ITestClass>] // For a Scoped service
    // [Local(typeof(ITestClass))] // Or in C# 11 [Local<ITestClass>] // For a Local service, see below
    [Transient(typeof(ITestClass))] // Or in C# 11 [Transient<ITestClass>] // For a transient service
    public class TestClass : ITestClass {}

    // Using service
    // [Singleton] // For a Singleton service
    // [Scoped] // For a Scoped service
    // [Local] // For a Local service, see above
    [Transient] // For a transient service
    public class TestUserClass
    {
        public TestUserClass(ITestClass testClass) {}
    }

### 4. MustInitialize

- Allows you enforce that the given property or field has to be initialized when instantiated.
- Removes the need to set a value when in a nullable context (in C# 8 and upwards) for such a property or field (NOTE: This only works in projects compatible with .Net Standard 2.0, as otherwise the functionality isn't available in Roslyn)
- Also adds the ability to have DI services that the caller has to initialize before usage via `ILocalFactory<>`
- We can also specify on a subclass override `Initialized` to indicate that the property has been initalized already and the caller doesn't have to do it anymore
- Note that for structs created via `default` it will not currently enforce (neither does the C#11 compiler do on the `required` keyword)
- If a ctor initializes some members then the ctor can be decorated with the `Initializes` attribute, or `InitializesAllRequired` if it initializes everything
    - Note that the analyzers currently don't verify that the members has actually been initialized (or that they even exists), it is as of now the responsibility of the caller

##### Update for C#11
As C# 11 introduced the `required` keyword which has even more features than we have currently (but we hope to add and way more) then in general you should the new keyword instead.

However MustInitalize stil has a use even in C#11 in the following situations:
- When you want to be able to suppress it (as in C#11 it is a compile error not a warning)
- When you want to be able to use in generic class with the `new()` constraint (which isn't allowed in C#)
- When `set`/`init` is less visible than the class but not less than the constructors
- We can initialize in a DI service by using a `LocalService<>` and passing an anonymous object with the required properties
- If a subclass overrides the property and intializes it we can use the `Initialized` attribute on it to indicate that the caller is no longer required to initalize it

##### Example Code

    public class TestClass
    {
        [MustInitialize] public string TestProperty { get; set; } 
    }
    var testObj = new TestClass();

Without MustInitialize the following error will be reported on line 3:

    warning CS8618: Non-nullable field 'TestProperty' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.

However with MustInitialize it will report the following on line 5:

    warning DNPE0103: Property 'TestProperty' must be initialized

##### Example of the Initialized attribute

    public class TestBaseClass
    {
        [MustInitialize] public virtual string TestProperty { get; set; } 
    }
    public class TestSubClass : TestBaseClass
    {
        [Initialized] public virtual string TestProperty { get; set; } = "Initial value";
    }
    var willRequire = new TestBaseClass(); // Will warn that TestProperty has to be intialized
    var willNotRequire = new TestSubClass(); // Will not warn


#### 4.1 DI
If the class containing the property/field decorated with MustInitialized is a service (i.e. it has one of the `Singleton`/`Scoped`/`Transient` attributes) it will warn that `Local` should be used instead.

And when using `ILocalFactory` to resolve the service the caller has to pass an anonymous object with the property names matching the properties/fields decorated with MustInitialize.

Note that it has to also match the casing of the name, and the value supplied has to match (at compile time) the compile time type of the orignal property/field, otheriwse a warning will be issued.

##### Example Code for DI service with ILocalFactory

    // Declaring service
    [Local]
    public class TestClass
    {
        [MustInitialize] public string TestProperty { get; set; } 
    }

    // Using service
    // [Singleton] // For a Singleton service
    // [Scoped] // For a Scoped service
    // [Local] // For a Local service
    [Transient] // For a transient service
    public class TestUserClass
    {
        private ILocalFactory<TestClass> testClassFactory;
        public TestUserClass(ILocalFactory<TestClass> testClassFactory)
        {
            this.testClassFactory = testClassFactory;
        }

        public void SomeMethod()
        {
            var testClass = testClassFactory.Get(new { TestProperty = "SomeString" });
            Assert.Equals(testClass.TestProperty, "SomeString");
        }
    }

#### 5.2 MightRequire

When we use DI we many times have the service contract (an interface or base class) and then the actual service is provided by an implementation class.
If the implementation class has (or might have in the future) members decorated with `MustInitialize` then we need a way to notify the consumer of the interface/base to initlaize it.
For this purpose there is the `MightRequire` attribute that should be used to decorate the base class and then the consumer will have to initalize it.

##### Example Code for DI base implementation pair with ILocalFactory

    // Declaring service
    [MightRequire("TestProperty", typeof(string))] // Or in C# 11 [MightRequire<string>(nameof(TestClass.TestProperty))]
    public interface ITestClass
    {
    }
    [Local(typeof(ITestClass))] // Or in C# 11 [Local<ITestClass>]
    public class TestClass : ITestClass
    {
        [MustInitialize] public string TestProperty { get; set; } 
    }

    public class TestUserClass
    {
        private ILocalFactory<ITestClass> testClassFactory;
        public TestUserClass(ILocalFactory<ITestClass> testClassFactory) // Note we are using ITestClass and not TestClass but we still have to provide the TestProperty
        {
            this.testClassFactory = testClassFactory;
        }

        public void SomeMethod()
        {
            var testClass = testClassFactory.Get(new { TestProperty = "SomeString" });
            // Assert.Equals(testClass.TestProperty, "SomeString"); this will throw a compile error because it is types as ITestClass and not TestClass...
        }
    }

## Componenets of MustInitialize Analyzer
- A Roslyn analyzer which can be installed as a Nuget package
- A Visual Studio extension

## Installing as Nuget from local
Add in the same folder as the `.sln` file for your project a file named `nuget.config` with the following content (and replace Path/to/dll/folder with the actual path for the nupkg is stored) 

    <?xml version="1.0" encoding="utf-8"?>
    <configuration>
        <packageSources>
            <add key="MyLocalSharedSource" value="Path/to/nupkg/folder" />
        </packageSources>
    </configuration>

Afterwards it will show up as a package source in the Nuget Package sources, so you can install it from there

See sample image from the Visaul Studio `Tools -> Nuget Package Manager -> Manage Nuget Packages for Solution...` page

![image](https://user-images.githubusercontent.com/92554300/150062703-d9dcb61d-236c-4355-ac7f-e8c628372a4d.png)

## Debugging

##### To run in Visual Studio
Just load up the project and hit start.

When running the project sometimes the breakpoints are not being hit, you can use the suggestions described in [this issue](https://github.com/dotnet/roslyn-sdk/issues/515):
    - Turn off `Use 64 bit process for code analysis`
    - Add a `Debugger.Launch()` statement
    
##### To run in [dnSpy](https://github.com/dnSpy/dnSpy)
Here are the steps involved:
1. Open VS Develoepr Command Prompt and run MS build on your project
2. From the output get the entire `csc` command line
    - Since it won't fit on the command line save the entire string (besides the csc path) in a file named `options.rsp`

3. Open [dnSpy](https://github.com/dnSpy/dnSpy)
    - Use the 64 Bit version if you are on a x64 machine and csc is in `Program Files` not in `Program Files (x86)`, otherwise use the 32 Bit version
4. From `File -> Open` load all `MustInitializeAnalyzer.dll` files referenced in the string of step 2
    - Set breakpoints in all loaded files (to ensure that it is breaking)
5. Run `Start` in dnSpy and set the following parameters:
      - Executable: Should be set to the `csc` path of step 2
      - Arguments: Should be `@"options.rsp"` (the full path of the file of step 2)
      - Working Directory: Should be set the directory of your .csproj file to be debugged
