﻿using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using DotNetPowerExtensions.Reflection;
using DotNetPowerExtensions.Reflection.Models;

namespace DotNetPowerExtensions.Reflection;

internal class TypeInfoService
{
    private readonly Type type;
    private readonly FieldInfo[] fields;
    private readonly PropertyInfo[] props;
    private readonly MethodInfo[] methods;
    private readonly EventInfo[] events;

    private readonly List<FieldInfo> backingFields = new List<FieldInfo>();
    private readonly List<MethodInfo> handledMethods = new List<MethodInfo>();

    private readonly List<MethodDetail> shadowedMethods = new List<MethodDetail>();
    private readonly List<PropertyDetail> shadowedProperties = new List<PropertyDetail>();
    private readonly List<EventDetail> shadowedEvents = new List<EventDetail>();
    private readonly List<FieldDetail> shadowedFields = new List<FieldDetail>();

    private static ConcurrentDictionary<Type, TypeDetailInfo> types = new ConcurrentDictionary<Type, TypeDetailInfo>();

    public TypeInfoService(Type type)
    {
        this.type = type;
        fields = type.GetFields(BindingFlagsExtensions.AllBindings); // Fields don't have explicit implementations
        props = type.GetProperties(BindingFlagsExtensions.AllBindings).Where(m => !m.IsExplicitImplementation()).ToArray();
        methods = type.GetMethods(BindingFlagsExtensions.AllBindings).Where(m => !m.IsExplicitImplementation()).ToArray();
        events = type.GetEvents(BindingFlagsExtensions.AllBindings).Where(m => !m.IsExplicitImplementation()).ToArray();
    }

    private (TDetail?, TDetail?, DeclarationTypes, bool) GetBaseDetails<TDetail, TReflection>(TReflection member,
                                                                    Func<TypeDetailInfo, TDetail?> detailsFunc, Func<MethodInfo, bool> shadowMethodPredicate)
        where TDetail : MemberDetail<TReflection>
        where TReflection : MemberInfo
    {
        if (type.IsInterface || type.BaseType is null || type.BaseType == typeof(object)) return (null, null, DeclarationTypes.Decleration, false);

        if (member.DeclaringType != type)
        {
            var baseProp = detailsFunc(types[member.DeclaringType!]);
            return (baseProp, null, baseProp?.DeclarationType ?? DeclarationTypes.Decleration, false);
        }

        var originalProp = detailsFunc(types[type.BaseType]);
        if (originalProp is null) return (null, null, DeclarationTypes.Decleration, false);

        var isNewShadow = type.BaseType!.GetMethods(BindingFlagsExtensions.AllBindings).Count(shadowMethodPredicate) < methods.Count(shadowMethodPredicate);

        DeclarationTypes decl;
        if (originalProp.DeclarationType == DeclarationTypes.ShadowOverride) decl = DeclarationTypes.ShadowOverride;
        else if (isNewShadow) decl = DeclarationTypes.Shadow;
        else if (originalProp!.DeclarationType == DeclarationTypes.Shadow) decl = DeclarationTypes.ShadowOverride;
        else decl = DeclarationTypes.Override;

        return (null, originalProp, decl, isNewShadow);
    }

