using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

namespace Endciv
{
	public class AIGroupSystem : BaseGameSystem, IAIJob, ISaveable, ILoadable<AIGroupSystemSaveData>
	{
		GameManager gameManager;
		TimeManager timeManager;
		static protected NotificationSystem notificationSystem;
		bool canHaveExpedition;

		const int ExpeditionGatheringTime = 60;
		int ExpeditionExplorationTime = 300;
		float ExpeditionSafeValue = 1;
		const int MaxAssignessPerExpedition = 6;

		public ExpeditionFeature expeditionFeature;
		AISettings aiSettings;
		CitizenAISystem citizenAISystem;
		EntitySystem entitySystem;
        SimpleEntityFactory factory;

		public AIGroupSystem(int factions, GameManager gameManager, CitizenAISystem citizenAISystem) : base()
		{
			Workers = new List<CitizenAIAgentFeature>();
			UpdateStatistics();

			this.citizenAISystem = citizenAISystem;
			this.gameManager = gameManager;
			timeManager = gameManager.timeManager;
			timeManager.OnDayChanged -= NewDay;
			timeManager.OnDayChanged += NewDay;
			notificationSystem = gameManager.SystemsManager.NotificationSystem;

			MaxWorkers = MaxAssignessPerExpedition;
            factory = gameManager.Factories.SimpleEntityFactory;

            expeditionFeature = factory.CreateInstance("expedition").GetFeature<ExpeditionFeature>();
			expeditionFeature.HideView();
			aiSettings = this.gameManager.gameMechanicSettings.aiSettings;
			entitySystem = this.gameManager.SystemsManager.EntitySystem;

			canHaveExpedition = true;
		}

		public void Run()
		{
			gameManager.SystemsManager.JobSystem.RegisterJob(this);			
		}

		public override void UpdateGameLoop()
		{
			var e = expeditionFeature;
			switch (e.state)
			{
				case ExpeditionFeature.EState.None:
					//Only have an expediton if scavangers are available. In case of multiple expedetions we have to subtract assigned scavangers from available scavangers.
					var scavangers = citizenAISystem.GetAssignedOccupants(EOccupation.Scavenger).Count;

					if (canHaveExpedition && scavangers > 0 && timeManager.CurrentDaytimeProgress >= 0.2f)
						ExpeditionStartState(e);
					break;
				//Wait for all attendendees
				case ExpeditionFeature.EState.Gathering:
					ExpeditionGatheringState(e);
					break;
				case ExpeditionFeature.EState.Started:
				case ExpeditionFeature.EState.Active:
					e.timer--;
					//When e is fnished
					if (e.timer <= 0)
						ExpeditionFinishState(e);
					break;
				default:
					break;
			}
			//UpdateStatistics();
		}

		void ExpeditionStartState(ExpeditionFeature e)
		{
			canHaveExpedition = false;
			//Initialize Expedition
			e.state = ExpeditionFeature.EState.Gathering;
			Vector2i pos = Vector2i.Zero;
			if (!gameManager.GridMap.FindRandomEmptyEdgePoint(out pos))
			{
				Debug.LogError("No edge point found to start expedition.");
				return;
			}
			e.expeditionLocation = new Location(pos);
			//Try to find a place halfway between the startingLocation and the city center (for that we use the center of the map instead)
			if (!gameManager.GridMap.FindClosestEmptyTile((Vector2i)Vector2.Lerp(pos, gameManager.GridMap.MapCenter, 0.5f), 30, out pos))
			{
				Debug.LogError("No gathering point found to start expedition.");
				return;
			}
			e.gatherLocation = new Location(pos);
			e.ShowView();
			e.Entity.GetFeature<EntityFeature>().View.transform.position = gameManager.GridMap.View.GetTileWorldPosition(e.gatherLocation.Indecies[0]).To3D();
			e.view.SetTooltip(LocalizationManager.GetText("#UI/Game/Tooltip/Exploration/Gathering"));
			e.timer = ExpeditionGatheringTime;

		}
		void ExpeditionGatheringState(ExpeditionFeature e)
		{
			if (e.assignees.Count > 0)
			{
				var remainingScavangers = GetUnassignedScavangers();
				var waitingForNewAssignees = remainingScavangers > 0 && e.timer > 0;
				e.timer--;

				if (AreAllAssigneesReady(e) && !waitingForNewAssignees)    // e.timer <= 0
				{
					e.Entity.GetFeature<EntityFeature>().View.transform.position = gameManager.GridMap.View.GetTileWorldPosition(e.expeditionLocation.Indecies[0]).To3D();
					e.view.SetTooltip(LocalizationManager.GetText("#UI/Game/Tooltip/Exploration/Active"));
					e.state = ExpeditionFeature.EState.Started;
					e.timer = ExpeditionExplorationTime;
				}

			}
		}
		void ExpeditionFinishState(ExpeditionFeature e)
		{
			e.state = ExpeditionFeature.EState.Finished;
			ExpeditionExplorationTime += 15;
			ExpeditionSafeValue = Mathf.Max(0, ExpeditionSafeValue - 0.1f);

			ProcessExpeditionResults(e);
			expeditionFeature.assignees.Clear();
			expeditionFeature.HideView();			
		}

