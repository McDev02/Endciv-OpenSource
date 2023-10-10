using System.Collections.Generic;

using System.Linq;
using System;
using System.Collections;

namespace Endciv
{
	/// <summary>
	/// Manages Agriculture
	/// </summary>
	public class AgricultureSystem : EntityFeatureSystem<FarmlandFeature>, IAIJob, ISaveable, ILoadable<AgricultureSystemSaveData>
	{
		TimeManager timeManager;
		WeatherSystem weatherSystem;
		static GridMap gridMap;
		public AgricultureSystemConfig agricultureSettings;

		public Dictionary<string, float> Seeds;

		float tickFactor;
		//Todo: add statistics data
		static SimpleEntityFactory Factory;

		#region IAIJob
		public bool IsWorkplace { get { return true; } }
		public bool HasWork { get { return true; } }
		public bool Disabled { get; set; }
		public EOccupation[] Occupations { get { return new EOccupation[] { EOccupation.Farmer }; } }
		public float Priority { get; set; }
		public int MaxWorkers { get; private set; }
		public int WorkerCount { get { return Workers.Count; } }

		// Workers who are registered to work here
		public List<CitizenAIAgentFeature> Workers { get; private set; }
		// Workers who perform transportation work
		//public List<CitizenAIAgentFeature> Transporters { get; private set; }

		public void RegisterWorker(CitizenAIAgentFeature unit, EWorkerType type = EWorkerType.Worker)
		{
			if (!Workers.Contains(unit))
				Workers.Add(unit);

		}

		public void DeregisterWorker(CitizenAIAgentFeature unit)
		{
			Workers.Remove(unit);
		}

		public AITask AskForTask(EOccupation occupation, AIAgentFeatureBase unit)
		{
			var citizen = unit as CitizenAIAgentFeature;
			//AI is not a citizen
			if (citizen == null)
			{
				Debug.LogWarning("No citizen requested Task from Agriculture System");
				return default(AITask);
			}

			if (occupation != EOccupation.Farmer)
				return default(AITask);

			var orderedFarmlands = FeaturesByFaction[unit.FactionID].OrderBy(x => Vector2i.Distance(x.Entity.GetFeature<EntityFeature>().GridID, citizen.Entity.GetFeature<EntityFeature>().GridID));

			HarvestingTask harvestingTask = GetHarvestingTask(citizen, orderedFarmlands);
			if (harvestingTask != null)
				return harvestingTask;

			BringResourcesTask bringResourcesTask = GetBringResourcesTask(citizen, orderedFarmlands);
			if (bringResourcesTask != null)
				return bringResourcesTask;

			WateringTask wateringTask = GetWateringTask(citizen, orderedFarmlands);
			if (wateringTask != null)
				return wateringTask;

			PlantingTask plantingTask = GetPlantingTask(citizen, orderedFarmlands);
			if (plantingTask != null)
				return plantingTask;

			return null;
		}

		private HarvestingTask GetHarvestingTask(CitizenAIAgentFeature unit, IOrderedEnumerable<FarmlandFeature> orderedFarmlands)
		{
			Vector2i plantIndex = new Vector2i();
			bool canHarvest = false;
			FarmlandFeature farmland = null;
			foreach (var farm in orderedFarmlands)
			{
				if (canHarvest = farm.HasGrownPlant(out plantIndex))
				{
					farmland = farm;
					break;
				}
			}
			if (canHarvest)
			{
				return new HarvestingTask(unit.Entity, farmland, plantIndex);
			}
			return null;
		}

		private WateringTask GetWateringTask(CitizenAIAgentFeature unit, IOrderedEnumerable<FarmlandFeature> orderedFarmlands)
		{
			Vector2i plantIndex = new Vector2i();
			bool canWater = false;
			FarmlandFeature farmland = null;
			foreach (var farm in orderedFarmlands)
			{
				if (canWater = farm.HasUnwateredPlant(agricultureSettings.cropsWateringThreshold, out plantIndex))
				{
					farmland = farm;
					break;
				}
			}
			if (canWater)
			{
				return new WateringTask(unit.Entity, farmland, plantIndex);
			}
			return null;
		}

		private PlantingTask GetPlantingTask(CitizenAIAgentFeature unit, IOrderedEnumerable<FarmlandFeature> orderedFarmlands)
		{
			Vector2i plantIndex = new Vector2i();
			bool canPlant = false;
			FarmlandFeature farmland = null;
			foreach (var farm in orderedFarmlands)
			{
				if (canPlant = farm.HasUnplantedPlant(out plantIndex))
				{
					farmland = farm;
					break;
				}
			}
			if (canPlant)
			{
				return new PlantingTask(unit.Entity, farmland, plantIndex);
			}
			return null;
		}

