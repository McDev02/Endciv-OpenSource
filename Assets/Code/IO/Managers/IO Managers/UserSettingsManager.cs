using UnityEngine;
using System.IO;
namespace Endciv
{
	/// <summary>
	/// A game data manager designed to manipulate user specific settings data.
	/// </summary>
	public class UserSettingsManager : BaseDataManager
	{
		public void SaveUserSettings(string savename, string username = "default")
		{
			string savePath = RootFolder + username + "\\";
			string filename = savename.ToLower() + FILETYPE;
			string fullpath = savePath + filename;

			if (!Directory.Exists(savePath))
				Directory.CreateDirectory(savePath);
            var userSettings = new UserSettingsDataBase();
            userSettings.CollectData();
            if (!WriteToDisk(userSettings, fullpath, true))
			{
				Debug.LogError("File not saved.");
			}
			else
			{
				Debug.Log("User settings saved to : " + fullpath);
			}

		}

		public UserSettingsDataBase LoadUserSettings(string savename, string username = "default")
		{
			string savePath = RootFolder + username + "\\";
			string filename = savename.ToLower() + FILETYPE;
			string fullpath = savePath + filename;
			Debug.Log($"LoadUserSettings: {savePath} - {filename}");
			if (!Directory.Exists(savePath))
			{
				Debug.LogWarning("No user settings found.");
				return null;
			}
			if (!File.Exists(fullpath))
			{
				Debug.LogWarning("User settings not found.");
				return null;
			}
			UserSettingsDataBase userSettings = null;
			if (LoadFromDisk(fullpath, out userSettings))
			{
				Debug.Log("User settings loaded successfully.");
			}
			else
			{
				Debug.LogError("Failed to load user settings.");
			}
			return userSettings;
		}
	}
}