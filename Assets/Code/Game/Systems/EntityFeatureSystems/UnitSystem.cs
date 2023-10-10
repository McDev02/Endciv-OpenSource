using System;
using System.Collections.Generic;


namespace Endciv
{
	public class UnitSystem : BaseGameSystem
	{
		public const float HumanSizeFactor = 1f;
		public const float GenderGenerationThreshold = 0.6f;
		public const float AdultGenerationThreshold = 0.5f;

		float dayTickFactor;
		public bool SimulateLivingBeings = true;
		UnitSystemConfig config;

		EntitySystem entitySystem;
		TimeManager timeManager;

		public Dictionary<Guid, BaseEntity> UnitPool;
		public List<List<BaseEntity>> Units;
		public List<List<BaseEntity>> Citizens;
		public List<List<LivingBeingFeature>> LivingBeings;
		public List<List<CattleFeature>> Cattle;
		public List<List<BaseEntity>> DeadUnits;

		public List<BaseEntity> hungryCitizens;
		public List<BaseEntity> hungryAnimals;
		public List<BaseEntity> thirstyCitizens;
		public List<BaseEntity> thirstyAnimals;

		//Callbacks
		public Action OnUnitAdded;
		public Action OnUnitRemoved;
		public Action OnUnitDied;

		public Action OnDeadUnitRemoved;
		public Action OnHungryCitizenAdded;
		public Action OnHungryCitizenRemoved;
		public Action OnThirstyCitizenAdded;
		public Action OnThirstyCitizenRemoved;

		public Action OnTroubledCattleAdded;
		public Action OnTroubledCattleRemoved;

		public Action OnCitizenAdded;
		public Action OnCitizenRemoved;
		public Action OnCattleAdded;
		public Action OnCattleRemoved;

		public enum ECattleProduction { None, Milk, Eggs, Wool }

		public UnitSystem(int factions, EntitySystem entitySystem, TimeManager timeManager, UnitSystemConfig config) : base()
		{
			this.entitySystem = entitySystem;
			this.timeManager = timeManager;
			this.config = config;

			UnitPool = new Dictionary<Guid, BaseEntity>(128);
			Units = new List<List<BaseEntity>>(factions);
			DeadUnits = new List<List<BaseEntity>>(factions);
			Citizens = new List<List<BaseEntity>>(factions);
			LivingBeings = new List<List<LivingBeingFeature>>(factions);
			Cattle = new List<List<CattleFeature>>(factions);
			thirstyAnimals = new List<BaseEntity>(32);
			thirstyCitizens = new List<BaseEntity>(32);
			hungryAnimals = new List<BaseEntity>(32);
			hungryCitizens = new List<BaseEntity>(32);

			for (int i = 0; i < factions; i++)
			{
				Units.Add(new List<BaseEntity>(i == 0 ? 32 : 8));
				DeadUnits.Add(new List<BaseEntity>(i == 0 ? 32 : 8));
				Citizens.Add(new List<BaseEntity>(i == 0 ? 32 : 8));
				LivingBeings.Add(new List<LivingBeingFeature>(i == 0 ? 32 : 8));
				Cattle.Add(new List<CattleFeature>(i == 0 ? 32 : 8));
			}
			dayTickFactor = timeManager.dayTickFactor;
            timeManager.OnDayChanged -= UpdateUnitAge;
            timeManager.OnDayChanged += UpdateUnitAge;
        }

		/*
		public IEnumerable<BaseEntity> GetAllUnits(bool includeDead = false)
		{
            if(!includeDead)
			    return Units.CollectAll();
            else
            {
                var list = Units.CollectAll();
                list.AddRange(DeadUnits.CollectAll());
                return list;
            }
		}
        */
		public BaseEntity[] GetUnits(int factionID)
		{
			return Units[factionID].ToArray();
		}

