
using SequelPay.DotNetPowerExtensions.AutoMapper;

namespace DotNetPowerExtensions.AutoMapper.Tests.Mapper;

// Source class to be mapped
[AutoMap(typeof(TargetClass))]
public class SourceClass
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age = 30;
}

// Source class to be mapped
[GenerateMapperObject("GeneratedClass")]
public class SourceGeneratorClass
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age = 30;
}


// Target class for mapping
[AutoMap(typeof(SourceClass))]
public class TargetClass
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
}

// Invalid target class for mapping
public class InvalidTargetClass
{
    public int Id { get; set; }
    public string Name { get; set; }
}

// Source class with no default constructor
[AutoMap(typeof(TargetClassWithNoDefaultConstructor))]
public class SourceClassWithNoDefaultConstructor
{
    public int Id { get; set; }
    public string Name { get; set; }
}

// Target class with no default constructor
public class TargetClassWithNoDefaultConstructor
{
    public int Id { get; set; }
    public string Name { get; set; }

    public TargetClassWithNoDefaultConstructor(int id, string name)
    {
        Id = id;
        Name = name;
    }
}
