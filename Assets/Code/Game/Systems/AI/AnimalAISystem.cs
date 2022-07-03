using System.Linq;
using System;

namespace Endciv
{
	public class AnimalAISystem : EntityFeatureSystem<AnimalAIAgentFeature>
	{
		GridMap GridMap;
		SystemsManager SystemsManager;
		JobSystem JobSystem;
		AISettings aiSettings;
		UnitSystemConfig unitConfig;

		static TimeManager timeManager;
		const float CattleWalkingDistance = 8 * GridMapView.GridTileFactorInv;
		const float PredatorWalkingDistance = 24 * GridMapView.GridTileFactorInv;

		public override void UpdateStatistics()
		{
		}

		public AnimalAISystem(int factions, GameManager gameManager, SystemsManager systemsManager, AISettings aiSettings) : base(factions)
		{
			this.aiSettings = aiSettings;
			unitConfig = gameManager.gameConfig.UnitSystemData;
			SystemsManager = systemsManager;
			GridMap = gameManager.GridMap;
			JobSystem = systemsManager.JobSystem;
			timeManager = gameManager.timeManager;
		}

		public override void UpdateGameLoop()
		{
			float tickByDay = timeManager.dayTickFactor;
			AnimalAIAgentFeature animal;

			for (int f = 0; f < FeaturesByFaction.Count; f++)
			{
				for (int i = 0; i < FeaturesByFaction[f].Count; i++)
				{
					animal = FeaturesByFaction[f][i];
					UpdateAnimalNeeds(animal, tickByDay);
					AIAgentSystem.CalculateMood(animal);
				}
			}
			UpdateStatistics();
		}

		public void UpdateEachFrame()
		{
			for (int f = 0; f < FeaturesByFaction.Count; f++)
			{
				for (int i = 0; i < FeaturesByFaction[f].Count; i++)
				{
					var animal = FeaturesByFaction[f][i];
					if (animal.Entity.HasFeature<CattleFeature>())
						ExecuteCattleAI(animal);
					else if (animal.Entity.StaticData.ID == "dog") //Shall be replaced with feature
						ExecutePredatorAI(animal);
					else
						ExecuteAnimalAI(animal);
				}
			}
		}

		void UpdateAnimalNeeds(AnimalAIAgentFeature animal, float tickByDay)
		{
		}

		private void ExecuteAnimalAI(AnimalAIAgentFeature agent)
		{

		}

		private void ExecuteCattleAI(AnimalAIAgentFeature agent)
		{
			if (Main.Instance.GameManager.SystemsManager.JobSystem == null)
				return;
			//Execute current task
			if (agent.CurrentTask != null)
			{
				if (!agent.CurrentTask.Execute())
				{
					StopCurrentTask(agent);
				}
				return;
			}

			AITask task = null;
			var cattle = agent.Entity.GetFeature<CattleFeature>();
			if (!cattle.HasHome)
			{
				var pasture = SystemsManager.PastureSystem.GetAvailablePastureForCattle(cattle);

				if (pasture != null)
				{
					task = new AnimalEnterPastureTask(cattle.Entity, pasture);
					SystemsManager.PastureSystem.AddCattleToPasture(pasture, cattle);
				}
			}

			//Randomly pick between roam, eat and sleep
			if (task == null)
			{
				float rand = UnityEngine.Random.Range(0f, 1f);

				if (rand >= 0.75f)
				{
					task = new AnimalSleepTask(agent.Entity, CattleWalkingDistance, new MinMax(30f, 80f));
				}
				//Fallback Roaming
				else
				{
					task = new RoamingTask(agent.UnitEntity, CattleWalkingDistance, new MinMax(12f, 30f), Main.Instance.GameManager.GridMap);
				}
			}
			task.Initialize();
			SetCurrentTask(agent, task);
		}

		private void ExecutePredatorAI(AnimalAIAgentFeature agent)
		{
			if (Main.Instance.GameManager.SystemsManager.JobSystem == null)
				return;
			//Execute current task
			if (agent.CurrentTask != null)
			{
				if (!agent.CurrentTask.Execute())
				{
					StopCurrentTask(agent);
				}
				return;
			}
			AITask task = null;

			//Randomly pick between roam, eat and sleep
			if (task == null)
			{
				float rand = UnityEngine.Random.Range(0f, 1f);
				if (rand >= 0.75f)
				{
					task = new AnimalSleepTask(agent.Entity, PredatorWalkingDistance, new MinMax(30f, 80f));
				}
				//Else if FindFood
				else
				{
					task = new RoamingTask(agent.UnitEntity, PredatorWalkingDistance, new MinMax(12f, 30f), Main.Instance.GameManager.GridMap);
				}
			}
			task.Initialize();
			SetCurrentTask(agent, task);
		}


		internal override void RegisterFeature(AnimalAIAgentFeature feature)
		{
			base.RegisterFeature(feature);
		}

		internal override void DeregisterFeature(AnimalAIAgentFeature feature, int faction = -1)
		{
			base.DeregisterFeature(feature);
			if (faction < 0) faction = feature.FactionID;
		}

		void SetCurrentTask(AnimalAIAgentFeature agent, AITask task)
		{
			StopCurrentTask(agent);
			agent.CurrentTask = task;
		}

		void StopCurrentTask(AnimalAIAgentFeature agent)
		{
			//if (agent.CurrentTask != null && agent.CurrentTask.job != null)
			//	agent.CurrentTask.job.DeregisterWorker(agent);
			agent.CurrentTask = null;
		}

		public AnimalAIAgentFeature GetAnimalByUID(Guid UID)
		{
			return FeaturesByFaction.SelectMany(x => x).FirstOrDefault(x => x.Entity.UID == UID);
		}

		public void OnAnimalEnterPastureComplete(AIAgentFeatureBase animal)
		{
			if (!(animal.CurrentTask is AnimalEnterPastureTask))
				return;
			var cattle = animal.Entity.GetFeature<CattleFeature>();
			if (!cattle.HasHome)
				return;
			if (animal.CurrentTask.CurrentState == AITask.TaskState.Success)
				SystemsManager.PastureSystem.UnreserveCattle(cattle.Pasture, cattle);
			else if (animal.CurrentTask.CurrentState == AITask.TaskState.Failed)
				SystemsManager.PastureSystem.RemoveCattleFromPasture(cattle.Pasture, cattle);
		}
	}
}