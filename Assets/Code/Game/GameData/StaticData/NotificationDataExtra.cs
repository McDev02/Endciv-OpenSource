using UnityEngine;
using System;
using System.ComponentModel;

namespace Endciv
{
    public enum ENotificationType
    {
        Notification,
        Objective,
        Achievement
    }

    [Serializable]
    public enum EConditionOperator
    {
        [Description("==")]
        EQUAL,
        [Description("!=")]
        NOT_EQUAL,
        [Description("<")]
        LESS_THAN,        
        [Description(">")]
        GREATER_THAN,
        [Description("<=")]
        LESS_EQUAL_THAN,
        [Description(">=")]
        GREATER_EQUAL_THAN
        
    }

    [Serializable]
    public enum EValueType
    {
        Int,        
        Float,
        Double,
        String,
        Bool,
        Vector2,
        Vector3,
        Vector4,
        Vector2Int,
        Vector3Int
    }

    [Serializable]
    public class NotificationConditionSaveData : ISaveable
    {
        public string valueName;        
        public int intValue;
        public int intOffset;
        public float floatValue;
        public float floatOffset;
        public double doubleValue;
        public double doubleOffset;

        public ISaveable CollectData()
        {
            return this;
        }
    }

    [Serializable]
    public class NotificationCondition : ISaveable, ILoadable<NotificationConditionSaveData>
    {        
        [HideInInspector]
        public EConditionOperator conditionOperator;
        public string valueName;
        public bool isRelative;
        public EValueType valueType;        
        public T GetValue<T>()
        {
            switch (valueType)
            {
                case EValueType.Int:
                    return (T)Convert.ChangeType(intValue, typeof(T));
                case EValueType.Float:
                    return (T)Convert.ChangeType(floatValue, typeof(T));
                case EValueType.Double:
                    return (T)Convert.ChangeType(doubleValue, typeof(T));
                case EValueType.String:
                    return (T)Convert.ChangeType(stringValue, typeof(T));
                case EValueType.Bool:
                    return (T)Convert.ChangeType(boolValue, typeof(T));
                case EValueType.Vector2:
                    return (T)Convert.ChangeType(vector2Value, typeof(Vector2));
                case EValueType.Vector3:
                    return (T)Convert.ChangeType(vector3Value, typeof(Vector3));
                case EValueType.Vector4:
                    return (T)Convert.ChangeType(vector4Value, typeof(Vector4));
                case EValueType.Vector2Int:
                    return (T)Convert.ChangeType(vector2IntValue, typeof(Vector2Int));
                case EValueType.Vector3Int:
                    return (T)Convert.ChangeType(vector3IntValue, typeof(Vector3Int));
                default:
                    return default(T);
            }
        }

        public object GetValueRaw()
        {
            switch (valueType)
            {
                case EValueType.Int:
                    return intValue;
                case EValueType.Float:
                    return floatValue;
                case EValueType.Double:
                    return doubleValue;
                case EValueType.String:
                    return stringValue;
                case EValueType.Bool:
                    return boolValue;
                case EValueType.Vector2:
                    return vector2Value;
                case EValueType.Vector3:
                    return vector3Value;
                case EValueType.Vector4:
                    return vector4Value;
                case EValueType.Vector2Int:
                    return vector2IntValue;
                case EValueType.Vector3Int:
                    return vector3IntValue;
                default:
                    return null;
            }
        }

        [HideInInspector]
        public int intValue;
        [HideInInspector]
        public int intOffset;
        [HideInInspector]
        public float floatValue;
        [HideInInspector]
        public float floatOffset;
        [HideInInspector]
        public double doubleValue;
        [HideInInspector]
        public double doubleOffset;
        [HideInInspector]
        public string stringValue;
        [HideInInspector]
        public bool boolValue;
        [HideInInspector]
        public Vector2 vector2Value;
        [HideInInspector]
        public Vector3 vector3Value;
        [HideInInspector]
        public Vector4 vector4Value;
        [HideInInspector]
        public Vector2Int vector2IntValue;
        [HideInInspector]
        public Vector3Int vector3IntValue;

        public NotificationCondition Copy()
        {
            var condition = new NotificationCondition();
            condition.conditionOperator = conditionOperator;
            condition.valueName = valueName;
            condition.isRelative = isRelative;
            condition.valueType = valueType;
            condition.intValue = intValue;
            condition.intOffset = intOffset;
            condition.floatValue = floatValue;
            condition.floatOffset = floatOffset;
            condition.doubleValue = doubleValue;
            condition.doubleOffset = doubleOffset;
            condition.stringValue = stringValue;        
            condition.boolValue = boolValue;        
            condition.vector2Value = vector2Value;            
            condition.vector3Value = vector3Value;        
            condition.vector4Value = vector4Value;        
            condition.vector2IntValue = vector2IntValue;        
            condition.vector3IntValue = vector3IntValue;
            return condition;
        }

        public ISaveable CollectData()
        {
            if (!isRelative)
                return null;
            if (valueType != EValueType.Int && valueType != EValueType.Float && valueType != EValueType.Double)
                return null;
            var data = new NotificationConditionSaveData();
            data.valueName = valueName;
            data.intValue = intValue;
            data.intOffset = intOffset;
            data.floatValue = floatValue;
            data.floatOffset = floatOffset;
            data.doubleValue = doubleValue;
            data.doubleOffset = doubleOffset;
            return data;

        }

        public void ApplySaveData(NotificationConditionSaveData data)
        {
            if (data == null)
                return;
            intValue = data.intValue;
            intOffset = data.intOffset;
            floatValue = data.floatValue;
            floatOffset = data.floatOffset;
            doubleValue = data.doubleValue;
            doubleOffset = data.doubleOffset;
        }

    }
}