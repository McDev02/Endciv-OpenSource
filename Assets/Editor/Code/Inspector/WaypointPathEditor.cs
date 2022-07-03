using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Endciv.Editor
{
	[CustomEditor(typeof(WaypointPath))]
	public class WaypointPathEditor : UnityEditor.Editor
	{
		private bool editWaypoints;

		public override void OnInspectorGUI()
		{
			var myTarget = (WaypointPath)target;
			EditorGUILayout.LabelField("Waypoints");
			EditorGUILayout.Space();
			//if (myTarget.points != null && myTarget.points.Length > 0)
			//	selectedPoint = (int)EditorGUILayout.Slider("Selection : ", selectedPoint, -1, myTarget.points.Length - 1);
			//else
			//	selectedPoint = -1;
			if (GUILayout.Button(editWaypoints ? "Hide Waypoints" : "Edit Waypoints"))
				editWaypoints = !editWaypoints;

			EditorGUILayout.Space();
			serializedObject.Update();
			SerializedProperty prop = serializedObject.FindProperty("points");
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(prop, true);
			if (EditorGUI.EndChangeCheck())
				serializedObject.ApplyModifiedProperties();
		}

		private void OnSceneGUI()
		{
			var myTarget = (WaypointPath)target;
			if (myTarget.points == null)
				return;
			Handles.color = Color.green;
			var localToWorld = myTarget.transform.localToWorldMatrix;
			var worldToLocal = myTarget.transform.worldToLocalMatrix;
			Vector3 offset = new Vector3(0, 0.1f, 0);
			Vector3[] lineDrawArray = new Vector3[myTarget.points.Length];

			for (int i = 0; i < myTarget.points.Length; i++)
			{
				var pos = myTarget.points[i];
				var wpos = localToWorld.MultiplyPoint(myTarget.points[i]);
				if (editWaypoints)
				{
					wpos = Handles.PositionHandle(wpos, Quaternion.identity);
					pos = worldToLocal.MultiplyPoint(wpos);
				}
				pos.y = 0;
				myTarget.points[i] = pos;
				lineDrawArray[i] = wpos + offset;
			}

			Handles.DrawAAPolyLine(lineDrawArray);
			//SceneView.RepaintAll();
		}
	}
}