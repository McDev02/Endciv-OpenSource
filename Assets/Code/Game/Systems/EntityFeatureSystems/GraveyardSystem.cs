using System.Collections.Generic;

using System.Linq;
using System;

namespace Endciv
{
	/// <summary>
	/// Manages Agriculture
	/// </summary>
	public class GraveyardSystem : EntityFeatureSystem<GraveyardFeature>, IAIJob, ISaveable, ILoadable<GraveyardSystemSaveData>
	{
		static ModelFactory factory;
		public List<List<GraveyardFeature>> graveyards;

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

		public void RegisterWorker(CitizenAIAgentFeature unit, EWorkerType type = EWorkerType.Worker)
		{
			if (!Workers.Contains(unit))
				Workers.Add(unit);

		}

		public static int GetGraveyardSpots(GridObjectData gridData)
		{
			return (gridData.Rect.Width + 1) * (int)Mathf.Floor((gridData.Rect.Length + 1) / 3f);
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

			var unitSystem = Main.Instance.GameManager.SystemsManager.UnitSystem;

			if (unitSystem.DeadUnits[unit.FactionID].Count <= 0)
				return default(AITask);

			if (graveyards[unit.FactionID].Count <= 0)
				return default(AITask);

			//Check if dead units are not getting burried
			LivingBeingFeature deceased = null;
			foreach (var candidate in unitSystem.DeadUnits[unit.FactionID])
			{
				if (!candidate.HasFeature<LivingBeingFeature>())
					continue;
				var feature = candidate.GetFeature<LivingBeingFeature>();
				if (feature.isGettingBurried)
					continue;
				deceased = feature;
				break;
			}
			if (deceased == null)
				return default(AITask);

			//Check closest graveyards with vaccancy
			GraveyardFeature graveyard = null;
			var ordererdGraveyards = graveyards[unit.FactionID].
				OrderBy(x => Vector2i.Distance
				(
					x.Entity.GetFeature<EntityFeature>().GridID,
					citizen.Entity.GetFeature<EntityFeature>().GridID
				));
			foreach (var candidate in ordererdGraveyards)
			{
				if (!candidate.HasSpace())
					continue;
				graveyard = candidate;
				break;
			}
			if (graveyard == null)
				return default(AITask);

			int plotID = graveyard.ReserveGravePlot();
			if (plotID < 0)
				return default(AITask);

			//Assign task    
			var task = new BuryCorpseTask(citizen.Entity, deceased.Entity, graveyard, plotID);
			task.priority = 10000;
			return task;
		}

        public void OnTaskStart()
        {

        }

		public void OnTaskComplete(AIAgentFeatureBase unit)
		{
			var citizen = unit as CitizenAIAgentFeature;
			if (citizen == null)
				return;
			if (!Workers.Contains(unit))
				return;
			if (unit.CurrentTask != null)
			{
				var buryTask = (unit.CurrentTask as BuryCorpseTask);
				var id = buryTask.GetMemberValue<int>(BuryCorpseTask.gravePlotIDKey);
				var graveyard = buryTask.GetMemberValue<GraveyardFeature>(BuryCorpseTask.graveyardKey);
				if (graveyard != null && id > -1)
				{
					graveyard.UnreserveGravePlot(id);
				}
			}
			DeregisterWorker(citizen);
		}
		#endregion

		public GraveyardSystem(int factions, ModelFactory factory) : base(factions)
		{
			Workers = new List<CitizenAIAgentFeature>();
			MaxWorkers = int.MaxValue;

			GraveyardSystem.factory = factory;
			graveyards = new List<List<GraveyardFeature>>(factions);
			for (int i = 0; i < factions; i++)
			{
				graveyards.Add(new List<GraveyardFeature>(i == 0 ? 16 : 4));
			}
			Main.Instance.GameManager.SystemsManager.AIAgentSystem.TaskEndedCallback -= OnTaskComplete;
			Main.Instance.GameManager.SystemsManager.AIAgentSystem.TaskEndedCallback += OnTaskComplete;
		}

		public void Run()
		{
			Main.Instance.GameManager.SystemsManager.JobSystem.RegisterJob(this);
		}

		internal override void RegisterFeature(GraveyardFeature feature)
		{
			base.RegisterFeature(feature);
			graveyards[feature.FactionID].Add(feature);
		}

		internal override void DeregisterFeature(GraveyardFeature feature, int faction = -1)
		{
			if (faction < 0)
				faction = feature.FactionID;
			base.DeregisterFeature(feature);
			if (graveyards[faction].Contains(feature))
				graveyards[faction].Remove(feature);
		}

		public override void UpdateGameLoop()
		{
		}

		public override void UpdateStatistics()
		{

		}

		internal static int BuryDeceased(GraveyardFeature feature, int plotID)
		{
			int var;
			var viewModel = factory.GetRandomModelObject<GraveModelView>("grave", out var);
			viewModel.name = "grave";
			return feature.PlaceGrave(viewModel, var, plotID);
		}

		#region Save System        
		public ISaveable CollectData()
		{
			var data = new GraveyardSystemSaveData();
			data.workerUIDs = new List<string>();
			foreach (var worker in Workers)
			{
				data.workerUIDs.Add(worker.Entity.UID.ToString());
			}
			return data;
		}

		public void ApplySaveData(GraveyardSystemSaveData data)
		{
			if (data == null)
				return;
			if (data.workerUIDs != null)
			{
				foreach (var workerID in data.workerUIDs)
				{
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