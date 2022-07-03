using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Endciv.Editor
{
	public class LocaEditorWindow2 : EditorWindow
	{
		private bool hasDataChanged;
		private SortedDictionary<string, Dictionary<string, string>> localizationData;
		string[] localizationLanguages;
		Dictionary<string, string> keysToRename = new Dictionary<string, string>();
		List<string> renamedKeys = new List<string>();
		List<string> keysToDelete = new List<string>();

		string searchWord;
		string newEntryKey;
		List<string> newEntryValues;

		Vector2 tableScrollViewValue;
		int smallButtonWidth = 24;

		[MenuItem(EditorHelper.EditorToolsPath + "Localization/Open Loca Editor", false, 0)]
		public static void Open()
		{
			GetWindow<LocaEditorWindow2>(false, "Loca Editor", true).Show();
		}

		private void OnEnable()
		{
			Load();
		}

		private void OnDisable()
		{
			CheckSaveWarningDisplay();
		}


		private void OnGUI()
		{
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

			if (GUILayout.Button("Reload Files", EditorStyles.toolbarButton))
				Load();

			if (GUILayout.Button("Save", EditorStyles.toolbarButton))
				Save();

			GUILayout.FlexibleSpace();

			EditorGUILayout.EndHorizontal();

			DrawMainTable();
		}

		void DrawMainTable()
		{
			var drawWidth = position.width - 100;
			int minWidth = 50;

			int entryWidth = (int)Mathf.Max(minWidth, drawWidth / (localizationLanguages.Length + 1f));
			EditorGUILayout.BeginVertical();

			//Renamed keys info
			DrawRenamedKeysInfo();

			//Search Bar
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Search:", GUILayout.Width(60));
			if (GUILayout.Button("x", EditorStyles.miniButton, GUILayout.Width(18), GUILayout.Height(18)))
				searchWord = "";
			searchWord = EditorGUILayout.TextField(searchWord);
			EditorGUILayout.EndHorizontal();

			//Headlines
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Keys", GUILayout.Width(entryWidth));
			for (int i = 0; i < localizationLanguages.Length; i++)
			{
				GUILayout.Label(localizationLanguages[i], GUILayout.Width(entryWidth));
			}
			GUILayout.Space(smallButtonWidth);
			EditorGUILayout.EndHorizontal();

			//New Entry
			DrawTableNewEntry(entryWidth);
			EditorGUILayout.Space();

			//Entries
			tableScrollViewValue = EditorGUILayout.BeginScrollView(tableScrollViewValue);
			DrawTableEntries(entryWidth);

			//Delete entries
			for (int i = 0; i < keysToDelete.Count; i++)
			{
				localizationData.Remove(keysToDelete[i]);
				keysToRename.Remove(keysToDelete[i]);
				hasDataChanged = true;
			}

			EditorGUILayout.EndVertical();
			EditorGUILayout.EndScrollView();
		}

		void DrawTableNewEntry(int entryWidth)
		{
			EditorGUILayout.BeginHorizontal();
			newEntryKey = EditorGUILayout.TextField(newEntryKey, GUILayout.Width(entryWidth));
			for (int i = 0; i < localizationLanguages.Length; i++)
			{
				if (newEntryValues.Count <= i)
					newEntryValues.Add("");

				newEntryValues[i] = EditorGUILayout.TextField(newEntryValues[i], GUILayout.Width(entryWidth));
			}
			bool doesKeyExist = localizationData.ContainsKey(newEntryKey);

			EditorGUI.BeginDisabledGroup(doesKeyExist);
			if (GUILayout.Button("Add"))
			{
				var data = new Dictionary<string, string>();
				for (int i = 0; i < localizationLanguages.Length; i++)
				{
					data.Add(localizationLanguages[i], newEntryValues[i]);
				}
				localizationData.Add(newEntryKey, data);
				//newEntryKey = "";
				newEntryValues.Clear();
			}
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndHorizontal();
		}

		void DrawTableEntries(int entryWidth)
		{
			var hasSearchTerm = !string.IsNullOrEmpty(searchWord);
			keysToDelete.Clear();
			foreach (var languageEntry in localizationData)
			{
				//Filter
				if (hasSearchTerm)
				{
					searchWord = searchWord.ToLower();
					bool foundTerm = false;
					if (languageEntry.Key.ToLower().Contains(searchWord))
						foundTerm = true;
					else
					{
						//Search all values
						for (int i = 0; i < localizationLanguages.Length; i++)
						{
							var key = localizationLanguages[i];
							if (languageEntry.Value.ContainsKey(key))
							{
								if (languageEntry.Value[key].ToLower().Contains(searchWord))
								{
									foundTerm = true;
									break;
								}
							}
						}
					}
					//Search term not found
					if (!foundTerm)
						continue;
				}

				string keyExistsWarning = null;
				EditorGUILayout.BeginHorizontal();


				//Loca Key
				string tmp = languageEntry.Key;
				if (keysToRename.ContainsKey(languageEntry.Key))
				{
					tmp = keysToRename[languageEntry.Key];
				}

				tmp = EditorGUILayout.TextField(tmp, GUILayout.Width(entryWidth));
				if (tmp != languageEntry.Key)
				{
					if (keysToRename.ContainsKey(languageEntry.Key))
						keysToRename[languageEntry.Key] = tmp;
					else
						keysToRename.Add(languageEntry.Key, tmp);

					if (localizationData.ContainsKey(tmp))
						keyExistsWarning = languageEntry.Key;
					else
						keyExistsWarning = null;
				}
				//Remove change if values match
				else if (keysToRename.ContainsKey(languageEntry.Key))
				{
					keysToRename.Remove(languageEntry.Key);
				}

				//Language Entries
				var values = languageEntry.Value;
				for (int i = 0; i < localizationLanguages.Length; i++)
				{
					var key = localizationLanguages[i];
					var prev = values.ContainsKey(key) ? values[key] : "";
					tmp = EditorGUILayout.TextField(prev, GUILayout.Width(entryWidth));
					tmp = string.IsNullOrEmpty(tmp) ? "" : tmp;
					if (tmp != prev)
						hasDataChanged = true;
					values[key] = tmp;
				}

				//Delete whole key entry
				if (GUILayout.Button("X", GUILayout.Width(smallButtonWidth)))
				{
					keysToDelete.Add(languageEntry.Key);
				}
				EditorGUILayout.EndHorizontal();

				if (keyExistsWarning != null)
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.HelpBox($"Key already exists!", MessageType.Error);

					EditorGUILayout.BeginVertical();
					GUILayout.Label($"Original Key: {keyExistsWarning}");
					if (GUILayout.Button("Resore"))
					{
						keysToRename.Remove(keyExistsWarning);
					}
					EditorGUILayout.EndVertical();
					EditorGUILayout.EndHorizontal();
				}
			}
		}

		void DrawRenamedKeysInfo()
		{
			int errorCount = 0;
			foreach (var item in keysToRename)
			{
				if (localizationData.ContainsKey(item.Value))
					errorCount++;
			}

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.HelpBox($"Keys to rename: " + keysToRename.Count.ToString(), MessageType.Info);
			if (errorCount > 0)
				EditorGUILayout.HelpBox($"Key conflicts:" + errorCount.ToString(), MessageType.Error);

			EditorGUI.BeginDisabledGroup(keysToRename.Count == 0);
			if (GUILayout.Button("Rename keys"))
				RenameKeys();
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndHorizontal();

		}

		void RenameKeys()
		{
			//Rename keys
			renamedKeys.Clear();
			foreach (var item in keysToRename)
			{
				if (!localizationData.ContainsKey(item.Value))
				{
					var data = localizationData[item.Key];
					localizationData.Remove(item.Key);
					localizationData.Add(item.Value, data);
					renamedKeys.Add(item.Key);
					hasDataChanged = true;
				}
			}
			//Cleanup renamed Keys
			for (int i = 0; i < renamedKeys.Count; i++)
			{
				keysToRename.Remove(renamedKeys[i]);
			}
		}
		private bool CheckSaveWarningDisplay()
		{
			bool toReturn = false;
			if (keysToRename.Count > 0)
			{
				var rename = EditorUtility.DisplayDialog("Rename keys?", $"You have {keysToRename.Count} keys to rename.", "Rename valid changes", "Ignore changes");
				if (rename)
				{
					RenameKeys();
					toReturn = true;
				}

			}
			if (hasDataChanged)
			{
				var save = EditorUtility.DisplayDialog("Save?", "Loca data changed", "Save", "Ignore");
				if (save)
				{
					Save();
					toReturn = true;
				}
			}
			return toReturn;
		}


		private void Load()
		{
			var folder = LocalizationManager.Instance.LocaFolder;
			localizationLanguages = Directory.GetFiles(folder, "*.dat");

			newEntryKey = "";
			newEntryValues = new List<string>(localizationLanguages.Length);

			localizationData = new SortedDictionary<string, Dictionary<string, string>>();
			for (int i = 0; i < localizationLanguages.Length; i++)
			{
				var language = Path.GetFileNameWithoutExtension(localizationLanguages[i]);
				localizationLanguages[i] = language;

				var path = LocalizationManager.GetLocaFilePath(language);
				var data = LocalizationManager.LoadLocaJsonFile(path);

				foreach (var entry in data)
				{
					if (localizationData.ContainsKey(entry.Key))
					{
						var languageEntries = localizationData[entry.Key];
						if (!languageEntries.ContainsKey(language))
						{
							languageEntries.Add(language, entry.Value);
						}
					}
					else
					{
						var languageEntries = new Dictionary<string, string>();
						languageEntries.Add(language, entry.Value);
						localizationData.Add(entry.Key, languageEntries);
					}
				}
			}
		}


		public void Save()
		{
			for (int i = 0; i < localizationLanguages.Length; i++)
			{
				var language = localizationLanguages[i];
				var data = new Dictionary<string, string>();
				foreach (var entries in localizationData)
				{
					//Only add entry if it exists, this tells us later that the values are missing
					if (entries.Value.ContainsKey(language) && !string.IsNullOrEmpty(entries.Value[language]))
					{
						data.Add(entries.Key, entries.Value[language]);
					}
				}
				LocalizationManager.Instance.Save(language, data);
			}

			hasDataChanged = false;
		}
	}
}