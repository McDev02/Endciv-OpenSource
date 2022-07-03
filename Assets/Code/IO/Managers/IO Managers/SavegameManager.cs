using System.IO;
using UnityEngine;
using System.Collections.Generic;

namespace Endciv
{
	/// <summary>
	/// A game data manager designed to manipulate savegame data.
	/// </summary>
	public class SavegameManager : BaseDataManager
	{
		const string savegameFolder = "\\savegames\\";

		public void SaveGame(string savegameename, string username = "default", bool isReadable = false)
		{
			string savegamePath = RootFolder + username + savegameFolder;
			string filename = savegameename.ToLower().Replace(" ", "_") + ".sav";
			string metaname = savegameename.ToLower().Replace(" ", "_") + ".meta";
			string fullpath = savegamePath + filename;
			string fullMetaPath = savegamePath + metaname;

			if (!Directory.Exists(savegamePath))
				Directory.CreateDirectory(savegamePath);

			SavegameDataBase savegameBase = new SavegameDataBase();
			savegameBase.CollectData();
			MetaData meta = new MetaData();
			meta.name = savegameename;
			meta.saveVersion = SaveFileConverter.currentSaveVersion;
			meta.date = System.DateTime.Now.ToString();
			if (!WriteToDisk(savegameBase, fullpath, isReadable))
			{
				Debug.LogError("File not saved.");
				return;
			}
			else
			{
				Debug.Log("Game saved to : " + fullpath);
			}
			if (!WriteToDisk(meta, fullMetaPath, isReadable))
			{
				Debug.LogError("Meta file could not be generated.");
				return;
			}
			else
			{
				Debug.Log("Meta file created successfully at " + fullpath);
			}

		}

		public SavegameDataBase LoadGame(string savegameename, string username = "default")
		{
			string savegamePath = RootFolder + username + savegameFolder;
			string filename = savegameename.ToLower().Replace(" ", "_") + ".sav";
			string fullpath = savegamePath + filename;
			fullpath.Replace('/', '\\');

			if (!Directory.Exists(savegamePath))
			{
				Debug.LogWarning("No savegames found");
				return null;
			}
			if (!File.Exists(fullpath))
			{
				Debug.LogWarning("File not found");
				return null;
			}
			SavegameDataBase savegame = null;
			if (LoadFromDisk(fullpath, out savegame))
			{
				Debug.Log("Save file loaded successfully.");
			}
			else
			{
				Debug.LogError("Failed to load Save file.");
			}
			return savegame;
		}

		public MetaData LoadGameMetaData(string savegameename, string username = "default")
		{
			string savegamePath = RootFolder + username + savegameFolder;
			string filename = savegameename.ToLower().Replace(" ", "_") + ".meta";
			string fullpath = savegamePath + filename;
			fullpath.Replace('/', '\\');

			if (!Directory.Exists(savegamePath))
			{
				Debug.LogWarning("Metadata directory not found");
				return null;
			}
			if (!File.Exists(fullpath))
			{
				Debug.LogWarning("Metadata file not found");
				return null;
			}
			MetaData meta = null;
			if (!LoadFromDisk(fullpath, out meta))
				Debug.LogError("Failed to load Save file.");

			//else			
			//	Debug.Log("Metadata file loaded successfully.");

			return meta;
		}


		internal void CleanupInvalidFiles(string user = "default")
		{
			string savegamePath = RootFolder + user + savegameFolder;
			if (!Directory.Exists(savegamePath))
				Directory.CreateDirectory(savegamePath);

			DirectoryInfo d = new DirectoryInfo(RootFolder + user + "/savegames");
			FileInfo[] files = d.GetFiles("*.meta");
			for (int i = 0; i < files.Length; i++)
			{
				var name = Path.GetFileNameWithoutExtension(files[i].Name);
				var meta = LoadGameMetaData(name, user);
				if (meta == null || meta.IsDeprecated)
					DeleteSave(name);
			}
		}

		public bool DeleteSave(string fileName, string user = "default")
		{
			bool hasDeleted = false;
			if (File.Exists(RootFolder + user + savegameFolder + fileName + ".sav"))
			{
				File.Delete(RootFolder + user + savegameFolder + fileName + ".sav");
				hasDeleted = true;
			}
			if (File.Exists(RootFolder + user + savegameFolder + fileName + ".meta"))
			{
				File.Delete(RootFolder + user + savegameFolder + fileName + ".meta");
				hasDeleted = true;
			}
			return hasDeleted;
		}

		public Dictionary<string, MetaData> GetSavegames(out int invalidSavegames, string user = "default")
		{
			string savegamePath = RootFolder + user + savegameFolder;
			if (!Directory.Exists(savegamePath))
				Directory.CreateDirectory(savegamePath);
			Dictionary<string, MetaData> gameList = null;
			DirectoryInfo d = new DirectoryInfo(RootFolder + user + "/savegames");
			FileInfo[] files = d.GetFiles("*.meta");
			gameList = new Dictionary<string, MetaData>();
			invalidSavegames = 0;
			for (int i = 0; i < files.Length; i++)
			{
				var meta = LoadGameMetaData(Path.GetFileNameWithoutExtension(files[i].Name), user);
				//if (meta.saveVersion >= SaveFileConverter.lowestSupportedSaveVersion)
				if (meta != null)
					gameList.Add(files[i].Name, meta);
				else invalidSavegames++;
			}
			return gameList;
		}
	}
}