		internal void RegisterUnit(BaseEntity unit, int faction = -1)
		{
			if (faction < 0) faction = unit.factionID;
			if (Units[faction].Contains(unit))
			{
				Debug.LogError("Unit already registered.");
				return;
			}
			UnitPool.Add(unit.UID, unit);
			Units[faction].Add(unit);
			OnUnitAdded?.Invoke();

			if (unit.HasFeature<CitizenAIAgentFeature>())
			{
				Citizens[faction].Add(unit);
				OnCitizenAdded?.Invoke();
				if (faction == SystemsManager.MainPlayerFaction)
				{
					Main.Instance.GameManager.SystemsManager.NotificationSystem.SetVariable<int>("currentCitizenPopulation", Citizens[faction].Count);
					Main.Instance.GameManager.SystemsManager.NotificationSystem.IncreaseInteger("totalCitizenJoined");
				}
			}

			if (unit.HasFeature<LivingBeingFeature>())
				LivingBeings[faction].Add(unit.GetFeature<LivingBeingFeature>());


			if (unit.HasFeature<CattleFeature>())
			{
				var cattle = unit.GetFeature<CattleFeature>();
				Cattle[faction].Add(cattle);
				OnCattleAdded?.Invoke();
				if (faction == SystemsManager.MainPlayerFaction)
				{
					Main.Instance.GameManager.SystemsManager.ConstructionSystem.AddTech(ETechnologyType.Livestock);
					Main.Instance.GameManager.SystemsManager.NotificationSystem.SetVariable<int>("currentCattlePopulation", Cattle[faction].Count);
				}
			}
		}

		internal virtual void DeregisterUnit(BaseEntity unit, int faction = -1)
		{
			if (faction < 0) faction = unit.factionID;
			if (!Units[faction].Contains(unit))
			{
				throw new ArgumentException("Unit was not registered.");
			}
			Units[faction].Remove(unit);
			if (!unit.GetFeature<EntityFeature>().IsAlive)
			{
				RegisterCorpse(unit, faction);
			}
			else
			{
				UnitPool.Remove(unit.UID);
			}
			OnUnitRemoved?.Invoke();

			if (unit.HasFeature<CitizenAIAgentFeature>())
			{
				Citizens[faction].Remove(unit);
				OnCitizenRemoved?.Invoke();
				if (faction == SystemsManager.MainPlayerFaction)
					Main.Instance.GameManager.SystemsManager.NotificationSystem.SetVariable<int>("currentCitizenPopulation", Citizens[faction].Count);
			}
			if (unit.HasFeature<LivingBeingFeature>())
				LivingBeings[faction].Remove(unit.GetFeature<LivingBeingFeature>());

			if (unit.HasFeature<CattleFeature>())
			{
				var cattle = unit.GetFeature<CattleFeature>();
				Cattle[faction].Remove(cattle);
				OnCattleRemoved?.Invoke();
				if (faction == SystemsManager.MainPlayerFaction)
					Main.Instance.GameManager.SystemsManager.NotificationSystem.SetVariable<int>("currentCattlePopulation", Cattle[faction].Count);
			}
		}

		public void RegisterCorpse(BaseEntity unit, int faction = -1)
		{
			if (faction < 0)
				faction = unit.factionID;
			if (!DeadUnits[faction].Contains(unit))
			{
				DeadUnits[faction].Add(unit);
				OnUnitDied?.Invoke();
			}

		}

		public void DeregisterCorpse(BaseEntity unit, int faction)
		{
			if (DeadUnits[faction].Contains(unit))
			{
				DeadUnits[faction].Remove(unit);
				UnitPool.Remove(unit.UID);
				OnDeadUnitRemoved?.Invoke();
			}
		}