    private PropertyDetail GetPropertyDetail(PropertyInfo prop, PropertyInfo? interfaceProp = null)
    {
        var nameForTesting = new[] { prop.GetMethod?.Name, prop.SetMethod?.Name }.First(n => n is not null)!;

        // TODO... this won't work correctly by default implemented overrides
        var (baseProp, originalProp, decl, isNewShadow) = GetBaseDetails(prop,
            ty => ty.PropertyDetails.FirstOrDefault(p => p.Name == prop.Name), m => m.Name == nameForTesting);
        if (isNewShadow) shadowedProperties.Add(originalProp!); // If shadow then furtherBase is not null

        var backingField = baseProp is not null ? null : fields.FirstOrDefault(f => f.Name == $"<{prop.Name}>k__BackingField" && f.IsPrivate && !f.IsStatic);
        if (backingField is not null) backingFields.Add(backingField);

        if (prop.GetMethod is not null) handledMethods.Add(prop.GetMethod);
        if (prop.SetMethod is not null) handledMethods.Add(prop.SetMethod);

        decl = interfaceProp is null ? decl
                    : interfaceProp == prop ? DeclarationTypes.Decleration // Default interface prop
                        : isNewShadow && originalProp!.ExplicitInterface == interfaceProp.DeclaringType
                            ? DeclarationTypes.ExplicitReimplementation
                            : DeclarationTypes.ExplicitImplementation;

        var overridenProp = isNewShadow ? null : originalProp ?? baseProp?.OverridenProperty;

        return new PropertyDetail
        {
            Name = interfaceProp?.Name ?? prop.Name,
            ReflectionInfo = prop,
            ExplicitInterface = interfaceProp?.DeclaringType,
            IsExplicit = interfaceProp is not null,
            InReflectionForCurrentType = prop.ReflectedType == type,
            IsInherited = prop.DeclaringType != type,
            BackingField = baseProp?.BackingField ?? (backingField is null ? null : new FieldDetail
            {
                Name = backingField.Name,
                ReflectionInfo = backingField,
                ExplicitInterface = null,
                IsExplicit = false,
                MemberDetailType = MemberDetailTypes.PropertyBackingField,
                IsInherited = backingField.DeclaringType != type,
                InReflectionForCurrentType = true,
                DeclarationType = DeclarationTypes.Decleration,
            }),
            GetMethod = prop.GetMethod is null ? null : GetActualMethodDetail(prop.GetMethod, decl, overridenProp?.GetMethod, interfaceProp?.GetMethod),
            SetMethod = prop.SetMethod is null ? null : GetActualMethodDetail(prop.SetMethod, decl, overridenProp?.SetMethod, interfaceProp?.SetMethod),
            DeclarationType = decl,
            BasePrivateGetMethod = prop.GetMethod is not null ? null : baseProp?.GetMethod ?? baseProp?.BasePrivateGetMethod
                                                                        ?? (!isNewShadow ? originalProp?.GetMethod ?? originalProp?.BasePrivateGetMethod : null),
            BasePrivateSetMethod = prop.SetMethod is not null ? null : baseProp?.SetMethod ?? baseProp?.BasePrivateSetMethod
                                                                        ?? (!isNewShadow ? originalProp?.SetMethod ?? originalProp?.BasePrivateSetMethod : null),
            OverridenProperty = overridenProp,
        };
    }

    private EventDetail GetEventDetail(EventInfo eventInfo, EventInfo? interfaceEvent = null)
    {
        var (baseEvent, originalEvent, decl, isNewShadow) = GetBaseDetails(eventInfo, ty => ty.EventDetails.FirstOrDefault(p => p.Name == eventInfo.Name),
                                                                                                                        m => m.Name == eventInfo.AddMethod!.Name);
        if (isNewShadow) shadowedEvents.Add(originalEvent!); // If shadow then furtherBase is not null

        // An event can have a backing field with the same name if it doesn't have specified the add/remove
        // For explicit implementation we always need to have add/remove so we don't have to worry about the naming
        var backingField = baseEvent is not null ? null : fields.FirstOrDefault(f => f.Name == eventInfo.Name && f.IsPrivate && !f.IsStatic);
        if (backingField is not null) backingFields.Add(backingField);

        handledMethods.AddRange(new[] { eventInfo.AddMethod!, eventInfo.RemoveMethod! });

        decl = interfaceEvent is null ? decl
                    : eventInfo == interfaceEvent ? DeclarationTypes.Decleration // Default interface event
                        : isNewShadow && originalEvent!.ExplicitInterface == interfaceEvent.DeclaringType
                            ? DeclarationTypes.ExplicitReimplementation
                            : DeclarationTypes.ExplicitImplementation;

        var overridenEvent = isNewShadow ? null : originalEvent ?? baseEvent?.OverridenEvent;

        return new EventDetail
        {
            Name = interfaceEvent?.Name ?? eventInfo.Name,
            ReflectionInfo = eventInfo,
            ExplicitInterface = interfaceEvent?.DeclaringType,
            IsExplicit = interfaceEvent is not null,
            InReflectionForCurrentType = eventInfo.ReflectedType == type,
            IsInherited = eventInfo.DeclaringType != type,
            AddMethod = GetActualMethodDetail(eventInfo.AddMethod!, decl, overridenEvent?.AddMethod, interfaceEvent?.AddMethod!),
            RemoveMethod = GetActualMethodDetail(eventInfo.RemoveMethod!, decl, overridenEvent?.RemoveMethod, interfaceEvent?.RemoveMethod!),
            DeclarationType = decl,
            OverridenEvent = overridenEvent,
            BackingField = baseEvent?.BackingField ?? (backingField is null ? null : new FieldDetail
            {
                Name = backingField.Name,
                ReflectionInfo = backingField,
                ExplicitInterface = null,
                IsExplicit = false,
                MemberDetailType = MemberDetailTypes.EventBackingField,
                IsInherited = backingField.DeclaringType != type,
                InReflectionForCurrentType = true,
                DeclarationType = DeclarationTypes.Decleration,
            }),
        };
    }

