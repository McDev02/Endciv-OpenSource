using UnityEngine;
using System.IO;
namespace Endciv
{
	/// <summary>
	/// A data manager designed to manipulate game settings data.
	/// </summary>
	public class GameSettingsManager : BaseDataManager
	{
		public void SaveGameSettings(string savename)
		{
			string filename = savename.ToLower() + FILETYPE;
			string fullpath = RootFolder + filename;

			if (!Directory.Exists(RootFolder))
				Directory.CreateDirectory(RootFolder);

			GameSettingsDataBase settingsBase = new GameSettingsDataBase();
			settingsBase.CollectData();
			if (!WriteToDisk(settingsBase, fullpath, true))
			{
				Debug.LogError("Game settings file not saved.");
			}
			else
			{
				Debug.Log("Game settings file saved to : " + fullpath);
			}

		}

		public GameSettingsDataBase LoadGameSettings(string savename)
		{
			string filename = savename.ToLower() + FILETYPE;
			string fullpath = RootFolder + filename;

			if (!Directory.Exists(RootFolder))
			{
				Debug.LogWarning("No game settings found.");
				return null;
			}
			if (!File.Exists(fullpath))
			{
				Debug.LogWarning("Settings file not found.");
				return null;
			}
			GameSettingsDataBase settings = null;
			if (LoadFromDisk(fullpath, out settings))
			{
				Debug.Log("Game Settings loaded successfully.");
			}
			else
			{
				Debug.LogError("Failed to load Game Settings.");
			}
			return settings;
		}
	}
}