		public void RegisterThirstyUnit(BaseEntity unit)
		{
			if (unit.HasFeature<CitizenAIAgentFeature>())
			{
				if (!thirstyCitizens.Contains(unit))
				{
					thirstyCitizens.Add(unit);
					OnThirstyCitizenAdded?.Invoke();
				}
			}
			else if (unit.HasFeature<CattleFeature>())
			{
				if (!thirstyAnimals.Contains(unit))
				{
					thirstyAnimals.Add(unit);
					OnTroubledCattleAdded?.Invoke();
				}
			}

		}

		public void UnregisterThirstyUnit(BaseEntity unit)
		{
			if (unit.HasFeature<CitizenAIAgentFeature>())
			{
				if (thirstyCitizens.Contains(unit))
				{
					thirstyCitizens.Remove(unit);
					OnThirstyCitizenRemoved?.Invoke();
				}
			}
			else if (unit.HasFeature<CattleFeature>())
			{
				{
					if (thirstyAnimals.Contains(unit))
					{
						thirstyAnimals.Remove(unit);
						OnTroubledCattleRemoved?.Invoke();
					}
				}
			}
		}

		public void RegisterHungryUnit(BaseEntity unit)
		{
			if (unit.HasFeature<CitizenAIAgentFeature>())
			{
				if (!hungryCitizens.Contains(unit))
				{
					hungryCitizens.Add(unit);
					OnHungryCitizenAdded?.Invoke();
				}
			}
			else if (unit.HasFeature<CitizenAIAgentFeature>())
			{
				{
					if (!hungryAnimals.Contains(unit))
					{
						hungryAnimals.Add(unit);
						OnTroubledCattleAdded?.Invoke();
					}
				}
			}
		}

		public void UnregisterHungryUnit(BaseEntity unit)
		{
			if (unit.HasFeature<CitizenAIAgentFeature>())
			{
				if (hungryCitizens.Contains(unit))
				{
					hungryCitizens.Remove(unit);
					OnHungryCitizenRemoved?.Invoke();
				}
			}
			else if (unit.HasFeature<CitizenAIAgentFeature>())
			{
				{
					if (hungryAnimals.Contains(unit))
					{
						hungryAnimals.Remove(unit);
						OnTroubledCattleRemoved?.Invoke();
					}
				}
			}
		}

		public void OnFactionChanged(BaseEntity unit, int oldFaction)
		{
			if (!unit.GetFeature<EntityFeature>().IsAlive)
			{
				DeregisterCorpse(unit, oldFaction);
				RegisterCorpse(unit);
			}
			else
			{
				DeregisterUnit(unit, oldFaction);
				RegisterUnit(unit);
			}
		}

		public static void ResetDailyValues(LivingBeingFeature being)
		{
			being.nutritionConsumed = 0;
			being.waterConsumed = 0;
		}

        public void UpdateUnitAge()
        {
            for (int f = 0; f < LivingBeings.Count; f++)
            {
                for (int i = 0; i < LivingBeings[f].Count; i++)
                {
                    var being = LivingBeings[f][i];
                    being.AgeDayCounter++;
                    being.UpdateAge();

                    //Kill by Age
                    if (being.AgeDayCounter >= being.DeathDay)
                    {
                        entitySystem.KillEntity(being.Entity);
                    }
                        
                }
            }
        }

		public static void ConsumeItem(LivingBeingFeature being, ConsumableFeature item)
		{
			float nutrition = item.StaticData.Nutrition;
			float water = item.StaticData.Water;

			being.nutritionConsumed += nutrition;
			being.waterConsumed += water;
			being.Hunger.Value += nutrition;
			being.Thirst.Value += water;
		}
		
		public static void ConsumeItems(LivingBeingFeature being, ConsumableFeature[] items)
		{
			float nutrition = 0;
			float water = 0;

			for (int i = 0; i < items.Length; i++)
			{
				nutrition += items[i].StaticData.Nutrition;
				water += items[i].StaticData.Water;
			}

			being.nutritionConsumed += nutrition;
			being.waterConsumed += water;
			being.Hunger.Value += nutrition;
			being.Thirst.Value += water;
		}

