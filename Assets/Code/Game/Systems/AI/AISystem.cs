
using System;

namespace Endciv
{
	public class AIAgentSystem : EntityFeatureSystem<AIAgentFeatureBase>
	{
		public Action<AIAgentFeatureBase> TaskEndedCallback;

		AISettings aiSettings;
		public CitizenAISystem CitizenAISystem;
		public AnimalAISystem AnimalAISystem;
		public NpcAISystem NpcAISystem;
		TimeManager timeManager;

		public AIAgentSystem(int factions, GameManager gameManager, SystemsManager systemsManager) : base(factions)
		{
			aiSettings = gameManager.gameMechanicSettings.aiSettings;
			CitizenAISystem = new CitizenAISystem(factions, gameManager, systemsManager, aiSettings);
			AnimalAISystem = new AnimalAISystem(factions, gameManager, systemsManager, aiSettings);
			NpcAISystem = new NpcAISystem(factions, gameManager, systemsManager, aiSettings);
			timeManager = gameManager.timeManager;

			TaskEndedCallback += CitizenAISystem.OnFindFoodWaterTaskComplete;
			TaskEndedCallback += CitizenAISystem.OnVisitedToiletTaskComplete;
			TaskEndedCallback += CitizenAISystem.OnVisitedShowerTaskComplete;

			TaskEndedCallback += AnimalAISystem.OnAnimalEnterPastureComplete;
		}

		internal override void RegisterFeature(AIAgentFeatureBase feature)
		{
			base.RegisterFeature(feature);
			if (feature is CitizenAIAgentFeature)
				CitizenAISystem.RegisterFeature(feature as CitizenAIAgentFeature);
			else if (feature is AnimalAIAgentFeature)
				AnimalAISystem.RegisterFeature(feature as AnimalAIAgentFeature);
			else
				NpcAISystem.RegisterFeature(feature);
		}

		internal override void DeregisterFeature(AIAgentFeatureBase feature, int faction = -1)
		{
			base.DeregisterFeature(feature, faction);
			StopCurrentTask(feature);
			if (faction < 0) faction = feature.FactionID;
			if (feature is CitizenAIAgentFeature)
				CitizenAISystem.DeregisterFeature(feature as CitizenAIAgentFeature);
			else if (feature is AnimalAIAgentFeature)
				AnimalAISystem.DeregisterFeature(feature as AnimalAIAgentFeature);
			else
				NpcAISystem.DeregisterFeature(feature);
		}

		public void UpdateEachFrame()
		{
			CitizenAISystem.UpdateEachFrame();
			AnimalAISystem.UpdateEachFrame();
			NpcAISystem.UpdateEachFrame();
		}

		public override void UpdateGameLoop()
		{
			float tickByDay = timeManager.dayTickFactor;
			for (int f = 0; f < FeaturesByFaction.Count; f++)
			{
				for (int i = 0; i < FeaturesByFaction[f].Count; i++)
				{
					UpdateBasicNeeds(FeaturesByFaction[f][i], tickByDay);
					CalculateMood(FeaturesByFaction[f][i]);
				}
			}
			CitizenAISystem.UpdateGameLoop();
			AnimalAISystem.UpdateGameLoop();
			NpcAISystem.UpdateGameLoop();

			UpdateStatistics();
		}

		void UpdateBasicNeeds(AIAgentFeatureBase agent, float tickByDay)
		{
			agent.HealthNeed.Value = agent.Entity.GetFeature<EntityFeature>().Health.Progress;

			if (agent.Entity.HasFeature<LivingBeingFeature>())
			{
				var being = agent.Entity.GetFeature<LivingBeingFeature>();
				agent.HungerNeed.Value = being.Hunger.Progress;
				agent.ThirstNeed.Value = being.Thirst.Progress;

				agent.StressNeed.Value = 1f - agent.Entity.GetFeature<EntityFeature>().Stress.Progress;

				agent.StressNeed.Value -= tickByDay;
				agent.CleaningNeed.Value -= tickByDay;
			}
		}

		public static void CalculateMood(AIAgentFeatureBase agent)
		{
			float mood = 0; float totalWeight = 0;
			float w, min;
			min = 99;

			EntityNeed need;
			var count = agent.needsLookup.Count;
			for (int n = 0; n < count; n++)
			{
				need = agent.needsLookup[n];
				w = need.weight;// * Mathf.Lerp(1, 5 * count, Mathf.Pow(Mathf.Clamp01(-need.Progress), 1.5f));
				mood += need.Mood * w;
				totalWeight += w;
				if (need.Mood < min)
					min = Mathf.Lerp(min, need.Mood, need.weight - 1);  //Reduce min mood only if weight > 1
			}
			agent.mood = Mathf.Lerp(mood / Mathf.Max(0.01f, totalWeight), -1, -min);
		}

		public override void UpdateStatistics()
		{
		}

		void SetCurrentTask(AIAgentFeatureBase agent, AITask task)
		{
			StopCurrentTask(agent);
			agent.CurrentTask = task;
		}

		void StopCurrentTask(AIAgentFeatureBase agent)
		{
			agent.CurrentTask?.Cancel();
			agent.CurrentTask = null;
		}
	}
}