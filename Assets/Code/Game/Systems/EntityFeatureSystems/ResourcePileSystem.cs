using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

namespace Endciv
{
	public class ResourcePileSystem : EntityFeatureSystem<ResourcePileFeature>, IAIJob, ISaveable, ILoadable<ResourcePileSystemSaveData>
	{
		public enum EResourcePileType
		{
			StoragePile,
			ResourcePile,
			Corpse
		}
		//TODO: SPlit up in Corps Feature / System
		public enum ECorpseState
		{
			Fresh,
			Rotten,
			Skeleton
		}

		List<ResourcePileFeature> Corpses;
		/// <summary>
		/// Used for storing items, that are already in the economy, jsut pick them up
		/// </summary>
		List<ResourcePileFeature> StoragePiles;
		/// <summary>
		/// Used for piles in the environment to mine items which are not yet in the economy
		/// </summary>
		List<ResourcePileFeature> ResourcePiles;
		List<ResourcePileFeature> BlockingResourcePiles;

		//Pool
		private List<ResourcePileFeature> m_ResourcePilePool = new List<ResourcePileFeature>();
		public BaseEntity[] ResourcePilePool
		{
			get
			{
				var arr = new BaseEntity[m_ResourcePilePool.Count];
				for (int i = 0; i < m_ResourcePilePool.Count; i++)
				{
					arr[i] = m_ResourcePilePool[i].Entity;
				}
				return arr;
			}
		}

		#region IAIJob
		public bool IsWorkplace { get { return true; } }
		public bool HasWork { get { return true; } }
		public bool Disabled { get; set; }
		public EOccupation[] Occupations { get { return new EOccupation[] { EOccupation.Labour }; } }
		public float Priority { get; set; }
		public int MaxWorkers { get; private set; }
		public int WorkerCount { get { return Workers.Count; } }

		// Workers who are registered to work here
		public List<CitizenAIAgentFeature> Workers { get; private set; }


		public const float PickupSpeed = 1;

		public void RegisterWorker(CitizenAIAgentFeature unit, EWorkerType type = EWorkerType.Worker)
		{
			if (!Workers.Contains(unit))
			{
				Workers.Add(unit);
			}

		}
		public void DeregisterWorker(CitizenAIAgentFeature unit)
		{
			Workers.Remove(unit);
		}

