using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

namespace Endciv
{
	public class StorageSystem : EntityFeatureSystem<StorageFeature>, IAIJob, ISaveable, ILoadable<StorageSystemSaveData>
	{
		/// <summary>
		/// The main system that controls storages
		/// </summary>
		public static InventoryStatistics Statistics;
		private static SimpleEntityFactory resourceFactory;
		TimeManager timeManager;

		public bool IsWorkplace { get { return true; } }
		public bool HasWork { get { return true; } }
		public bool Disabled { get; set; }
		public EOccupation[] Occupations { get { return new EOccupation[] { EOccupation.Labour }; } }
		public float Priority { get; set; }
		public int MaxWorkers { get; private set; }
		public int WorkerCount { get { return Workers.Count; } }

		public List<CitizenAIAgentFeature> Workers { get; private set; }

		//Processing and Lookup tables
		private Dictionary<StorageFeature, List<ResourceStack>> StoragesForChange { get; set; }
		private HashSet<InventoryFeature> InventoriesForProcessing { get; set; }
		private bool IsProcessingInventories { get; set; }
		private bool CanRedistribute { get; set; }
		private int redistributionTimer;

		public void Run()
		{
			timeManager = Main.Instance.GameManager.timeManager;
			Main.Instance.GameManager.SystemsManager.JobSystem.RegisterJob(this);
			CanRedistribute = true;
		}

		public void Stop()
		{
			Main.Instance.GameManager.SystemsManager.JobSystem.DeregisterJob(this);
		}


		public void RegisterWorker(CitizenAIAgentFeature unit, EWorkerType type = EWorkerType.Worker)
		{
			if (!Workers.Contains(unit))
				Workers.Add(unit);
		}

		public void DeregisterWorker(CitizenAIAgentFeature unit)
		{
			Workers.Remove(unit);
		}

        public void OnTaskStart()
        {

        }

        public void OnTaskComplete(AIAgentFeatureBase unit)
		{

		}

		public AITask AskForTask(EOccupation occupation, AIAgentFeatureBase unit)
		{
			if (!CanRedistribute)
			{
				return default(AITask);
			}
			lock (StoragesForChange)
			{
				if (StoragesForChange.Count <= 0)
					return default(AITask);
			}

			var citizen = unit as CitizenAIAgentFeature;
			//AI is not a citizen
			if (citizen == null)
			{
				Debug.LogWarning("No citizen requested Task from Storage System");
				return default(AITask);
			}
			if (occupation != EOccupation.Labour)
				return default(AITask);

			var pair = default(KeyValuePair<StorageFeature, List<ResourceStack>>);
			lock (StoragesForChange)
			{
				//Recalculate storages because they could have changed. This can be removed in case we add another solution to this problem.
				//Also this will only work as long the method is not threaded.
				RecheckAllInventoryPolicies();

				if (StoragesForChange.Count <= 0)
					return default(AITask);

				pair = GetFirstTransferableStorage(StoragesForChange, unit.FactionID);
				if (pair.Value == null)
					return default(AITask);
			}
			Dictionary<string, int> transferedResources = new Dictionary<string, int>();
			int capacity = unit.Entity.Inventory.CapacityLeft;
			foreach (var stack in pair.Value)
			{
				var data = resourceFactory.GetStaticData<ItemFeatureStaticData>(stack.ResourceID);
				if (data == null)
					continue;
				for (int i = 0; i < stack.Amount; i++)
				{
					if (capacity < data.Mass)
						break;
					capacity -= data.Mass;
					if (transferedResources.ContainsKey(stack.ResourceID))
						transferedResources[stack.ResourceID]++;
					else
						transferedResources.Add(stack.ResourceID, 1);
					if (capacity <= 0)
						break;
				}
				if (capacity <= 0)
					break;
			}
			if (transferedResources.Count <= 0)
				return default(AITask);
			var itemList = transferedResources.ToResourceStackList();
			return new StoreResourcesTask(unit.Entity, itemList, pair.Key.Entity.GetFeature<InventoryFeature>(), InventorySystem.ChamberMainID);
		}