		private BringResourcesTask GetBringResourcesTask(CitizenAIAgentFeature unit, IOrderedEnumerable<FarmlandFeature> orderedFarmlands)
		{
			bool needsWater = false;
			FarmlandFeature farmland = null;
			foreach (var farm in orderedFarmlands)
			{
				if (farm.Entity.Inventory.LoadProgress < 0.8f && farm.assignedWaterTransporter == null)
				{
					farmland = farm;
					break;
				}
			}
			if (!needsWater)
				return null;
			int itemCount = InventorySystem.GetAddableAmount(unit.Entity.Inventory, FactoryConstants.WaterID);
			if (itemCount <= 0)
				return null;
			var resources = new List<ResourceStack>() { new ResourceStack(FactoryConstants.WaterID, itemCount) };
			var storage = Main.Instance.GameManager.SystemsManager.StorageSystem.GetBestStorageForResources(resources, unit.FactionID);
			if (storage == null)
				return null;
			int targetCount = Mathf.Min(itemCount, InventorySystem.GetItemCount(storage.Inventory, FactoryConstants.WaterID));
			if (targetCount <= 0)
				return null;
			resources[0].Amount = targetCount;
			return new BringResourcesTask(unit.Entity, resources, storage.Inventory, farmland.Entity.Inventory, this);
		}

        public void OnTaskStart()
        {

        }

		public void OnTaskComplete(AIAgentFeatureBase unit)
		{
			if (!Workers.Contains(unit))
				return;
			if (unit.CurrentTask != null)
			{
				if (unit.CurrentTask is AgriculturingTask)
				{
					var task = unit.CurrentTask as AgriculturingTask;
					task.UnassignFarmer();
				}
				else if (unit.CurrentTask is BringResourcesTask)
				{
					var task = unit.CurrentTask as BringResourcesTask;
					var taskTarget = task.GetMemberValue<InventoryFeature>("TargetInventory");
					taskTarget.Entity.GetFeature<FarmlandFeature>().assignedWaterTransporter = null;
				}

			}
			DeregisterWorker(unit as CitizenAIAgentFeature);
		}
		#endregion

		public AgricultureSystem(int factions, GridMap gridMap, TimeManager timeManager, WeatherSystem weatherSystem, SimpleEntityFactory factory, AgricultureSystemConfig agricultureSettings) : base(factions)
		{
			Workers = new List<CitizenAIAgentFeature>();
			MaxWorkers = int.MaxValue;
			this.weatherSystem = weatherSystem;
			this.timeManager = timeManager;
			this.agricultureSettings = agricultureSettings;
			AgricultureSystem.Factory = factory;
			AgricultureSystem.gridMap = gridMap;
			Seeds = new Dictionary<string, float>();
			UpdateStatistics();
			tickFactor = timeManager.dayTickFactor;
			Main.Instance.GameManager.SystemsManager.AIAgentSystem.TaskEndedCallback -= OnTaskComplete;
			Main.Instance.GameManager.SystemsManager.AIAgentSystem.TaskEndedCallback += OnTaskComplete;
		}

		public void Run()
		{
			Main.Instance.GameManager.SystemsManager.JobSystem.RegisterJob(this);
		}

		public override void UpdateGameLoop()
		{
			FarmlandFeature farm;
			for (int f = 0; f < FeaturesByFaction.Count; f++)
			{
				for (int i = 0; i < FeaturesByFaction[f].Count; i++)
				{
					farm = FeaturesByFaction[f][i];
					int xlen = farm.cropModels.GetLength(0);
					int ylen = farm.cropModels.GetLength(1);
					float gain, loss;
					for (int x = 0; x < xlen; x++)
					{
						for (int y = 0; y < ylen; y++)
						{
							var crop = farm.cropModels[x, y];
							if (crop == null) continue;

							gain = weatherSystem.RainfillPerTile * 0.8f;
							loss = crop.staticData.waterConsumption * tickFactor * agricultureSettings.cropsGrowthRate;
							crop.humidity.Value -= loss - gain;
							if (crop.cropState == ECropState.Growing)
							{
								crop.fruits += crop.fruitGrowFactor * tickFactor * agricultureSettings.cropsGrowthRate;
								crop.Progress += crop.growFactor * tickFactor * agricultureSettings.cropsGrowthRate;
								if (crop.Progress >= 1)
								{
									crop.Progress = 1;
									crop.cropState = ECropState.Mature;
								}
							}
						}
					}
				}
			}
			//UpdateStatistics();
		}