		public ResourcePileFeature GetNewResourcePile(CitizenAIAgentFeature citizen)
		{
			//Sort based on distance form unit
			float storageThresholdBlocking = 0.1f;
			float storageThresholdStoragePiles = 0.1f;
			float storageThresholdGathering = 0.1f;

			//1: construction site blocking resource piles
			var orderedObstacles = BlockingResourcePiles.OrderBy
                (x => Vector2i.Distance
                    (
						x.Entity.GetFeature<EntityFeature>().GridID,
                        citizen.Entity.GetFeature<EntityFeature>().GridID
                    )
                );
			foreach (var obstacle in orderedObstacles)
			{
				if (!obstacle.markedForCollection || obstacle.assignedCollector != null)
					continue;

				if (Main.Instance.GameManager.SystemsManager.StorageSystem.GetTotalStorageCapacityLeft(citizen.FactionID, obstacle.StoragePolicy) <= storageThresholdBlocking)
					continue;

				if (obstacle.ResourcePileType == EResourcePileType.StoragePile)
				{
					var keys = obstacle.Entity.Inventory.ItemPoolByChambers[0].Keys.ToList();
					string id = string.Empty;
					foreach(var key in keys)
					{
						if (!InventorySystem.CanAddItems(citizen.Entity.Inventory, key, 1))
						{
							continue;
						}
						id = key;
					}
					
					if (id == string.Empty)
						continue;
				}
				else
				{
					string id = string.Empty;
					foreach (var res in obstacle.resources)
					{
						if (!InventorySystem.CanAddItems(citizen.Entity.Inventory, res.ResourceID, 1))
						{
							continue;
						}
						id = res.ResourceID;
						break;
					}
					if (id == string.Empty)
						continue;
				}
				return obstacle;
			}

			//2: storage piles
			var orderedStoragePiles =
				StoragePiles.OrderBy(x =>
                    Vector2i.Distance
                    (
						x.Entity.GetFeature<EntityFeature>().GridID,
                        citizen.Entity.GetFeature<EntityFeature>().GridID
                    )
                );
			ResourcePileFeature storagePile = null;
			float dist = 0f;
			foreach (var pile in orderedStoragePiles)
			{
				if (pile.assignedCollector != null)
					continue;

				if (Main.Instance.GameManager.SystemsManager.StorageSystem.GetTotalStorageCapacityLeft(citizen.FactionID, pile.StoragePolicy) <= storageThresholdStoragePiles)
					continue;

				if (pile.ResourcePileType == EResourcePileType.StoragePile)
				{
					string id = string.Empty;
					var keys = pile.Entity.Inventory.ItemPoolByChambers[0].Keys.ToList();
					foreach(var key in keys)
					{						
						if (!InventorySystem.CanAddItems(citizen.Entity.Inventory, key, 1))
						{
							continue;
						}
						id = key;
						break;
					}					
					if (id == string.Empty)
						continue;
				}
				else
				{
					string id = string.Empty;
					foreach (var res in pile.resources)
					{
						if (!InventorySystem.CanAddItems(citizen.Entity.Inventory, res.ResourceID, 1))
						{
							continue;
						}
						id = res.ResourceID;
						break;
					}
					if (id == string.Empty)
						continue;
				}

				if ((dist = Vector2i.Distance(pile.Entity.GetFeature<EntityFeature>().GridID, citizen.Entity.GetFeature<EntityFeature>().GridID)) > MAX_STORAGE_PILE_DISTANCE && storagePile == null)
				{
					storagePile = pile;
					break;
				}
				return pile;
			}

			//3: resource piles
			//Only execute when we have enough storage space
			if (Main.Instance.GameManager.SystemsManager.StorageSystem.GetTotalStorageCapacityLeft(citizen.FactionID, EStoragePolicy.Scrapyard) >= storageThresholdGathering)
			{
				var orderedResourcePiles = ResourcePiles.OrderBy(x => Vector2i.Distance(x.Entity.GetFeature<EntityFeature>().GridID, citizen.Entity.GetFeature<EntityFeature>().GridID)).ToList();
				var markedPiles = new List<ResourcePileFeature>();
				var unmarkedPiles = new List<ResourcePileFeature>();

				//Split piles to marked/unmarked but preserving their distance sorting
				foreach (var pile in orderedResourcePiles)
				{
					if (pile.markedForCollection)
						markedPiles.Add(pile);
					else
						unmarkedPiles.Add(pile);
				}

				//Merge the seperated piles into the original array
				//Both sides preserve distance sorting. Marked resource piles are evaluated first
				orderedResourcePiles = markedPiles.Concat(unmarkedPiles).ToList();

				foreach (var pile in orderedResourcePiles)
				{
					if (pile.assignedCollector != null)
						continue;

					if (Main.Instance.GameManager.SystemsManager.StorageSystem.GetTotalStorageCapacityLeft(citizen.FactionID, pile.StoragePolicy) <= storageThresholdGathering)
						continue;

					if (pile.ResourcePileType == EResourcePileType.StoragePile)
					{
						string id = string.Empty;
						var keys = pile.Entity.Inventory.ItemPoolByChambers[0].Keys.ToList();
						foreach(var key in keys)
						{
							if (!InventorySystem.CanAddItems(citizen.Entity.Inventory, key, 1))
							{
								continue;
							}
							id = key;
							break;
						}						
						if (id == string.Empty)
							continue;
					}
					else
					{
						string id = string.Empty;
						foreach (var res in pile.resources)
						{
							if (!InventorySystem.CanAddItems(citizen.Entity.Inventory, res.ResourceID, 1))
							{
								continue;
							}
							id = res.ResourceID;
							break;
						}
						if (id == string.Empty)
							continue;
					}

					if ((dist = Vector2i.Distance(pile.Entity.GetFeature<EntityFeature>().GridID, citizen.Entity.GetFeature<EntityFeature>().GridID)) > MAX_STORAGE_PILE_DISTANCE && storagePile == null)
					{
						storagePile = pile;
						break;
					}
					if (storagePile != null && Vector2i.Distance(pile.Entity.GetFeature<EntityFeature>().GridID, citizen.Entity.GetFeature<EntityFeature>().GridID) > dist)
					{
						break;
					}
					return pile;
				}
			}

			if (storagePile != null)
			{
				return storagePile;
			}
			return null;
		}


