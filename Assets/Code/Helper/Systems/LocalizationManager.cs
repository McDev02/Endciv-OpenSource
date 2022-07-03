using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;

namespace Endciv
{
	public class LocalizationManager : ResourceSingleton<LocalizationManager>
	{
		public const string StructurePath = "#Structures/";
		public const string ResourcePath = "#Resources/";
		public const string CropPath = "#Crops/";
		public const string DEFAULT_LANGUAGE = "en_us";

		[SerializeField] private string m_LocalizationFolder = null;

		public enum ETextStyle { Normal, Lowercase, Uppercase }
		public enum ETextVersion { Singular, Plural }

		public static Action OnLanguageChanges;

		[NonSerialized] public static Dictionary<string, string> m_LocaText;
		static bool m_Loaded;

		public string LocaFolder
		{
			get { return Application.streamingAssetsPath + "/" + m_LocalizationFolder; }
		}

		public static string GetText(string locaId, ETextVersion singularOrPlural, ETextStyle textStyle = ETextStyle.Normal)
		{
			var newLocaId = locaId + (singularOrPlural == ETextVersion.Plural ? "_p" : "_s");

			string output;
			if (!GetTextSafely(newLocaId, out output, textStyle))
			{
				//try normal string instead
				if (!GetTextSafely(locaId, out output, textStyle))
				{
					output = "Missing: " + output;
				}
				else return output;
			}
			return output;
		}
		public static string GetText(string locaId, ETextStyle textStyle = ETextStyle.Normal)
		{
			string output;
			if (!GetTextSafely(locaId, out output, textStyle))
			{
				output = "Missing: " + output;
			}
			return output;
		}

		public static  string GetMissingText()
		{
			return GetText("#Loca/MissingText");
		}

		public static bool GetTextSafely(string locaId, out string output, ETextStyle textStyle = ETextStyle.Normal)
		{
			if (locaId == null)
			{
				output = string.Empty;
				return false;
			}
#if !UNITY_EDITOR
			if (!m_Loaded)
				Debug.LogError("Loca file not loaded");//	Load();
#endif
			string text;
			if (!string.IsNullOrEmpty(locaId)
				&& m_LocaText != null
				&& m_LocaText.TryGetValue(locaId, out text)
				&& !string.IsNullOrEmpty(text))
			{
				switch (textStyle)
				{
					case ETextStyle.Lowercase:
						output = text.ToLower();
						return true;
					case ETextStyle.Uppercase:
						output = text.ToUpper();
						return true;
				}
				output = text;
				return true;
			}

			//Return key -Fail
			var split = locaId.Split('/');
			if (split != null && split.Length > 0)
			{
				var txt = split[split.Length - 1];
				txt = txt.Replace('_', ' ');
				output = txt.FirstLetterUppercase();
				return false;
			}
			//Fallback return full path

			output = locaId;
			return false;
		}

		internal static string GetStructureName(string structureID)
		{
			return GetText($"{StructurePath}{structureID}/name");
		}
		internal static string GetResourceName(string resID)
		{
			return GetText($"{ResourcePath}{resID}/name");
		}

		public static string GetLocaFilePath(string languageCode)
		{
			var path = Instance.LocaFolder;
			return path + "/" + languageCode + ".dat";
		}

		// languageCode, en-us; de-de; usw...
		public static void Load(string languageCode = DEFAULT_LANGUAGE)
		{
			m_Loaded = true;
			var path = GetLocaFilePath(languageCode);
			m_LocaText = new Dictionary<string, string>(LoadLocaJsonFile(path));

			OnLanguageChanges?.Invoke();
		}

		public void Save(string languageCode = DEFAULT_LANGUAGE, Dictionary<string, string> data = null)
		{
			var path = GetLocaFilePath(languageCode);

			if (data == null)
				data = m_LocaText;
			var sort = new SortedDictionary<string, string>(data, StringComparer.InvariantCultureIgnoreCase);

			var fs = new FileStream(path, FileMode.OpenOrCreate);
			var formatter = new BinaryFormatter();
			formatter.Serialize(fs, sort);
			fs.Close();
			Debug.Log($"Localization file ({languageCode}) saved");
		}

		public static SortedDictionary<string, string> LoadLocaJsonFile(string filePath)
		{
			var fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);
			var formatter = new BinaryFormatter();
			var dict = (SortedDictionary<string, string>)formatter.Deserialize(fs);
			fs.Close();
			return dict;					
		}

		public class LocaDictionary : Dictionary<string, string>
		{
			public LocaDictionary()
				: base(StringComparer.InvariantCultureIgnoreCase)
			{
			}
		}

#if UNITY_EDITOR
		[MenuItem("Endciv/Localization/Reload German")]
		public static void EditorReloadGerman()
		{
			OnLanguageChanges = null;
			Load("de_de");
			UpdateAllTextFields();
		}
		[MenuItem("Endciv/Localization/Reload English")]
		public static void EditorReloadEnglish()
		{
			OnLanguageChanges = null;
			Load("en_us");
			UpdateAllTextFields();
		}

		public void AddMissingKeys(LocaDictionary entries, string languageCode = DEFAULT_LANGUAGE)
		{
			int c = 0;
			foreach (var entry in entries)
			{
				if (!m_LocaText.ContainsKey(entry.Key))
				{
					m_LocaText.Add(entry.Key, entry.Value);
					c++;
				}
			}
			Debug.Log($"Added {c} new entries to dict ({ languageCode})");
		}

		static void UpdateAllTextFields()
		{
			var localizedText = FindObjectsOfType<LocalizedText>();
			for (int i = 0; i < localizedText.Length; i++)
			{
				localizedText[i].UpdateText();
			}
		}
#endif
	}
}