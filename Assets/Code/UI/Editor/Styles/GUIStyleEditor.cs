using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	[CustomEditor(typeof(GUIStyle))]
	public class GUIStyleEditor : UnityEditor.Editor
    {
	protected	const int CheckboxWidth = 26;
	protected	const string CheckboxText = "";

		public override void OnInspectorGUI()
		{
			GUIStyle myTarget = (GUIStyle)target;

			EditorGUILayout.BeginHorizontal();
			GUIStyle.autoUpdate = EditorGUILayout.Toggle("Auto Update (Editor only)", GUIStyle.autoUpdate);
			if (GUILayout.Button("Update")) myTarget.forceUpdate = true;
			EditorGUILayout.Space();
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(10);

			//Draw enable buttons
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.Space();
			if (GUILayout.Button("Enable All")) myTarget.EnableAll();
			EditorGUILayout.Space();
			if (GUILayout.Button("Disable All")) myTarget.DisableAll();
			EditorGUILayout.Space();
			EditorGUILayout.EndHorizontal();
		}
	}
}