using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	[CustomEditor(typeof(GUIButtonStyle))]
	public class GUIButtonStyleEditor : GUIStyleEditor
	{
		public override void OnInspectorGUI()
		{

			EditorGUILayout.Space();

			GUIButtonStyle myTarget = (GUIButtonStyle)target;

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.Space();



			EditorGUILayout.BeginHorizontal();
			myTarget.hasColorNormal = EditorGUILayout.Toggle(CheckboxText, myTarget.hasColorNormal, GUILayout.Width(CheckboxWidth));
			myTarget.colorNormal = EditorGUILayout.ColorField("Color Normal", myTarget.colorNormal);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			myTarget.hasColorHighlighted = EditorGUILayout.Toggle(CheckboxText, myTarget.hasColorHighlighted, GUILayout.Width(CheckboxWidth));
			myTarget.colorHighlighted = EditorGUILayout.ColorField("Color Highlighted", myTarget.colorHighlighted);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			myTarget.hasColorPressed = EditorGUILayout.Toggle(CheckboxText, myTarget.hasColorPressed, GUILayout.Width(CheckboxWidth));
			myTarget.colorPressed = EditorGUILayout.ColorField("Color Pressed", myTarget.colorPressed);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			myTarget.hasColorDisabled = EditorGUILayout.Toggle(CheckboxText, myTarget.hasColorDisabled, GUILayout.Width(CheckboxWidth));
			myTarget.colorDisabled = EditorGUILayout.ColorField("Color Disabled", myTarget.colorDisabled);
			EditorGUILayout.EndHorizontal();

			var changes = EditorGUI.EndChangeCheck();
			if (changes) myTarget.unappliedChanges = true;
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