		public AITask AskForTask(EOccupation occupation, AIAgentFeatureBase unit)
		{
			var citizen = unit as CitizenAIAgentFeature;
			//AI is not a citizen
			if (citizen == null)
			{
				Debug.LogWarning("No citizen requested Task from ResourcePile System");
				return null;
			}

			if (occupation != EOccupation.Labour)
				return null;

			var pile = GetNewResourcePile(citizen);
			if (pile == null)
				return null;
			else
			{
				ResourceCollectionTask task = null;
				if (BlockingResourcePiles.Contains(pile))
				{
					task = new ResourceCollectionTask(citizen.Entity, pile, pile.overlappingConstructionSite.Inventory);
				}
				else
				{
					task = new ResourceCollectionTask(citizen.Entity, pile, null);
				}
				return task;
			}
		}

        public void OnTaskStart()
        {

        }

        public void OnTaskComplete(AIAgentFeatureBase unit)
		{
            if (!Workers.Contains(unit))
                return;
			(unit.CurrentTask as ResourceCollectionTask).UnassignGatherer();
			DeregisterWorker(unit as CitizenAIAgentFeature);
		}
		#endregion

		static GameManager manager;
		static protected NotificationSystem notificationSystem;
		const float MAX_TILES_RADIUS = 3;
		const int MAX_STORAGE_PILE_DISTANCE = 25;

		public ResourcePileSystem(int factions, GameManager manager) : base(factions)
		{
			UpdateStatistics();

			ResourcePileSystem.manager = manager;
			notificationSystem = manager.SystemsManager.NotificationSystem;

			Corpses = new List<ResourcePileFeature>(32);
			StoragePiles = new List<ResourcePileFeature>(64);
			ResourcePiles = new List<ResourcePileFeature>(256);
			BlockingResourcePiles = new List<ResourcePileFeature>(256);
			Workers = new List<CitizenAIAgentFeature>();

			MaxWorkers = 256;
            Main.Instance.GameManager.SystemsManager.AIAgentSystem.TaskEndedCallback -= OnTaskComplete;
            Main.Instance.GameManager.SystemsManager.AIAgentSystem.TaskEndedCallback += OnTaskComplete;
        }

		public void Run()
		{
			manager.SystemsManager.JobSystem.RegisterJob(this);
		}

		/// <summary>
		/// Used to pick up items or add additional ones
		/// </summary>
		public BaseEntity[] GetStoragePiles()
		{
			var piles = new BaseEntity[StoragePiles.Count];
			for (int i = 0; i < piles.Length; i++)
			{
				piles[i] = StoragePiles[i].Entity;
			}
			return piles;
		}
		/// <summary>
		/// Used only to gather items, never add to those piles
		/// </summary>
		public BaseEntity[] GetResourcePiles()
		{
			var piles = new BaseEntity[ResourcePiles.Count];
			for (int i = 0; i < piles.Length; i++)
			{
				piles[i] = ResourcePiles[i].Entity;
			}
			return piles;
		}

		internal override void RegisterFeature(ResourcePileFeature feature)
		{
			switch (feature.ResourcePileType)
			{
				case EResourcePileType.Corpse:
					if (!Corpses.Contains(feature))
						Corpses.Add(feature);
					break;

				case EResourcePileType.StoragePile:
					if (!StoragePiles.Contains(feature))
						StoragePiles.Add(feature);
					break;

				case EResourcePileType.ResourcePile:
					if (!ResourcePiles.Contains(feature))
						ResourcePiles.Add(feature);
					break;
			}
			m_ResourcePilePool.Add(feature);
			base.RegisterFeature(feature);
		}

		internal override void DeregisterFeature(ResourcePileFeature feature, int faction = -1)
		{
			if (faction < 0) faction = feature.FactionID;
			if (feature.resources.Count > 0)
			switch (feature.ResourcePileType)
			{
				case EResourcePileType.Corpse:
					if (Corpses.Contains(feature))
						Corpses.Remove(feature);
					break;

				case EResourcePileType.StoragePile:
					if (StoragePiles.Contains(feature))
						StoragePiles.Remove(feature);
					break;

				case EResourcePileType.ResourcePile:
					if (ResourcePiles.Contains(feature))
						ResourcePiles.Remove(feature);
					break;
			}
			m_ResourcePilePool.Remove(feature);
			base.DeregisterFeature(feature);
		}