        public static ItemFeature[] HarvestCrop(FarmlandFeature farmland, CropFeature crop)
		{
			var data = crop.staticData;
			List<ItemFeature> items = new List<ItemFeature>();
			if (crop.fruits >= 1f)
			{
				int amount = (int)Mathf.Floor(crop.fruits);
				for (int i = 0; i < amount; i++)
				{
					var item = Main.Instance.GameManager.Factories.SimpleEntityFactory.CreateInstance(data.fruit).GetFeature<ItemFeature>();
					item.Quantity = 1;
					items.Add(item);
				}
			}
			var system = farmland.System;
			float seedAmount = data.seeds * crop.Progress;
			system.ChangeSeeds(data.entity.ID, seedAmount);

			return items.ToArray();
		}


		public int GetSeeds(string cropId)
		{
			if (!Seeds.ContainsKey(cropId))
				return 0;
			return (int)Seeds[cropId];
		}

		public void ChangeSeeds(string cropId, float value)
		{
			if (Seeds.ContainsKey(cropId))
				Seeds[cropId] = Mathf.Clamp(Seeds[cropId] + value, 0, 999);
			else
				Seeds.Add(cropId, Mathf.Clamp(value, 0, 999));
		}

		public override void UpdateStatistics()
		{
		}

		public static void PlantCrops(FarmlandFeature feature, string id, int amount)
		{
			amount = Mathf.Min(amount, feature.SpaceLeft());
			if (amount <= 0) return;

			feature.System.ChangeSeeds(id, -amount);

			var group = new List<CropFeature>(amount);
            var data = Factory.GetStaticData<CropFeatureStaticData>(id);
			for (int i = 0; i < amount; i++)
			{				
                var factoryParams = new FactoryParams();
                factoryParams.SetParams
                    (
                        new EntityFeatureParams()
                        {
                            FactionID = SystemsManager.MainPlayerFaction
                        },
                        new CropFeatureParams()
                        {
                            CurrentViewID = data.GetRandomViewID()
                        }
                    );
                var entity = Factory.CreateInstance(id, null, factoryParams);
                entity.GetFeature<EntityFeature>().View.transform.parent = feature.Entity.GetFeature<EntityFeature>().View.transform;
                var crop = entity.GetFeature<CropFeature>();
                PlaceCropModel(feature, crop);
				group.Add(crop);
			}
			feature.CropGroups.Add(group);
		}

		private static void PlaceCropModel(FarmlandFeature feature, CropFeature crop)
		{
			var gridObject = feature.Entity.GetFeature<GridObjectFeature>().GridObjectData;
			var rect = gridObject.Rect;

			for (int x = 0; x < rect.Width; x++)
			{
				for (int y = 0; y < rect.Length; y++)
				{
					if (feature.cropModels[x, y] != null) continue;
					var posID = rect.Minimum + new Vector2i(x, y);
					var pos = gridMap.View.GetTileWorldPosition(posID).To3D(feature.Entity.GetFeature<EntityFeature>().View.transform.position.y);
                    crop.Entity.GetFeature<EntityFeature>().View.transform.position = pos;
					feature.cropModels[x, y] = crop;
					return;
				}
			}
		}

		/// <summary>
		/// Used by loading process
		/// </summary>
		/// <param name="feature"></param>
		/// <param name="data"></param>
		/// <param name="variationID"></param>
		/// <param name="index"></param>
		public static CropFeature PlantCrops(FarmlandFeature feature, string id, int variationID, SerVector2i index)
		{
            var factoryParams = new FactoryParams();
            factoryParams.SetParams
                (
                    new EntityFeatureParams()
                    {
                        FactionID = SystemsManager.MainPlayerFaction
                    },
                    new CropFeatureParams()
                    {
                        CurrentViewID = variationID
                    }
                );
            var entity = Factory.CreateInstance(id, null, factoryParams);
            entity.GetFeature<EntityFeature>().View.transform.parent = feature.Entity.GetFeature<EntityFeature>().View.transform;
            var crop = entity.GetFeature<CropFeature>();

			var gridObject = feature.Entity.GetFeature<GridObjectFeature>().GridObjectData;
			var rect = gridObject.Rect;

			var posID = rect.Minimum + new Vector2i(index.X, index.Y);
			var pos = gridMap.View.GetTileWorldPosition(posID).To3D(feature.Entity.GetFeature<EntityFeature>().View.transform.position.y);

            entity.GetFeature<EntityFeature>().View.transform.position = pos;
			feature.cropModels[index.X, index.Y] = crop;
			return crop;
		}

		#region Save System        
		public ISaveable CollectData()
		{
			var data = new AgricultureSystemSaveData();
			data.workerUIDs = new List<string>();
			foreach (var worker in Workers)
			{
				data.workerUIDs.Add(worker.Entity.UID.ToString());
			}
			data.seeds = Seeds;
			return data;
		}

		public void ApplySaveData(AgricultureSystemSaveData data)
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
			Seeds = new Dictionary<string, float>(data.seeds);
		}
		#endregion
	}
}