using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	[CustomEditor(typeof(LocalizedText))]
	public class LocalizedTextEditor : StylizedTextEditor
	{
		private SerializedProperty locaIDProperty;

		protected override void OnEnable()
		{
			locaIDProperty = serializedObject.FindProperty("locaID");

			base.OnEnable();
		}

		public override void OnInspectorGUI()
		{
			var context = (LocalizedText)target;

			EditorGUI.BeginChangeCheck();
			context.textStyle = (LocalizationManager.ETextStyle)EditorGUILayout.EnumPopup(context.textStyle, GUILayout.Width(80));

			var oldValue = context.locaID;
			EditorGUILayout.PropertyField(locaIDProperty);
			var newval = locaIDProperty.stringValue;

			if (newval != oldValue)
			{
				Undo.RecordObject(context, "LocaID changed");
				context.locaID = newval;
			}
			if (EditorGUI.EndChangeCheck() || newval != oldValue)
			{
				context.UpdateText();
			}

			//LocalizedText myTarget = (LocalizedText)target;
			//
			//myTarget.locaID = EditorGUILayout.TextField("LocaID", myTarget.locaID);
			//EditorGUILayout.Space();

			base.OnInspectorGUI();
		}
	}
}