		public override void UpdateGameLoop()
		{
			//UpdateStatistics();
		}

		public override void UpdateStatistics()
		{
		}

		public static string GetStoragePileIDByPolicy(EStoragePolicy policy)
		{
			return ("pile_" + policy).ToLower();
		}

		public static void PlaceStoragePile(Vector2i pos, string ID, int amount)
		{
			var entityFactory = Main.Instance.GameManager.Factories.SimpleEntityFactory;
			var list = new List<ItemFeature>();
			var data = entityFactory.GetStaticData<ItemFeatureStaticData>(ID);
			if(data.IsStackable)
			{
				var newItem = entityFactory.CreateInstance(ID).GetFeature<ItemFeature>();
				newItem.Quantity = amount;
				list.Add(newItem);
			}
			else
			{
				for(int i = 0; i < amount; i++)
				{
					var newItem = entityFactory.CreateInstance(ID).GetFeature<ItemFeature>();
					newItem.Quantity = 1;
					list.Add(newItem);
				}
			}
			PlaceStoragePile(pos, list);			
		}

		//Places resource pile with supplied materials in its inventory in the nearest empty cell from target entity
		public static bool PlaceStoragePile(BaseEntity entity, string itemID, int amount)
		{
			Vector2i pos = default(Vector2i);
			if (entity.HasFeature<StructureFeature>())
			{
				pos = entity.GetFeature<GridObjectFeature>().GridObjectData.Rect.Centeri;
			}
			else if (entity.HasFeature<UnitFeature>())
			{
				pos = entity.GetFeature<EntityFeature>().GridID;
			}
			else
			{
				if (!entity.HasFeature<ResourcePileFeature>())	//For resource piles it is OK to fail here.
				Debug.LogError("Could not assign position for BaseEntity.");
				return false;
			}
			PlaceStoragePile(pos, itemID, amount);
			return true;
		}

		//Places resource pile with supplied materials in its inventory in the nearest empty cell from target entity
		public static bool PlaceStoragePile(BaseEntity entity, List<ItemFeature> items)
		{
			Vector2i pos = default(Vector2i);
			if (entity.HasFeature<StructureFeature>())
			{
				pos = entity.GetFeature<GridObjectFeature>().GridObjectData.Rect.Centeri;
			}
			else if (entity.HasFeature<UnitFeature>())
			{
				pos = entity.GetFeature<EntityFeature>().GridID;
			}
			else
			{
				Debug.LogError("Could not assign position for BaseEntity.");
				return false;
			}
			PlaceStoragePile(pos, items);
			return true;
		}

		//Places resource pile with supplied materials in its inventory in the nearest empty cell from target position
		public static void PlaceStoragePile(Vector2i pos, List<ItemFeature> items)
		{
			for(int i = items.Count - 1; i >= 0; i--)
			{
				var item = items[i];
				var data = item.StaticData;
				bool transferComplete = false;
				Vector2i tile;
				if (manager.GridMap.FindClosestEmptyTile(pos, InventorySystem.maxPileDistance, out tile))
				{
					var nearPiles = GetNearbyStoragePiles(tile, item.StaticData.Category);
					if (nearPiles != null && nearPiles.Length > 0)
					{
						foreach (var nearPile in nearPiles)
						{
							int count = InventorySystem.GetAddableAmount(nearPile.Entity.Inventory, data.entity.ID, item.Quantity);
							if (count <= 0)
								continue;
							if(data.IsStackable)
							{
								if(count == item.Quantity)
								{
									InventorySystem.AddItem(nearPile.Entity.Inventory, item, false);
									items.RemoveAt(i);
									transferComplete = true;
									break;
								}
								else
								{
									var newItem = Main.Instance.GameManager.Factories.SimpleEntityFactory.CreateInstance(data.entity.ID).GetFeature<ItemFeature>();
									newItem.Quantity = count;
									item.Quantity -= count;
									InventorySystem.AddItem(nearPile.Entity.Inventory, newItem, false);
									continue;
								}																
							}
							else
							{
								InventorySystem.AddItem(nearPile.Entity.Inventory, item, false);
								items.RemoveAt(i);
								transferComplete = true;
								break;
							}							
						}
					}					
				}
				if (transferComplete)
					continue;
				while(item.Quantity > 0)
				{
					BaseEntity res;
					manager.UserToolSystem.GridObjectTool.CreateResourcePile(GetStoragePileIDByPolicy(item.StaticData.Category), tile, EDirection.North, out res, null);
					if (res == null)
						break;
					int count = InventorySystem.GetAddableAmount(res.Inventory, data.entity.ID, item.Quantity);
					if (count <= 0)
						break;
					count = Mathf.Min(count, item.Quantity);
					if(count == item.Quantity)
					{
						InventorySystem.AddItem(res.Inventory, item, false);
						break;
					}
					else
					{
						item.Quantity -= count;
						var newItem = Main.Instance.GameManager.Factories.SimpleEntityFactory.CreateInstance(data.entity.ID).GetFeature<ItemFeature>();
						newItem.Quantity = count;
						item.Quantity -= count;
						InventorySystem.AddItem(res.Inventory, newItem, false);
					}					
				}				
			}					
		}