		/// <summary>
		/// Make sure it is removed from another inventory first
		/// </summary>
		public static bool EquipItem(BaseEntity unit, ItemFeature item)
		{
			if (InventorySystem.GetChamberForItem(unit.Inventory, item) == InventorySystem.ChamberMainID)
			{
				InventorySystem.TransferItemsToChamber(unit.Inventory, new List<ItemFeature>() { item }, InventorySystem.ChamberMainID, unit.GetFeature<UnitFeature>().equippedChamberID);
				return true;
			}
			else
			{
				return InventorySystem.AddItem(unit.Inventory, item, false, unit.GetFeature<UnitFeature>().equippedChamberID);
			}
		}

		public override void UpdateGameLoop()
		{
			if (SimulateLivingBeings) UpdateLivingBeings();
			UpdateCattle();
			UpdateCitizens();

			UpdateStatistics();
		}

		private void UpdateCitizens()
		{
			for (int f = 0; f < Citizens.Count; f++)
			{
				for (int i = 0; i < Citizens[f].Count; i++)
				{
					var agent = Citizens[f][i].GetFeature<CitizenAIAgentFeature>();
					var unit = Citizens[f][i].GetFeature<EntityFeature>();
					unit.Stress.Value += unit.currentStressLevel * dayTickFactor;
				}
			}
		}

		public static void ConsumeFood(LivingBeingFeature feature, float nutrition, float water)
		{
			feature.Hunger.Value += nutrition;
			feature.Thirst.Value += water;
		}

		public override void UpdateStatistics()
		{
			GameStatistics.MainTownStatistics.TotalUnitsDead = 0;
			for (int f = 0; f < DeadUnits.Count; f++)
			{
				if (f == SystemsManager.MainPlayerFaction)
					GameStatistics.MainTownStatistics.TotalUnitsDead += DeadUnits[f].Count;
			}
		}

		private void UpdateLivingBeings()
		{
			float consumption;
			for (int f = 0; f < LivingBeings.Count; f++)
			{
				for (int i = 0; i < LivingBeings[f].Count; i++)
				{
					var being = LivingBeings[f][i];
					var agent = being.Entity.GetFirstAIFeature();
                    consumption = Mathf.Lerp(1f, being.Hunger.Value * config.hungerConsumptionCenter, config.hungerConsumptionBalance) * being.HungerConsumption;
					being.Hunger.Value -= consumption * dayTickFactor * GameConfig.Instance.DebugModifers.UnitHungerConsumption;
					consumption = Mathf.Lerp(1f, being.Thirst.Value * config.thirstConsumptionCenter, config.thirstConsumptionBalance) * being.ThirstConsumption;
					being.Thirst.Value -= consumption * dayTickFactor * GameConfig.Instance.DebugModifers.UnitThirstConsumption;

					//Calculate vitality
					var entityFeature = being.Entity.GetFeature<EntityFeature>();
					being.vitality.Value = CivMath.Min(new float[]{
						entityFeature.Health.Progress,
						1-entityFeature.Stress.Progress,
						being.Hunger.Progress,
						being.Thirst.Progress
					});

					//Update condition
					var vitality = being.vitality.Mood.NegativeToZero();
					var gridAgent = being.Entity.GetFeature<GridAgentFeature>();
					if (gridAgent != null)
					{
						gridAgent.speedModifer = Mathf.Lerp(0.1f, 1f, vitality);
					}

					//Kill by needs
					if (being.Thirst.Value <= 0 || being.Hunger.Value <= 0)
                    {
                        entitySystem.KillEntity(agent.Entity);
                    }
						
				}
			}
		}

		private void UpdateCattle()
		{
			for (int f = 0; f < Cattle.Count; f++)
			{
				for (int i = 0; i < Cattle[f].Count; i++)
				{
					var cattle = Cattle[f][i];
				}
			}
		}
	}
}