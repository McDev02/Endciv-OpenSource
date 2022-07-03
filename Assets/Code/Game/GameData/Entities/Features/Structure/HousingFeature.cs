using System.Collections.Generic;
using System;

namespace Endciv
{
	//Provides shelter for Units, animals and humanoids
	public class HousingFeature : Feature<HousingFeatureSaveData>, IAIJob
	{
		public override void Setup(BaseEntity entity, FeatureParamsBase args = null)
		{
			base.Setup(entity);
			StaticData = Entity.StaticData.GetFeature<HouseStaticData>();
			Occupants = new List<BaseEntity>(MaxOccupants);
            MaxWorkers = 1;
            Workers = new List<CitizenAIAgentFeature>();
		}

		//Static Data
		public int MaxOccupants { get { return StaticData.MaxOccupants; } }
		public HouseStaticData StaticData { get; private set; }

		//Properties
		public int CurrentOccupants { get { return Occupants.Count; } }
		public float OccupantsProgress { get { return MaxOccupants <= 0 ? 0 : ((float)CurrentOccupants / MaxOccupants); } }
		public List<BaseEntity> Occupants { get; private set; }
		public bool HasFreeSpace { get { return CurrentOccupants < MaxOccupants; } }

        public bool IsWorkplace { get { return true; } }        
        public EOccupation[] Occupations { get { return new EOccupation[] { EOccupation.Labour }; } }
        public int MaxWorkers { get; private set; }
        public int WorkerCount { get { return Workers.Count; } }
        public bool HasWork { get { return true; } }
        public bool Disabled { get; set; }
        public float Priority { get; set; }
        public List<CitizenAIAgentFeature> Workers { get; private set; }

        public bool HasRestocked { get; set; }

        public void RegisterWorker(CitizenAIAgentFeature unit, EWorkerType type = EWorkerType.Worker)
        {
            Workers.Add(unit);
        }

        public void DeregisterWorker(CitizenAIAgentFeature unit)
        {
            Workers.Remove(unit);
        }

        public AITask AskForTask(EOccupation occupation, AIAgentFeatureBase unit)
        {
            if (HasRestocked)
                return null;            
            var citizen = (CitizenAIAgentFeature)unit;            
            if (citizen == null)
                return null;
            if (!Occupants.Contains(unit.Entity))
                return null;
            if (unit.Entity.Inventory.CapacityLeft <= 0)
                return null;
            return TransferConsumablesTask(citizen);
        }

        private BringResourcesTask TransferConsumablesTask(CitizenAIAgentFeature citizen)
        {
            if (Entity.Inventory.CapacityLeft <= 0)
                return null;
            StorageFeature storage = null;
            storage = SystemsManager.StorageSystem.GetClosestFoodStorage(citizen.Entity.GetFeature<GridAgentFeature>());
            if(storage == null)
                storage = SystemsManager.StorageSystem.GetClosestWaterStorage(citizen.Entity.GetFeature<GridAgentFeature>());
            if (storage == null)
                return null;
            int maxCapacity = CivMath.Min(Entity.Inventory.CapacityLeft, citizen.Entity.Inventory.CapacityLeft);
            Dictionary<string, int> transferedResources = new Dictionary<string, int>();            
            foreach(var pair in storage.Inventory.ItemPoolByChambers[0])
            {
                var entity = pair.Value[0].Entity;
                if (CitizenAISystem.consumableFilter.Contains(entity.StaticData.ID))
                    continue;
                if (!entity.HasFeature<ConsumableFeature>())
                    continue;
                if (maxCapacity - pair.Value[0].StaticData.Mass < 0)
                    continue;
                foreach(var item in pair.Value)
                {                    
                    for(int i = 0; i < item.Quantity; i++)
                    {
                        if (!transferedResources.ContainsKey(pair.Key))
                            transferedResources.Add(pair.Key, 0);
                        transferedResources[pair.Key]++;
                        maxCapacity -= item.StaticData.Mass;
                        if(maxCapacity <= 0)
                        {
                            break;
                        }
                    }
                    if (maxCapacity <= 0)
                    {
                        break;
                    }
                }
                if (maxCapacity <= 0)
                {
                    break;
                }
            }
            if (transferedResources.Count <= 0)
                return null;
            var list = transferedResources.ToResourceStackList();
            var task = new BringResourcesTask(citizen.Entity, list, storage.Inventory, Entity.Inventory, this);
            task.priority = 2;
            return task;
        }

        public void OnTaskStart()
        {
            HasRestocked = true;
        }

        public void OnTaskComplete(AIAgentFeatureBase unit)
        {
            var citizen = unit as CitizenAIAgentFeature;
            if (citizen == null)
                return;
            if (unit.CurrentTask is BringResourcesTask && Workers.Contains(citizen))
            {                
                DeregisterWorker(citizen);
            }
        }        

        public float quality;

		public override void Run(SystemsManager manager)
		{
			base.Run(manager);
            SystemsManager.HousingSystem.RegisterFeature(this);
            SystemsManager.JobSystem.RegisterJob(this);
            SystemsManager.AIAgentSystem.TaskEndedCallback -= OnTaskComplete;
            SystemsManager.AIAgentSystem.TaskEndedCallback += OnTaskComplete;
        }

		public override void Stop()
		{
			base.Stop();
			SystemsManager.HousingSystem.DeregisterFeature(this);
            SystemsManager.JobSystem.DeregisterJob(this);
            SystemsManager.AIAgentSystem.TaskEndedCallback -= OnTaskComplete;            
        }

		public override void OnFactionChanged(int oldFaction)
		{
			base.OnFactionChanged(oldFaction);
			SystemsManager.HousingSystem.DeregisterFeature(this, oldFaction);
			SystemsManager.HousingSystem.RegisterFeature(this);
		}

		public override ISaveable CollectData()
		{
			var data = new HousingFeatureSaveData();
			data.occupants = new List<string>();
			foreach (var occupant in Occupants)
			{
				data.occupants.Add(occupant.UID.ToString());
			}
            data.workers = new string[Workers.Count];
            for (int i = 0; i < Workers.Count; i++)
            {
                data.workers[i] = Workers[i].Entity.UID.ToString();
            }
            data.hasRestocked = HasRestocked;
            return data;
		}

		public override void ApplyData(HousingFeatureSaveData data)
		{
			if (data.occupants != null && data.occupants.Count > 0)
			{
				var unitPool = Main.Instance.GameManager.SystemsManager.Entities;
				foreach (var id in data.occupants)
				{
					if (string.IsNullOrEmpty(id))
						continue;
					var guid = Guid.Parse(id);
					if (unitPool.ContainsKey(guid))
					{
						Occupants.Add(unitPool[guid]);						
						unitPool[guid].GetFeature<CitizenAIAgentFeature>().Home = this;
					}
				}
			}
            if(data.workers != null && data.workers.Length > 0)
            {
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
                }
            }
            HasRestocked = data.hasRestocked;
        }        
    }
}