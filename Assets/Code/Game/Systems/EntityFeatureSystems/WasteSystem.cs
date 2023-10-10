using System.Collections.Generic;

using System.Linq;
using System;

namespace Endciv
{
	/// <summary>
	/// Manages Agriculture
	/// </summary>
	public class WasteSystem : BaseGameSystem, IAIJob, ISaveable, ILoadable<WasteSystemSaveData>
	{
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
		// Workers who perform transportation work
		//public List<CitizenAIAgentFeature> Transporters { get; private set; }

		private HashSet<Vector2i> tilesGathered;
		private GridMap gridMap;

		public Vector2i? GetNewWasteTile(CitizenAIAgentFeature citizen)
		{
			var policy = Main.Instance.GameManager.Factories.SimpleEntityFactory.GetStaticData<ItemFeatureStaticData>(FactoryConstants.WasteID).Category;
			if (Main.Instance.GameManager.SystemsManager.StorageSystem.GetTotalStorageCapacityLeft(citizen.FactionID, policy) <= 0.1f)
				return null;
			if (!InventorySystem.CanAddItems(citizen.Entity.Inventory, FactoryConstants.WasteID, 1))
				return null;
			SortedList<float, Vector2i> sortedPartitions = new SortedList<float, Vector2i>(new DuplicateKeyComparerDescending<float>());
			Vector2i unitTile = citizen.Entity.GetFeature<EntityFeature>().GridID;
			Vector2i unitPartitionTile = citizen.Entity.GetFeature<EntityFeature>().PartitionID;

			for (int i = 0; i < gridMap.Data.wasteSummary.GetLength(0); i++)
			{
				for (int j = 0; j < gridMap.Data.wasteSummary.GetLength(1); j++)
				{
					if (gridMap.Data.wasteSummary[i, j].NodeAverage <= 0f)
						continue;
					Vector2i currentPartitionID = new Vector2i(i, j);
					float weight = gridMap.Data.wasteSummary[i, j].NodeAverage / Vector2i.Distance(unitPartitionTile, currentPartitionID);
					sortedPartitions.Add(weight, new Vector2i(i, j));
				}
			}
			if (sortedPartitions.Count <= 0)
				return null;

			//Iterate partition indices by order of Waste rating
			SortedList<float, Vector2i> sortedTiles = new SortedList<float, Vector2i>(new DuplicateKeyComparerDescending<float>());
			foreach (var index in sortedPartitions.Values)
			{
				var tiles = gridMap.partitionSystem.GetGridMapIndicesAtPartition(index);
				foreach (var tile in tiles)
				{
					if (tilesGathered.Contains(tile))
						continue;
					if (gridMap.Data.waste[tile.X, tile.Y] <= 0f)
						continue;
					float weight = gridMap.Data.waste[tile.X, tile.Y] / Vector2i.Distance(unitTile, tile);
					sortedTiles.Add(weight, tile);
				}
				if (sortedTiles.Count <= 0)
					continue;
				foreach (var sortedTile in sortedTiles.Values)
				{
					//Check for passability missing
					//continue if not passable

					return sortedTile;

				}
				sortedTiles.Clear();
			}
			return null;
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

		public AITask AskForTask(EOccupation occupation, AIAgentFeatureBase unit)
		{
			var citizen = unit as CitizenAIAgentFeature;
			//AI is not a citizen
			if (citizen == null)
			{
				Debug.LogWarning("No citizen requested Task from Graveyard System");
				return default(AITask);
			}

			if (occupation != EOccupation.Labour)
				return default(AITask);
			var tile = GetNewWasteTile(citizen);
			if (tile == null)
				return default(AITask);
			return new WasteGatheringTask(citizen.Entity, new Location(tile.Value));
		}

		public void RegisterTile(Vector2i centerTile)
		{
			var tiles = gridMap.partitionSystem.GetAdjacentTiles(centerTile, true);
			foreach (var tile in tiles)
			{
				tilesGathered.Add(tile);
			}
		}

		public void UnregisterTile(Vector2i centerTile)
		{
			var tiles = gridMap.partitionSystem.GetAdjacentTiles(centerTile, true);
			foreach (var tile in tiles)
			{
				tilesGathered.Remove(tile);
			}
		}

        public void OnTaskStart()
        {

        }

        public void OnTaskComplete(AIAgentFeatureBase unit)
		{
            if (!Workers.Contains(unit))
                return;
			var citizen = unit as CitizenAIAgentFeature;
            if (citizen == null)
                return;
			var task = unit.CurrentTask;
			if (task != null)
			{
				var wasteTask = (WasteGatheringTask)task;
				if (wasteTask != null)
				{
					var loc = wasteTask.GetMemberValue<Location>(WasteGatheringTask.targetLocationKey);
					UnregisterTile(loc.Index);
				}
			}
			DeregisterWorker(citizen);
		}
		#endregion

		public WasteSystem(int factions, GridMap gridMap)
		{
			Workers = new List<CitizenAIAgentFeature>();
			MaxWorkers = int.MaxValue;
			this.gridMap = gridMap;
            Main.Instance.GameManager.SystemsManager.AIAgentSystem.TaskEndedCallback -= OnTaskComplete;
            Main.Instance.GameManager.SystemsManager.AIAgentSystem.TaskEndedCallback += OnTaskComplete;
        }

		public void Run()
		{
			Main.Instance.GameManager.SystemsManager.JobSystem.RegisterJob(this);
			tilesGathered = new HashSet<Vector2i>();
		}

		public override void UpdateGameLoop()
		{
		}

		public override void UpdateStatistics()
		{

		}

		#region Save System        
		public ISaveable CollectData()
		{
			var data = new WasteSystemSaveData();
			data.workerUIDs = new List<string>();
			foreach (var worker in Workers)
			{
				data.workerUIDs.Add(worker.Entity.UID.ToString());
			}
			data.tilesGathered = tilesGathered.ToList();
			return data;
		}

		public void ApplySaveData(WasteSystemSaveData data)
		{
			if (data == null)
				return;
            if (data.workerUIDs != null)
            {
                foreach (var workerID in data.workerUIDs)
                {
                    if (string.IsNullOrEmpty(workerID))
                        continue;
                    var guid = Guid.Parse(workerID);
                    if (Main.Instance.GameManager.SystemsManager.Entities.ContainsKey(guid))
                    {
                        Workers.Add(Main.Instance.GameManager.SystemsManager.Entities[guid].GetFeature<CitizenAIAgentFeature>());
                    }
                }
            }
            if (data.tilesGathered.Count > 0)
			{
				tilesGathered = new HashSet<Vector2i>(data.tilesGathered);
			}
		}
		#endregion
	}
}