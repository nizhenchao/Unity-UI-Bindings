﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UiBinding.Extensions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UiBinding.Core
{
    public static class MemberFilters
    {
        private static readonly IList<PropertyInfo> MonoBehaviourPropertyTypes = typeof(MonoBehaviour).GetProperties(MemberBindingFlags.Source);
        private static readonly IList<MethodInfo> MonoBehaviourMethodTypes = typeof(MonoBehaviour).GetMethods(MemberBindingFlags.Source);

        private static class MemberBindingFlags
        {
            public const BindingFlags Source = BindingFlags.Public | BindingFlags.Instance;

            public const BindingFlags Target = BindingFlags.Public | BindingFlags.Instance;
        }

        public static PropertyFilter SourceProperties { get; } = new PropertyFilter(MemberBindingFlags.Source, m => !IsMonoBehaviourProperty(m));

        public static PropertyFilter TargetProperties { get; } = new PropertyFilter(MemberBindingFlags.Target);

        public static MethodFilter EventCallbacks { get; } = new MethodFilter(MemberBindingFlags.Source, m => !m.IsSpecialName && m.IsParameterless() && !IsMonoBehaviourMethod(m));

        public static MethodFilter EventTriggerCallbacks { get; } = new MethodFilter(MemberBindingFlags.Source, m => !m.IsSpecialName && (m.IsParameterless() || m.HasParameter<EventTriggerType>()) && !IsMonoBehaviourMethod(m));

        public static PropertyFilter TargetEvents { get; } = new PropertyFilter(MemberBindingFlags.Target, m => typeof(UnityEventBase).IsAssignableFrom(m.PropertyType));

        public static PropertyFilter Lists { get; } = new PropertyFilter(MemberBindingFlags.Source, m => typeof(IEnumerable).IsAssignableFrom(m.PropertyType) && m.PropertyType != typeof(string) && !IsMonoBehaviourProperty(m));

        private static bool IsMonoBehaviourProperty(PropertyInfo property)
        {
            return MonoBehaviourPropertyTypes.Any(p => p.Name == property.Name);
        }

        private static bool IsMonoBehaviourMethod(MethodInfo method)
        {
            return MonoBehaviourMethodTypes.Any(p => p.Name == method.Name);
        }
    }
}