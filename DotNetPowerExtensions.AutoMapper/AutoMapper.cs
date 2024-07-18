
namespace SequelPay.DotNetPowerExtensions.AutoMapper;

public class AutoMapper : IAutoMapper
{
    public TTarget Map<TSource, TTarget>(TSource source) where TTarget : new()
    {
        var mapInternal = IsMapInternal<TSource, TTarget>();

        return Map<TSource, TTarget>(source, mapInternal);
    }

    internal TTarget Map<TSource, TTarget>(TSource source, bool mapInternal) where TTarget : new()
    {
        var target = new TTarget();

        PerformMap(source, target, mapInternal);

        return target;
    }

    internal void PerformMap<TSource, TTarget>(TSource source, TTarget target, bool mapInternal)
    {
        var sourceMembers = GetMembers<TSource>(mapInternal);
        var targetMembers = GetMembers<TTarget>(mapInternal);

        foreach (var sourceMember in sourceMembers)
        {
            var targetMember = targetMembers.FirstOrDefault(member => member.Name == sourceMember.Name);

            if (targetMember is FieldInfo field && sourceMember is FieldInfo)
            {
                try { field.SetValue(target, ((FieldInfo)sourceMember).GetValue(source)); }
                catch (Exception ex) { Console.Error.WriteLine(ex.Message); }
            }
            else if (targetMember is PropertyInfo property && sourceMember is PropertyInfo && property.CanWrite)
            {
                try { property.SetValue(target, ((PropertyInfo)sourceMember).GetValue(source)); }
                catch (Exception ex) { Console.Error.WriteLine(ex.Message); }
            }
        }
    }

    private MemberInfo[] GetMembers<TType>(bool includeInternal)
    {
        var type = typeof(TType);

        if(!includeInternal)
        {
            return type.GetFields().OfType<MemberInfo>().Concat(type.GetProperties()).ToArray();
        }

        return type.GetFields(BindingFlagsExtensions.AllBindings)
                                .Where(f => f.IsPublicOrInternal()).OfType<MemberInfo>()
                .Concat(type.GetProperties(BindingFlagsExtensions.AllBindings)
                                .Where(p => p.GetMethod?.IsPublicOrInternal() == true))
                .ToArray();

    }

    private bool IsMapInternal<TSource, TTarget>()
    {
        var sourceType = typeof(TSource);
        var targetType = typeof(TTarget);

        var sourceAttribute = sourceType.GetCustomAttributes<AutoMapAttribute>().FirstOrDefault(a => a.TargetClass == targetType);
        var targetAttribute = targetType.GetCustomAttributes<AutoMapAttribute>().FirstOrDefault(a => a.TargetClass == sourceType);

        var sourceGenerateAttribute = sourceType.GetCustomAttributes<GenerateMapperObjectAttribute>().FirstOrDefault(a => a.ObjectName == targetType.Name);
        var targetGenerateAttribute = targetType.GetCustomAttributes<GenerateMapperObjectAttribute>().FirstOrDefault(a => a.ObjectName == sourceType.Name);

        if ((sourceType is not null || targetType is not null) && (sourceGenerateAttribute is not null || targetGenerateAttribute is not null))
        {
            throw new InvalidOperationException("Invalid mapping configuration, cannot have both AutoMap and GenerateObjectMapper attributes at once.");
        }

        if (sourceGenerateAttribute is not null && targetGenerateAttribute is not null)
        {
            throw new InvalidOperationException("Invalid mapping configuration, cannot haveGenerateObjectMapper attribute on both types at once.");
        }

        if ((sourceAttribute is null || targetAttribute is null) && sourceGenerateAttribute is null && targetGenerateAttribute is null)
        {
            throw new InvalidOperationException("Invalid mapping configuration, both source and target need to have the AutoMapAttribute referencing each other.");
        }

        if ((sourceAttribute.MapInternal != targetAttribute.MapInternal)
                    && sourceGenerateAttribute is null && targetGenerateAttribute is null)
        {
            throw new InvalidOperationException("Map internal of source and target do not match");
        }

        // Verify that the target type has a default constructor
        var targetConstructor = targetType.GetConstructor(Type.EmptyTypes);
        if (targetConstructor == null)
        {
            throw new InvalidOperationException("Target type must have a default constructor.");
        }

        return sourceAttribute?.MapInternal ?? targetGenerateAttribute?.MapInternal ?? sourceGenerateAttribute!.MapInternal;
    }
}