using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

namespace Endciv
{	
	[Serializable]
	public class ProductionFeature : Feature<ProductionFeatureSaveData>, IAIJob
	{
		//Static Data
		public ProductionStaticData StaticData { get; private set; }

		public override void Setup(BaseEntity entity, FeatureParamsBase args = null)
		{
			base.Setup(entity);
			factory = Main.Instance.GameManager.Factories.SimpleEntityFactory;
			StaticData = Entity.StaticData.GetFeature<ProductionStaticData>();
			MaxWorkers = StaticData.ProductionLines;

			Workers = new List<CitizenAIAgentFeature>(MaxWorkers);
			ActiveWorkers = new CitizenAIAgentFeature[MaxWorkers];
			Transporters = new List<CitizenAIAgentFeature>(MaxWorkers);
			ProductionLines = new RecipeFeature[StaticData.ProductionLines];

			if (!Entity.Inventory.Chambers.Contains("Output"))
				outputChamberID = InventorySystem.AddChamber(Entity.Inventory, "Output");
			else
				outputChamberID = Entity.Inventory.Chambers.IndexOf("Output");
		}		

		//Properties
		public RecipeFeature[] ProductionLines { get; private set; }

		#region IJob
		public bool IsWorkplace { get { return true; } }
		public bool HasWork { get { return true; } }
		public bool Disabled { get; set; }
		public EOccupation[] Occupations { get { return new EOccupation[] { EOccupation.Production, EOccupation.Supply }; } }

		public float UserDefinedPriority { get; set; }
		public float Priority { get { return UserDefinedPriority; } }
		public int MaxWorkers { get; private set; }
		public int WorkerCount { get { return Workers.Count; } }

		// Workers who are registered to work here
		public List<CitizenAIAgentFeature> Workers { get; private set; }
		// Workers who work at which production line currently
		public CitizenAIAgentFeature[] ActiveWorkers { get; private set; }
		// Workers who perform transportation work
		public List<CitizenAIAgentFeature> Transporters { get; private set; }
		#endregion

		private SystemsManager manager;
		private SimpleEntityFactory factory;

		public int outputChamberID;

		//Methods
		public override void Run(SystemsManager manager)
		{
			this.manager = manager;
			manager.JobSystem.RegisterJob(this);
			manager.ProductionSystem.RegisterFeature(this);
            manager.AIAgentSystem.TaskEndedCallback -= OnTaskComplete;
            manager.AIAgentSystem.TaskEndedCallback += OnTaskComplete;
            base.Run(manager);
		}

		public override void Stop()
		{
			manager.JobSystem.DeregisterJob(this);
			manager.ProductionSystem.DeregisterFeature(this);
            manager.AIAgentSystem.TaskEndedCallback -= OnTaskComplete;
            base.Stop();
		}

		public override void OnFactionChanged(int oldFaction)
		{
			base.OnFactionChanged(oldFaction);
			SystemsManager.ProductionSystem.DeregisterFeature(this, oldFaction);
			SystemsManager.ProductionSystem.RegisterFeature(this);
		}

		public void RegisterWorker(CitizenAIAgentFeature unit, EWorkerType type = EWorkerType.Worker)
		{
			switch (type)
			{
				case EWorkerType.Worker:
					if (!Workers.Contains(unit))
					{
						Workers.Add(unit);
					}
					break;
				case EWorkerType.Transporter:
					if (!Transporters.Contains(unit))
					{
						Transporters.Add(unit);
					}
					break;
				default:
					break;
			}
		}

		public void DeregisterWorker(CitizenAIAgentFeature unit)
		{
				Workers.Remove(unit);
				Transporters.Remove(unit);
		}
		
