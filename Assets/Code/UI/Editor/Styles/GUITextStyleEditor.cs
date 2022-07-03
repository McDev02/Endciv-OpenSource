using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	[CustomEditor(typeof(GUITextStyle))]
	public class GUITextStyleEditor : GUIStyleEditor
	{
		public override void OnInspectorGUI()
		{

			EditorGUILayout.Space();

			GUITextStyle myTarget = (GUITextStyle)target;

			EditorGUI.BeginChangeCheck();


			EditorGUILayout.BeginHorizontal();
			myTarget.hasFont = EditorGUILayout.Toggle(CheckboxText, myTarget.hasFont, GUILayout.Width(CheckboxWidth));
			EditorGUI.BeginDisabledGroup(!myTarget.hasFont);
			myTarget.font = (Font)EditorGUILayout.ObjectField("Font", myTarget.font, typeof(Font), false);
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			myTarget.hasFontStyle = EditorGUILayout.Toggle(CheckboxText, myTarget.hasFontStyle, GUILayout.Width(CheckboxWidth));
			EditorGUI.BeginDisabledGroup(!myTarget.hasFontStyle);
			myTarget.fontStyle = (FontStyle)EditorGUILayout.EnumPopup("Font Style", myTarget.fontStyle);
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			myTarget.hasFontSize = EditorGUILayout.Toggle(CheckboxText, myTarget.hasFontSize, GUILayout.Width(CheckboxWidth));
			EditorGUI.BeginDisabledGroup(!myTarget.hasFontSize);
			myTarget.fontSize = EditorGUILayout.IntField("Font Size", myTarget.fontSize);
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			myTarget.hasColor = EditorGUILayout.Toggle(CheckboxText, myTarget.hasColor, GUILayout.Width(CheckboxWidth));
			EditorGUI.BeginDisabledGroup(!myTarget.hasColor);
			myTarget.color = EditorGUILayout.ColorField("Color", myTarget.color);
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			myTarget.hasRichText = EditorGUILayout.Toggle(CheckboxText, myTarget.hasRichText, GUILayout.Width(CheckboxWidth));
			EditorGUI.BeginDisabledGroup(!myTarget.hasRichText);
			myTarget.richText = EditorGUILayout.Toggle("RichText", myTarget.richText);
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			myTarget.hasRaycastTarget = EditorGUILayout.Toggle(CheckboxText, myTarget.hasRaycastTarget, GUILayout.Width(CheckboxWidth));
			EditorGUI.BeginDisabledGroup(!myTarget.hasRaycastTarget);
			myTarget.raycastTarget = EditorGUILayout.Toggle("Raycast Target", myTarget.raycastTarget);
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndHorizontal();

			var changes = EditorGUI.EndChangeCheck();
			if (changes)
			{
				myTarget.unappliedChanges = true;
				EditorUtility.SetDirty(myTarget);
			}
			if (myTarget.forceUpdate || myTarget.unappliedChanges && GUIStyle.autoUpdate)
			{
				myTarget.ApplyChangesToAllObjects();
				myTarget.unappliedChanges = false;
			}

			GUILayout.Space(20);
			base.OnInspectorGUI();
		}
	}
}