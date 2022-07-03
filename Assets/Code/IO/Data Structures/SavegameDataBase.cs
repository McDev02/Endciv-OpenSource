using System;
using System.Linq;

namespace Endciv
{
	/// <summary>
	/// The root savegame data. It contains other ISaveables whose data it collects
	/// when its own CollectData is called. Used to generate a chain reaction
	/// that eventually sets up all data for saving.
	/// /// </summary>
	[Serializable]
	public class SavegameDataBase : ISaveable
	{
		public GameMapSettings.GameMapSettingsSaveData gameMapSettingsData;
		public GridMapSaveData gridMapSaveData;

		//Statistics
		public TownStatistics townStatisticsData;
		public InventoryStatistics inventoryStatisticsData;

		//ISavable Children
		public EntitySaveData[] unitData;
		public EntitySaveData[] structureData;
		public EntitySaveData[] resourcePileData;

		//Systems
		public ProductionSystemSaveData productionSystemData;
		public ResourcePileSystemSaveData resourcePileSystemData;
		public AgricultureSystemSaveData agricultureSystemData;
		public CitizenAISystemSaveData citizenAISystemData;
		public NotificationSystemSaveData notificationSystemData;
		public TimeManagerSaveData timeManagerSaveData;
		public GraveyardSystemSaveData graveyardSystemData;
		public WasteSystemSaveData wasteSystemData;
		public NpcSpawnSystemSaveData npcSpawnSystemData;
		public AnimalSpawnSystemSaveData animalSpawnSystemData;
		public AIGroupSystemSaveData aiGroupSystemData;
		public StorageSystemSaveData storageSystemData;

		public ISaveable CollectData()
		{
			//Call CollectData on ISavable children

			//Game Map Settings
			gameMapSettingsData = (GameMapSettings.GameMapSettingsSaveData)Main.Instance.GameManager.gameMapSettings.CollectData();
			gridMapSaveData = (GridMapSaveData)Main.Instance.GameManager.GridMap.CollectData();

			//Statistics
			townStatisticsData = (TownStatistics)GameStatistics.MainTownStatistics.CollectData();
			inventoryStatisticsData = (InventoryStatistics)GameStatistics.InventoryStatistics.CollectData();

			//Units
			var units = Main.Instance.GameManager.SystemsManager.UnitSystem.UnitPool.Values.ToArray();
			unitData = PopulateData(units);

			//Structures
			var structures = Main.Instance.GameManager.SystemsManager.StructureSystem.structurePool.Values.ToArray();
			structureData = PopulateData(structures);

			//Resource Piles
			var resourcePiles = Main.Instance.GameManager.SystemsManager.ResourcePileSystem.ResourcePilePool;
			resourcePileData = PopulateData(resourcePiles);

			//Systems
			productionSystemData = (ProductionSystemSaveData)Main.Instance.GameManager.SystemsManager.ProductionSystem.CollectData();
			resourcePileSystemData = (ResourcePileSystemSaveData)Main.Instance.GameManager.SystemsManager.ResourcePileSystem.CollectData();
			agricultureSystemData = (AgricultureSystemSaveData)Main.Instance.GameManager.SystemsManager.AgricultureSystem.CollectData();
			citizenAISystemData = (CitizenAISystemSaveData)Main.Instance.GameManager.SystemsManager.AIAgentSystem.CitizenAISystem.CollectData();
			notificationSystemData = (NotificationSystemSaveData)Main.Instance.GameManager.SystemsManager.NotificationSystem.CollectData();
			timeManagerSaveData = (TimeManagerSaveData)Main.Instance.GameManager.timeManager.CollectData();
			graveyardSystemData = (GraveyardSystemSaveData)Main.Instance.GameManager.SystemsManager.GraveyardSystem.CollectData();
			wasteSystemData = (WasteSystemSaveData)Main.Instance.GameManager.SystemsManager.WasteSystem.CollectData();
			npcSpawnSystemData = (NpcSpawnSystemSaveData)Main.Instance.GameManager.SystemsManager.NpcSpawnSystem.CollectData();
			animalSpawnSystemData = (AnimalSpawnSystemSaveData)Main.Instance.GameManager.SystemsManager.AnimalSpawnSystem.CollectData();
			aiGroupSystemData = (AIGroupSystemSaveData)Main.Instance.GameManager.SystemsManager.AIGroupSystem.CollectData();
			storageSystemData = (StorageSystemSaveData)Main.Instance.GameManager.SystemsManager.StorageSystem.CollectData();
			return this;
		}

		private EntitySaveData[] PopulateData(BaseEntity[] dataList)
		{
			var entityData = new EntitySaveData[dataList.Length];
			for (int i = 0; i < dataList.Length; i++)
				entityData[i] = dataList[i].CollectData() as EntitySaveData;
			return entityData;
		}
	}
}