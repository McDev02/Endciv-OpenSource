using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Endciv
{
	public class SaveManager
	{
		public UserSettingsDataBase UserSettings { get; private set; }
		public Dictionary<string, MetaData> SaveGames { get; private set; }
		public int InvalidSavegames { get; private set; }
		private int currentSaveGame;

		private UserSettingsManager userSettingsManager;
		private SavegameManager savegameManager;
		private GameSettingsManager gameSettingsManager;

		public Action OnSaveGameComplete;

		const string GAME_SETTINGS_NAME = "gamesettings";
		const string USER_SETTINGS_NAME = "usersettings";

		public SaveManager()
		{
			savegameManager = new SavegameManager();
			gameSettingsManager = new GameSettingsManager();
			userSettingsManager = new UserSettingsManager();
			Initialize();
		}

		private void Initialize()
		{
			//LoadGameSettings();
			LoadUserSettings();
			LoadSaveGames();
			ObjectReferenceManager.Initialize();
		}

		public void SaveGameSettings()
		{
			gameSettingsManager.SaveGameSettings(GAME_SETTINGS_NAME);
		}

		public void LoadGameSettings()
		{
			gameSettingsManager.LoadGameSettings(GAME_SETTINGS_NAME);
		}

		public void SaveUserSettings()
		{
			userSettingsManager.SaveUserSettings(USER_SETTINGS_NAME, "default");
		}

		public void LoadUserSettings()
		{
			UserSettings = userSettingsManager.LoadUserSettings(USER_SETTINGS_NAME, "default");
			if (UserSettings == null)
			{
				UserSettings = new UserSettingsDataBase();
				UserSettings.CollectData();
			}
		}

		public void SaveGame(string filename)
		{
			savegameManager.SaveGame(filename, "default", true);
			OnSaveGameComplete?.Invoke();
		}

		public void QuickSave()
		{
			savegameManager.SaveGame("QuickSave_" + Main.Instance.saveManager.SaveGames.Count, "default", true);
			OnSaveGameComplete?.Invoke();
		}

		public void AutoSave()
		{
			savegameManager.SaveGame("AutoSave", "default", true);
			OnSaveGameComplete?.Invoke();
		}

		public void DeleteGame(int id)
		{
		}

		public void SetSaveGame(int id)
		{
			if (id < 0 || id >= SaveGames.Count)
			{
				UnityEngine.Debug.LogWarning("Warning : Invalid Save Game requested id : " + id);
				return;
			}
			currentSaveGame = id;
		}

		public SavegameDataBase GetCurrentSaveGame()
		{
			var meta = GetCurrentSave();
			return savegameManager.LoadGame(meta.name);
		}

		public MetaData GetSaveByID(int id)
		{
			var saves = new List<MetaData>(SaveGames.Values);
			return saves[id];
		}

		public bool DeleteSaveByID(int id)
		{
			var saves = new List<MetaData>(SaveGames.Values);
			var save = saves[id];
			foreach (var pair in SaveGames)
			{
				if (pair.Value == save)
				{
					var fileName = Path.GetFileNameWithoutExtension(pair.Key);
					return savegameManager.DeleteSave(fileName);
				}
			}
			return false;
		}

		public MetaData GetCurrentSave()
		{
			var games = new List<MetaData>(SaveGames.Values);
			int id = games.Count - currentSaveGame - 1;
			return games[id];
		}

		public void LoadGameMapSettings(SavegameDataBase save, GameMapSettingsReference reference)
		{
			if (save.gameMapSettingsData == null)
				return;
			if (reference == null)
				return;
			var settings = reference.GetSettingsByID(save.gameMapSettingsData.id);
			if (settings == null)
				return;
			settings.ApplySaveData(save.gameMapSettingsData);
			Main.Instance.GameManager.gameMapSettings = settings;
		}

		// Routine that handles unit instantiation.
		public IEnumerator LoadUnits(SavegameDataBase save, LoadingState loadingState)
		{
			var time = DateTime.Now;
			if (save == null)
			{
				UnityEngine.Debug.LogWarning("Save file is corrupted.");
				loadingState.SetMessage("LoadUnits: Save file is corrupted.");
				yield return null;

			}
			else
			{
				int id = 0;
				foreach (var unit in save.unitData)
				{
					if ((DateTime.Now - time).Milliseconds >= 500)
					{
						time = DateTime.Now;
						loadingState.SetMessage($"Load Unit: {id++}/{save.unitData.Length} (UID: {unit.UID})\nThis process will become faster :)");
						yield return null;
					}
					BaseEntity entity;
					var guid = Guid.Parse(unit.UID);
					var livingBeingData = unit.GetSaveData<LivingBeingFeatureSaveData>();
					var unitData = unit.GetSaveData<UnitFeatureSaveData>();
					var factoryParams = new FactoryParams();
					factoryParams.SetParams
						(
							new GridAgentFeatureParams()
							{
								Position = unit.GetSaveData<EntityFeatureSaveData>().position.ToVector3()
							},
							new EntityFeatureParams()
							{
								FactionID = unit.factionID
							}
						);
					if (livingBeingData != null)
					{
						factoryParams.SetParams
							(
								new UnitFeatureParams()
								{
									Age = livingBeingData.age,
									Gender = livingBeingData.gender,
									UnitType = unitData.unitType
								}
							);
					}
					entity = Main.Instance.GameManager.Factories.SimpleEntityFactory.CreateInstance(unit.id, guid.ToString(), factoryParams);
					entity.ApplySaveData(unit);
				}
			}
		}

		// Routine that handles unit instantiation.
		public IEnumerator LoadStructures(SavegameDataBase save, LoadingState loadingState)
		{
			var time = DateTime.Now;
			if (save == null)
			{
				UnityEngine.Debug.LogWarning("Save file is corrupted.");
				loadingState.SetMessage("LoadStructures: Save file is corrupted.");
				yield return null;
			}
			else
			{
				int id = 0;
				foreach (var structure in save.structureData)
				{
					if ((DateTime.Now - time).Milliseconds >= 500)
					{
						time = DateTime.Now;
						loadingState.SetMessage($"Load Structure: {id++}/{save.structureData.Length} (UID: {structure.UID})");
						yield return null;
					}

					try
					{
						var constructionFeature = structure.GetSaveData<ConstructionFeatureSaveData>();
						bool isConstruction = constructionFeature != null && (ConstructionSystem.EConstructionState)constructionFeature.constructionState == ConstructionSystem.EConstructionState.Construction;
						var pos = structure.GetSaveData<EntityFeatureSaveData>().gridPosition.ToVector2i();

						BaseEntity entity = null;
						EDirection direction = 0;
						GridObjectData gridData = null;
						var gridObjectSaveData = structure.GetSaveData<GridObjectSaveData>();
						if (gridObjectSaveData != null)
						{
							direction = (EDirection)gridObjectSaveData.direction;
							gridData = gridObjectSaveData.gridObjectData;
							gridData.Direction = direction;
						}
						var guid = Guid.Parse(structure.UID);
						Main.Instance.GameManager.UserToolSystem.GridObjectTool.CreateStructure(structure.id, structure.factionID, pos, direction, isConstruction, out entity, guid, true, gridData);

						entity.ApplySaveData(structure);


					}
					catch (Exception e)
					{
						UnityEngine.Debug.LogError($"Loading Structure id: {id} (UID: {structure.UID}) failed: {e}");
					}
				}
			}
		}
		// Routine that handles resource pile instantiation.
		public IEnumerator LoadResourcePiles(SavegameDataBase save, LoadingState loadingState)
		{
			if (save == null)
			{
				UnityEngine.Debug.LogWarning("Save file is corrupted.");
				loadingState.SetMessage("LoadResourcePiles: Save file is corrupted.");

			}
			else
			{
				loadingState.SetMessage("LoadResourcePiles");
				yield return null;
				int counter = 0;
				foreach (var pile in save.resourcePileData)
				{
					//loadingState.SetMessage($"Load ResourcePile: {id++}/{save.resourcePileData.Length} (UID: {pile.UID})");
					var pos = pile.GetSaveData<EntityFeatureSaveData>().gridPosition.ToVector2i();
					BaseEntity entity = null;
					var pileSaveData = pile.GetSaveData<ResourcePileSaveData>();
					EDirection direction = 0;
					var gridObjectSaveData = pile.GetSaveData<GridObjectSaveData>();
					if (gridObjectSaveData != null)
						direction = (EDirection)gridObjectSaveData.direction;
					var guid = Guid.Parse(pile.UID);
					Main.Instance.GameManager.UserToolSystem.GridObjectTool.CreateResourcePile(pile.id, pos, direction, out entity, guid, false);

					if ((counter++) % 100 == 0)
					{

#if _DEBUG || UNITY_EDITOR
						loadingState.SetMessage($"Load ResourcePile: {counter}/{save.resourcePileData.Length} (UID: {pile.UID})");
#else
						loadingState.SetMessage($"Load ResourcePile: {counter}/{save.resourcePileData.Length})");
#endif
						yield return null;
					}

					entity.ApplySaveData(pile);
				}
				//Calculate islands
				Main.Instance.GameManager.GridMap.RecalculateGrid();
			}
		}

		internal void CleanupInvalidFiles()
		{
			savegameManager.CleanupInvalidFiles();
		}

		public void LoadSystems(SavegameDataBase save, LoadingState loadingState)
		{
			var systemManager = Main.Instance.GameManager.SystemsManager;
			systemManager.ProductionSystem.ApplySaveData(save.productionSystemData);
			systemManager.ResourcePileSystem.ApplySaveData(save.resourcePileSystemData);
			systemManager.AgricultureSystem.ApplySaveData(save.agricultureSystemData);
			systemManager.AIAgentSystem.CitizenAISystem.ApplySaveData(save.citizenAISystemData);
			systemManager.NotificationSystem.ApplySaveData(save.notificationSystemData);
			Main.Instance.GameManager.timeManager.ApplySaveData(save.timeManagerSaveData);
			systemManager.GraveyardSystem.ApplySaveData(save.graveyardSystemData);
			systemManager.WasteSystem.ApplySaveData(save.wasteSystemData);
			systemManager.NpcSpawnSystem.ApplySaveData(save.npcSpawnSystemData);
			systemManager.AnimalSpawnSystem.ApplySaveData(save.animalSpawnSystemData);
			systemManager.AIGroupSystem.ApplySaveData(save.aiGroupSystemData);
			systemManager.StorageSystem.ApplySaveData(save.storageSystemData);
		}

		public IEnumerator LoadAI(SavegameDataBase save, LoadingState loadingState)
		{
			var time = DateTime.Now;
			if (save == null)
			{
				UnityEngine.Debug.LogWarning("Save file is corrupted.");
				loadingState.SetMessage("LoadAI: Save file is corrupted.");
				yield return null;
			}
			else
			{
				int id = 0;
				foreach (var unit in save.unitData)
				{
					if ((DateTime.Now - time).Milliseconds >= 500)
					{
						time = DateTime.Now;
						loadingState.SetMessage($"Load AI: {id++}/{save.unitData.Length} (UID: {unit.UID})");
						yield return null;
					}
					var guid = Guid.Parse(unit.UID);
					if (!Main.Instance.GameManager.SystemsManager.Entities.ContainsKey(guid))
						continue;
					var entity = Main.Instance.GameManager.SystemsManager.Entities[guid];
					var saveData = unit.GetFirstAIFeature();
					if (saveData != null)
					{
						var aiFeature = entity.GetFirstAIFeature();
						if (aiFeature != null)
						{
							aiFeature.ApplySaveData(saveData);
						}
					}
					entity.RefreshUI3D();
				}
			}
		}

		public void LoadSaveGames()
		{
			int invalid;
			SaveGames = savegameManager.GetSavegames(out invalid);
			InvalidSavegames = invalid;
		}
	}
}