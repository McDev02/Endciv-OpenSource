using UnityEditor;
using UnityEngine;

namespace Endciv.Editor
{
	[CustomEditor(typeof(CitizenShedule))]
	public class CitizenSheduleEditor : UnityEditor.Editor
	{
		int deleteID;

		public override void OnInspectorGUI()
		{
			var context = (CitizenShedule)target;

			//DrawDefaultInspector();
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.LabelField("Shedule", EditorStyles.boldLabel);
			EditorGUILayout.BeginVertical();
			int size;
			int fullsize = 0;
			int scale = 500;

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("New Entry", GUILayout.Width(100)))
				context.states.Add(new CitizenShedule.SheduleState(1));
			if (GUILayout.Button("Resort", GUILayout.Width(100)))
				context.states.Sort((a, b) => a.beginTime.CompareTo(b.beginTime));
			EditorGUILayout.EndHorizontal();

			for (int i = 0; i < context.states.Count; i++)
			{
				var state = context.states[i];
				state.id = i;
				float nextTime = (i >= context.states.Count - 1) ? 1 : context.states[i + 1].beginTime;
				float diff = nextTime - state.beginTime;
				size = (int)(diff * scale);
				fullsize += size;

				EditorGUILayout.BeginHorizontal();
				state.type = (CitizenShedule.ESheduleType)EditorGUILayout.EnumPopup(context.states[i].type);
				state.beginTime = Mathf.Clamp01(EditorGUILayout.FloatField("Begin Time", context.states[i].beginTime));

				var hours = (int)(state.beginTime * 24);
				var minutes = (int)((state.beginTime * 24 - hours) * 60);
				GUILayout.Label($"Time: {hours.ToString("00")}:{minutes.ToString("00")}");

				hours = (int)(diff * 24);
				minutes = (int)((diff * 24 - hours) * 60);
				GUILayout.Label($"Duration: {hours.ToString()}:{minutes.ToString("00")}");

				if (GUILayout.Button("X", GUILayout.Width(20))) DeleteEntry(i);
				EditorGUILayout.EndHorizontal();
				context.states[i] = state;

				Rect rect = GUILayoutUtility.GetRect(size, size);
				EditorGUI.HelpBox(rect, state.type.ToString(), MessageType.None);
				GUILayout.Space(6);
			}

			EditorGUILayout.EndVertical();

			if (deleteID > 0 && deleteID <= context.states.Count)
				context.states.RemoveAt(deleteID);
			deleteID = -1;

			if (EditorGUI.EndChangeCheck())
				EditorUtility.SetDirty(context);
		}

		void DeleteEntry(int id)
		{
			deleteID = id;
		}
	}
}