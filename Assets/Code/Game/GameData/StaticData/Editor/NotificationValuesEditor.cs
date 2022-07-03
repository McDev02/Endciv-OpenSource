using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	[CustomEditor(typeof(NotificationValues))]
	public class NotificationValuesEditor : UnityEditor.Editor
	{
		protected const int CheckboxWidth = 26;
		protected const string CheckboxText = "";

		private string newName = "";
		private EValueType newValue = EValueType.Int;

		int editEntry = -1;
		bool saveEditedKey;
		string editKeyNew;
		EValueType editValueNew;
		int deleteEntry;
		bool reallyDelete;

		void ResetEditor()
		{
			saveEditedKey = false;
			reallyDelete = false;
		}

		void Awake()
		{
			newName = "";
			newValue = EValueType.Int;
			deleteEntry = -1;
			editEntry = -1;
			ResetEditor();
		}

		public override void OnInspectorGUI()
		{
			NotificationValues myTarget = (NotificationValues)target;

			if (!myTarget.DoValuesMatch())
			{
				GUILayout.Label("Values do not match!");
				return;
			}

			ResetEditor();

			var names = myTarget.names;
			var values = myTarget.values;
			if (deleteEntry >= 0)
			{
				if (myTarget.ValueExists(deleteEntry))
				{
					GUILayout.Label($"Really delete entry #{deleteEntry}?");
					GUILayout.Label($"Name: {names[deleteEntry]}");
					GUILayout.Label($"Value: {values[deleteEntry]}");

					EditorGUILayout.BeginHorizontal();
					if (GUILayout.Button("Yes"))
					{
						reallyDelete = true;
					}
					if (GUILayout.Button("Nope"))
					{
						deleteEntry = -1;
						ResetEditor();
					}
					EditorGUILayout.EndHorizontal();
				}
				else
				{
					GUILayout.Label($"Really delete entry #{deleteEntry}?");

					if (GUILayout.Button("Nope"))
					{
						deleteEntry = -1;
						ResetEditor();
					}
				}
			}

			else
			{
				//Draw new entry input field
				EditorGUILayout.BeginHorizontal();
				newName = EditorGUILayout.TextField(newName);
				//do more checks, only a.Z and underscore? Maybe digits?
				newName = newName.Replace(' ', '_');
				newValue = (EValueType)EditorGUILayout.EnumPopup(newValue, GUILayout.Width(80));

				if (GUILayout.Button("Create New"))
				{
					if (myTarget.AddValue(newName, newValue))
					{
						newName = "";
						//newValue = EValueType.Int;
					}
					else
						newName = "Error, already existing?";
				}
				EditorGUILayout.EndHorizontal();

				//Draw entires
				for (int i = 0; i < names.Count; i++)
				{
					EditorGUILayout.BeginHorizontal();
					GUILayout.Label(i.ToString(), GUILayout.Width(18));
					if (editEntry == i)
					{
						editKeyNew = EditorGUILayout.TextField(editKeyNew);
						editValueNew = (EValueType)EditorGUILayout.EnumPopup(editValueNew, GUILayout.Width(80));
						if (GUILayout.Button("Save", GUILayout.Width(42)))
						{
							saveEditedKey = true;
						}
						if (GUILayout.Button("X", GUILayout.Width(20)))
						{
							editEntry = -1;
						}
					}
					else
					{
						EditorGUILayout.TextField(names[i]);
						EditorGUILayout.LabelField(values[i].ToString(), GUILayout.Width(80));
						if (GUILayout.Button("Edit", GUILayout.Width(42)))
						{
							editEntry = i;
							editKeyNew = names[i];
							editValueNew = values[i];
						}
						if (GUILayout.Button("X", GUILayout.Width(20)))
						{
							deleteEntry = i;
						}
					}
					EditorGUILayout.EndHorizontal();
				}
			}


			if (deleteEntry >= 0 && reallyDelete)
			{
				myTarget.RemoveValue(deleteEntry);
				ResetEditor();
				deleteEntry = -1;
			}
			if (saveEditedKey)
			{
				myTarget.UpdateValue(editEntry, editKeyNew, editValueNew);
				saveEditedKey = false;
				editEntry = -1;
				ResetEditor();
			}
		}
	}
}