		public AITask AskForTask(EOccupation occupation, AIAgentFeatureBase unit)
		{
			var citizen = unit as CitizenAIAgentFeature;
			if (citizen == null)
			{
				Debug.LogWarning("Non citizen requested Task from Production Facility");
				return default(AITask);
			}

			if (citizen.Occupation == EOccupation.Supply || citizen.Occupation == EOccupation.Production)
			{
                AITask task;
				//Bump priority of store resources task if inventory is full
				if (Entity.Inventory.CapacityLeft <= 0)
				{
					task = GetStoreResourcesTask(occupation, citizen);
					if (task != null)
						return task;
				}

				if (citizen.Occupation == EOccupation.Production)
				{
					ProductionTask productionTask = GetProductionTask(occupation, citizen);
					if (productionTask != null)
						return productionTask;
				}

				BringResourcesTask bringResourcesTask = GetBringResourcesTask(occupation, citizen);
				if (bringResourcesTask != null)
					return bringResourcesTask;
				//If no other task could be assigned, call store resources task which either returns null or the task
				task = GetStoreResourcesTask(occupation, citizen);
				if (task != null)
				{
					return task;
				}
			}

			return null;
		}

		public StoreResourcesTask GetStoreResourcesTask(EOccupation occupation, CitizenAIAgentFeature citizen)
		{
			var inventory = Entity.Inventory;
			if (inventory.ItemPoolByChambers[outputChamberID].Count <= 0)
			{
				return null;
			}

			//Get a list of all resources in output chamber
			var resources = InventorySystem.GetChamberContentList(inventory, outputChamberID);

			if (resources.Length <= 0)
			{
				return null;
			}

			var transferedResources = new List<ResourceStack>();

			//Iterate all resources and collect those whose policies match those of existing storages and that the unit can carry
			for (int i = resources.Length - 1; i >= 0; i--)
			{
				var data = Main.Instance.GameManager.Factories.SimpleEntityFactory.EntityStaticData[resources[i].ResourceID].
					GetFeature<ItemFeatureStaticData>();				

				int amount = InventorySystem.GetAddableAmount(citizen.Entity.Inventory, resources[i].ResourceID);

				//Unit can't even carry 1 unit of said resource - skip
				if (amount <= 0)
					continue;

				//Resource's policy doesn't let it get stored anywhere - skip
				var storages = manager.StorageSystem.GetAllStoragesAccepting(data, FactionID);
				if (storages.Length <= 0)
					continue;

				//Item can be deposited, add as much as possible for the unit to carry
				amount = Mathf.Min(amount, resources[i].Amount);
				transferedResources.Add(new ResourceStack(resources[i].ResourceID, amount));
			}

			//No resources could be transfered, return nothing
			if (transferedResources.Count <= 0)
			{
				return null;
			}
			if (!Transporters.Contains(citizen))
				Transporters.Add(citizen);
			return new StoreResourcesTask(citizen.Entity, transferedResources, inventory, outputChamberID);
		}