		/// <summary>
		/// Retrieves all storages that contain items that shouldn't exist due to Policy   
		/// Only looks up inventories that changed since last lookup
		/// </summary>
		/// <param name="factionID"></param>
		/// <returns></returns>
		private Dictionary<StorageFeature, List<ResourceStack>> GetStoragesForRedistribution(HashSet<InventoryFeature> inventories)
		{
			Dictionary<StorageFeature, List<ResourceStack>> storages = new Dictionary<StorageFeature, List<ResourceStack>>();
			foreach (var inventory in inventories)
			{
				if (inventory.Entity.HasFeature<ConstructionFeature>() &&
					inventory.Entity.GetFeature<ConstructionFeature>().MarkedForDemolition)
					continue;
				List<ResourceStack> resources;
				var storage = inventory.Entity.GetFeature<StorageFeature>();
				if (storage.StaticData.IncludeForMaintenance && HasPolicyMismatch(inventory, storage.policy, out resources))
				{
					storages.Add(storage, resources);
				}
			}
			return storages;
		}

		/// <summary>
		/// Returns true if storage inventory contains items with wrong policy
		/// </summary>
		/// <param name="inventory">Storage's inventory</param>
		/// <param name="storagePolicy">Storage's item policy</param>
		/// <returns></returns>
		private bool HasPolicyMismatch(InventoryFeature inventory, EStoragePolicy storagePolicy, out List<ResourceStack> items)
		{
			Dictionary<string, int> itemReference = new Dictionary<string, int>();
			//Check Materials
			var keys = inventory.ItemPoolByChambers[0].Keys.ToArray();
			foreach (var key in keys)
			{
				var data = resourceFactory.GetStaticData<ItemFeatureStaticData>(key);
				if (ComparePolicyWithCategory(storagePolicy, data.Category))
					continue;
				var count = InventorySystem.GetItemCount(inventory, key, 0);
				if (itemReference.ContainsKey(key))
					itemReference[key] += count;
				else
					itemReference.Add(key, count);
			}
			if (itemReference.Count <= 0)
			{
				items = new List<ResourceStack>();
				return false;
			}
			items = new List<ResourceStack>(itemReference.Count);
			foreach (var pair in itemReference)
			{
				items.Add(new ResourceStack(pair.Key, pair.Value));
			}
			return true;
		}

		/// <summary>
		/// Put Storage policy first! The second argument should not be a flag
		/// </summary>
		/// <param name="storagePolicy"></param>
		/// <param name="itemPolicy"></param>
		/// <returns></returns>
		private bool ComparePolicyWithCategory(EStoragePolicy storagePolicy, EStoragePolicy itemPolicy)
		{
			return (storagePolicy & itemPolicy) != 0;
		}

		public float GetTotalStorageCapacityLeft(int factionID, EStoragePolicy category)
		{
			if (FeaturesByFaction[factionID].Count == 0f)
				return 0;
			float sum = 0f;
			int count = 0;
			foreach (var feature in FeaturesByFaction[factionID])
			{
				if (!ComparePolicyWithCategory(feature.policy, category))
					continue;
				if (feature.Inventory.LoadProgress >= 0.9f)
					continue;
				if (feature.Entity.HasFeature<ConstructionFeature>() &&
					feature.Entity.GetFeature<ConstructionFeature>().MarkedForDemolition)
					continue;
				sum += feature.Inventory.LoadProgress;
				count++;
			}
			if (count <= 0)
				return 0;
			return 1f - (sum / count);
		}

		/// <summary>
		/// Iterates target storages and returns the first storage 
		/// with items that can be transfered.        
		/// </summary>
		/// <param name="storages"></param>
		/// <returns></returns>
		private KeyValuePair<StorageFeature, List<ResourceStack>> GetFirstTransferableStorage
			(Dictionary<StorageFeature, List<ResourceStack>> storages, int factionID)
		{
			var keys = storages.Keys.ToArray();
			foreach (var key in keys)
			{
				var items = storages[key];
				foreach (var item in items)
				{
					var data = Main.Instance.GameManager.Factories.SimpleEntityFactory.EntityStaticData[item.ResourceID].
						GetFeature<ItemFeatureStaticData>();
					var acceptingStorages = GetAllStoragesAccepting(data, factionID);
					if (acceptingStorages.Length > 0)
						return new KeyValuePair<StorageFeature, List<ResourceStack>>
							(key, storages[key]);
				}
				storages.Remove(key);
			}
			return default(KeyValuePair<StorageFeature, List<ResourceStack>>);
		}