    private MethodDetail? GetMethodDetail(MethodInfo methodInfo, MethodInfo? interfaceMethod = null)
    {
        if (methodInfo.DeclaringType == typeof(object)) return null;

        // NOTE: We don't have to be concerned about type parameters since reflection updates the base type the use the constructed generic base
        var (baseMethod, originalMethod, decl, isNewShadow) = GetBaseDetails(methodInfo,
                                                                    ty => ty.MethodDetails.FirstOrDefault(md => md.ReflectionInfo.IsSignatureEqual(methodInfo)),
                                                                    m => m.IsSignatureEqual(methodInfo));

        if (isNewShadow) shadowedMethods.Add(originalMethod!); // If shadow then furtherBase is not null

        if (interfaceMethod is not null && isNewShadow && originalMethod!.ExplicitInterface == interfaceMethod.DeclaringType) decl = DeclarationTypes.ExplicitReimplementation;
        else if (interfaceMethod is not null && methodInfo == interfaceMethod) decl = DeclarationTypes.Decleration;
        else if (interfaceMethod is not null) decl = DeclarationTypes.ExplicitImplementation;

        var overridenMethod = isNewShadow ? null : originalMethod ?? baseMethod?.OverridenMethod;

        return GetActualMethodDetail(methodInfo, decl, overridenMethod, interfaceMethod);
    }

    private MethodDetail GetActualMethodDetail(MethodInfo methodInfo, DeclarationTypes decl, MethodDetail? overridenMethod, MethodInfo? interfaceMethod = null)
        => new MethodDetail
        {
            Name = interfaceMethod?.Name ?? methodInfo.Name,
            ReflectionInfo = methodInfo,
            ExplicitInterface = interfaceMethod?.DeclaringType,
            IsExplicit = interfaceMethod is not null,
            InReflectionForCurrentType = methodInfo.ReflectedType == type,
            IsInherited = methodInfo.DeclaringType != type,
            ArgumentTypes = methodInfo.GetParameters().Select(p => p.ParameterType).ToArray(),
            ReturnType = methodInfo.ReturnType,
            GenericArguments = methodInfo.GetGenericArguments(),
            DeclarationType = decl,
            OverridenMethod = overridenMethod,
        };

    public TypeDetailInfo GetTypeInfo()
    {
        return types.GetOrAdd(type, t => CreateTypeInfo());
    }