		public void RegisterBlockingPile(ResourcePileFeature pile)
		{
			if (!BlockingResourcePiles.Contains(pile))
				BlockingResourcePiles.Add(pile);
		}

		public void UnregisterBlockingPile(ResourcePileFeature pile)
		{
			if (BlockingResourcePiles.Contains(pile))
				BlockingResourcePiles.Remove(pile);
		}

		public static void MarkPileGathering(ResourcePileFeature pile, bool gather, bool registerNotification = false)
		{
			if (!pile.canCancelGathering && !gather)
				return;
			if (pile.markedForCollection != gather)
			{
				if (gather)
				{
					pile.markIcon = UI3DFactory.Instance.ShowIconMark(pile.Entity.GetFeature<EntityFeature>().View.transform, UI3DFactory.EIcon.Collect, 1);
					if (registerNotification) notificationSystem.IncreaseInteger("totalPilesMarked");
				}
				else
				{
					UI3DFactory.Instance.Recycle(pile.markIcon);
					if (registerNotification) notificationSystem.DecreaseInteger("totalPilesMarked");
				}
			}
			pile.markedForCollection = gather;
		}

		public static ResourcePileFeature[] GetNearbyStoragePiles(Vector2i pos, EStoragePolicy itemPolicy)
		{
			Stack<ResourcePileFeature> storagePiles = new Stack<ResourcePileFeature>();
			foreach (var pile in manager.SystemsManager.ResourcePileSystem.StoragePiles)
			{
				if (Vector2i.Distance(pos, pile.Entity.GetFeature<EntityFeature>().GridID) <= MAX_TILES_RADIUS
					&& pile.StoragePolicy == itemPolicy)
				{
					storagePiles.Push(pile);
				}
			}
			return storagePiles.ToArray();
		}
		public static ResourcePileFeature[] GetNearbyResourcePiles(Vector2i pos)
		{
			return manager.SystemsManager.ResourcePileSystem.ResourcePiles.Where(x => Vector2i.Distance(pos, x.Entity.GetFeature<EntityFeature>().GridID) <= MAX_TILES_RADIUS).ToArray();
		}

		#region Save System        
		public ISaveable CollectData()
		{
			var data = new ResourcePileSystemSaveData();
			data.workerIDs = new List<string>();
			foreach (var worker in Workers)
			{
				data.workerIDs.Add(worker.Entity.UID.ToString());
			}
			return null;
		}

		public void ApplySaveData(ResourcePileSystemSaveData data)
		{
			if (data == null)
				return;
            if (data.workerIDs != null)
            {
                foreach (var workerID in data.workerIDs)
                {
                    if (string.IsNullOrEmpty(workerID))
                        continue;
                    var id = Guid.Parse(workerID);
                    if (Main.Instance.GameManager.SystemsManager.Entities.ContainsKey(id))
                    {
                        Workers.Add(Main.Instance.GameManager.SystemsManager.Entities[id].GetFeature<CitizenAIAgentFeature>());
                    }
                }
            }
        }
		#endregion
	}
}