		public StorageSystem(int factions, SimpleEntityFactory resourceFactory) : base(factions)
		{
			StoragesForChange = new Dictionary<StorageFeature, List<ResourceStack>>();
			InventoriesForProcessing = new HashSet<InventoryFeature>();

			Workers = new List<CitizenAIAgentFeature>();
			MaxWorkers = int.MaxValue;

			StorageSystem.resourceFactory = resourceFactory;
			Statistics = InventorySystem.GetNewInventoryStatistics();
			GameStatistics.InventoryStatistics = Statistics;
			UpdateStatistics();

			Main.Instance.GameManager.SystemsManager.AIAgentSystem.TaskEndedCallback -= OnTaskComplete;
			Main.Instance.GameManager.SystemsManager.AIAgentSystem.TaskEndedCallback += OnTaskComplete;
		}

		public override void UpdateStatistics()
		{
			Statistics.Clear();
			foreach (var storage in FeaturesByFaction[SystemsManager.MainPlayerFaction])
			{
				if (storage.Inventory == null)
					continue;
				InventorySystem.UpdateInventoryStatistics(storage.Inventory, Statistics);
			}
			UpdateStorageStatisticsNotifications(Statistics);
		}

		internal override void RegisterFeature(StorageFeature feature)
		{
			base.RegisterFeature(feature);
			if (!feature.Entity.HasFeature<InventoryFeature>())
				return;
			if (feature.Entity.factionID != SystemsManager.MainPlayerFaction)
				return;
			var inventory = feature.Entity.GetFeature<InventoryFeature>();
			//inventory.OnInventoryChanged -= OnStorageChanged;
			//inventory.OnInventoryChanged += OnStorageChanged;
			feature.OnPolicyChanged -= OnPolicyChanged;
			feature.OnPolicyChanged += OnPolicyChanged;
		}

		internal override void DeregisterFeature(StorageFeature feature, int faction = -1)
		{
			if (faction < 0)
				faction = feature.FactionID;
			base.DeregisterFeature(feature, faction);
			if (feature.Entity.factionID != SystemsManager.MainPlayerFaction)
				return;
			if (!feature.Entity.HasFeature<InventoryFeature>())
				return;
			var inventory = feature.Entity.GetFeature<InventoryFeature>();
			inventory.OnInventoryChanged -= OnStorageChanged;
			feature.OnPolicyChanged -= OnPolicyChanged;
		}

		private void RecheckAllInventoryPolicies()
		{
			IsProcessingInventories = true;
			InventoriesForProcessing.Clear();
			var inventories = new HashSet<InventoryFeature>(InventoriesForProcessing);
			foreach (var feature in FeaturesByFaction[SystemsManager.MainPlayerFaction])
			{
				inventories.Add(feature.Entity.GetFeature<InventoryFeature>());
			}
			//Task.Run(() => { ProcessChangedInventories(inventories); });
			ProcessChangedInventories(inventories);
		}

		private void OnPolicyChanged(StorageFeature storage)
		{
			OnStorageChanged(storage.Inventory);
		}

		private void OnStorageChanged(InventoryFeature inventory)
		{
			InventoriesForProcessing.Add(inventory);
			if (!IsProcessingInventories)
			{
				IsProcessingInventories = true;
				var inventories = new HashSet<InventoryFeature>(InventoriesForProcessing);
				InventoriesForProcessing.Clear();
				//Task.Run(() => { ProcessChangedInventories(inventories); });
				ProcessChangedInventories(inventories);
			}
		}

		private void ProcessChangedInventories(HashSet<InventoryFeature> inventories)
		{
			lock (StoragesForChange)
			{
				try
				{
					var keys = StoragesForChange.Keys.ToArray();
					foreach (var key in keys)
					{
						inventories.Add(key.Entity.GetFeature<InventoryFeature>());
					}
				}
				catch (Exception e)
				{
					Debug.LogError(e);
				}
			}
			try
			{
				var storages = GetStoragesForRedistribution(inventories);

				lock (StoragesForChange)
				{
					StoragesForChange = storages;
				}
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}

			IsProcessingInventories = false;
		}

		internal StorageFeature[] GetAllStorages(int factionID)
		{
			return FeaturesByFaction[factionID].ToArray();
		}

		public override void UpdateGameLoop()
		{
			if (redistributionTimer <= 0)
			{
				redistributionTimer = timeManager.dayTickLength / 4;
				RecheckAllInventoryPolicies();
			}
			redistributionTimer--;
			UpdateStatistics();
		}

