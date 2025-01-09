using static SequelPay.DotNetPowerExtensions.Reflection.MethodInfoExtensions;

namespace SequelPay.DotNetPowerExtensions.Reflection;

/*************************************************************/
/*
 * This got already way too complicated
 * We need to break it down
 * Also we need to test all aspects of it, that's way more tests to go...
/*************************************************************/

internal class TypeInfoFactory
{
    public class TypeInfo
    {
        [MustInitialize] public PropertyDetail[] PropertyDetails { get; set; }
        [MustInitialize] public MethodDetail[] MethodDetails { get; set;  }
        [MustInitialize] public EventDetail[] EventDetails { get; set;  }
        [MustInitialize] public FieldDetail[] FieldDetails { get; set;  }

        [MustInitialize] public PropertyDetail[] ShadowedPropertyDetails { get; set;  }
        [MustInitialize] public MethodDetail[] ShadowedMethodDetails { get; set;  }
        [MustInitialize] public EventDetail[] ShadowedEventDetails { get; set;  }
        [MustInitialize] public FieldDetail[] ShadowedFieldDetails { get; set;  }

        [MustInitialize] public PropertyDetail[] BasePrivatePropertyDetails { get; set;  }
        [MustInitialize] public MethodDetail[] BasePrivateMethodDetails { get; set;  }
        [MustInitialize] public EventDetail[] BasePrivateEventDetails { get; set;  }
        [MustInitialize] public FieldDetail[] BasePrivateFieldDetails { get; set;  }

        /// <summary>
        /// Array of <see cref="IPropertyDetail"/> for properties that must be used explictly, will include default interface implmentations
        /// </summary>
        [MustInitialize] public PropertyDetail[] ExplicitPropertyDetails { get; set;  }
        [MustInitialize] public MethodDetail[] ExplicitMethodDetails { get; set;  }
        [MustInitialize] public EventDetail[] ExplicitEventDetails { get; set;  }
    }

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

    public TypeInfoFactory(Type type)
    {
        this.type = type;
        fields = type.GetFields(BindingFlagsExtensions.AllBindings); // Fields don't have explicit implementations
        props = type.GetProperties(BindingFlagsExtensions.AllBindings).Where(m => !m.IsExplicitImplementation()).ToArray();
        methods = type.GetMethods(BindingFlagsExtensions.AllBindings).Where(m => !m.IsExplicitImplementation()).ToArray();
        events = type.GetEvents(BindingFlagsExtensions.AllBindings).Where(m => !m.IsExplicitImplementation()).ToArray();
    }

    private static Type[] CoreBaseTypes = new[] { typeof(object), typeof(ValueType) };
    private bool HasValidBaseType() => type.BaseType is not null && !CoreBaseTypes.Contains(type.BaseType);

    private (TDetail?, TDetail?, DeclarationTypes, bool) GetBaseDetails<TDetail, TReflection>(TReflection member,
                                            Func<TypeDetailInfo, TDetail?> detailsFunc,
                                            Func<MethodInfo, bool> shadowMethodPredicate,
                                            Func<TReflection, MethodInfo> toMethodFunc)
        where TDetail : MemberDetail<TReflection>
        where TReflection : MemberInfo
    {
        if (type.IsInterface || !HasValidBaseType()) return (null, null, DeclarationTypes.Decleration, false);

        if (member.DeclaringType != type)
        {
            var baseMember = detailsFunc(member.DeclaringType!.GetTypeDetailInfo());
            return (baseMember, null, baseMember?.DeclarationType ?? DeclarationTypes.Decleration, false);
        }

        var originalMember = detailsFunc(type.BaseType.GetTypeDetailInfo());
        if (originalMember is null) return (null, null, DeclarationTypes.Decleration, false);

        var isNewShadow = type.BaseType!.GetMethods(BindingFlagsExtensions.AllBindings).Count(shadowMethodPredicate) < methods.Count(shadowMethodPredicate);
        // Sometimes the Type object does not return the base class methods even for a shadowed method so w ehave to use other tricks
        if (!isNewShadow)
        {
            var baseMethod = toMethodFunc(originalMember.ReflectionInfo);
            var newMethod = toMethodFunc(member);

            if(!baseMethod.Attributes.HasFlag(MethodAttributes.Virtual) // Non virtual methods cannot be overriden
                || !newMethod.Attributes.HasFlag(MethodAttributes.Virtual) // Overriden appears to have the virtual atribute
                    // An overriden method does not appear to have `VtableLayoutMask` besides the 4 methods `GetType`, `GetHashCode`, `Equals`, `ToString`
                || newMethod.Attributes.HasFlag(MethodAttributes.VtableLayoutMask)) isNewShadow = true;
        }

        DeclarationTypes decl;
        if (originalMember.DeclarationType == DeclarationTypes.ShadowOverride) decl = DeclarationTypes.ShadowOverride;
        else if (isNewShadow) decl = DeclarationTypes.Shadow;
        else if (originalMember!.DeclarationType == DeclarationTypes.Shadow) decl = DeclarationTypes.ShadowOverride;
        else decl = DeclarationTypes.Override;

        return (null, originalMember, decl, isNewShadow);
    }

