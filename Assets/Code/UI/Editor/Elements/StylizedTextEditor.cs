using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	[CustomEditor(typeof(StylizedText))]
	public class StylizedTextEditor: UnityEditor.UI.TextEditor
	{

		public override void OnInspectorGUI()
		{
			StylizedText myTarget = (StylizedText)target;

			int styleCount = Mathf.Clamp(EditorGUILayout.IntField("Styles Count", myTarget.Styles.Count), 1, 32);
			int safecount = 0;
			while (safecount < 1000 && styleCount != myTarget.Styles.Count)
			{
				safecount++;
				if (myTarget.Styles.Count < styleCount)
					myTarget.Styles.Add(null);
				else myTarget.Styles.RemoveAt(myTarget.Styles.Count - 1);
			}

			EditorGUI.BeginChangeCheck();
			for (int i = 0; i < styleCount; i++)
			{
				var obj = myTarget.Styles[i] as GUITextStyle;
				myTarget.Styles[i] = (GUITextStyle)EditorGUILayout.ObjectField("Style", obj, typeof(GUITextStyle), false);
			}
			//May sort out null references

			if (EditorGUI.EndChangeCheck())
			{
				myTarget.UpdateStyle();
			}

			EditorGUILayout.Space();

			base.OnInspectorGUI();
		}
	}
}