		public int CountResources(string resourceID, int factionID)
		{
			int count = 0;
			foreach (var storageFeature in FeaturesByFaction[factionID])
			{
				count += InventorySystem.GetItemCount(storageFeature.Entity.Inventory, resourceID);
			}
			return count;
		}

		public StorageFeature[] GetAllStoragesContaining(string resourceID, int factionID)
		{
			List<StorageFeature> storages = new List<StorageFeature>();
			foreach (var storageFeature in FeaturesByFaction[factionID])
			{
				if (storageFeature.Entity.HasFeature<ConstructionFeature>() &&
					storageFeature.Entity.GetFeature<ConstructionFeature>().MarkedForDemolition)
					continue;
				if (InventorySystem.HasItems(storageFeature.Entity.Inventory, resourceID))
					storages.Add(storageFeature);
			}
			return storages.ToArray();
		}

		public StorageFeature GetClosestFoodStorage(GridAgentFeature agent)
		{
			List<StorageFeature> orderedStorages = FeaturesByFaction[agent.FactionID].OrderBy(x => Vector2i.Distance(x.Entity.GetFeature<EntityFeature>().GridID, agent.Entity.GetFeature<EntityFeature>().GridID)).ToList();
			StorageFeature storage = null;
			foreach (var store in orderedStorages)
			{
				if (store.Inventory.Statistics.ConsumableNutritionAvailable <= 0)
					continue;
				storage = store;
				break;
			}
			return storage;
		}

		public StorageFeature GetClosestWaterStorage(GridAgentFeature agent)
		{
			List<StorageFeature> orderedStorages = FeaturesByFaction[agent.FactionID].OrderBy(x => Vector2i.Distance(x.Entity.GetFeature<EntityFeature>().GridID, agent.Entity.GetFeature<EntityFeature>().GridID)).ToList();
			StorageFeature storage = null;
			foreach (var store in orderedStorages)
			{
				if (store.Inventory.Statistics.ConsumableWaterAvailable <= 0)
					continue;
				storage = store;
				break;
			}
			//	if (storage != null) Debug.LogError("ClosestWaterStorage found: " + storage.Entity.StaticData.ID);
			return storage;
		}

		public List<StorageFeature> GetAllStoragesWithNutrition(int faction = -1)
		{
			if (faction >= 0 && faction < FeaturesByFaction.Count)
				return FeaturesByFaction[faction].OrderBy(x => x.Inventory.Statistics.NutritionAvailable > 0).ToList();
			else
				return FeaturesCombined.OrderBy(x => x.Inventory.Statistics.NutritionAvailable > 0).ToList();
		}
		public List<StorageFeature> GetAllStoragesWithFood(int faction = -1)
		{
			if (faction >= 0 && faction < FeaturesByFaction.Count)
				return FeaturesByFaction[faction].OrderBy(x => x.Inventory.Statistics.WaterAvailable > 0).ToList();
			else
				return FeaturesCombined.OrderBy(x => x.Inventory.Statistics.WaterAvailable > 0).ToList();
		}

		/// <summary>
		/// Returns the Storage Feature containing the highest quantity of items from the list of resources provided.
		/// </summary>
		/// <param name="resources">Resources to check against</param>
		/// <returns>StorageFeature</returns>
		public StorageFeature GetBestStorageForResources(List<ResourceStack> resources, int factionID)
		{
			Dictionary<StorageFeature, int> storages = new Dictionary<StorageFeature, int>();

			foreach (var resource in resources)
			{
				foreach (var storage in FeaturesByFaction[factionID])
				{
					int count = 0;
					if ((count = InventorySystem.GetItemCount(storage.Inventory, resource.ResourceID)) > 0)
					{
						if (!storages.ContainsKey(storage))
						{
							storages.Add(storage, Mathf.Min(resource.Amount, count));
						}
						else
						{
							storages[storage] += Mathf.Min(resource.Amount, count);
						}
					}
				}
			}
			if (storages.Count <= 0)
				return null;
			var myList = storages.OrderByDescending(d => d.Value).ToList();
			return myList[0].Key;
		}

