using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

namespace Endciv
{
	public class ConstructionFeature : Feature<ConstructionFeatureSaveData>, IAIJob
	{
		//Static Data
		public ConstructionStaticData StaticData { get; private set; }
		public UI3DConstruction constructionInfo;

		//Properties
		private ConstructionSystem.EConstructionState constructionState;
		public ConstructionSystem.EConstructionState ConstructionState
		{
			get
			{
				return constructionState;
			}
			set
			{
				if (value != constructionState)
				{
					constructionState = value;
					OnConstructionStateChanged?.Invoke(this, constructionState);
				}
			}
		}
		public bool MarkedForDemolition;
		public float DemolitionStartingProgress = 0f;
		public float CurrentConstructionPoints;
		/// <summary>
		/// Progress between 0 - 1
		/// </summary>
		public float ConstructionProgress
		{
			get
			{
				if (StaticData.MaxConstructionPoints <= 0) return 0;
				else return CurrentConstructionPoints / StaticData.MaxConstructionPoints;
			}
		}

		public float DemolitionProgress { get { return 1f - ConstructionProgress; } }

		public Action<ConstructionFeature, ConstructionSystem.EConstructionState> OnConstructionStateChanged;

		private List<ResourceStack> missingResources;

		public float ResourceProgress
		{
			get
			{
				int totalSum = StaticData.Cost.Sum(x => x.Amount);
				if (totalSum == 0)
					return 1f;
				int currentSum = 0;
				foreach (var item in StaticData.Cost)
				{
					currentSum += InventorySystem.GetItemCount(Entity.Inventory, item.ResourceID);
				}
				return currentSum / (float)totalSum;
			}
		}

		private void CalculateMissingResources(InventoryFeature inventory)
		{
			if (Entity.Inventory == null)
				return;
			missingResources = new List<ResourceStack>();

			int totalSum = StaticData.Cost.Sum(x => x.Amount);
			if (totalSum == 0)
				return;
			foreach (var item in StaticData.Cost)
			{
				var amount = item.Amount - InventorySystem.GetItemCount(Entity.Inventory, item.ResourceID);
				if (amount > 0)
					missingResources.Add(new ResourceStack(item.ResourceID, amount));
			}
		}

		public List<ResourceStack> GetMissingResources()
		{
			if (missingResources == null)
				CalculateMissingResources(Entity.Inventory);
			return missingResources;
		}

		public List<ResourcePileFeature> BlockingResourcePiles = new List<ResourcePileFeature>();

		#region IJob
		public bool IsWorkplace { get { return true; } }
		public EOccupation[] Occupations { get { return new EOccupation[] { EOccupation.Construction, EOccupation.Supply }; } }
		public bool HasWork
		{
			get
			{
				return ConstructionState == ConstructionSystem.EConstructionState.Construction;
			}
		}
		public bool Disabled { get; set; }
		public float UserDefinedPriority { get; set; }
		public float Priority { get { return UserDefinedPriority; } }
		public int MaxWorkers { get; private set; }
		public int WorkerCount { get { return Workers.Count; } }

		// Workers who are registered to work here
		public List<CitizenAIAgentFeature> Workers { get { return Constructors; } }
		// Workers who perform construction work
		public List<CitizenAIAgentFeature> Constructors { get; private set; }
		// Workers who perform transportation work
		public List<CitizenAIAgentFeature> Transporters { get; private set; }
		#endregion

		private SystemsManager manager;

