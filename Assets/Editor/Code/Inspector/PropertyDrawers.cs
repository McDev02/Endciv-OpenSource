using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;

namespace Endciv.Editor
{
	[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
	public class ReadOnlyAttributeDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			label.text = label.text + " [Read Only]";
			GUI.enabled = false;
			EditorGUI.PropertyField(position, property, label);
			GUI.enabled = true;
		}
	}

	[CustomPropertyDrawer(typeof(TooltipAttribute))]
    public class TooltipDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label.tooltip = ((TooltipAttribute)attribute).Tooltip;
            EditorGUI.PropertyField(position, property, label);
            label.tooltip = null;
        }
    }

    [CustomPropertyDrawer(typeof(EnumMaskAttribute))]
    public class EnumMaskDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EnumMaskAttribute flagSettings = (EnumMaskAttribute)attribute;
            Enum targetEnum = GetBaseProperty<Enum>(property);

            EditorGUI.BeginProperty(position, label, property);
            Enum enumNew = EditorGUI.EnumFlagsField(position, label.text, targetEnum);
            property.intValue = (int)Convert.ChangeType(enumNew, targetEnum.GetType());
            EditorGUI.EndProperty();
        }

        private T GetBaseProperty<T>(SerializedProperty prop)
        {
            string[] separatedPaths = prop.propertyPath.Split('.');

            var reflectionTarget = prop.serializedObject.targetObject as object;
            // Walk down the path to get the target object
            foreach (var path in separatedPaths)
            {
                FieldInfo fieldInfo = reflectionTarget.GetType().GetField(path);
                reflectionTarget = fieldInfo.GetValue(reflectionTarget);
            }
            return (T)reflectionTarget;
        }
    }


    [CustomPropertyDrawer(typeof(LocaIdAttribute))]
    public class LocaIdAttributePropertyDrawer : PropertyDrawer
    {
        public static bool HideInfo = false;

        private static List<string> m_LocaIds;

        private GenericMenu m_ContextMenu;
        private string m_Preview;
        private string m_SetValue;
        private string m_HelpBoxText = "Loca data not loaded...";
        private bool m_FirstCheck;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = base.GetPropertyHeight(property, label);
            if (!HideInfo) height += 16 * 3;
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!m_FirstCheck)
            {
                m_FirstCheck = true;
                Check(property.stringValue);
            }

            var valueRect = position;

            if (!HideInfo) valueRect.height -= 16 * 3;

            var infoRect = position;
            infoRect.y += valueRect.height;
            infoRect.height -= valueRect.height;

            if (Event.current.type == EventType.Repaint && m_Preview != null)
            {
                var valuePosition = EditorGUI.IndentedRect(valueRect);
                valuePosition.x += EditorGUIUtility.labelWidth;
                valuePosition.width -= EditorGUIUtility.labelWidth;

                GUI.color = new Color(1, 1, 1, 0.4f);
                GUI.Label(valuePosition, m_Preview);
                GUI.color = Color.white;
            }

            valueRect.xMax -= 35;

            if (m_SetValue != null)
            {
                property.stringValue = m_SetValue;
                m_SetValue = null;
            }

            var text = property.stringValue;
            var newText = EditorGUI.TextField(valueRect, label, text);

            if (text != newText)
            {
                property.stringValue = newText;

                Check(newText);
            }

            valueRect.x = valueRect.xMax;
            valueRect.width = 15;

            if (GUI.Button(valueRect, GUIContent.none, EditorStyles.radioButton))
            {
                Check(newText);
                GenericMenu.MenuFunction2 callback = (a) =>
                {
                    m_SetValue = (string)a;
                    m_Preview = null;
                    Check(m_SetValue);
                };

                m_ContextMenu = new GenericMenu();
                foreach (var item in m_LocaIds)
                {
                    m_ContextMenu.AddItem(new GUIContent(item), false, callback, item);
                }
                m_ContextMenu.ShowAsContext();
            }

            valueRect.x = valueRect.xMax;
            valueRect.width = 20;

            if (GUI.Button(valueRect, "R", EditorStyles.miniButton))
            {
                Debug.Log("Reload loca!\n");
                Load(true);
                Check(newText);
            }

            if (!HideInfo)
            {
                EditorGUI.HelpBox(infoRect, m_HelpBoxText, MessageType.None);
            }
        }

        private void Check(string current)
        {
            Load();

            m_HelpBoxText = "Invalid ID";
            m_Preview = null;

            if (!string.IsNullOrEmpty(current))
            {
                for (int i = 0; i < m_LocaIds.Count; i++)
                {
                    if (m_LocaIds[i].IndexOf(current, 0, Mathf.Min(current.Length, m_LocaIds[i].Length), StringComparison.InvariantCultureIgnoreCase) != -1)
                    {
                        if (string.Equals(m_LocaIds[i], current, StringComparison.InvariantCultureIgnoreCase))
                        {
                            m_HelpBoxText = "'" + LocalizationManager.GetText(m_LocaIds[i]) + "'";
                        }
                        else
                        {
                            m_HelpBoxText = "'" + LocalizationManager.GetText(m_LocaIds[i]) + "'";
                            m_Preview = current + m_LocaIds[i].Remove(0, current.Length);
                        }
                        break;
                    }
                }
            }
        }

        private void Load(bool forceReload = false)
        {
            if (LocalizationManager.m_LocaText == null || forceReload)
            {
                LocalizationManager.Load();
            }
            if (m_LocaIds == null || forceReload || LocalizationManager.m_LocaText.Count != m_LocaIds.Count)
            {
                if (m_LocaIds == null)
                    m_LocaIds = new List<string>();
                else
                    m_LocaIds.Clear();
                m_LocaIds.AddRange(LocalizationManager.m_LocaText.Keys);
                m_LocaIds.Sort(StringComparer.InvariantCultureIgnoreCase);
            }
        }
    }

	[CustomPropertyDrawer(typeof(StaticDataIDAttribute))]
	public class StaticDataIDAttributeDrawer : PropertyDrawer
	{
		private SimpleEntityFactory factory;
		private StaticDataIDAttribute idAttribute;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{			
			if(property.propertyType != SerializedPropertyType.String)
			{
				EditorGUI.PropertyField(position, property, label);				
				return;
			}			
			if (factory == null)
			{
				factory = new SimpleEntityFactory();
				idAttribute = (StaticDataIDAttribute)attribute;
				factory.Setup(idAttribute.path, null);
			}
			var ids = factory.EntityStaticData.Keys.ToList();
			if(idAttribute.requiredTypes != null && idAttribute.requiredTypes.Length > 0)
			{
				for (int i = ids.Count - 1; i >= 0; i--)
				{
					var entityData = factory.EntityStaticData[ids[i]];
					bool isValid = true;
					foreach(var type in idAttribute.requiredTypes)
					{
						if (!typeof(FeatureStaticDataBase).IsAssignableFrom(type))
							continue;
						if(!entityData.HasFeature(type))
						{
							isValid = false;
							break;
						}
					}
					if(!isValid)
					{
						ids.RemoveAt(i);
					}
				}
			}
			
			ids.Insert(0, "None");
			GUIContent[] contents = new GUIContent[ids.Count];
			for(int i = 0; i < contents.Length; i++)
			{
				contents[i] = new GUIContent(ids[i]);
			}
			EditorGUI.BeginProperty(position, label, property);
			int currentIndex = ids.IndexOf(property.stringValue);
			if (currentIndex < 0)
				currentIndex = 0;
			currentIndex = EditorGUI.Popup(position, label, currentIndex, contents);
			property.stringValue = ids[currentIndex];
			EditorGUI.EndProperty();
		}
	}
}
