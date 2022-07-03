using UnityEditor;
using Endciv;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.ComponentModel;

[CustomPropertyDrawer(typeof(NotificationCondition))]
public class NotificationConditionEditor : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.PropertyField(position, property, label, true);        
        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;
            var valueType = property.FindPropertyRelative("valueType");
            var condition = property.FindPropertyRelative("conditionOperator");
            Rect valueRect = new Rect(position.xMin, position.yMax - 37f, position.width, 17f);
            Rect conditionRect = new Rect(position.xMin, position.yMax - 17f, position.width, 17f);
            GUIContent content = new GUIContent("Value");
            var enumValues = Enum.GetValues(typeof(EConditionOperator));
            int limit = enumValues.Length;
            var typeEnum = (EValueType)valueType.enumValueIndex;
            if (typeEnum == EValueType.Bool || typeEnum == EValueType.String)
                limit = 2;
            if (condition.enumValueIndex >= limit)
                condition.enumValueIndex = limit - 1;
            condition.enumValueIndex = EditorGUI.Popup(conditionRect, "Condition Operator", condition.enumValueIndex, enumValues
                                                                                                  .Cast<Enum>()
                                                                                                  .Select(e => typeof(EConditionOperator).GetMember(e.ToString()).First())
                                                                                                  .Select(info => info.GetCustomAttribute<DescriptionAttribute>().Description)
                                                                                                  .Where(attribute => attribute != null)
                                                                                                  .Take(limit)
                                                                                                  .ToArray());
            switch ((EValueType)valueType.enumValueIndex)
            {
                case EValueType.Int:
                    {
                        var value = property.FindPropertyRelative("intValue");
                        value.intValue = EditorGUI.IntField(valueRect, content, value.intValue);
                    }
                    break;

                case EValueType.Float:
                    {
                        var value = property.FindPropertyRelative("floatValue");
                        value.floatValue = EditorGUI.FloatField(valueRect, content, value.floatValue);
                    }
                    break;

                case EValueType.Double:
                    {
                        var value = property.FindPropertyRelative("doubleValue");
                        value.doubleValue = EditorGUI.DoubleField(valueRect, content, value.doubleValue);
                    }
                    break;

                case EValueType.String:
                    {
                        var value = property.FindPropertyRelative("stringValue");
                        value.stringValue = EditorGUI.TextField(valueRect, content, value.stringValue);
                    }
                    break;

                case EValueType.Bool:
                    {
                        var value = property.FindPropertyRelative("boolValue");
                        value.boolValue = EditorGUI.Toggle(valueRect, content, value.boolValue);                        
                    }                    
                    break;

                case EValueType.Vector2:
                    {
                        var value = property.FindPropertyRelative("vector2Value");
                        value.vector2Value = EditorGUI.Vector2Field(valueRect, content, value.vector2Value);
                    }
                    break;

                case EValueType.Vector3:
                    {
                        var value = property.FindPropertyRelative("vector3Value");
                        value.vector3Value = EditorGUI.Vector3Field(valueRect, content, value.vector3Value);
                    }
                    break;

                case EValueType.Vector4:
                    {
                        var value = property.FindPropertyRelative("vector4Value");
                        value.vector4Value = EditorGUI.Vector4Field(valueRect, content, value.vector4Value);
                    }
                    break;

                case EValueType.Vector2Int:
                    {
                        var value = property.FindPropertyRelative("vector2IntValue");
                        value.vector2IntValue = EditorGUI.Vector2IntField(valueRect, content, value.vector2IntValue);
                    }
                    break;

                case EValueType.Vector3Int:
                    {
                        var value = property.FindPropertyRelative("vector3IntValue");
                        value.vector3IntValue = EditorGUI.Vector3IntField(valueRect, content, value.vector3IntValue);
                    }
                    break;
            }
            EditorGUI.indentLevel--;

        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.isExpanded)
            return EditorGUI.GetPropertyHeight(property) + 40f;
        return EditorGUI.GetPropertyHeight(property);
    }
}