		public void RegisterWorker(CitizenAIAgentFeature unit, EWorkerType type = EWorkerType.Worker)
		{
			switch (type)
			{
				case EWorkerType.Worker:
					if (!Workers.Contains(unit))
					{
						Constructors.Add(unit);
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
			Constructors.Remove(unit);
			Transporters.Remove(unit);
		}

		public override void Setup(BaseEntity entity, FeatureParamsBase args = null)
		{
			base.Setup(entity);
			StaticData = Entity.StaticData.GetFeature<ConstructionStaticData>();
			MaxWorkers = StaticData.MaxWorkers;
			Constructors = new List<CitizenAIAgentFeature>();
			Transporters = new List<CitizenAIAgentFeature>();
			if (args == null)
				return;
			var featureParams = (ConstructionFeatureParams)args;
			if (featureParams == null)
				return;			
			if (!featureParams.AsConstruction)
			{
				ConstructionSystem.FinishConstructionSite(this);
				ConstructionState = ConstructionSystem.EConstructionState.Ready;
			}
			else
				ConstructionState = ConstructionSystem.EConstructionState.Construction;			
		}

		public override void Run(SystemsManager manager)
		{
			base.Run(manager);
			this.manager = manager;
			manager.ConstructionSystem.RegisterFeature(this);
			manager.JobSystem.RegisterJob(this);
			if (Entity.Inventory != null)
			{
				Entity.Inventory.OnInventoryChanged -= CalculateMissingResources;
				Entity.Inventory.OnInventoryChanged += CalculateMissingResources;
			}
			manager.gameManager.gridRectController.AddRect(this);
			manager.AIAgentSystem.TaskEndedCallback -= OnTaskComplete;
			manager.AIAgentSystem.TaskEndedCallback += OnTaskComplete;

			var constructionSystem = SystemsManager.ConstructionSystem;

			if(!Entity.HasFeature<InventoryFeature>())			
			{
				InventoryFeature inventory = new InventoryFeature();
				inventory.staticData = constructionSystem.ConstructionSiteInventoryData;
				inventory.Setup(Entity);				
				inventory.SetStatistics(InventorySystem.GetNewInventoryStatistics());				
				Entity.AttachFeature(inventory);
			}
            if (ConstructionState == ConstructionSystem.EConstructionState.Construction)
                Entity.GetFeature<InventoryFeature>().
                    SetMaxCapacity(ConstructionSystem.ConstructionInventoryLoad);
            if (ConstructionState == ConstructionSystem.EConstructionState.Ready)
			{
				ConvertConstructionIntoBuilding();
			}			
		}

		public void ConvertConstructionIntoBuilding()
		{
			var inventoryData = Entity.StaticData.GetFeature<InventoryStaticData>();
			if(inventoryData == null && Entity.HasFeature<InventoryFeature>())
			{
				Entity.RemoveFeature<InventoryFeature>();
			}
			if(inventoryData != null)
			{
				if (!Entity.HasFeature<InventoryFeature>())
				{
					var feature = new InventoryFeature();
					feature.Setup(Entity);
					feature.SetStatistics(InventorySystem.GetNewInventoryStatistics());
					Entity.AttachFeature(feature);					
				}
				else
				{
					//Adjust existing feature
					var feature = Entity.GetFeature<InventoryFeature>();
					feature.staticData = inventoryData;
					feature.SetStatistics(feature.Statistics);
				}
			}
			var keys = Entity.Features.Keys.ToArray();
			foreach(var key in keys)
			{
				if(!Entity.Features[key].IsRunning && !Entity.Features[key].AutoRun)
					Entity.Features[key].Run(SystemsManager);
			}
		}

		public override void Stop()
		{
			if (Entity.Inventory != null)
				Entity.Inventory.OnInventoryChanged -= CalculateMissingResources;
			manager.ConstructionSystem.DeregisterFeature(this);
			manager.JobSystem.DeregisterJob(this);
			foreach (var pile in BlockingResourcePiles)
			{
				pile.canCancelGathering = true;
			}
			manager.gameManager.gridRectController.RemoveRect(this);
			manager.AIAgentSystem.TaskEndedCallback -= OnTaskComplete;
			base.Stop();
		}

		public override void OnFactionChanged(int oldFaction)
		{
			base.OnFactionChanged(oldFaction);
			manager.ConstructionSystem.UpdateFaction(this);
		}

		public void RegisterDeconstructionJob()
		{
			manager.JobSystem.RegisterJob(this);
		}
		public void DeregisterDeconstructionJob()
		{
			//Deregister job
			manager.JobSystem.DeregisterJob(this);

			//Cleanup worker cache
			Workers.Clear();
			Transporters.Clear();
			Constructors.Clear();

		}
		public void DeregisterConstructionJob()
		{
			//Deregister job
			manager.JobSystem.DeregisterJob(this);

			//Cleanup worker cache
			Workers.Clear();
			Transporters.Clear();
			Constructors.Clear();
			//Remove Construction Resourceshope
			foreach (var item in StaticData.Cost)
			{
				InventorySystem.WithdrawItems(Entity.Inventory, item.ResourceID, item.Amount);
			}
		}

		public AITask AskForTask(EOccupation occupation, AIAgentFeatureBase unit)
		{
			var citizen = unit as CitizenAIAgentFeature;
			//AI is not a citizen
			if (citizen == null)
			{
				Debug.LogWarning("Non citizen requested Task from Production Facility");
				return default(AITask);
			}

			if (citizen.Occupation != EOccupation.Construction && citizen.Occupation != EOccupation.Supply)
				return default(AITask);

			if (MarkedForDemolition)
			{
				//Begin Construction  
				return GetDemolitionTask(occupation, citizen);
			}

			if (ConstructionState == ConstructionSystem.EConstructionState.Ready)
			{
				return default(AITask);
			}

			if (IsObstructed())
			{
				return default(AITask);
			}

			//Cache values to avoid repeated iterations
			float resourceProgress = ResourceProgress;
			float constructionProgress = ConstructionProgress;

			if (citizen.Occupation == EOccupation.Construction && resourceProgress > 0f && (resourceProgress - ConstructionSystem.EPSILON) > constructionProgress)
			{
				//Begin Construction  
				return GetConstructionTask(occupation, citizen);
			}
			//Do we need more resources before starting construction?
			else if (resourceProgress < 1)
			{
				return GetBringResourcesTask(occupation, citizen);
			}
			return default(AITask);
		}

		public ConstructionTask GetConstructionTask(EOccupation occupation, CitizenAIAgentFeature citizen)
		{
			//Check if max concurrent constructors reached
			if (Constructors.Count >= StaticData.MaxWorkers)
				return null;
			if (Constructors.Contains(citizen))
				return null;
			return new ConstructionTask(citizen.Entity, this);

		}

		public DemolitionTask GetDemolitionTask(EOccupation occupation, CitizenAIAgentFeature citizen)
		{
			//Check if max concurrent constructors reached
			if (Constructors.Count >= StaticData.MaxWorkers)
				return null;
			if (Constructors.Contains(citizen))
				return null;
			return new DemolitionTask(citizen.Entity, this);

		}

		public BringResourcesTask GetBringResourcesTask(EOccupation occupation, CitizenAIAgentFeature citizen)
		{
			//Get list of resources needed for construction
			List<ResourceStack> resources = new List<ResourceStack>();
			foreach (var entry in StaticData.Cost)
			{
				ResourceStack stack = null;
				if ((stack = resources.FirstOrDefault(x => x.ResourceID == entry.ResourceID)) == null)
				{
					stack = new ResourceStack(entry.ResourceID, entry.Amount);
					resources.Add(stack);
				}
				else
				{
					stack.Amount += entry.Amount;
				}
				//Reduce number of items already existing in inventory
				stack.Amount -= InventorySystem.GetItemCount(Entity.Inventory, stack.ResourceID);
				if (stack.Amount <= 0)
				{
					resources.Remove(stack);
				}
			}

			//If we require no resources, return no task
			if (resources == null || resources.Count <= 0)
				return null;


			//Reduce total resources by the resources already being brought by other units
			foreach (var worker in Transporters)
			{
				var workerTask = worker.CurrentTask as BringResourcesTask;

				//Skip worker if his task isn't transport
				if (workerTask == null)
					continue;
				var workerResources = workerTask.GetMemberValue<List<ResourceStack>>("TransferedResources");
				//Reduce total resources by the resources carried by this worker
				for (int d = 0; d < workerResources.Count; d++)
				{
					for (int r = 0; r < resources.Count; r++)
					{
						var res = resources[r];
						if (workerResources[d].ResourceID == res.ResourceID)
						{
							res.Amount -= Mathf.Min(res.Amount, workerResources[d].Amount);
							if (res.Amount <= 0)
							{
								resources.RemoveAt(r);
								break;
							}
						}
					}
				}
			}

			//Trim down resources needed
			for (int i = resources.Count - 1; i >= 0; i--)
			{
				//Check if total resources exist in storages in general, remove from list those that don't exist
				int overallCount = manager.StorageSystem.CountResources(resources[i].ResourceID, citizen.FactionID);
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
				var count = Mathf.Min((int)Mathf.Floor(capacity / mat.Mass), resources[i].Amount);

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
				var storage = manager.StorageSystem.GetBestStorageForResources(taskResources, citizen.FactionID);
				return new BringResourcesTask(citizen.Entity, taskResources, storage.Inventory, Entity.Inventory, this);
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
			if (unit.CurrentTask is BringResourcesTask && Transporters.Contains(citizen))
			{
				Transporters.Remove(citizen);
				DeregisterWorker(citizen);
			}
			else if ((unit.CurrentTask is ConstructionTask || unit.CurrentTask is DemolitionTask) &&
				Constructors.Contains(citizen))
			{
				Constructors.Remove(citizen);
				DeregisterWorker(citizen);
			}
		}

		public override ISaveable CollectData()
		{
			var data = new ConstructionFeatureSaveData();
			data.markedForDemolition = MarkedForDemolition;
			data.demolitionStartingProgress = DemolitionStartingProgress;
			data.constructionState = (int)ConstructionState;
			data.currentConstructionPoints = CurrentConstructionPoints;
			data.workers = new string[Workers.Count];
			for (int i = 0; i < Workers.Count; i++)
			{
				data.workers[i] = Workers[i].Entity.UID.ToString();
			}

			data.constructors = new string[Constructors.Count];
			for (int i = 0; i < Constructors.Count; i++)
			{
				data.constructors[i] = Constructors[i].Entity.UID.ToString();
			}

			data.transporters = new string[Transporters.Count];
			for (int i = 0; i < Transporters.Count; i++)
			{
				data.transporters[i] = Transporters[i].Entity.UID.ToString();
			}
			return data;
		}

		public override void ApplyData(ConstructionFeatureSaveData data)
		{
			MarkedForDemolition = data.markedForDemolition;
			if (MarkedForDemolition)
			{
				ConstructionSystem.MarkForDemolition(this, true);
			}
			DemolitionStartingProgress = data.demolitionStartingProgress;
			ConstructionState = (ConstructionSystem.EConstructionState)data.constructionState;
			CurrentConstructionPoints = data.currentConstructionPoints;
			foreach (var workerID in data.workers)
			{
				if (string.IsNullOrEmpty(workerID))
					continue;
				var id = Guid.Parse(workerID);
				if (!Main.Instance.GameManager.SystemsManager.Entities.ContainsKey(id))
					continue;
				var worker = Main.Instance.GameManager.SystemsManager.Entities[id];
				if (!worker.HasFeature<CitizenAIAgentFeature>())
					continue;
				var agent = worker.GetFeature<CitizenAIAgentFeature>();
				Workers.Add(agent);

				if (data.transporters.Contains(workerID))
				{
					Transporters.Add(agent);
				}
			}

			foreach (var constructorID in data.constructors)
			{
				if (string.IsNullOrEmpty(constructorID))
					continue;
				var id = Guid.Parse(constructorID);
				if (!Main.Instance.GameManager.SystemsManager.Entities.ContainsKey(id))
					continue;
				var constructor = Main.Instance.GameManager.SystemsManager.Entities[id];
				if (!constructor.HasFeature<CitizenAIAgentFeature>())
					continue;
				var agent = constructor.GetFeature<CitizenAIAgentFeature>();
			}

			foreach (var transporterID in data.transporters)
			{
				if (string.IsNullOrEmpty(transporterID))
					continue;
				var id = Guid.Parse(transporterID);
				if (!Main.Instance.GameManager.SystemsManager.Entities.ContainsKey(id))
					continue;
				var transporter = Main.Instance.GameManager.SystemsManager.Entities[id];
				if (!transporter.HasFeature<CitizenAIAgentFeature>())
					continue;
				var agent = transporter.GetFeature<CitizenAIAgentFeature>();
				Transporters.Add(agent);
			}
		}

		public bool IsObstructed()
		{
			if (BlockingResourcePiles.Count <= 0)
				return false;
			for (int i = BlockingResourcePiles.Count - 1; i >= 0; i--)
			{
				if (BlockingResourcePiles[i] == null || BlockingResourcePiles[i].Entity == null)
					BlockingResourcePiles.RemoveAt(i);
			}
			return BlockingResourcePiles.Count > 0;
		}
	}
}