using System;
using UnityEngine;
using System.Collections.Generic;

namespace Endciv
{
	/// <summary>
	/// Responsble to spawn outsiders that visit the town. Plit up for each npc type?
	/// </summary>
	public class NpcSpawnSystem : BaseGameSystem, ISaveable, ILoadable<NpcSpawnSystemSaveData>
	{
		private FactoryManager factories;
		private GridMap gridMap;
		private GameManager gameManager;
		private float newImmigrantCounter;

		public enum ETraderState { Arrival, Waiting, Leaving, Left }

		public TraderAIAgentFeature currentTrader;
		private float newTraderCounter;

		public List<ImmigrationGroup> immigrationGroups;
		public int TotalImmigrants
		{
			get
			{
				int count = 0;
				for (int i = 0; i < immigrationGroups.Count; i++)
				{
					count += immigrationGroups[i].immigrants.Count;
				}
				return count;
			}
		}

		public Dictionary<ImmigrantAIAgentFeature, ImmigrationGroup> immigrantGroupReference;

		public Action OnImmigrantCountChanged;
		public Action OnTraderAvailabilityChanged;

		public NpcSpawnSystem(GameManager gameManager, FactoryManager factories) : base()
		{
			this.factories = factories;
			this.gameManager = gameManager;
			gridMap = gameManager.GridMap;
			immigrationGroups = new List<ImmigrationGroup>();
			immigrantGroupReference = new Dictionary<ImmigrantAIAgentFeature, ImmigrationGroup>();
			newImmigrantCounter = GameConfig.Instance.GlobalAIData.ImmigrationBaseValue;
			newTraderCounter = GameConfig.Instance.GlobalAIData.NewTraderTime;

			OnImmigrantCountChanged += UpdateImmigrationCount;
		}

		public void ResetTraderCounter()
		{
			newTraderCounter = 0;
		}

		public override void UpdateStatistics()
		{
		}

		public override void UpdateGameLoop()
		{
			UpdateImmigrantLogic();
			UpdateTraderLogic();
		}

		bool wasWaiting;
		void UpdateTraderLogic()
		{
			if (currentTrader == null)
			{
				if (gameManager.SystemsManager.ConstructionSystem.HasTech(ETechnologyType.Storage))
				{
					if (newTraderCounter <= 0)
					{
						if (gameManager.timeManager.CurrentDaytime == EDaytime.Morning || gameManager.timeManager.CurrentDaytime == EDaytime.Noon)
						{
							currentTrader = SpawnTrader();
							wasWaiting = false;
						}
					}
					else
						newTraderCounter--;
				}
			}
			else
			{
				if (currentTrader.state == ETraderState.Waiting)
				{
					if (!wasWaiting)
					{
						Main.Instance.GameManager.SystemsManager.NotificationSystem.SetVariable<bool>("TraderAvailable", true);
						OnTraderAvailabilityChanged?.Invoke();
					}
					wasWaiting = true;
				}

				if (currentTrader.waitCounter <= 0)
				{
					if (currentTrader.state < ETraderState.Leaving)
					{
						currentTrader.state = ETraderState.Leaving;
						Main.Instance.GameManager.SystemsManager.NotificationSystem.SetVariable<bool>("TraderAvailable", false);
						OnTraderAvailabilityChanged?.Invoke();
					}
					if (currentTrader.state == ETraderState.Left)
					{
						currentTrader.Entity.Destroy();
						currentTrader = null;
						newTraderCounter = GameConfig.Instance.GlobalAIData.NewTraderTime;
					}
				}
				else
					currentTrader.waitCounter--;
			}
		}

		void UpdateImmigrantLogic()
		{
			var stats = GameStatistics.MainTownStatistics;

			bool changed = false;
			for (int i = immigrationGroups.Count - 1; i >= 0; i--)
			{
				immigrationGroups[i].timeRemaining -= 1;
				if (immigrationGroups[i].timeRemaining <= 0)
				{
					DenyImmigrationGroup(immigrationGroups[i]);
					changed = true;
				}

			}
			if (changed)
				OnImmigrantCountChanged?.Invoke();

			if (newImmigrantCounter <= 0)
			{
				if (gameManager.timeManager.CurrentDaytime != EDaytime.Night)
				{
					AddImmigrationGroup();
				}
			}
			else
			{
				float factor = stats.TotalHomeSpace - (stats.TotalPeople + TotalImmigrants) + 4;
				factor = Mathf.Sqrt(Mathf.Max(0, factor));

				newImmigrantCounter -= factor * GameConfig.Instance.GlobalAIData.ImmigrationFactor;
			}
		}

		public void AddImmigrationGroup()
		{
			var group = GetImmigrationGroup();
			if (group != null)
			{
				immigrationGroups.Add(group);
				for (int i = 0; i < group.immigrants.Count; i++)
				{
					immigrantGroupReference.Add(group.immigrants[i], group);
				}
				OnImmigrantCountChanged?.Invoke();
			}
			newImmigrantCounter = GameConfig.Instance.GlobalAIData.ImmigrationBaseValue;
		}

		void UpdateImmigrationCount()
		{
			Main.Instance.GameManager.SystemsManager.NotificationSystem.SetVariable<int>("totalImmigrants", TotalImmigrants);
		}