    private TypeDetailInfo CreateTypeInfo()
    {
        type.GetBasesAndInterfaces().ToList().ForEach(ty => _ = new TypeInfoService(ty).GetTypeInfo()); // Ensure the bases and interfaces are in the cache

        var baseType = type.BaseType is null || type.BaseType == typeof(object) ? null : types[type.BaseType];

        var defaultImplementProps = props.Where(p => type.IsInterface && !p.IsAbstract()).ToArray();
        var defaultImplementEvents = events.Where(e => type.IsInterface && !e.IsAbstract()).ToArray();

        var propDetails = new List<PropertyDetail>();
        var eventDetails = new List<EventDetail>();
        foreach (var prop in props.Except(defaultImplementProps)) propDetails.Add(GetPropertyDetail(prop)); // GetPropertyDetail has side effects so not using Linq
        foreach(var evt in events.Except(defaultImplementEvents)) eventDetails.Add(GetEventDetail(evt)); // GetPropertyDetail has side effects so not using Linq

        var methodDetails = new List<MethodDetail>();
        // CAUTION: The following should only be done after the earlier have materilized as they have side effects for the handledMethods
        var handledMethodsNames = handledMethods.Select(m => m.Name).ToArray(); // Remember that we might have methods with the same names becasue of shadows
        var methodsToUse = methods
                .Where(m1 => !handledMethodsNames.Contains(m1.Name) && (baseType is null || !baseType.ShadowedMethodDetails.Any(m => m.ReflectionInfo.IsEqual(m1))))
                .ToArray();
        var defaultImplementMethods = methodsToUse.Where(m => type.IsInterface && !m.IsAbstract).ToArray();
        foreach (var m in methodsToUse.Except(defaultImplementMethods))
        {
            var methodDetail = GetMethodDetail(m);
            if(methodDetail is not null) methodDetails.Add(methodDetail);
        }
        // Remove shadowed methods (since GetMethods also returns shadowed methods), but since we removed all shadowed by base we can assume that the remianing shadows ahve one declared in the current type
        methodDetails = methodDetails
                .GroupBy(md => new { md.Name, md.ArgumentTypes, md.GenericArguments })
                .Select(md => md.Count() == 1 ? md.First() : md.First(md1 => md1.ReflectionInfo.DeclaringType == type))
                .ToList();

        var explicitMethods = new List<MethodDetail>();
        var explicitProperties = new List<PropertyDetail>();
        var explicitEvents = new List<EventDetail>();

        // Default implmented properties are explicit, and since we are dealing with an interface then the ones declared here are the most specific
        foreach (var prop in defaultImplementProps) explicitProperties.Add(GetPropertyDetail(prop, prop)); // GetPropertyDetail has side effects so not using Linq
        foreach (var evt in defaultImplementEvents) explicitEvents.Add(GetEventDetail(evt, evt)); // GetPropertyDetail has side effects so not using Linq
        // Since we removed in the beginning all explicit implemented methods then it most be Decleration
        // TODO... do we have to handle override??
        foreach (var m in defaultImplementMethods.Except(handledMethods)) explicitMethods.Add(GetActualMethodDetail(m, DeclarationTypes.Decleration, null, m));

        foreach (var iface in type.GetInterfaces())
        {
            var map = type.GetInterfaceMapForInterface(iface);
            var methodPairs = Enumerable.Range(0, map.TargetMethods.Length)
                .Where(i => !map.InterfaceMethods[i].IsExplicitImplementation()) // Only the map for the original interface shows the correct implementation
                .Select(i => (target: map.TargetMethods[i], iface: map.InterfaceMethods[i]))
                .Where(i => i.target == i.iface || i.target.IsExplicitImplementation()); // We only look for explicit implemtations but remember that the original interface won't look like one

            foreach (var methodPair in methodPairs)
            {
                var prop = methodPair.iface.GetDeclaringProperty();
                if (prop is not null)
                {
                    if (prop.GetMethod is null || prop.GetMethod == methodPair.iface) // Avoid duplicates so only go with get if not set
                    {
                        var targetProp = methodPair.target.GetDeclaringProperty();
                        explicitProperties.Add(GetPropertyDetail(targetProp!, prop));
                    }
                    continue;
                }

                var evt = methodPair.iface.GetDeclaringEvent();
                if (evt is not null)
                {
                    if (evt.AddMethod == methodPair.iface) // Avoid duplicates so only use the Add
                    {
                        var targetEvent = methodPair.target.GetDeclaringEvent();
                        explicitEvents.Add(GetEventDetail(targetEvent!, evt));
                    }
                    continue;
                }

                // TODO... event
                explicitMethods.Add(GetMethodDetail(methodPair.target, methodPair.iface)!);
            }
        }

        // Do it only after handling explicit properties
        var handledFieldsNames = backingFields.Select(f => f.Name).ToList();
        var fieldsToUse = fields
            .Where(f => !handledFieldsNames.Contains(f.Name)
                && (baseType is null || !baseType.ShadowedFieldDetails
                                                .Any(f1 => f1.ReflectionInfo.Name == f.Name && f1.ReflectionInfo.DeclaringType == f.DeclaringType)))
            .ToArray();
        var groupedFields = fieldsToUse.GroupBy(f => f.Name);
        var shadowed = groupedFields.Where(g => g.Count() > 1).Select(g => g.First(g1 => g1.DeclaringType != type)).ToArray();
        var nonShadowed = groupedFields.Select(g => g.Count() == 1 ? g.First() : g.First(g1 => g1.DeclaringType == type)).ToArray();

        return new TypeDetailInfo
        {
            Type = type,
            PropertyDetails = propDetails.ToArray(),
            EventDetails = eventDetails.ToArray(),
            FieldDetails = nonShadowed.Select(f => new FieldDetail
            {
                Name = f.Name, // Fields don't have explicit implementation...
                ReflectionInfo = f,
                ExplicitInterface = null,
                IsExplicit = false,
                MemberDetailType = MemberDetailTypes.Field,
                IsInherited = f.DeclaringType != type,
                InReflectionForCurrentType = true,
                DeclarationType = shadowed.Any(s => s.Name == f.Name) || baseType?.ShadowedFieldDetails.Any(s => s.ReflectionInfo.Name == f.Name) == true
                                            ? DeclarationTypes.Shadow
                                            :  DeclarationTypes.Decleration,
            }).ToArray(),
            MethodDetails = methodDetails.ToArray(),
            // TODO... we cannot just copy the parent shadows as it will have the wrong reflected types
            ShadowedPropertyDetails = (baseType?.ShadowedPropertyDetails ?? new PropertyDetail[] { }).Concat(shadowedProperties).ToArray(),
            ShadowedMethodDetails = (baseType?.ShadowedMethodDetails ?? new MethodDetail[] { }).Concat(shadowedMethods).ToArray(),
            ShadowedEventDetails = (baseType?.ShadowedEventDetails ?? new EventDetail[] { }).Concat(shadowedEvents).ToArray(),
            ShadowedFieldDetails = (baseType?.ShadowedFieldDetails ?? new FieldDetail[] { }).Concat(shadowed.Select(f => new FieldDetail
            {
                Name = f.Name, // Fields don't have explicit implementation...
                ReflectionInfo = f,
                ExplicitInterface = null,
                IsExplicit = false,
                MemberDetailType = MemberDetailTypes.Field,
                IsInherited = f.DeclaringType != type,
                InReflectionForCurrentType = true,
                DeclarationType = baseType?.ShadowedFieldDetails.Any(s => s.ReflectionInfo.Name == f.Name) == true
                                            ? DeclarationTypes.Shadow
                                            : DeclarationTypes.Decleration,
            }).ToArray()).ToArray(),
            ExplicitPropertyDetails = explicitProperties.ToArray(),
            ExplicitEventDetails = explicitEvents.ToArray(),
            ExplicitMethodDetails = explicitMethods.ToArray(),
            BasePrivateFieldDetails = baseType?.BasePrivateFieldDetails.Union(baseType.FieldDetails.Where(fd => fd.ReflectionInfo.IsPrivate)).ToArray() ?? new FieldDetail[] { },
            BasePrivateMethodDetails = baseType?.BasePrivateMethodDetails.Union(baseType.MethodDetails.Where(fd => fd.ReflectionInfo.IsPrivate)).ToArray() ?? new MethodDetail[] { },
            BasePrivatePropertyDetails = baseType?.BasePrivatePropertyDetails.Union(baseType.PropertyDetails.Where(fd => fd.ReflectionInfo.IsPrivate())).ToArray() ?? new PropertyDetail[] { },
            BasePrivateEventDetails = baseType?.BasePrivateEventDetails.Union(baseType.EventDetails.Where(fd => fd.ReflectionInfo.IsPrivate())).ToArray() ?? new EventDetail[] { },
        };
    }
}