		void ProcessExpeditionResults(ExpeditionFeature e)
		{
			//Here we handle death and injuries
			var d = e.Entity.StaticData.GetFeature<ExpeditionFeatureStaticData>();
			if (CivRandom.Chance(Mathf.Clamp01(d.deathChance - ExpeditionSafeValue)))
			{
				var list = Enumerable.ToList(e.assignees.Keys);
				int deadCount = Mathf.RoundToInt(e.assignees.Count * CivRandom.Range(d.deathRatio));

				for (int i = 0; i < deadCount; i++)
				{
					if (e.assignees.Count <= 0) break;
					var dead = CivRandom.SelectRandom(list);
					list.Remove(dead);
					e.assignees.Remove(dead);
					entitySystem.KillEntity(dead.Entity);
				}
			}

            //handle loot
            List<InventoryFeature> inventories = new List<InventoryFeature>(e.assignees.Count);
			float inventoryCapacity = 0;
            {
                int i = 0;
                foreach (var assignee in e.assignees)
                {
                    var unit = assignee.Key.UnitEntity;
                    var inv = unit.Inventory;
                    inventories.Add(inv);
                    inventoryCapacity += inv.CapacityLeft * CivRandom.Range(d.lootCarryFactor);
                    i++;
                }
            }
            
			var loot = GenerateLoot(d, inventoryCapacity);
			foreach (var item in loot)
			{
                if (inventories.Count <= 0)
                    break;
                for(int i = 0; i < item.Value; i++)
                {
                    if (inventories.Count <= 0)
                        break;
                    for (int j = 0; j < inventories.Count; j++)
                    {
                        if(InventorySystem.CanAddItems(inventories[j], item.Key, 1))
                        {
                            var res = factory.CreateInstance(item.Key).GetFeature<ItemFeature>();
                            res.Quantity = 1;
                            InventorySystem.AddItem(inventories[j], res, false);

                            if (inventories[j].CapacityLeft <= 0)
                                inventories.Remove(inventories[j]);

                            break;
                        }
                    }
                }				
			}
		}


		public Dictionary<string, int> GenerateLoot(ExpeditionFeatureStaticData e, float mass)
		{
			var returnList = new Dictionary<string, int>();
			if (e.lootPool == null || e.lootPool.Length <= 0)
				return returnList;

			var lootChooser = new ProbabilityUtility<int>();
			for (int i = 0; i < e.lootPool.Length; i++)
			{
				lootChooser.RegisterItem(i, e.lootPool[i].probability);
			}

			float massCounter = 0;
			int loopCounter = 100;
			for (int i = 0; i < loopCounter; i++)
			{
				var lootData = e.lootPool[lootChooser.PickItem()];
				var loot = lootData.foodPool.SelectRandom();
				int amount = 1;

				var data = Main.Instance.GameManager.Factories.SimpleEntityFactory.GetStaticData<ItemFeatureStaticData>(loot);
				if (data == null) continue;
				if (data.Mass * amount + massCounter > mass)
				{
					loopCounter -= 2;
					continue;
				}
				massCounter += data.Mass * amount;

				if (!returnList.ContainsKey(loot))
				{
					returnList.Add(loot, amount);
				}
				else
				{
					returnList[loot] += amount;
				}
			}
			return returnList;
		}

