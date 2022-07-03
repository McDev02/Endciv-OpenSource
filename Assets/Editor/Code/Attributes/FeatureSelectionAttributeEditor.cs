using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Endciv.Editor
{
    [CustomPropertyDrawer(typeof(FeatureSelectionAttribute))]
    public class FeatureSelectionAttributeEditor : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var selectionAttribute = attribute as FeatureSelectionAttribute;
            var featureData = (FeatureStaticDataBase)property.serializedObject.targetObject;
            if (featureData == null)
            {
                return base.GetPropertyHeight(property, label);
            }
            if (featureData.entity == null)
            {
                return base.GetPropertyHeight(property, label);
            }
            if (property.propertyType != SerializedPropertyType.String)
            {
                return base.GetPropertyHeight(property, label);
            }
            List<string> existingFeatures = new List<string>();
            foreach (var data in featureData.entity.FeatureStaticData)
            {
                existingFeatures.Add(data.GetType().ToString());
            }
            if (existingFeatures.Count <= 0)
            {
                return base.GetPropertyHeight(property, label);
            }
            return EditorGUIUtility.singleLineHeight * (existingFeatures.Count + 1);
            
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var selectionAttribute = attribute as FeatureSelectionAttribute;
            var featureData = (FeatureStaticDataBase)property.serializedObject.targetObject;
            if(featureData == null)
            {
                EditorGUI.LabelField(position, label.text, "FeatureSelection cannot work outside of FeatureStaticData objects.");
                return;
            }
            if(featureData.entity == null)
            {
                EditorGUI.LabelField(position, label.text, "FeatureSelection cannot work with null Entity values.");
                return;
            }
            if(property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "FeatureSelection can only work with string values.");
                return;
            }
            List<string> existingFeatures = new List<string>();
            foreach(var data in featureData.entity.FeatureStaticData)
            {
                existingFeatures.Add(data.GetType().ToString());
            }
            if(existingFeatures.Count <= 0)
            {
                EditorGUI.LabelField(position, label.text, "No features attached on entity "+featureData.entity.name+"!");
                return;
            }
            bool[] selections = new bool[existingFeatures.Count];
            var selectedFeatures = GetFeatures(property.stringValue);
            Rect currentRect = position;
            currentRect.x += 20;
            currentRect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(position, label.text);
            EditorGUI.BeginChangeCheck();
            for (int i = 0; i < selections.Length; i++)
            {
                selections[i] = selectedFeatures.Contains(existingFeatures[i]);
                currentRect.y += EditorGUIUtility.singleLineHeight;
                selections[i] = EditorGUI.ToggleLeft(currentRect, existingFeatures[i], selections[i]);
            }            
            if(EditorGUI.EndChangeCheck())
            {
                for(int i = 0; i < selections.Length; i++)
                {
                    if(selections[i])
                    {
                        if(!selectedFeatures.Contains(existingFeatures[i]))
                        {
                            selectedFeatures.Add(existingFeatures[i]);
                        }
                    }
                    else
                    {
                        if (selectedFeatures.Contains(existingFeatures[i]))
                        {
                            selectedFeatures.Remove(existingFeatures[i]);
                        }
                    }
                }
                property.stringValue = MergeFeatures(selectedFeatures);
            }            
        }

        public List<string> GetFeatures(string value)
        {
            return value.Split('|').ToList();
        }

        public string MergeFeatures(List<string> features)
        {
            string value = string.Empty;
            foreach(var feature in features)
            {
                if (string.IsNullOrEmpty(feature))
                    continue;
                value += feature + "|";
            }
            if (value.Length > 0)
                value = value.Substring(0, value.Length - 1);
            return value;
        }
    }    
}
