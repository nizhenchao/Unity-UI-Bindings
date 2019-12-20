﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UiBinding.Conversion;
using UiBinding.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UiBinding.Inspector
{
    [CustomEditor(typeof(PropertyBinding))]
    public class PropertyBindingInspector : Editor
    {
        [SerializeField] private PropertyBinding _binding;
        [SerializeField] private MemberCollection<PropertyInfo> _sourceProperties;
        [SerializeField] private MemberCollection<PropertyInfo> _targetProperties;
        [SerializeField] private string[] _bindingModes = Enum.GetNames(typeof(BindingMode)).ToArray();

        private void OnEnable()
        {
            _binding = target as PropertyBinding;

            _sourceProperties = new MemberCollection<PropertyInfo>(_binding.SourceType, MemberFilters.SourceProperties);
            _targetProperties = new MemberCollection<PropertyInfo>(_binding.TargetType, MemberFilters.TargetProperties);
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.Space();

            _targetProperties.ChangeTargetTypeIfNecessary(_binding.TargetType);
            _sourceProperties.ChangeTargetTypeIfNecessary(_binding.SourceType);

            _binding.Source = (BindableMonoBehaviour)EditorGUILayout.ObjectField("Source", _binding.Source, typeof(BindableMonoBehaviour), true);
            _binding.Target = EditorGUILayout.ObjectField("Target", _binding.Target, typeof(UnityEngine.Object), true);

            if (!_binding.HasSourceAndTarget)
            {
                ResetBinding();
                return;
            }

            GuiLine();

            var sourceIndex = IndexedIdentifier.For(_binding.SourceIdentifier, _sourceProperties.Names);
            var targetIndex = IndexedIdentifier.For(_binding.TargetIdentifier, _targetProperties.Names);

            sourceIndex.Index = EditorGUILayout.Popup("Source Property", sourceIndex.Index, Nicify(_sourceProperties.Names));
            targetIndex.Index = EditorGUILayout.Popup("Target Property", targetIndex.Index, Nicify(_targetProperties.Names));
            _binding.BindingMode = (BindingMode)EditorGUILayout.Popup("Mode", (int)_binding.BindingMode, _bindingModes);

            if (!TwoWayAvailable && _binding.BindingMode == BindingMode.TwoWay)
            {
                EditorGUILayout.HelpBox("Not available. Will default to OneWay", MessageType.Info);
            }

            GuiLine();

            // Handle converters
            var _converterNames = ConversionProvider.AvailableConverterNames.Select(n => n.Replace("Coverter", string.Empty)).ToArray();
            var converterIdentifier = _binding.ConverterIdentifier;
            var converterIndex = IndexedIdentifier.For(converterIdentifier, ConversionProvider.AvailableConverterNames.ToList());
            converterIndex.Index = EditorGUILayout.Popup("Conversion", converterIndex.Index, _converterNames);

            var converterProperties = ConversionProvider.PropertiesFor(converterIdentifier);
            EditorGUI.indentLevel++;
            foreach (var prop in converterProperties)
            {
                object propertyValue = converterIdentifier.GetPropertyValue(prop.Name);
                object newValue = null;

                if (prop.PropertyType == typeof(int))
                {
                    newValue = EditorGUILayout.IntField(prop.Name, (int)propertyValue);
                }

                if (prop.PropertyType == typeof(float))
                {
                    newValue = EditorGUILayout.FloatField(prop.Name, (float)propertyValue);
                }

                if (prop.PropertyType == typeof(string))
                {
                    newValue = EditorGUILayout.TextField(prop.Name, (string)propertyValue);
                }

                if (prop.PropertyType == typeof(bool))
                {
                    newValue = EditorGUILayout.Toggle(prop.Name, (bool)propertyValue);
                }

                if (prop.PropertyType == typeof(Color))
                {
                    newValue = EditorGUILayout.ColorField(prop.Name, (Color)propertyValue);
                }

                if (prop.PropertyType == typeof(Color32))
                {
                    newValue = EditorGUILayout.ColorField(prop.Name, (Color32)propertyValue);
                }

                if (prop.PropertyType == typeof(Rect))
                {
                    newValue = EditorGUILayout.RectField(prop.Name, (Rect)propertyValue);
                }

                if (prop.PropertyType == typeof(LayerMask))
                {
                    newValue = EditorGUILayout.LayerField(prop.Name, (LayerMask)propertyValue);
                }

                if (prop.PropertyType == typeof(Vector2))
                {
                    newValue = EditorGUILayout.Vector2Field(prop.Name, (Vector2)propertyValue);
                }

                if (prop.PropertyType == typeof(Vector3))
                {
                    newValue = EditorGUILayout.Vector3Field(prop.Name, (Vector3)propertyValue);
                }

                if (prop.PropertyType == typeof(Vector4))
                {
                    newValue = EditorGUILayout.Vector4Field(prop.Name, (Vector4)propertyValue);
                }

                if (prop.PropertyType.IsEnum)
                {
                    newValue = EditorGUILayout.EnumPopup(prop.Name, (Enum)propertyValue);
                }

                converterIdentifier.SetPropertyValue(prop.Name, newValue);
            }
            EditorGUI.indentLevel--;

            if (EditorGUI.EndChangeCheck())
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }

        /// <summary>
        /// See: https://forum.unity.com/threads/horizontal-line-in-editor-window.520812/
        /// Also: How does Unity still doesn't have this implemented?
        /// </summary>
        private void GuiLine()
        {
            EditorGUILayout.Space();
            var rect = EditorGUILayout.GetControlRect(false, 1);
            rect.height = 1;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
            EditorGUILayout.Space();
        }

        private void ResetBinding()
        {
            _binding.BindingMode = default;
            _binding.SourceIdentifier.Name = string.Empty;
            _binding.TargetIdentifier.Name = string.Empty;
            _binding.ConverterIdentifier.Name = ConverterIdentifier.Default;
        }

        private bool TwoWayAvailable =>
            _binding != null &&
            _binding.HasSourceAndTarget &&
            UiEventLookup.HasEventFor(_binding.TargetType, _binding.TargetIdentifier.Name);

        private string[] Nicify(string[] input)
        {
            return input.Select(s => ObjectNames.NicifyVariableName(s)).ToArray();
        }
    }
}