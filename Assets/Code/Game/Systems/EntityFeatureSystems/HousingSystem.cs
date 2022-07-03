using System.Collections.Generic;
using System.Linq;
using System;

namespace Endciv
{
	//TODO: Move to UnitSystem?
	public class HousingSystem : EntityFeatureSystem<HousingFeature>
	{
		public InventoryStatistics HouseStatistics { get; set; }
		EntitySystem EntitySystem;

		const int UpdateAllLoopCount = 20;

		List<List<BaseEntity>> HomelessUnits;

		public Action OnHomelessCountChanged;

		public int HomelessUnitCount { get { return HomelessUnits[SystemsManager.MainPlayerFaction].Count; } }

		int updateAllTimer;

		public BaseEntity[] GetHomelessUnits(int factionID)
		{
			return HomelessUnits[factionID].ToArray();
		}

		public HousingSystem(int factions, EntitySystem entitySystem, TimeManager timeManager) : base(factions)
		{
			timeManager.OnDayChanged += OnDayChanged;
			HomelessUnits = new List<List<BaseEntity>>(factions);
			for (int i = 0; i < factions; i++)
			{
				HomelessUnits.Add(new List<BaseEntity>(32));
			}
			HouseStatistics = InventorySystem.GetNewInventoryStatistics();
			EntitySystem = entitySystem;
			UpdateStatistics();
		}

		private void OnDayChanged()
		{
			for (int i = 0; i < FeaturesCombined.Count; i++)
			{
				var house = FeaturesCombined[i];
				house.HasRestocked = false;
			}
		}

		internal override void DeregisterFeature(HousingFeature feature, int faction = -1)
		{
			base.DeregisterFeature(feature, faction);
			if (feature.Occupants != null && feature.Occupants.Count > 0)
			{
				foreach (var occupant in feature.Occupants)
				{
					if (!occupant.HasFeature<UnitFeature>())
						continue;
					var unitFeature = occupant.GetFeature<CitizenAIAgentFeature>();
					unitFeature.Home = null;
					RegisterUnit(occupant);
				}
				feature.Occupants.Clear();
			}
		}

		public void RegisterUnit(BaseEntity entity)
		{
			if (!entity.HasFeature<UnitFeature>())
				return;
			var unitFeature = entity.GetFeature<CitizenAIAgentFeature>();
			if (!unitFeature.HasHome)
			{
				HomelessUnits[entity.factionID].Add(entity);
				if (entity.factionID == SystemsManager.MainPlayerFaction)
					OnHomelessCountChanged?.Invoke();
			}
			UpdateStatistics();
		}

		public void DeregisterUnit(BaseEntity unit, int factionID = -1)
		{
			if (factionID == -1)
				factionID = unit.factionID;
			if (HomelessUnits[factionID].Contains(unit))
			{
				HomelessUnits[factionID].Remove(unit);
				if (factionID == SystemsManager.MainPlayerFaction)
					OnHomelessCountChanged?.Invoke();
			}
			UpdateStatistics();
		}

		public override void UpdateGameLoop()
		{
			//reimplement later
			if (false)
			{
				updateAllTimer++;
				if (updateAllTimer >= UpdateAllLoopCount)
				{
					UpdateAll();
					updateAllTimer = 0;
				}
			}
			for (int f = 0; f < HomelessUnits.Count; f++)
			{
				if (HomelessUnits[f].Count > 0)
				{
					int count = HomelessUnits[f].Count;
					for (int i = count - 1; i >= 0; i--)
					{
						var unit = HomelessUnits[f][i];
						var unitFeature = unit.GetFeature<CitizenAIAgentFeature>();
						if (unitFeature.HasHome)
						{
							HomelessUnits[f].RemoveAt(i);
						}
						else if (FindHome(unit))
						{
							if (unit.factionID == SystemsManager.MainPlayerFaction)
								OnHomelessCountChanged?.Invoke();
						}
					}
				}
			}
			//Update house quality
			for (int i = 0; i < FeaturesCombined.Count; i++)
			{
				var house = FeaturesCombined[i];
				var condition = house.Entity.GetFeature<EntityFeature>().Health.Progress;
				//TODO: Calculate crowdedness. Do people live with strangers or family? Happyness per occupant

				house.quality = condition * house.StaticData.Quality;
			}

			UpdateStatistics();
		}

		bool FindHome(BaseEntity unit)
		{
			for (int i = 0; i < FeaturesByFaction[unit.factionID].Count; i++)
			{
				var house = FeaturesByFaction[unit.factionID][i];
				if (house.HasFreeSpace)
				{
					RegisterHome(unit, house);
					return true;
				}
			}
			return false;
		}

		public void RegisterHome(BaseEntity unit, HousingFeature house)
		{
			HomelessUnits[unit.factionID].Remove(unit);
			unit.GetFeature<CitizenAIAgentFeature>().Home = house;
			house.Occupants.Add(unit);
		}

		public void DeregisterHome(BaseEntity unit)
		{
			var unitFeature = unit.GetFeature<CitizenAIAgentFeature>();
			if (unitFeature.Home != null && unitFeature.Home.Occupants.Contains(unit))
				unitFeature.Home.Occupants.Remove(unit);
			unitFeature.Home = null;
			if (!HomelessUnits[unit.factionID].Contains(unit))
				HomelessUnits[unit.factionID].Add(unit);
		}

		//Todo: Update to use to find better home
		void UpdateAll()
		{
			var units = Main.Instance.GameManager.SystemsManager.UnitSystem.Citizens;
			for (int f = 0; f < units.Count; f++)
			{
				int count = units[f].Count;
				for (int i = 0; i < count; i++)
				{
					FindHome(units[f][i]);
				}
			}
		}

		public override void UpdateStatistics()
		{
			int space = 0;
			int occupied = 0;
			HouseStatistics.Clear();
			for (int i = 0; i < FeaturesByFaction[SystemsManager.MainPlayerFaction].Count; i++)
			{
				var house = FeaturesByFaction[SystemsManager.MainPlayerFaction][i];
				space += house.MaxOccupants;
				occupied += house.CurrentOccupants;
				InventorySystem.UpdateInventoryStatistics(house.Entity.Inventory, HouseStatistics);
			}
			GameStatistics.MainTownStatistics.TotalHomeSpace = space;
			GameStatistics.MainTownStatistics.TotalHomeSpaceLeft = space - occupied;

			GameStatistics.MainTownStatistics.TotalPeople = Main.Instance.GameManager.SystemsManager.UnitSystem.Citizens[SystemsManager.MainPlayerFaction].Count;
			GameStatistics.MainTownStatistics.TotalPeopleHoused = occupied;
			GameStatistics.MainTownStatistics.TotalPeopleHomeless = HomelessUnits[SystemsManager.MainPlayerFaction].Count;

			Main.Instance.GameManager.SystemsManager.NotificationSystem.SetVariable<int>("totalHomeSpace", GameStatistics.MainTownStatistics.TotalHomeSpace);
		}
	}
}