		bool AreAllAssigneesAttending(ExpeditionFeature e)
		{
			if (e.assignees.Count <= 0) return false;
			foreach (var state in e.assignees)
			{
				if (state.Value < EAssigneeState.Attending)
					return false;
			}
			return true;
		}

		int GetUnassignedScavangers()
		{
			var scavangers = citizenAISystem.GetAssignedOccupants(EOccupation.Scavenger);
			return scavangers.Count - expeditionFeature.assignees.Count;
		}

		bool AreAllAssigneesReady(ExpeditionFeature e)
		{
			if (e.assignees.Count <= 0) return false;
			foreach (var state in e.assignees)
			{
				if (state.Value < EAssigneeState.Ready)
					return false;
			}
			return true;
		}

		public override void UpdateStatistics()
		{
		}
				
		public enum EAssigneeState { None, Attending, Ready }

		#region IAIJob
		public bool IsWorkplace { get { return false; } }
		public bool HasWork { get { return true; } }
		public bool Disabled { get; set; }
		public EOccupation[] Occupations { get { return new EOccupation[] { EOccupation.Scavenger }; } }
		public float Priority { get; set; }
		public int MaxWorkers { get; private set; }
		public int WorkerCount { get { return Workers.Count; } }

		// Workers who are registered to work here (Scavangers)
		public List<CitizenAIAgentFeature> Workers { get; private set; }


		public void RegisterWorker(CitizenAIAgentFeature unit, EWorkerType type = EWorkerType.Worker)
		{
			if (!Workers.Contains(unit))
			{
				Workers.Add(unit);
			}
		}

		public void DeregisterWorker(CitizenAIAgentFeature unit)
		{
			Workers.Remove(unit);
		}

		public AITask AskForTask(EOccupation occupation, AIAgentFeatureBase unit)
		{
			if (expeditionFeature == null || !CanAttendExpedition(expeditionFeature))
				return null;

			var citizen = unit as CitizenAIAgentFeature;
			//AI is not a citizen
			if (citizen == null)
			{
				Debug.LogWarning("No citizen requested Task from AIGroup System");
				return null;
			}

			if (occupation != EOccupation.Scavenger)
				return null;

			else
			{
				if (expeditionFeature.assignees.ContainsKey(citizen))
					Debug.LogError("Should not happen, scavanger is already assigned to an expedition");
				else expeditionFeature.assignees.Add(citizen, EAssigneeState.None);
				return new ScavengingTask(citizen.Entity, expeditionFeature);
			}
		}

		bool CanAttendExpedition(ExpeditionFeature e)
		{
			//State must be right, not too many assignees and the wait time must not be over.
			//If hte wait time is over we do only wait for the assignees which are already walking to the gather point.
			return e.state == ExpeditionFeature.EState.Gathering && e.assignees.Count < MaxAssignessPerExpedition && e.timer > 0;
		}

        public void OnTaskStart()
        {

        }

		public void OnTaskComplete(AIAgentFeatureBase unit)
		{
		}
		#endregion

		void NewDay()
		{
			canHaveExpedition = true;
			expeditionFeature.state = ExpeditionFeature.EState.None;
		}


		#region Save System        
		public ISaveable CollectData()
		{
			var data = new AIGroupSystemSaveData();
			if(expeditionFeature != null)
			{
				data.expedition = (EntitySaveData)expeditionFeature.Entity.CollectData();
			}
			data.workerIDs = new List<string>();
			foreach (var worker in Workers)
			{
				data.workerIDs.Add(worker.Entity.UID.ToString());
			}
			return data;
		}

		public void ApplySaveData(AIGroupSystemSaveData data)
		{
			if (data == null)
				return;
			if(data.expedition != null)
			{
				var entity = Main.Instance.GameManager.Factories.SimpleEntityFactory.CreateInstance("expedition", data.expedition.UID);
				entity.ApplySaveData(data.expedition);
				expeditionFeature = entity.GetFeature<ExpeditionFeature>();
				
			}
			if (data.workerIDs != null)
			{
				foreach (var workerID in data.workerIDs)
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