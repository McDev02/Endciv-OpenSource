using System.Linq;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Endciv
{
	[Serializable]
	public class OccupationSettingSaveData : ISaveable
	{
		public int occupation;
		public int wantedAmount;
		public List<string> citizens;

		public ISaveable CollectData()
		{
			return this;
		}
	}

	public class CitizenAISystem : EntityFeatureSystem<CitizenAIAgentFeature>, ISaveable, ILoadable<CitizenAISystemSaveData>
	{
		GridMap GridMap;
		JobSystem JobSystem;
		AISettings aiSettings;
		UnitSystemConfig unitConfig;

		static TimeManager timeManager;
		public CitizenShedule.ESheduleType generalSheduleState;

		/// <summary>
		/// Runtime variants of aiSettings.citizenClasses
		/// </summary>
		public CitizenClass[] citizenClasses;

		//Occupation management
		public OccupationSetting[] OccupationSettings;
		//Cached occupation data
		int remainingOccupations;
		public Action OnOccupationUpdated;
		public Action OnRationsChanged;

		public int waterConsumePortions;
		public int nutritionConsumePortions;

		const int PortionsConsumedPerDay = 3;
		const float HumanWalkingDistance = 6 * GridMapView.GridTileFactorInv;

        public static HashSet<string> consumableFilter;

        public class OccupationSetting : ISaveable, ILoadable<OccupationSettingSaveData>
		{
			public EOccupation occupation;
			public int wantedAmount;
			public List<CitizenAIAgentFeature> assignedCitizens = new List<CitizenAIAgentFeature>(4);
			public float FillProgress { get { return wantedAmount <= 0 ? 1 : assignedCitizens.Count / wantedAmount; } }
			public OccupationSetting(EOccupation occupation)
			{
				this.occupation = occupation;
			}

			public ISaveable CollectData()
			{
				var data = new OccupationSettingSaveData();
				data.occupation = (int)occupation;
				data.wantedAmount = wantedAmount;
				data.citizens = new List<string>();
				foreach (var citizen in assignedCitizens)
				{
					data.citizens.Add(citizen.Entity.UID.ToString());
				}
				return data;
			}

			public void ApplySaveData(OccupationSettingSaveData data)
			{
				if (data == null)
					return;
				occupation = (EOccupation)data.occupation;
				wantedAmount = data.wantedAmount;
				assignedCitizens = new List<CitizenAIAgentFeature>(4);
				Main.Instance.GameManager.SystemsManager.NotificationSystem.SetVariable<int>($"totalOccupation_{occupation.ToString()}", 0);

				if (data.citizens != null && data.citizens.Count > 0)
				{
					foreach (var citizenID in data.citizens)
					{
						var id = Guid.Parse(citizenID);
						if (Main.Instance.GameManager.SystemsManager.Entities.ContainsKey(id))
						{
							var entity = Main.Instance.GameManager.SystemsManager.Entities[id];
							if (entity.HasFeature<CitizenAIAgentFeature>())
							{
								var feature = entity.GetFeature<CitizenAIAgentFeature>();
								assignedCitizens.Add(feature);
								Main.Instance.GameManager.SystemsManager.NotificationSystem.SetVariable<int>($"totalOccupation_{occupation.ToString()}", assignedCitizens.Count);
							}
						}
					}
				}
			}
		}
		public override void UpdateStatistics()
		{
			GameStatistics.MainTownStatistics.averageNeedMood = 0;
			GameStatistics.MainTownStatistics.averageNeedHealth = 0;
			GameStatistics.MainTownStatistics.averageNeedHunger = 0;
			GameStatistics.MainTownStatistics.averageNeedThirst = 0;
			GameStatistics.MainTownStatistics.averageNeedResting = 0;
			GameStatistics.MainTownStatistics.averageNeedStress = 0;
			GameStatistics.MainTownStatistics.averageNeedSettlement = 0;
			GameStatistics.MainTownStatistics.averageNeedToilet = 0;
			GameStatistics.MainTownStatistics.averageNeedCleaning = 0;

			int count = FeaturesByFaction[SystemsManager.MainPlayerFaction].Count;
			CitizenAIAgentFeature citizen;
			for (int i = 0; i < count; i++)
			{
				citizen = FeaturesByFaction[SystemsManager.MainPlayerFaction][i];
				GameStatistics.MainTownStatistics.averageNeedMood += citizen.mood;
				GameStatistics.MainTownStatistics.averageNeedHealth += citizen.HealthNeed.Mood;
				GameStatistics.MainTownStatistics.averageNeedHunger += citizen.HungerNeed.Mood;
				GameStatistics.MainTownStatistics.averageNeedThirst += citizen.ThirstNeed.Mood;
				//GameStatistics.MainTownStatistics.averageNeedResting += citizen.StressNeed.Mood;
				GameStatistics.MainTownStatistics.averageNeedStress += citizen.StressNeed.Mood;
				GameStatistics.MainTownStatistics.averageNeedSettlement += citizen.SettlementNeed.Mood;
				GameStatistics.MainTownStatistics.averageNeedToilet += citizen.ToiletNeed.Mood;
				GameStatistics.MainTownStatistics.averageNeedCleaning += citizen.CleaningNeed.Mood;
			}
			GameStatistics.MainTownStatistics.averageNeedMood /= count;
			GameStatistics.MainTownStatistics.averageNeedHealth /= count;
			GameStatistics.MainTownStatistics.averageNeedHunger /= count;
			GameStatistics.MainTownStatistics.averageNeedThirst /= count;
			GameStatistics.MainTownStatistics.averageNeedResting /= count;
			GameStatistics.MainTownStatistics.averageNeedStress /= count;
			GameStatistics.MainTownStatistics.averageNeedSettlement /= count;
			GameStatistics.MainTownStatistics.averageNeedToilet /= count;
			GameStatistics.MainTownStatistics.averageNeedCleaning /= count;
		}

		public CitizenAISystem(int factions, GameManager gameManager, SystemsManager systemsManager, AISettings aiSettings) : base(factions)
		{
			this.aiSettings = aiSettings;
			unitConfig = gameManager.gameConfig.UnitSystemData;

			GridMap = gameManager.GridMap;
			JobSystem = systemsManager.JobSystem;
			timeManager = gameManager.timeManager;

            consumableFilter = new HashSet<string>();

			//Initialize OccupationTable
			int count = (int)EOccupation.COUNT;
			OccupationSettings = new OccupationSetting[count];
			for (int i = 0; i < count; i++)
			{
				var occ = (EOccupation)i;
				OccupationSettings[i] = new OccupationSetting(occ);
				////Defaults
				//if (occ == EOccupation.Construction)
				//	OccupationSettings[i].wantedAmount = 2;
				//if (occ == EOccupation.Supply)
				//	OccupationSettings[i].wantedAmount = 2;
			}

			citizenClasses = new CitizenClass[aiSettings.citizenClasses.Length];
			//Intitialize Citizen Classes
			for (int i = 0; i < aiSettings.citizenClasses.Length; i++)
			{
				citizenClasses[i] = GameObject.Instantiate(aiSettings.citizenClasses[i]);
			}
		}

		internal void IncreaseWantedOccupation(EOccupation id)
		{
			int amount = OccupationSettings[(int)id].wantedAmount + 1;
			OccupationSettings[(int)id].wantedAmount = Mathf.Clamp(amount, 0, 99);
			UpdateOccupationCache();
			if (OnOccupationUpdated != null) OnOccupationUpdated.Invoke();
		}
		internal void DecreaseWantedOccupation(EOccupation id)
		{
			var settings = OccupationSettings[(int)id];
			int amount = settings.wantedAmount - 1;
			amount = Mathf.Clamp(amount, 0, 99);
			settings.wantedAmount = amount;
			while (settings.assignedCitizens.Count > amount)
			{
				if (settings.assignedCitizens.Count <= 0) break;
				var citizen = settings.assignedCitizens[settings.assignedCitizens.Count - 1];
				ChangeOccupation(citizen, EOccupation.Labour);
			}
			UpdateOccupationCache();
			if (OnOccupationUpdated != null) OnOccupationUpdated.Invoke();
		}

		void UpdateOccupationCache()
		{
			remainingOccupations = 0;
			for (int i = 0; i < OccupationSettings.Length; i++)
			{
				var remaining = OccupationSettings[i].wantedAmount - OccupationSettings[i].assignedCitizens.Count;
				if (remaining > 0)
					remainingOccupations += remaining;
			}
		}

		public EOccupation GetNewOccupation()
		{
			OccupationSetting occ = null;

			var settings = OccupationSettings.ToList();
			//settings.Sort((a, b) => a.citizens.Count.CompareTo(b.citizens.Count));
			settings = settings.OrderBy(x => (x.wantedAmount <= 0 ? 1 : (x.assignedCitizens.Count / (float)x.wantedAmount))).ToList();

			for (int i = 0; i < settings.Count; i++)
			{
				var newocc = settings[i];
				if (newocc.occupation <= EOccupation.Labour)
					continue;

				if (newocc.wantedAmount > newocc.assignedCitizens.Count)
				{
					if (occ == null || occ.FillProgress > newocc.FillProgress)
						occ = newocc;
				}
			}
			if (occ != null) return occ.occupation;
			else return EOccupation.Labour;
		}

		public override void UpdateGameLoop()
		{
			float tickByDay = timeManager.dayTickFactor;
			CitizenAIAgentFeature citizen;

			for (int f = 0; f < FeaturesByFaction.Count; f++)
			{
				for (int i = 0; i < FeaturesByFaction[f].Count; i++)
				{
					citizen = FeaturesByFaction[f][i];
					UpdateCitizenNeeds(citizen, tickByDay);
					AIAgentSystem.CalculateMood(citizen);
					UpdateClassSatisfaction(citizen);
				}
			}
			UpdateStatistics();
		}

		public void UpdateEachFrame()
		{
			generalSheduleState = aiSettings.labourShedule.GetCurrentShedule(timeManager.CurrentDaytimeProgress).type;
			if (Main.Instance.workOnly) generalSheduleState = CitizenShedule.ESheduleType.Work;
			else if (Main.Instance.noSpareTime && generalSheduleState == CitizenShedule.ESheduleType.SpareTime) generalSheduleState = CitizenShedule.ESheduleType.Work;

			for (int f = 0; f < FeaturesByFaction.Count; f++)
			{
				for (int i = 0; i < FeaturesByFaction[f].Count; i++)
				{
					ExecuteCitizenAI(FeaturesByFaction[f][i]);
				}
			}
		}

		internal void SetRationValues(int water, int nutrition)
		{
			bool actuallyChanged = waterConsumePortions != water || nutritionConsumePortions != nutrition;

			waterConsumePortions = water;
			nutritionConsumePortions = nutrition;

			if (actuallyChanged)
				OnRationsChanged?.Invoke();
		}

		/// <summary>
		/// Mood standards change over lifetime depending on if the overal mood is good or bad in a negative Feedback loop
		/// </summary>
		/// <param name="citizen"></param>
		void UpdateClassSatisfaction(CitizenAIAgentFeature citizen)
		{
			float tickByDay = timeManager.dayTickFactor;
			var needs = citizen.needsLookup;
			float min, max, scale;
			for (int i = 0; i < needs.Count; i++)
			{
				scale = Mathf.Pow(needs[i].Mood + 1, 1.5f) - 1f;
				scale *= tickByDay * citizen.citizenClass.satisfactionAdaption;
				scale /= Mathf.Abs(citizen.moodChangeOverLifetime) + 1;
				min = needs[i].minValue + scale;
				max = needs[i].maxValue + scale;
				citizen.moodChangeOverLifetime += scale;
				needs[i].SetMinMax(min, max);
			}
		}

		void UpdateCitizenNeeds(CitizenAIAgentFeature citizen, float tickByDay)
		{
			// Reduce some Citizen needs by the value of one tick relative to the whole day. So after one day the accumulated value is 1.
			for (int i = 0; i < citizen.dailyReducedNeeds.Count; i++)
			{
				citizen.dailyReducedNeeds[i].Value -= tickByDay;
			}
			if (citizen.HasHome)
				citizen.HomehNeed.Value = citizen.Home.quality;
			else
				citizen.HomehNeed.Value = 0;
		}

		private void ExecuteCitizenAI(CitizenAIAgentFeature agent)
		{
			if (JobSystem == null)
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

			//Assign occupation based on demand
			if (agent.Occupation == EOccupation.Labour && remainingOccupations > 0)
			{
				var newOcc = GetNewOccupation();
				if (newOcc > EOccupation.Labour)
					ChangeOccupation(agent, newOcc);
			}

			//Get New task First, always clear inventory
			if (agent.Entity.Inventory.Load > 0)
			{
				var task = new EmptyInventoryTask(agent.Entity);
				task.Initialize();
				SetCurrentTask(agent, task);
				return;
			}

			//Select Shedule State
			var lastShedule = agent.currentShedule;
			agent.currentShedule = GetCurrentUnitShedule(agent);
			if (agent.currentShedule != lastShedule)
				agent.lastSheduleData.Reset();

			agent.Entity.GetFeature<EntityFeature>().currentStressLevel = 0;


			//Exhausted unit does not work
			var being = agent.Entity.GetFeature<LivingBeingFeature>();
			if (being != null && being.vitality.Mood <= 0)
			{
				ExecuteStateExhausted(agent, being);
			}
			else
			{
				switch (agent.currentShedule)
				{
					case CitizenShedule.ESheduleType.Sleep:
						ExecuteStateSleep(agent);
						break;
					case CitizenShedule.ESheduleType.Work:
						ExecuteStateWork(agent);
						break;
					case CitizenShedule.ESheduleType.Lunch:
						ExecuteStateLunch(agent);
						break;
					case CitizenShedule.ESheduleType.SpareTime:
						ExecuteStateSpareTime(agent);
						break;
					case CitizenShedule.ESheduleType.Hometime:
						ExecuteStateHometime(agent);
						break;
					default:
						ExecuteStateSpareTime(agent);
						break;
				}
			}

			//If no task was found and it wasn't spare time, try sparetime
			if (agent.CurrentTask == null && agent.currentShedule != CitizenShedule.ESheduleType.SpareTime)
				ExecuteStateSpareTime(agent);

			//If still no task was found, walk around
			if (agent.CurrentTask == null)
			{
				var task = new RoamingTask(agent.UnitEntity, HumanWalkingDistance, new MinMax(1f, 5f), GridMap);
				task.Initialize();
				SetCurrentTask(agent, task);
			}
		}

		public static CitizenShedule.ESheduleType GetCurrentUnitShedule(CitizenAIAgentFeature agent)
		{
			var shedule = agent.shedule.GetCurrentShedule(timeManager.CurrentDaytimeProgress).type;
			if (Main.Instance.workOnly) shedule = CitizenShedule.ESheduleType.Work;
			else if (Main.Instance.noSpareTime && shedule == CitizenShedule.ESheduleType.SpareTime) agent.currentShedule = CitizenShedule.ESheduleType.Work;

			return shedule;
		}

		internal override void RegisterFeature(CitizenAIAgentFeature feature)
		{
			base.RegisterFeature(feature);
			var being = feature.Entity.GetFeature<LivingBeingFeature>();

			feature.shedule = being.age == ELivingBeingAge.Child ? aiSettings.childShedule : aiSettings.undefinedShedule;
			feature.currentShedule = CitizenShedule.ESheduleType.SpareTime;

			ChangeOccupation(feature, feature.Occupation, true);
		}

		internal override void DeregisterFeature(CitizenAIAgentFeature feature, int faction = -1)
		{
			base.DeregisterFeature(feature);
			if (faction < 0) faction = feature.FactionID;
			bool updated = false;
			foreach (var setting in OccupationSettings)
			{
				if (setting.assignedCitizens.Contains(feature))
				{
					setting.assignedCitizens.Remove(feature);
					Main.Instance.GameManager.SystemsManager.NotificationSystem.SetVariable<int>($"totalOccupation_{setting.occupation.ToString()}", setting.assignedCitizens.Count);
					updated = true;
				}

			}
			if (updated)
				OnOccupationUpdated?.Invoke();
		}

		//Shedule States
		void ExecuteStateWork(CitizenAIAgentFeature agent)
		{

			//Get other work
			AITask task;
			if (agent.Occupation > EOccupation.Labour)
			{
				task = JobSystem.GetTaskByOccupation(agent.Occupation, agent);
				if (task != null)
				{
					SetCurrentTask(agent, task);
					agent.Entity.GetFeature<EntityFeature>().currentStressLevel = unitConfig.stressUnderWork;
					return;
				}

			}
			//Get Labour task
			if (agent.CurrentTask == null)
			{
				task = JobSystem.GetTaskByOccupation(EOccupation.Labour, agent);
				if (task != null)
				{
					SetCurrentTask(agent, task);
					agent.Entity.GetFeature<EntityFeature>().currentStressLevel = unitConfig.stressUnderWork;
					return;
				}
			}
		}

		void ExecuteStateSpareTime(CitizenAIAgentFeature agent)
		{
			agent.Entity.GetFeature<EntityFeature>().currentStressLevel = unitConfig.stressUnderRelaxing;
			AITask task;
			if (!TryToFulfillNeeds(agent, true, out task))
			{
				task = new RoamingTask(agent.UnitEntity, HumanWalkingDistance, new MinMax(1f, 5f), GridMap);
			}

			if (task != null)
			{
				task.Initialize();
				SetCurrentTask(agent, task);
			}
		}

		void ExecuteStateSleep(CitizenAIAgentFeature agent)
		{
			agent.Entity.GetFeature<EntityFeature>().currentStressLevel = unitConfig.stressUnderRelaxing;
			var livingBeing = agent.Entity.GetFeature<LivingBeingFeature>();
			AITask task;
			if (agent.HasHome)
				task = new RestHomeTask(agent.Entity, GridMap);
			else
				task = new SleepOutsideTask(agent.Entity, 8, GridMap);

			if (task != null)
			{
				task.Initialize();
				SetCurrentTask(agent, task);
			}
		}

		void ExecuteStateExhausted(CitizenAIAgentFeature agent, LivingBeingFeature being)
		{
			AITask task = null;
			if (being.vitality.Mood < -0.99f)
			{
				//Immobile
				task = new ImmobileTask(being);
			}
			else
			{
				//Try to find nutritions regardless of consumption limit
				TryToFulfillNeeds(agent, false, out task);
				//If nothing found, spare time will be executed.
			}

			if (task != null)
			{
				task.Initialize();
				SetCurrentTask(agent, task);
			}
		}

		void ExecuteStateHometime(CitizenAIAgentFeature agent)
		{
			AITask task;
			if (agent.HasHome)
			{
				task = new StayHomeTask(agent.Entity, GridMap);
				task.Initialize();
				SetCurrentTask(agent, task);
				agent.Entity.GetFeature<EntityFeature>().currentStressLevel =
					unitConfig.stressUnderRelaxing;
			}
			else
				ExecuteStateSpareTime(agent);
		}

		void ExecuteStateLunch(CitizenAIAgentFeature agent)
		{
			agent.Entity.GetFeature<EntityFeature>().currentStressLevel =
				unitConfig.stressUnderIdle;
			AITask task;
			if (TryToFulfillNeeds(agent, true, out task))
			{
				task.Initialize();
				SetCurrentTask(agent, task);
			}
			else
			{
				agent.lastSheduleData.hadPause = true;
				ExecuteStateWork(agent);
			}
		}

		bool TryToFulfillNeeds(CitizenAIAgentFeature agent, bool considerConsumptionLimit, out AITask task)
		{
			agent.Entity.GetFeature<EntityFeature>().currentStressLevel =
				unitConfig.stressUnderIdle;
			if (!considerConsumptionLimit || !agent.lastSheduleData.hasConsumedFood)
			{
				task = GetFoodWaterTask(agent);
				return task != null;
			}
			if (!agent.lastSheduleData.lookedForToilet && agent.ToiletNeed.Value <= -0.35f)
			{
				agent.lastSheduleData.lookedForToilet = true;
				task = new GoToToiletTask(agent.Entity);
				return true;
			}
			if (!agent.lastSheduleData.lookedForShower && agent.CleaningNeed.Mood <= 0.55f)
			{
				agent.lastSheduleData.lookedForShower = true;
				task = new GoToShowerTask(agent.Entity);
				return true;
			}
			task = null;
			return false;
		}

		private FindFoodWaterTask GetFoodWaterTask(CitizenAIAgentFeature agent)
		{
			var being = agent.Entity.GetFeature<LivingBeingFeature>();

			var canConsume = Mathf.Max(0, waterConsumePortions - being.waterConsumed);
			int water = (int)Mathf.Min((waterConsumePortions / 2f), canConsume);

			canConsume = Mathf.Max(0, nutritionConsumePortions - being.nutritionConsumed);
			float nutrition = Mathf.Min((nutritionConsumePortions * 3 / 2f), canConsume);
            //nutrition = Mathf.Min(nutritionConsumePortions, Mathf.Max(0, nutritionConsumePortions * PortionsConsumedPerDay - being.nutritionConsumed));
            //water = (int)Mathf.Min(waterConsumePortions, Mathf.Max(0, waterConsumePortions * PortionsConsumedPerDay - being.waterConsumed));
            if (water == 0 && nutrition == 0)
                return null;
            if(agent.HasHome)
            {
                var homeInventory = agent.Home.Entity.Inventory;
                if ((homeInventory.Statistics.NutritionAvailable > 0 && nutrition > 0) ||
                        (homeInventory.Statistics.WaterAvailable > 0 && water > 0))
                    return new FindFoodWaterTask(agent.Entity, nutrition, water);
            }
			if ((StorageSystem.Statistics.WaterAvailable > 0 && water > 0) ||
				(StorageSystem.Statistics.NutritionAvailable > 0 && nutrition > 0))
				return new FindFoodWaterTask(agent.Entity, nutrition, water);
			return null;
		}

		public void OnFindFoodWaterTaskComplete(AIAgentFeatureBase agent)
		{
			var citizen = agent as CitizenAIAgentFeature;
			if (citizen == null)
				return;
			if (!(citizen.CurrentTask is FindFoodWaterTask))
				return;
			citizen.lastSheduleData.hasConsumedFood = true;
		}

		public void OnVisitedToiletTaskComplete(AIAgentFeatureBase agent)
		{
			var citizen = agent as CitizenAIAgentFeature;
			if (citizen == null)
				return;
			var toiletTask = agent.CurrentTask as GoToToiletTask;
			if (toiletTask == null)
				return;
			if (toiletTask.CurrentState == AITask.TaskState.Success)
			{
				citizen.ToiletNeed.Value = 0;
			}
			var toilet = toiletTask.GetToilet();
			if (toilet == null)
				return;
			if (toilet.Occupants.Contains(agent.UnitEntity))
			{
				toilet.Occupants.Remove(agent.UnitEntity);
			}
		}

		public void OnVisitedShowerTaskComplete(AIAgentFeatureBase agent)
		{
			var citizen = agent as CitizenAIAgentFeature;
			if (citizen == null)
				return;
			var showerTask = agent.CurrentTask as GoToShowerTask;
			if (showerTask == null)
				return;
			if (showerTask.CurrentState == AITask.TaskState.Success)
			{
				citizen.CleaningNeed.Value = 0;
			}
			var shower = showerTask.GetShower();
			if (shower == null)
				return;
			if (shower.Occupants.Contains(agent.UnitEntity))
			{
				shower.Occupants.Remove(agent.UnitEntity);
			}
		}

		void SetCurrentTask(CitizenAIAgentFeature agent, AITask task)
		{
			StopCurrentTask(agent);
			agent.CurrentTask = task;
		}

		void StopCurrentTask(CitizenAIAgentFeature agent)
		{
			if (agent.CurrentTask != null && agent.CurrentTask.job != null)
				agent.CurrentTask.job.DeregisterWorker(agent);
			agent.CurrentTask = null;
		}

		public List<CitizenAIAgentFeature> GetAssignedOccupants(EOccupation occupation)
		{
			var data = OccupationSettings[(int)occupation];
			return data.assignedCitizens;
		}

		public void ChangeOccupation(CitizenAIAgentFeature agent, EOccupation occupation, bool force = false)
		{
			if (!force && agent.Occupation == occupation) return;
			var data = OccupationSettings[(int)agent.Occupation];
			if (data.assignedCitizens.Contains(agent))
			{
				data.assignedCitizens.Remove(agent);
				Main.Instance.GameManager.SystemsManager.NotificationSystem.SetVariable<int>($"totalOccupation_{data.occupation.ToString()}", data.assignedCitizens.Count);
			}
			agent.Occupation = occupation;
			data = OccupationSettings[(int)occupation];
			if (!data.assignedCitizens.Contains(agent))
			{
				data.assignedCitizens.Add(agent);
				Main.Instance.GameManager.SystemsManager.NotificationSystem.SetVariable<int>($"totalOccupation_{data.occupation.ToString()}", data.assignedCitizens.Count);
			}
			OnOccupationUpdated?.Invoke();
		}

		public CitizenAIAgentFeature GetCitizenByUID(Guid UID)
		{
			return FeaturesByFaction.SelectMany(x => x).FirstOrDefault(x => x.Entity.UID == UID);
		}

		public ISaveable CollectData()
		{
			var data = new CitizenAISystemSaveData();
			data.settings = new List<OccupationSettingSaveData>();
            data.consumableFilter = new List<string>();
			foreach (var setting in OccupationSettings)
			{
				data.settings.Add((OccupationSettingSaveData)setting.CollectData());
			}

            foreach(var id in consumableFilter)
            {
                data.consumableFilter.Add(id);
            }

			data.waterConsumePortions = waterConsumePortions;
			data.nutritionConsumePortions = nutritionConsumePortions;

			return data;
		}

		public void ApplySaveData(CitizenAISystemSaveData data)
		{
			if (data == null)
				return;
			if (data.settings != null && data.settings.Count > 0)
			{
				for (int i = 0; i < OccupationSettings.Length; i++)
				{
					if (i >= data.settings.Count)
						continue;
					OccupationSettings[i].ApplySaveData(data.settings[i]);
				}
			}

            consumableFilter.Clear();
            if(data.consumableFilter != null)
            {
                foreach(var id in data.consumableFilter)
                {
                    consumableFilter.Add(id);
                }
            }

			waterConsumePortions = data.waterConsumePortions;
			nutritionConsumePortions = data.nutritionConsumePortions;
		}
	}
}