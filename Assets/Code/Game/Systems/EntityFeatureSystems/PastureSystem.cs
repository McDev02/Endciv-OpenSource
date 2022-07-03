using System.Collections.Generic;
using UnityEngine;
using System;

namespace Endciv
{
	/// <summary>
	/// Manages Pastures
	/// </summary>
	public class PastureSystem : EntityFeatureSystem<PastureFeature>, IAIJob, ISaveable, ILoadable<PastureSystemSaveData>
	{
		private TimeManager TimeManager { get; set; }
		private List<List<CattleFeature>> Cattle { get; set; }

		#region IAIJob
		public bool IsWorkplace { get { return true; } }
		public bool HasWork { get { return true; } }
		public bool Disabled { get; set; }
		public EOccupation[] Occupations { get { return new EOccupation[] { EOccupation.Labour, EOccupation.Herder }; } }
		public float Priority { get; set; }
		public int MaxWorkers { get; private set; }
		public int WorkerCount { get { return Workers.Count; } }

		public List<CitizenAIAgentFeature> Workers { get; private set; }

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
				Debug.LogWarning("No citizen requested Task from Pasture System");
				return default(AITask);
			}

			if (occupation != EOccupation.Herder && occupation != EOccupation.Labour)
				return default(AITask);			

			if(occupation == EOccupation.Labour)
			{
				var task = GetStoreResourcesTask(citizen);
				return task;
			}
			else
			{
				var foodTask = GetBringFoodTask(unit);
				if (foodTask != null)
					return foodTask;

				var harvestTask = GetHarvestAnimalTask(unit);
				if (harvestTask != null)
					return harvestTask;

				var maintainanceTask = GetPastureMaintainanceTask(unit);
				if (maintainanceTask != null)
					return maintainanceTask;
			}
			return null;
		}

		public StoreResourcesTask GetStoreResourcesTask(CitizenAIAgentFeature citizen)
		{
			foreach(var pasture in FeaturesByFaction[citizen.Entity.factionID])
			{
				var inventory = pasture.Entity.Inventory;
				if (inventory.ItemPoolByChambers[0].Count <= 0)
				{
					continue;
				}

				//Get a list of all resources in output chamber
				var resources = InventorySystem.GetChamberContentList(inventory, InventorySystem.ChamberMainID);				

				if (resources.Length <= 0)
				{
					continue;
				}

				var transferedResources = new List<ResourceStack>();

				//Iterate all resources and collect those whose policies match those of existing storages and that the unit can carry
				for (int i = resources.Length - 1; i >= 0; i--)
				{
					var data = Main.Instance.GameManager.Factories.SimpleEntityFactory.EntityStaticData[resources[i].ResourceID].GetFeature<ItemFeatureStaticData>();

					int amount = InventorySystem.GetAddableAmount(citizen.Entity.Inventory, resources[i].ResourceID);

					//Unit can't even carry 1 unit of said resource - skip
					if (amount <= 0)
						continue;

					//Resource's policy doesn't let it get stored anywhere - skip
					var storages = Main.Instance.GameManager.SystemsManager.
						StorageSystem.GetAllStoragesAccepting(data, citizen.Entity.factionID);
					if (storages.Length <= 0)
						continue;

					//Item can be deposited, add as much as possible for the unit to carry
					amount = Mathf.Min(amount, resources[i].Amount);
					transferedResources.Add(new ResourceStack(resources[i].ResourceID, amount));
				}

				//No resources could be transfered, return nothing
				if (transferedResources.Count <= 0)
				{
					continue;
				}
				var task = new StoreResourcesTask(citizen.Entity, transferedResources, inventory, InventorySystem.ChamberMainID);
				task.priority = 10000;
				return task;
			}
			return null;
		}

		private AITask GetBringFoodTask(AIAgentFeatureBase unit)
		{
			var storageSystem = Main.Instance.GameManager.SystemsManager.StorageSystem;
			foreach(var pasture in FeaturesByFaction[unit.Entity.factionID])
			{
				if (pasture.WaterProgress > 0.4f && pasture.NutritionProgress > 0.4f)
					continue;
				StorageFeature storage = null;
				if(pasture.NutritionProgress <= 0.4f)
				{
					List<ResourceStack> resources = new List<ResourceStack>();
					foreach (var item in pasture.StaticData.food)
					{
						resources.Add(new ResourceStack(item, 10));
					}
					storage = storageSystem.GetBestStorageForResources(resources, unit.Entity.factionID);

					if (storage != null)
					{
						int capacityLeft = unit.Entity.Inventory.CapacityLeft;
						List<ResourceStack> transferedResources = new List<ResourceStack>();
						foreach(var res in resources)
						{
							if (capacityLeft <= 0f)
								break;
							int count = InventorySystem.GetItemCount(storage.Entity.Inventory, res.ResourceID);
							if (count <= 0)
								continue;
							var data = Main.Instance.GameManager.Factories.SimpleEntityFactory.EntityStaticData[res.ResourceID].GetFeature<ItemFeatureStaticData>();
							if (data == null)
								continue;
							if (data.Mass > capacityLeft)
								continue;
							int fits = capacityLeft / data.Mass;
							int finalCount = Mathf.Min(fits, count);
							if (finalCount <= 0)
								continue;
							transferedResources.Add(new ResourceStack(res.ResourceID, finalCount));
							capacityLeft -= finalCount * data.Mass;
							if (capacityLeft <= 0f)
								break;
						}
						if(transferedResources.Count > 0)
						{
							//Return transfer resources from that storage to pasture
							var task = new SupplyPastureTask(unit.Entity, transferedResources, storage.Entity.Inventory, pasture, this);
							task.priority = 10000;
							return task;
						}												
					}
				}
				
				if(pasture.WaterProgress <= 0.4f)
				{
					List<ResourceStack> resources = new List<ResourceStack>();
					resources.Add(new ResourceStack(FactoryConstants.WaterID, 10));
					storage = storageSystem.GetBestStorageForResources(resources, unit.Entity.factionID);

					if (storage != null)
					{
						int capacityLeft = unit.Entity.Inventory.CapacityLeft;
						List<ResourceStack> transferedResources = new List<ResourceStack>();
						foreach (var res in resources)
						{
							if (capacityLeft <= 0f)
								break;
							int count = InventorySystem.GetItemCount(storage.Entity.Inventory, res.ResourceID);
							if (count <= 0)
								continue;
							var data = Main.Instance.GameManager.Factories.SimpleEntityFactory.EntityStaticData[res.ResourceID].GetFeature<ItemFeatureStaticData>();
							if (data == null)
								continue;
							if (data.Mass > capacityLeft)
								continue;
							int fits = capacityLeft / data.Mass;
							int finalCount = Mathf.Min(fits, count);
							if (finalCount <= 0)
								continue;
							transferedResources.Add(new ResourceStack(res.ResourceID, finalCount));
							capacityLeft -= finalCount * data.Mass;
							if (capacityLeft <= 0f)
								break;
						}
						if (transferedResources.Count > 0)
						{
							//Return transfer resources from that storage to pasture
							var task = new SupplyPastureTask(unit.Entity, transferedResources, storage.Entity.Inventory, pasture, this);
							task.priority = 10000;
							return task;
						}
					}
				}				
			}
			return null;
		}

		private AITask GetHarvestAnimalTask(AIAgentFeatureBase unit)
		{
			var storageSystem = Main.Instance.GameManager.SystemsManager.StorageSystem;
			foreach (var pasture in FeaturesByFaction[unit.Entity.factionID])
			{
				if (pasture.Cattle.Count <= 0)
					continue;
				foreach(var cattle in pasture.Cattle)
				{
					if (!cattle.staticData.RequiresHarvesting)
						continue;
					if (cattle.ProducedGoods < 1)
						continue;
					if (cattle.ProducedGoods / cattle.staticData.ProductionCapacity < 0.5f)
						continue;
					if (!cattle.Entity.HasFeature<AnimalAIAgentFeature>())
						continue;
					var ai = cattle.Entity.GetFeature<AnimalAIAgentFeature>();
					if (!ai.IsRunning)
						continue;
					var data = Main.Instance.GameManager.Factories.SimpleEntityFactory.EntityStaticData[cattle.staticData.ProducedItem].GetFeature<ItemFeatureStaticData>();
					
					var storages = storageSystem.GetAllStoragesAccepting(data, unit.Entity.factionID);
					if (storages.Length <= 0)
						continue;

					var task = new HarvestAnimalTask(unit.Entity, pasture, cattle);
					task.priority = 10000;
					return task;
				}
			}
			return null;
		}

		private AITask GetPastureMaintainanceTask(AIAgentFeatureBase unit)
		{
			foreach (var pasture in FeaturesByFaction[unit.Entity.factionID])
			{
				if (pasture.Filth < 0.5f)
					continue;
				if (pasture.Cleaner != null)
					continue;
				var task = new PastureMaintainanceTask(unit.Entity, pasture);
				task.priority = 10000;
				return task;				
			}
			return null;
		}

        public void OnTaskStart()
        {

        }

        public void OnTaskComplete(AIAgentFeatureBase unit)
		{
			var harvestTask = unit.CurrentTask as HarvestAnimalTask;
			if (harvestTask != null)
			{
				var cattle = harvestTask.GetMemberValue<CattleFeature>(HarvestAnimalTask.cattleKey);
				if (cattle != null && cattle.Entity.HasFeature<EntityFeature>())
				{
					if (cattle.Entity.GetFeature<EntityFeature>().IsAlive)
					{
						cattle.Entity.GetFeature<AnimalAIAgentFeature>().
						   Run(Main.Instance.GameManager.SystemsManager);
					}										
				}
				return;
			}
			var maintainanceTask = unit.CurrentTask as PastureMaintainanceTask;
			if(maintainanceTask != null)
			{
				var pasture = maintainanceTask.
					GetMemberValue<PastureFeature>(PastureMaintainanceTask.pastureKey);
				if(pasture != null)
				{
					pasture.Cleaner = null;
				}
			}
		}
		#endregion

		public PastureSystem(int factions, TimeManager timeManager) : base(factions)
		{			
			Workers = new List<CitizenAIAgentFeature>();
			Cattle = new List<List<CattleFeature>>(factions);
			MaxWorkers = int.MaxValue;
			TimeManager = timeManager;
			for (int i = 0; i < factions; i++)
			{				
				Cattle.Add(new List<CattleFeature>(i == 0 ? 32 : 8));
			}
			Main.Instance.GameManager.SystemsManager.AIAgentSystem.TaskEndedCallback -= OnTaskComplete;
			Main.Instance.GameManager.SystemsManager.AIAgentSystem.TaskEndedCallback += OnTaskComplete;
			UpdateStatistics();			
		}

		public void RegisterCattle(CattleFeature cattle)
		{
			var factionID = cattle.Entity.factionID;
			if (Cattle[factionID].Contains(cattle))
				return;
			Cattle[factionID].Add(cattle);
		}

		public void DeregisterCattle(CattleFeature cattle, int factionID = -1)
		{
			if(factionID == -1)
				factionID = cattle.Entity.factionID;			
			if (!Cattle[factionID].Contains(cattle))
				return;
			Cattle[factionID].Remove(cattle);
		}

		public override void UpdateGameLoop()
		{
			UpdateCattle();
			UpdatePastures();
		}

		private void UpdateCattle()
		{
			for (int f = 0; f < Cattle.Count; f++)
			{
				for (int i = 0; i < Cattle[f].Count; i++)
				{
					var cattle = Cattle[f][i];
					if (!cattle.Entity.GetFeature<EntityFeature>().IsAlive)
						continue;
					if (cattle.staticData.ProductionCapacity <= 0f)
						continue;
					if (cattle.ProducedGoods >= cattle.staticData.ProductionCapacity)
					{
						TryDepositProductionToInventory(cattle);
						continue;
					}
						
					UpdateCattleProduction(cattle);
					TryDepositProductionToInventory(cattle);
				}
			}
		}

		private void UpdatePastures()
		{
			for(int i = 0; i < FeaturesByFaction.Count; i++)
			{
				foreach (var pasture in FeaturesByFaction[i])
				{
					//Add Filth
					pasture.Filth = Mathf.Clamp(pasture.Filth + 0.001f, 0f, 1f);

					//Update Pasture Cattle
					if (pasture.Cattle.Count <= 0)
						continue;
					if (pasture.CurrentWater <= 0f && pasture.CurrentNutrition <= 0f)
						continue;
					foreach(var cattle in pasture.Cattle)
					{
						if (!cattle.Entity.GetFeature<EntityFeature>().IsAlive)
							continue;
						if (!cattle.Entity.HasFeature<LivingBeingFeature>())
							continue;
						var being = cattle.Entity.GetFeature<LivingBeingFeature>();
						if(being.Thirst.Progress <= being.StaticData.ThirstUrgencyThreshold.max)
						{							
							float water = Mathf.Min(GameConfig.WaterPortion / 2f, pasture.CurrentWater);
							if(water > 0f)
							{
								pasture.CurrentWater -= water;
								being.Thirst.Value += water;
							}
						}
						
						if(being.Hunger.Progress <= being.StaticData.HungerUrgencyThreshold.max)
						{							
							float nutrition = Mathf.Min(0.5f, pasture.CurrentNutrition);
							if (nutrition > 0f)
							{
								pasture.CurrentNutrition -= nutrition;
								being.Hunger.Value += nutrition;
							}
						}						
					}
				}
			}			
		}

		private void UpdateCattleProduction(CattleFeature cattle)
		{
			var being = cattle.Entity.GetFeature<LivingBeingFeature>();
			if (being == null || being.age == ELivingBeingAge.Adult)
			{
				cattle.ProducedGoods += cattle.staticData.ProductionAmount * TimeManager.dayTickFactor;
				if (cattle.ProducedGoods > cattle.staticData.ProductionCapacity)
				{
					cattle.ProducedGoods = cattle.staticData.ProductionCapacity;
				}
			}
		}

		private void TryDepositProductionToInventory(CattleFeature cattle)
		{
			if (cattle.staticData.RequiresHarvesting)
				return;
			if (!cattle.HasHome)
				return;
			if (cattle.ProducedGoods < 1f)
				return;
			var pasture = cattle.Pasture;
			while(cattle.ProducedGoods >= 1f  && pasture.Entity.Inventory.LoadProgress <= 0.9f)
			{
				cattle.ProducedGoods--;
				var item = Main.Instance.GameManager.Factories.SimpleEntityFactory.
					CreateInstance(cattle.staticData.ProducedItem).GetFeature<ItemFeature>();
				
				InventorySystem.AddItem(pasture.Entity.Inventory, item, false, InventorySystem.ChamberMainID);
			}
		}

		public void Run()
		{
			Main.Instance.GameManager.SystemsManager.JobSystem.RegisterJob(this);
		}

		public override void UpdateStatistics()
		{
		}				

		public PastureFeature GetAvailablePastureForCattle(CattleFeature cattle)
		{
			foreach (var feature in FeaturesByFaction[cattle.Entity.factionID])
			{
				if(feature.Entity.HasFeature<ConstructionFeature>())
				{
					var construction = feature.Entity.GetFeature<ConstructionFeature>();
					if (construction.MarkedForDemolition)
						continue;
				}
				if (!feature.AnimalPolicy.Contains(cattle.Entity.StaticData.ID))
					continue;
				if (!CanPastureFitCattle(feature, cattle))
					continue;
				return feature;
			}
			return null;
		}

		public bool CanPastureFitCattle(PastureFeature pasture, CattleFeature cattle)
		{
			return pasture.CapacityLeft >= cattle.Entity.GetFeature<ItemFeature>().StaticData.Mass;			
		}

		public void AddCattleToPasture(PastureFeature pasture, CattleFeature cattle)
		{
			if (!CanPastureFitCattle(pasture, cattle))
				return;
			if (pasture.Cattle.Contains(cattle))
				return;
			if (pasture.ReservedCattle.Contains(cattle))
				return;
			pasture.ReservedCattle.Add(cattle);
			cattle.Pasture = pasture;
			pasture.AddLoad(cattle.Entity.GetFeature<ItemFeature>().StaticData.Mass);
		}

		public void ReserveCattle(PastureFeature pasture, CattleFeature cattle)
		{
			pasture.Cattle.Remove(cattle);
			pasture.ReservedCattle.Add(cattle);
		}

		public void UnreserveCattle(PastureFeature pasture, CattleFeature cattle)
		{
			pasture.ReservedCattle.Remove(cattle);
			pasture.Cattle.Add(cattle);
		}

		public void RemoveCattleFromPasture(PastureFeature pasture, CattleFeature cattle)
		{
			pasture.Cattle.Remove(cattle);
			pasture.ReservedCattle.Remove(cattle);
			cattle.Pasture = null;
			pasture.AddLoad(-cattle.Entity.GetFeature<ItemFeature>().StaticData.Mass);
		}

		#region Save System        
		public ISaveable CollectData()
		{
			var data = new PastureSystemSaveData();
			data.workerUIDs = new List<string>();
			foreach (var worker in Workers)
			{
				data.workerUIDs.Add(worker.Entity.UID.ToString());
			}
			return data;
		}

		public void ApplySaveData(PastureSystemSaveData data)
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
		#endregion
	}
}