		private ImmigrationGroup GetImmigrationGroup()
		{
			var diff = Mathf.Sqrt(GameStatistics.MainTownStatistics.TotalHomeSpaceLeft);
			var group = new ImmigrationGroup();
			int maxpeople = (int)Mathf.Clamp(diff * 2, 1, 5);
			int minpeople = (int)Mathf.Clamp(diff * 2 - 3, 1, 5);
			var people = CivRandom.Range(minpeople, maxpeople);

			if (people <= 0)
				return null;
			List<ImmigrantAIAgentFeature> immigrants = new List<ImmigrantAIAgentFeature>();
			Vector2i[] spawnIndex;
			if (gridMap.GetRandomPassablePositionsOnEdge(people, out spawnIndex))
			{
				for (int i = 0; i < people; i++)
				{
					Debug.Log("Immigrant Spawned at: " + spawnIndex[i].ToString());
					var age = CivRandom.Range(0, 10) <= 1 ? ELivingBeingAge.Child : ELivingBeingAge.Adult;
					var pos = gridMap.View.GetTileWorldPosition(spawnIndex[i]).To3D();
					var immigrant = gameManager.UserToolSystem.UnitTool.CreateImmigrant("human", pos, age);
					immigrants.Add(immigrant);
				}
			}
			group.Setup(immigrants, Main.Instance.GameManager.timeManager.dayTickLength);
			return group;
		}

		/// <summary>
		/// Currently spawn as player citizens
		/// </summary>
		public void ConvertImmigrantsToCitizens(ImmigrationGroup group)
		{
			RemoveImmigrationGroup(group);
			if (group.immigrants == null || group.immigrants.Count <= 0)
				return;
			foreach (var immigrant in group.immigrants)
			{
				if (immigrant == null)
					continue;
				immigrantGroupReference.Remove(immigrant);

				var unit = immigrant.Entity.GetFeature<UnitFeature>();
				unit.ConvertImmigrantToCitizen();

				var entity = immigrant.Entity;
				entity.ChangeFaction(SystemsManager.MainPlayerFaction);
			}
		}

		public void DenyImmigrationGroup(ImmigrationGroup group)
		{
			RemoveImmigrationGroup(group);
			foreach (var immigrant in group.immigrants)
			{
				if (immigrant == null)
					continue;
				immigrant.State = EImmigrantState.Leaving;
			}
		}

		public void RemoveImmigrationGroup(ImmigrationGroup group)
		{
			immigrationGroups.Remove(group);
			foreach (var immigrant in group.immigrants)
			{
				immigrantGroupReference.Remove(immigrant);
			}
			OnImmigrantCountChanged?.Invoke();
		}

		public TraderAIAgentFeature SpawnTrader()
		{
			if (currentTrader != null) return null;
			var traderPool = StaticDataIO.Instance.GetData<TraderStaticData>("Traders");
			var traderData = traderPool.SelectRandom();

			Vector2i spawnIndex;
			if (gridMap.GetRandomPassablePositionOnEdge(out spawnIndex))
			{
				Debug.Log("Trader Spawned at: " + spawnIndex.ToString());
				var trader = gameManager.UserToolSystem.UnitTool.CreateTrader("trader_car", gridMap.View.GetTileWorldPosition(spawnIndex).To3D(), traderData);
				trader.waitCounter = GameConfig.Instance.GlobalAIData.TraderWaitTicks;
				return trader;
			}
			return null;
		}

		public ISaveable CollectData()
		{
			var data = new NpcSpawnSystemSaveData();
			data.newImmigrantCounter = newImmigrantCounter;
			if (immigrationGroups != null && immigrationGroups.Count > 0)
			{
				data.immigrationGroups = new List<ImmigrationGroupSaveData>();
				foreach (var immigrationGroup in immigrationGroups)
				{
					data.immigrationGroups.Add((ImmigrationGroupSaveData)immigrationGroup.CollectData());
				}
			}
			data.newTraderCounter = newTraderCounter;
			data.currentTraderUID = string.Empty;
			if (currentTrader != null)
			{
				data.currentTraderUID = currentTrader.Entity.UID.ToString();
			}
			return data;
		}

		public void ApplySaveData(NpcSpawnSystemSaveData data)
		{
			if (data == null)
				return;
			newImmigrantCounter = data.newImmigrantCounter;
			if (data.immigrationGroups != null && data.immigrationGroups.Count > 0)
			{
				immigrationGroups = new List<ImmigrationGroup>();
				immigrantGroupReference = new Dictionary<ImmigrantAIAgentFeature, ImmigrationGroup>();
				foreach (var group in data.immigrationGroups)
				{
					var newGroup = new ImmigrationGroup();
					newGroup.ApplySaveData(group);
					immigrationGroups.Add(newGroup);
					foreach (var immigrant in newGroup.immigrants)
					{
						immigrantGroupReference.Add(immigrant, newGroup);
					}
				}
			}
			newTraderCounter = data.newTraderCounter;
			if (!string.IsNullOrEmpty(data.currentTraderUID))
			{
				var guid = Guid.Parse(data.currentTraderUID);
				if (Main.Instance.GameManager.SystemsManager.Entities.ContainsKey(guid))
				{
					var entity = Main.Instance.GameManager.SystemsManager.Entities[guid];
					if (entity.HasFeature<TraderAIAgentFeature>())
					{
						currentTrader = entity.GetFeature<TraderAIAgentFeature>();
					}
				}

			}
		}
	}
}