		public StorageFeature[] GetAllStoragesAccepting(ItemFeatureStaticData data, int factionID, float capacityThreshold = 0.95f)
		{
			List<StorageFeature> storages = new List<StorageFeature>();
			foreach (var storageFeature in FeaturesByFaction[factionID])
			{
				if (storageFeature.Inventory.CapacityLeft < data.Mass)
					continue;
				if (storageFeature.Entity.HasFeature<ConstructionFeature>() &&
					storageFeature.Entity.GetFeature<ConstructionFeature>().MarkedForDemolition)
					continue;
				if (ComparePolicyWithCategory(storageFeature.policy, data.Category))
					storages.Add(storageFeature);
			}
			return storages.ToArray();
		}

		public StorageFeature[] GetAllStoragesAccepting(EStoragePolicy category, int factionID)
		{
			List<StorageFeature> storages = new List<StorageFeature>();
			foreach (var storageFeature in FeaturesByFaction[factionID])
			{
				if (storageFeature.Entity.HasFeature<ConstructionFeature>() &&
					storageFeature.Entity.GetFeature<ConstructionFeature>().MarkedForDemolition)
					continue;
				if (ComparePolicyWithCategory(storageFeature.policy, category))
					storages.Add(storageFeature);
			}
			return storages.ToArray();
		}

		/// <summary>
		/// TODO : COMPLETE THIS AND APPLY IT TO FindDepositStorageAction
		/// Returns all storages that can accept even partial ammounts of mixed supplied resources
		/// linked to the bundles they accept
		/// </summary>
		/// <param name="resources"></param>
		/// <returns></returns>
		public Dictionary<StorageFeature, List<ResourceStack>> GetAllStoragesWithAvailableSpace(List<ResourceStack> resources, int factionID)
		{
			var storages = new Dictionary<StorageFeature, List<ResourceStack>>();
			foreach (var storage in FeaturesByFaction[factionID])
			{
				if (storage.Entity.HasFeature<ConstructionFeature>() &&
					storage.Entity.GetFeature<ConstructionFeature>().MarkedForDemolition)
					continue;
				List<ResourceStack> transferedResources = new List<ResourceStack>();
				foreach (var resource in transferedResources)
				{
					var data = resourceFactory.GetStaticData<ItemFeatureStaticData>(resource.ResourceID);
					if (!ComparePolicyWithCategory(storage.policy, data.Category))
						continue;
					//To be continued
				}
			}
			return storages;
		}

		public static void UpdateStorageStatisticsNotifications(InventoryStatistics stats)
		{
			Main.Instance.GameManager.SystemsManager.NotificationSystem.SetVariable<int>($"totalItems", stats.TotalItems);
			Main.Instance.GameManager.SystemsManager.NotificationSystem.SetVariable<int>($"totalNutrition", (int)stats.NutritionAvailable);

			foreach (var item in stats.Weapons)
			{
				Main.Instance.GameManager.SystemsManager.NotificationSystem.SetVariable<int>($"totalWeapon_{item.Key.ToString()}", item.Value);
			}
			foreach (var item in stats.Tools)
			{
				Main.Instance.GameManager.SystemsManager.NotificationSystem.SetVariable<int>($"totalTool_{item.Key.ToString()}", item.Value);
			}
			foreach (var item in stats.Foods)
			{
				Main.Instance.GameManager.SystemsManager.NotificationSystem.SetVariable<int>($"totalFood_{item.Key.ToString()}", item.Value);
			}
			foreach (var item in stats.Items)
			{
				Main.Instance.GameManager.SystemsManager.NotificationSystem.SetVariable<int>($"totalItem_{item.Key.ToString()}", item.Value);
			}
		}		

		public ISaveable CollectData()
		{
			var data = new StorageSystemSaveData();
			data.workerUIDs = new List<string>();
			foreach (var worker in Workers)
			{
				data.workerUIDs.Add(worker.Entity.UID.ToString());
			}
			return data;
		}

		public void ApplySaveData(StorageSystemSaveData data)
		{
			if (data == null)
				return;
			if (data.workerUIDs != null)
			{
				foreach (var workerID in data.workerUIDs)
				{
					Guid id = Guid.Parse(workerID);
					if (Main.Instance.GameManager.SystemsManager.Entities.ContainsKey(id))
					{
						Workers.Add(Main.Instance.GameManager.SystemsManager.Entities[id].GetFeature<CitizenAIAgentFeature>());
					}
				}
			}
		}
	}
}