		public BringResourcesTask GetBringResourcesTask(EOccupation occupation, CitizenAIAgentFeature citizen)
		{
			//Tally up all resources required by each production line
			List<ResourceStack> resources = new List<ResourceStack>();
			foreach (var recipe in ProductionLines)
			{
				if (recipe == null)
					continue;
				foreach (var entry in recipe.StaticData.InputResources)
				{
					ResourceStack stack = null;
					if ((stack = resources.FirstOrDefault(x => x.ResourceID == entry.ResourceID)) == null)
					{
						stack = new ResourceStack(entry.ResourceID, entry.Amount * recipe.BatchesLeft);
						resources.Add(stack);
					}
					else
					{
						stack.Amount += entry.Amount * recipe.BatchesLeft;
					}
				}
			}

			//If we require no resources, return no task
			if (resources == null || resources.Count <= 0)
				return null;

			//Trim down resources needed
			for (int i = resources.Count - 1; i >= 0; i--)
			{
				//Reduce total resources by the resources already in inventory
				resources[i].Amount -= Mathf.Min(InventorySystem.GetItemCount(Entity.Inventory, resources[i].ResourceID), resources[i].Amount);
				if (resources[i].Amount <= 0)
				{
					resources.RemoveAt(i);
					continue;
				}

				//Reduce total resources by the resources already being brought by other units
				foreach (var worker in Transporters)
				{
					var workerTask = citizen.CurrentTask as BringResourcesTask;

					//Skip worker if his task isn't transport
					if (workerTask == null)
						continue;
					var workerResources = workerTask.GetMemberValue<List<ResourceStack>>("TransferedResources");
					//Reduce total resources by the resources carried by this worker
					foreach (var workerResource in workerResources)
					{
						if (workerResource.ResourceID == resources[i].ResourceID)
						{
							resources[i].Amount -= Mathf.Min(resources[i].Amount, workerResource.Amount);
							if (resources[i].Amount <= 0)
							{
								resources.RemoveAt(i);
								continue;
							}
						}
					}
				}

				//Check if total resources exist in storages in general, remove from list those that don't exist
				int overallCount = manager.StorageSystem.CountResources(resources[i].ResourceID, FactionID);
				if (overallCount <= 0)
				{
					resources.RemoveAt(i);
					continue;
				}
				else
				{
					//We make sure we don't ask the unit for more resources than are generally available
					resources[i].Amount = Mathf.Min(overallCount, resources[i].Amount);
				}

			}

			//If no total resources are needed, no transport task is necessary 
			if (resources == null || resources.Count <= 0)
				return null;

			//Check unit's available inventory capacity and make sure we only ask items that it can carry
			float capacity = citizen.Entity.Inventory.CapacityLeft;

			//If unit can't carry more items, don't let it take a task at all
			if (capacity <= 0)
				return null;

			//Generate the task's item list
			List<ResourceStack> taskResources = new List<ResourceStack>();
			for (int i = resources.Count - 1; i >= 0; i--)
			{
				var mat = Main.Instance.GameManager.Factories.SimpleEntityFactory.GetStaticData<ItemFeatureStaticData>(resources[i].ResourceID);
				var count = Mathf.Min((int)Mathf.Round(capacity / mat.Mass), resources[i].Amount);

				//Unit can't carry even 1 item of this material, go to next material
				if (count <= 0)
				{
					resources.RemoveAt(i);
					continue;
				}

				//Add item to list of resources
				var entry = new ResourceStack(resources[i].ResourceID, count);
				taskResources.Add(entry);

				//Remove item from total resources needed
				resources.RemoveAt(i);

				//if the unit has no remaining capacity, stop looking up resources
				capacity -= count * mat.Mass;
				if (capacity <= 0f)
					break;
			}

			//If we have resources to send to the unit, generate the task
			if (taskResources.Count > 0)
			{
				var storage = manager.StorageSystem.GetBestStorageForResources(taskResources, FactionID);
				return new BringResourcesTask(citizen.Entity, taskResources, storage.Inventory, Entity.Inventory, this);
			}
			else
				return null;
		}

		public ProductionTask GetProductionTask(EOccupation occupation, CitizenAIAgentFeature citizen)
		{
			bool canProduceItems = false;
			int validOrder = -1;
			// Check if any order can be produced
			for (int i = 0; i < ProductionLines.Length; i++)
			{				
				// Skip if someone is already working the production line
				if (ActiveWorkers[i] != null)
					continue;

				//Try to generate a new order for line
				if (ProductionLines[i] == null)
					ProductionLines[i] = manager.ProductionSystem.GetLocalProductionOrder(this);

				//Check if order has been assigned for this production line yet
				if (ProductionLines[i] == null)
					continue;

				// Check if the Facility has enough materials for this order				
				bool hasResources = manager.ProductionSystem.HasMaterialsForRecipe(this, ProductionLines[i]);

				// Skip if we don't have resources to craft even 1 item
				if (!hasResources)
					continue;

				validOrder = i;
				canProduceItems = true;
			}

			if (canProduceItems)
			{
				//Return new Production Task based on validOrder id
				ActiveWorkers[validOrder] = citizen;
				return new ProductionTask(citizen.Entity, this);
			}
			else
				return null;
		}

        public void OnTaskStart()
        {

        }

