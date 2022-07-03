using UnityEditor;
using UnityEngine;

namespace Endciv.Editor
{
	public class EditorHelper
	{
		public const string EditorToolsPath = "Endciv/";

		// Custom GUILayout progress bar.
		public static void ProgressBar(float value, string label, float size)
		{
			// Get a rect for the progress bar using the same margins as a textfield:
			Rect rect = GUILayoutUtility.GetRect(size, size, "TextArea");
			EditorGUI.ProgressBar(rect, value, label);
			GUILayout.Space(6);
		}
		// Custom GUILayout progress bar.
		public static string GetProgressBarTitle(string title, float current, float max)
		{
			return title + ": " + current.ToString("0.##") + " / " + max.ToString("0.##");
		}

		internal static void EntityPropertyProgressBar(string title, EntityProperty property)
		{
			ProgressBar(property.Progress, GetProgressBarTitle(title, property.Value, property.maxValue), 16);
		}

		[MenuItem("Endciv/Commands/Instantiate Selected Objects")]
		public static void InstantiateSelectedObjects()
		{
			var currentSelection = Selection.gameObjects;
			for (int i = 0; i < currentSelection.Length; i++)
			{
				var obj = GameObject.Instantiate(currentSelection[i]);
				obj.name = obj.name.Replace("(Clone)", "");
			}
		}
		/*
		[MenuItem("Endciv/Commands/Cleanup Selected Objects")]
		public static void CleanupSelectedObjects()
		{
			var currentSelection = Selection.gameObjects;
			for (int i = 0; i < currentSelection.Length; i++)
			{
				var obj = currentSelection[i];

				var comps = obj.GetComponents<Component>();
				foreach (var item in comps)
				{
					Debug.Log(item.ToString());
				}
			}
		}*/
	}
}