    private PropertyDetail GetPropertyDetail(PropertyInfo prop, PropertyInfo? interfaceProp = null)
    {
        var nameForTesting = new[] { prop.GetMethod?.Name, prop.SetMethod?.Name }.First(n => n is not null)!;

        // TODO... this won't work correctly by default implemented overrides
        var (baseProp, originalProp, decl, isNewShadow) = GetBaseDetails(prop,
                                                ty => ty.PropertyDetails.FirstOrDefault(p => p.Name == prop.Name),
                                                m => m.Name == nameForTesting,
                                                p => p.GetAllMethods().First());

        if (isNewShadow)
        {
            var localShadow = props.FirstOrDefault(p => p.Name == originalProp!.Name && p.DeclaringType == originalProp.ReflectionInfo.DeclaringType);
            shadowedProperties.Add(originalProp! with
            {
                ReflectionInfo = localShadow ?? originalProp.ReflectionInfo,
                InReflectionForCurrentType = localShadow is not null,
                IsInherited = true,
                GetMethod = originalProp.GetMethod?.ReflectionInfo.IsPrivate != false ? null : originalProp.GetMethod,
                SetMethod = originalProp.SetMethod?.ReflectionInfo.IsPrivate != false ? null : originalProp.SetMethod,
                BasePrivateGetMethod = originalProp.GetMethod?.ReflectionInfo.IsPrivate == true ? originalProp.GetMethod : null,
                BasePrivateSetMethod = originalProp.SetMethod?.ReflectionInfo.IsPrivate == true ? originalProp.SetMethod : null,
                BackingField = originalProp!.BackingField is not null
                    ? originalProp.BackingField with { InReflectionForCurrentType = false, IsInherited = true } // Backing fields are never in reflection for a subclass as they are private ,
                    : null
            });
        }

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
            ExplicitDetail = interfaceProp is not null ? GetPropertyDetail(interfaceProp) : null,
            InReflectionForCurrentType = prop.ReflectedType == type,
            IsInherited = prop.DeclaringType != type,
            BackingField = backingField is not null // First take the local backing field as for an override we get a new backing field
            ? new FieldDetail
            {
                Name = backingField.Name,
                ReflectionInfo = backingField,
                MemberDetailType = MemberDetailTypes.PropertyBackingField,
                IsInherited = backingField.DeclaringType != type,
                InReflectionForCurrentType = true,
                DeclarationType = DeclarationTypes.Decleration,
            }
            : baseProp?.BackingField is not null
                ? baseProp.BackingField with { InReflectionForCurrentType = false, IsInherited = true } // Backing fields are never in reflection for a subclass as they are private
                : null,
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
        var (baseEvent, originalEvent, decl, isNewShadow) = GetBaseDetails(eventInfo,
                            ty => ty.EventDetails.FirstOrDefault(p => p.Name == eventInfo.Name),
                            m => m.Name == eventInfo.AddMethod!.Name,
                            e => e.AddMethod!);

        if (isNewShadow)
        {
            var localShadow = events.FirstOrDefault(e => e.Name == originalEvent!.Name && e.DeclaringType == originalEvent.ReflectionInfo.DeclaringType);
            shadowedEvents.Add(originalEvent! with
            {
                ReflectionInfo = localShadow ?? originalEvent.ReflectionInfo,
                InReflectionForCurrentType = localShadow is not null,
                IsInherited = true,
                BackingField = originalEvent!.BackingField is not null
                    ? originalEvent.BackingField with { InReflectionForCurrentType = false, IsInherited = true } // Backing fields are never in reflection for a subclass as they are private
                    : null
            });
        }


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
            ExplicitDetail = interfaceEvent is not null ? GetEventDetail(interfaceEvent) : null,
            InReflectionForCurrentType = eventInfo.ReflectedType == type,
            IsInherited = eventInfo.DeclaringType != type,
            AddMethod = GetActualMethodDetail(eventInfo.AddMethod!, decl, overridenEvent?.AddMethod, interfaceEvent?.AddMethod!),
            RemoveMethod = GetActualMethodDetail(eventInfo.RemoveMethod!, decl, overridenEvent?.RemoveMethod, interfaceEvent?.RemoveMethod!),
            DeclarationType = decl,
            OverridenEvent = overridenEvent,
            BackingField = backingField is not null // First take the local backing field as for an override we get a new backing field
            ? new FieldDetail
            {
                Name = backingField.Name,
                ReflectionInfo = backingField,
                MemberDetailType = MemberDetailTypes.EventBackingField,
                IsInherited = backingField.DeclaringType != type,
                InReflectionForCurrentType = true,
                DeclarationType = DeclarationTypes.Decleration,
            }
            : baseEvent?.BackingField is not null
                ? baseEvent.BackingField with { InReflectionForCurrentType = false, IsInherited = true } // Backing fields are never in reflection for a subclass as they are private
                : null,
        };
    }

    private MethodDetail? GetMethodDetail(MethodInfo methodInfo, MethodInfo? interfaceMethod = null)
    {
        if (CoreBaseTypes.Contains(methodInfo.DeclaringType)) return null;

        // NOTE: We don't have to be concerned about type parameters since reflection updates the base type to use the constructed generic base
        var (baseMethod, originalMethod, decl, isNewShadow) = GetBaseDetails(methodInfo,
                                                                    ty => ty.MethodDetails.FirstOrDefault(md => md.ReflectionInfo.IsSignatureEqual(methodInfo)),
                                                                    m => m.IsSignatureEqual(methodInfo),
                                                                    m => m);

        if (isNewShadow)
        {
            var localShadow = methods.FirstOrDefault(m => m.IsEqual(originalMethod!.ReflectionInfo));
            shadowedMethods.Add(originalMethod! with
            {
                ReflectionInfo = localShadow ?? originalMethod.ReflectionInfo,
                InReflectionForCurrentType = localShadow is not null,
                IsInherited = true,
            });
        }

        if (interfaceMethod is not null && isNewShadow && originalMethod!.ExplicitInterface == interfaceMethod.DeclaringType) decl = DeclarationTypes.ExplicitReimplementation;
        else if (interfaceMethod is not null && methodInfo == interfaceMethod) decl = DeclarationTypes.Decleration;
        else if (interfaceMethod is not null) decl = DeclarationTypes.ExplicitImplementation;

        var overridenMethod = isNewShadow ? null : originalMethod ?? baseMethod?.OverridenMethod;

        return GetActualMethodDetail(methodInfo, decl, overridenMethod, interfaceMethod);
    }

    private IEnumerable<PropertyInfo> GetPropsToUse(IEnumerable<PropertyInfo> props)
    {
        // In general only .GetMethods() typically show the shadowd methods and not .GetProperties()
        // However there is an excpetion if the shadowed property type is a subclass of the base property type
        // But we can figure this by checking for property type assignable to the others
        var groupedProps = props.GroupBy(p => p.Name);

        foreach ( var propGroup in groupedProps )
        {
            if (propGroup.HasOnlyOne())
            {
                yield return propGroup.First();
                continue;
            }

            var usable = propGroup.ToArray();
            while (!usable.HasOnlyOne())
            {
                // We need the type that can be assigned to all others as it is the sub class then...
                var firstType = usable.First().PropertyType;
                usable = usable.Where(u => u.PropertyType == firstType ||
                        (!u.PropertyType.IsAssignableFrom(firstType) && !firstType.IsAssignableFrom(u.PropertyType)))
                    .ToArray();
            }

            yield return usable.First();
        }
    }

    private MethodDetail GetActualMethodDetail(MethodInfo methodInfo, DeclarationTypes decl, MethodDetail? overridenMethod, MethodInfo? interfaceMethod = null)
        => new MethodDetail
        {
            Name = interfaceMethod?.Name ?? methodInfo.Name,
            ReflectionInfo = methodInfo,
            ExplicitDetail = interfaceMethod is not null ? GetMethodDetail(interfaceMethod) : null,
            InReflectionForCurrentType = methodInfo.ReflectedType == type,
            IsInherited = methodInfo.DeclaringType != type,
            DeclarationType = decl,
            OverridenMethod = overridenMethod,
            GenericDefinition = !methodInfo.IsGenericMethod || methodInfo.IsGenericMethodDefinition ? null
                : GetMethodDetail(methodInfo.GetGenericMethodDefinition(),
                    interfaceMethod?.IsGenericMethod != true ? null : interfaceMethod.IsGenericMethodDefinition ? interfaceMethod : interfaceMethod.GetGenericMethodDefinition()),
        };

    internal TypeInfo CreateTypeInfo()
    {
        if (type.IsByRef || type.IsPointer || type.IsGenericParameter || type.IsPrimitive) return new TypeInfo
        {
            PropertyDetails = new PropertyDetail[] { },
            MethodDetails = new MethodDetail[] { },
            EventDetails = new EventDetail[] { },
            FieldDetails = new FieldDetail[] { },
            ShadowedPropertyDetails = new PropertyDetail[] { },
            ShadowedMethodDetails = new MethodDetail[] { },
            ShadowedEventDetails = new EventDetail[] { },

            ShadowedFieldDetails = new FieldDetail[] { },
            BasePrivatePropertyDetails = new PropertyDetail[] { },
            BasePrivateMethodDetails = new MethodDetail[] { },
            BasePrivateEventDetails = new EventDetail[] { },
            BasePrivateFieldDetails = new FieldDetail[] { },
            ExplicitPropertyDetails = new PropertyDetail[] { },
            ExplicitMethodDetails = new MethodDetail[] { },
            ExplicitEventDetails = new EventDetail[] { }
        };
        var n = type.Name;

        type.GetBasesAndInterfaces().ToList().ForEach(ty => _ = new TypeInfoService(ty).GetTypeInfo()); // Ensure the bases and interfaces are in the cache

        var baseType = type.BaseType is null ? null : new TypeInfoService(type.BaseType).GetTypeInfo();

        // Let's handle the default implemented props/events in the current class, note we are not dealing here with ones that were declared in a base interface but implemented in this one as these were filtered in the ctor
        var defaultImplementProps = props.Where(p => type.IsInterface && !p.IsAbstract()).ToArray();
        var defaultImplementEvents = events.Where(e => type.IsInterface && !e.IsAbstract()).ToArray();

        var propDetails = new List<PropertyDetail>();
        var eventDetails = new List<EventDetail>();

        var propsToUse = props.Except(defaultImplementProps);
        if (!type.IsInterface && baseType is not null) propsToUse = GetPropsToUse(propsToUse);

        foreach (var prop in propsToUse) propDetails.Add(GetPropertyDetail(prop)); // GetPropertyDetail has side effects so not using Linq
        foreach(var evt in events.Except(defaultImplementEvents)) eventDetails.Add(GetEventDetail(evt)); // GetPropertyDetail has side effects so not using Linq

        // Sometimes the base props/events are not returned in the current type via reflection so we have to make sure we take care of it
        if (baseType is not null) propDetails.AddRange(baseType.PropertyDetails
                .Where(d => !d.ReflectionInfo.IsPrivate()
                        && !propDetails.Contains(d)
                        && !shadowedProperties.Contains(d)  // We can safely assume that if it is shadowed that the shadowedProperties will already contain it
                        && !propDetails.Any(md => md.Name == d.Name))
                .Select(p => p with
                {
                    InReflectionForCurrentType = false,
                    IsInherited = true,
                    GetMethod = p.GetMethod?.ReflectionInfo.IsPrivate != false ? null : p.GetMethod,
                    SetMethod = p.SetMethod?.ReflectionInfo.IsPrivate != false ? null : p.SetMethod,
                    BasePrivateGetMethod = p.GetMethod?.ReflectionInfo.IsPrivate == true ? p.GetMethod : null,
                    BasePrivateSetMethod = p.SetMethod?.ReflectionInfo.IsPrivate == true ? p.SetMethod : null,
                }));

        if (baseType is not null) eventDetails.AddRange(baseType.EventDetails
            .Where(d => !d.ReflectionInfo.IsPrivate()
                && !eventDetails.Contains(d)
                && !shadowedEvents.Contains(d)  // We can safely assume that if it is shadowed that the shadowedEvents will already contain it
                && !eventDetails.Any(md => md.Name == d.Name))
            .Select(e => e with { InReflectionForCurrentType = false, IsInherited = true }));

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
        // Remove shadowed methods (since GetMethods also returns shadowed methods), but since we removed all shadowed by base we can assume that the remianing shadows have one declared in the current type
        methodDetails = methodDetails
                .GroupBy(md => md.ReflectionInfo, new MethodSignatureEqualityComparer())
                .Select(md => md.Count() == 1 ? md.First() : md.First(md1 => md1.ReflectionInfo.DeclaringType == type))
                .ToList();

        //Sometimes the base methods are not returned in the current type methods via refelection so we have to make sure we take care of it
        if (baseType is not null) methodDetails.AddRange(baseType.MethodDetails
                .Where(d => !d.ReflectionInfo.IsPrivate // Base private will be handled separately direct
                        && !methodDetails.Contains(d)
                        && !shadowedMethods.Contains(d) // We can safely assume that if it is shadowed that the shadowedMethods will already contain it as the handler checks directly on the base
                        && !methodDetails.Any(md => md.Name == d.Name && (md.OverridenMethod == d || md.ReflectionInfo.IsSignatureEqual(d.ReflectionInfo))))
                .Select(d => GetActualMethodDetail(d.ReflectionInfo, d.DeclarationType, d.OverridenMethod, d.ExplicitInterfaceReflectionInfo)));

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
            if (type.IsArray && iface.IsGenericType) continue; // Cannot get interface map for array

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

        return new TypeInfo
        {
            PropertyDetails = propDetails.ToArray(),
            EventDetails = eventDetails.ToArray(),
            FieldDetails = nonShadowed.Select(f => new FieldDetail
            {
                Name = f.Name, // Fields don't have explicit implementation...
                ReflectionInfo = f,
                MemberDetailType = MemberDetailTypes.Field,
                IsInherited = f.DeclaringType != type,
                InReflectionForCurrentType = true,
                DeclarationType = shadowed.Any(s => s.Name == f.Name) || baseType?.ShadowedFieldDetails.Any(s => s.ReflectionInfo.Name == f.Name) == true
                                            ? DeclarationTypes.Shadow
                                            :  DeclarationTypes.Decleration,
            })
            // Sometimes the base fields are not returned in the current type fields via reflection so we have to make sure we take care of it
            .Concat(baseType?.FieldDetails
                        .Where(d => nonShadowed.All(f => f.Name != d.Name) && shadowed.All(f => f.Name != d.Name))
                        .Select(f => f with { InReflectionForCurrentType = false, IsInherited = true })
                    ?? [])
            .ToArray(),
            MethodDetails = methodDetails.ToArray(),
            ShadowedPropertyDetails = (baseType?.ShadowedPropertyDetails
                 .Where(p => shadowedProperties
                    .All(p1 => p1.Name != p.Name || (new FollowThroughEnumerable<PropertyDetail>(p1, p2 => p2.OverridenProperty)
                                                                     .All(p2 => p2.DeclaringType != p.DeclaringType))))
                .Select(p => p with
                {
                    InReflectionForCurrentType = props.Any(p1 => p1.Name == p.Name && p1.DeclaringType == p.ReflectionInfo.DeclaringType),
                    ReflectionInfo = props
                            .FirstOrDefault(p1 => p1.Name == p.Name
                                    && p1.DeclaringType == p.ReflectionInfo.DeclaringType)
                            ?? p.ReflectionInfo,
                    IsInherited = true,
                }) ?? new PropertyDetail[] { })
            .Concat(shadowedProperties).ToArray(), // We don't need to check for members not found on the current type only on the base, because if they were the same as a current member thus shadowing they would have been handled by the shadow system
            ShadowedMethodDetails = (baseType?.ShadowedMethodDetails
                .Where(m => shadowedMethods
                    .All(m1 => m1.Name != m.Name
                        || !m1.ReflectionInfo.IsEqual(m.ReflectionInfo)
                        || (new FollowThroughEnumerable<MethodDetail>(m, m2 => m2.OverridenMethod)
                                                                     .All(m2 => m2.DeclaringType != m.DeclaringType))))
                .Select(m => m with
                {
                    InReflectionForCurrentType = methods.Any(m1 => m1.Name == m.Name
                            && m1.DeclaringType == m.ReflectionInfo.DeclaringType
                            && m1.IsEqual(m.ReflectionInfo)),
                    ReflectionInfo = methods
                                        .FirstOrDefault(m1 => m1.Name == m.Name
                                                && m1.DeclaringType == m.ReflectionInfo.DeclaringType
                                                && m1.IsEqual(m.ReflectionInfo))
                                        ?? m.ReflectionInfo,
                    IsInherited = true,
                }) ?? new MethodDetail[] { }).Concat(shadowedMethods).ToArray(), // We don't need to check for members not found on the current type only on the base, because if they were the same as a current member thus shadowing they would have been handled by the shadow system
            ShadowedEventDetails = (baseType?.ShadowedEventDetails
                .Where(e => shadowedEvents
                    .All(e1 => e1.Name != e.Name
                        || (new FollowThroughEnumerable<EventDetail>(e1, e2 => e2.OverridenEvent)
                                                                     .All(e2 => e2.DeclaringType != e.DeclaringType))))

            .Select(e => e with
            {
                InReflectionForCurrentType = events.Any(e1 => e1.Name == e.Name && e1.DeclaringType == e.ReflectionInfo.DeclaringType),
                ReflectionInfo = events
                        .FirstOrDefault(e1 => e1.Name == e.Name
                                && e1.DeclaringType == e.ReflectionInfo.DeclaringType)
                        ?? e.ReflectionInfo,
                IsInherited = true,
            }) ?? new EventDetail[] { }).Concat(shadowedEvents).ToArray(), // We don't need to check for members not found on the current type only on the base, because if they were the same as a current member thus shadowing they would have been handled by the shadow system
            ShadowedFieldDetails = (baseType?.ShadowedFieldDetails
                .Where(f => shadowed
                    .All(f1 => f1.Name != f.Name || f1.DeclaringType != f.ReflectionInfo.DeclaringType))
                .Select(f => f with
                {
                    InReflectionForCurrentType = fields.Any(f1 => f1.Name == f.Name && f1.DeclaringType == f.ReflectionInfo.DeclaringType),
                    ReflectionInfo = fields
                        .FirstOrDefault(f1 => f1.Name == f.Name
                                && f1.DeclaringType == f.ReflectionInfo.DeclaringType)
                        ?? f.ReflectionInfo,
                    IsInherited = true,
                }) ?? new FieldDetail[] { })
                .Concat(shadowed.Select(f => new FieldDetail
                {
                    Name = f.Name, // Fields don't have explicit implementation...
                    ReflectionInfo = f,
                    MemberDetailType = MemberDetailTypes.Field,
                    IsInherited = true,
                    InReflectionForCurrentType = f.ReflectedType == type,
                    DeclarationType = baseType?.ShadowedFieldDetails.Any(s => s.ReflectionInfo.Name == f.Name) == true
                                                ? DeclarationTypes.Shadow
                                                : DeclarationTypes.Decleration,
                })
                // Sometimes the base fields are not returned in the current type fields via reflection so we have to make sure we take care of it
                .Concat(baseType?.FieldDetails
                    .Where(d => nonShadowed.Any(f => f.Name == d.Name && f.DeclaringType != d.ReflectionInfo.DeclaringType) && shadowed.All(f => f.Name != d.Name || f.DeclaringType != d.ReflectionInfo.DeclaringType))
                    .Select(f => f with { InReflectionForCurrentType = false, IsInherited = true })
                    ?? [])
            ).ToArray(),
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