        public void OnTaskComplete(AIAgentFeatureBase unit)
		{
			var citizen = unit as CitizenAIAgentFeature;
            if (citizen == null)
                return;
			if ((unit.CurrentTask is BringResourcesTask || unit.CurrentTask is StoreResourcesTask) &&
                Transporters.Contains(citizen))
            {
                Transporters.Remove(citizen);
                DeregisterWorker(citizen);
            }
			else if (unit.CurrentTask is ProductionTask)
			{
				for (int i = 0; i < ActiveWorkers.Length; i++)
				{
					if (unit == ActiveWorkers[i])
					{
						ActiveWorkers[i] = null;
                        DeregisterWorker(citizen);
                        break;
					}
				}
			}			
		}		

		public override ISaveable CollectData()
		{
			var data = new ProductionFeatureSaveData();
			data.recipes = new EntitySaveData[ProductionLines.Length];
			for (int i = 0; i < data.recipes.Length; i++)
			{
				if (ProductionLines[i] != null)
					data.recipes[i] = (EntitySaveData)ProductionLines[i].Entity.CollectData();				
			}
			data.workers = new string[Workers.Count];
			for (int i = 0; i < data.workers.Length; i++)
			{
				data.workers[i] = Workers[i].Entity.UID.ToString();
			}
			data.activeWorkers = new string[ActiveWorkers.Length];
			for (int i = 0; i < data.activeWorkers.Length - 1; i++)
			{
				if (ActiveWorkers[i] != null)
					data.activeWorkers[i] = ActiveWorkers[i].Entity.UID.ToString();
				else
					data.activeWorkers[i] = string.Empty;
			}
			data.transporters = new string[Transporters.Count];
			for (int i = 0; i < data.transporters.Length; i++)
			{
				data.transporters[i] = Transporters[i].Entity.UID.ToString();
			}
			data.structureUID = Entity.UID.ToString();
			return data;
		}

		public override void ApplyData(ProductionFeatureSaveData data)
		{
			ProductionLines = new RecipeFeature[StaticData.ProductionLines];
			for (int i = 0; i < data.recipes.Length; i++)
			{
				if (data.recipes[i] == null)
					continue;
				var entity = factory.CreateInstance(data.recipes[i].id, data.recipes[i].UID);
				entity.ApplySaveData(data.recipes[i]);
				ProductionLines[i] = entity.GetFeature<RecipeFeature>();
			}
			Workers = new List<CitizenAIAgentFeature>();
			for (int i = 0; i < data.workers.Length; i++)
			{
                if (string.IsNullOrEmpty(data.workers[i]))
                    continue;
                var guid = Guid.Parse(data.workers[i]);
				var worker = manager.AIAgentSystem.CitizenAISystem.GetCitizenByUID(guid);
				if (worker == null)
				{
					Debug.Log("Could not find citizen with UID " + data.workers[i] + ".");
					continue;
				}
				Workers.Add(worker);
			}
			ActiveWorkers = new CitizenAIAgentFeature[MaxWorkers];
			for (int i = 0; i < data.activeWorkers.Length; i++)
			{
                if (string.IsNullOrEmpty(data.activeWorkers[i]))
                    continue;
                var guid = Guid.Parse(data.activeWorkers[i]);
                var worker = manager.AIAgentSystem.CitizenAISystem.GetCitizenByUID(guid);
				if (worker == null)
				{
					Debug.Log("Could not find citizen with UID " + data.activeWorkers[i] + ".");
					continue;
				}
				ActiveWorkers[i] = worker;
			}
			Transporters = new List<CitizenAIAgentFeature>();
			for (int i = 0; i < data.transporters.Length; i++)
			{
                if (string.IsNullOrEmpty(data.transporters[i]))
                    continue;
                var guid = Guid.Parse(data.transporters[i]);
                var worker = manager.AIAgentSystem.CitizenAISystem.GetCitizenByUID(guid);
				if (worker == null)
				{
					Debug.Log("Could not find citizen with UID " + data.transporters[i] + ".");
					continue;
				}
				Transporters.Add(worker);
			}
		}
	}
}