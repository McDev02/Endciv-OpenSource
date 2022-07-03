using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	[CustomEditor(typeof(StylizedButton))]
	public class StylizedButtonEditor: UnityEditor.UI.ButtonEditor
	{

		public override void OnInspectorGUI()
		{
			StylizedButton myTarget = (StylizedButton)target;

			int styleCount = Mathf.Clamp(EditorGUILayout.IntField("Styles Count", myTarget.Styles.Count), 0, 32);
			int safecount = 0;
			while (safecount < 1000 && styleCount != myTarget.Styles.Count)
			{
				safecount++;
				if (myTarget.Styles.Count < styleCount)
					myTarget.Styles.Add(null);
				else myTarget.Styles.RemoveAt(myTarget.Styles.Count - 1);
			}

			for (int i = 0; i < styleCount; i++)
			{
				var obj = myTarget.Styles[i] as GUIButtonStyle;
				myTarget.Styles[i] = (GUIButtonStyle)EditorGUILayout.ObjectField("Style", obj, typeof(GUIButtonStyle), false);
			}
			//May sort out null references

			EditorGUILayout.Space();

			base.OnInspectorGUI();
		}
	}
}