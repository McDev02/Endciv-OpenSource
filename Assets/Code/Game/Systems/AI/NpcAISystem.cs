using System.Linq;
using System;

namespace Endciv
{
	public class NpcAISystem : EntityFeatureSystem<AIAgentFeatureBase>
	{
		GridMap GridMap;
		JobSystem JobSystem;
		AISettings aiSettings;
		UnitSystemConfig unitConfig;

		static TimeManager timeManager;

		const float NpcWalkingDistance = 64 * GridMapView.GridTileFactorInv;

		public override void UpdateStatistics()
		{
		}

		public NpcAISystem(int factions, GameManager gameManager, SystemsManager systemsManager, AISettings aiSettings) : base(factions)
		{
			this.aiSettings = aiSettings;
			unitConfig = gameManager.gameConfig.UnitSystemData;

			GridMap = gameManager.GridMap;
			JobSystem = systemsManager.JobSystem;
			timeManager = gameManager.timeManager;
		}


		public override void UpdateGameLoop()
		{
			float tickByDay = timeManager.dayTickFactor;
            AIAgentFeatureBase npc;

			for (int f = 0; f < FeaturesByFaction.Count; f++)
			{
				for (int i = 0; i < FeaturesByFaction[f].Count; i++)
				{
					npc = FeaturesByFaction[f][i];
					UpdateNpcNeeds(npc, tickByDay);
					AIAgentSystem.CalculateMood(npc);
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
					ExecuteNpcAI(FeaturesByFaction[f][i]);
				}
			}
		}

		void UpdateNpcNeeds(AIAgentFeatureBase npc, float tickByDay)
		{
		}

		private void ExecuteNpcAI(FeatureBase feature)
		{
			if (Main.Instance.GameManager.SystemsManager.JobSystem == null)
				return;
            var agent = feature.Entity.GetFirstAIFeature();
			//Execute current task
			if (agent.CurrentTask != null)
			{
				if (!agent.CurrentTask.Execute())
				{
					StopCurrentTask(agent);
				}
				return;
			}

			var trader = agent as TraderAIAgentFeature;
			if (trader != null)
			{
				ExecuteStateTrader(trader);
				return;
			}

			var immigrant = agent as ImmigrantAIAgentFeature;
			if(immigrant != null)
			{
				ExecuteStateImmigrant(immigrant);
				return;
			}			
			ExecuteStateRoaming(agent);
		}


		internal override void RegisterFeature(AIAgentFeatureBase feature)
		{
			base.RegisterFeature(feature);
		}

		internal override void DeregisterFeature(AIAgentFeatureBase feature, int faction = -1)
		{
			base.DeregisterFeature(feature);
			if (faction < 0) faction = feature.FactionID;
		}

		//Shedule States
		void ExecuteStateRoaming(AIAgentFeatureBase agent)
		{
            AITask task;

			task = new RoamingTask(agent.UnitEntity, NpcWalkingDistance, new MinMax(3f, 8f), GridMap);

			task.Initialize();
			SetCurrentTask(agent, task);
		}

		//Shedule States
		void ExecuteStateTrader(TraderAIAgentFeature agent)
		{
			if (agent.state != NpcSpawnSystem.ETraderState.Arrival) return;
            AITask task;

			Vector2i pos;
			GridMap.GetPossitionNearPlayerTown(out pos);
			task = new TraderTask(agent.Entity, new Location(pos), GridMap);

			task.Initialize();
			SetCurrentTask(agent, task);
		}

		void ExecuteStateImmigrant(ImmigrantAIAgentFeature agent)
		{
			if (agent.State != EImmigrantState.Arriving)
				return;

			AITask task;
			Vector2i? pos = null;

			var npcSpawnSystem = Main.Instance.GameManager.SystemsManager.NpcSpawnSystem;
			if(npcSpawnSystem.immigrantGroupReference.ContainsKey(agent))
			{
				Vector2i? leaderPos = null;
				var group = npcSpawnSystem.immigrantGroupReference[agent];
				foreach(var immigrant in group.immigrants)
				{
					if (immigrant == agent)
						continue;
					if (immigrant.CurrentTask == null)
						continue;
					var loc = immigrant.CurrentTask.GetMemberValue<Location>("Destination");
					if (loc == null)
						continue;
					if (loc.Indecies == null || loc.Indecies.Length <= 0)
						continue;
					leaderPos = loc.Indecies[0];
					break;
				}
				if(leaderPos != null)
				{
					Vector2i myPos;
					if(GridMap.GetRandomPositionAroundPoint(leaderPos.Value, out myPos))
					{
						pos = myPos;
					}						
				}
			}
			if(pos == null)
			{
				Vector2i myPos;
				if(GridMap.GetPossitionNearPlayerTown(out myPos))
				{
					pos = myPos;
				}
				else
				{
					return;
				}
				
			}						
			task = new ImmigrantTask(agent.Entity, new Location(pos.Value), GridMap);			
			task.Initialize();
			SetCurrentTask(agent, task);
		}
		
		void SetCurrentTask(AIAgentFeatureBase agent, AITask task)
		{
			StopCurrentTask(agent);
			agent.CurrentTask = task;
		}

		void StopCurrentTask(AIAgentFeatureBase agent)
		{
			//if (agent.CurrentTask != null && agent.CurrentTask.job != null)
			//	agent.CurrentTask.job.DeregisterWorker(agent);
			agent.CurrentTask = null;
		}

		public AIAgentFeatureBase GetNpcByUID(Guid UID)
		{
			return FeaturesByFaction.SelectMany(x => x).FirstOrDefault(x => x.Entity.UID == UID);
		}		
	}
}