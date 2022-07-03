using System.Linq;
using System.Collections.Generic;

namespace Endciv
{
	/// <summary>
	/// Manages Utilities
	/// </summary>
	public class UtilitySystem : EntityFeatureSystem<UtilityFeature>
	{
		private const float MAX_HOMEUTILITY_DISTANCE = 10f;

		private List<List<UtilityFeature>> Toilets;
		private List<List<UtilityFeature>> Showers;

		public UtilitySystem(int factions) : base(factions)
		{
			Toilets = new List<List<UtilityFeature>>(factions);
			Toilets.Add(new List<UtilityFeature>(32));

			Showers = new List<List<UtilityFeature>>(factions);
			Showers.Add(new List<UtilityFeature>(32));

			for (int i = 1; i < factions; i++)
			{
				Toilets.Add(new List<UtilityFeature>(8));
				Showers.Add(new List<UtilityFeature>(8));
			}
		}

		internal override void RegisterFeature(UtilityFeature feature)
		{
			base.RegisterFeature(feature);
			switch (feature.StaticData.type)
			{
				case EUtilityType.Shower:
					if (!Showers[feature.Entity.factionID].Contains(feature))
						Showers[feature.Entity.factionID].Add(feature);
					break;

				case EUtilityType.Toilet:
					if (!Toilets[feature.Entity.factionID].Contains(feature))
						Toilets[feature.Entity.factionID].Add(feature);
					break;
			}
		}

		internal override void DeregisterFeature(UtilityFeature feature, int faction = -1)
		{
			base.DeregisterFeature(feature, faction);
			if (faction < 0) faction = feature.FactionID;
			switch (feature.StaticData.type)
			{
				case EUtilityType.Shower:
					Showers[faction].Remove(feature);
					break;

				case EUtilityType.Toilet:
					Toilets[faction].Remove(feature);
					break;
			}
		}

		public BaseEntity GetBestToiletForAgent(BaseEntity unit)
		{
			List<BaseEntity> availableUtilities = new List<BaseEntity>();
			BaseEntity unitHome = null;
			foreach (var utility in Toilets[unit.factionID])
			{
				if (utility.Entity.GetFeature<InventoryFeature>().LoadProgress > 0.9f)
					continue;
				if (!utility.Entity.HasFeature<HousingFeature>())
				{
					if (utility.Occupants.Count >= utility.MaxOccupants)
						continue;
					availableUtilities.Add(utility.Entity);
					continue;
				}
				if (utility.Entity.GetFeature<HousingFeature>().Occupants.Contains(unit))
				{
					unitHome = utility.Entity;
					continue;
				}
			}
			return GetClosestUtility(availableUtilities, unitHome, unit);
		}

		public BaseEntity GetBestShowerForAgent(BaseEntity unit)
		{
			List<BaseEntity> availableUtilities = new List<BaseEntity>();
			BaseEntity unitHome = null;
			foreach (var utility in Showers[unit.factionID])
			{
				if (!utility.Entity.HasFeature<HousingFeature>())
				{
					if (utility.Occupants.Count >= utility.MaxOccupants)
						continue;
					availableUtilities.Add(utility.Entity);
					continue;
				}
				if (utility.Entity.GetFeature<HousingFeature>().Occupants.Contains(unit))
				{
					unitHome = utility.Entity;
					continue;
				}
			}
			return GetClosestUtility(availableUtilities, unitHome, unit);
		}

		public BaseEntity GetClosestUtility(List<BaseEntity> utilities, BaseEntity unitHome, BaseEntity unit)
		{
			var sortedUtilities = utilities.OrderBy(x => Vector2i.Distance(x.GetFeature<EntityFeature>().GridID, unit.GetFeature<EntityFeature>().GridID)).ToArray();
			if (unitHome != null)
			{
				if (sortedUtilities.Length <= 0)
					return unitHome;
				var homeDist = Vector2i.Distance(unitHome.GetFeature<EntityFeature>().GridID, unit.GetFeature<EntityFeature>().GridID);
				if (homeDist <= MAX_HOMEUTILITY_DISTANCE)
				{
					return unitHome;
				}
				var closestDist = Vector2i.Distance(sortedUtilities[0].GetFeature<EntityFeature>().GridID, unit.GetFeature<EntityFeature>().GridID);
				if (closestDist < homeDist)
					return sortedUtilities[0];
				else
					return unitHome;
			}
			if (sortedUtilities.Length > 0)
			{
				return sortedUtilities[0];
			}
			return null;
		}

		public override void UpdateGameLoop()
		{
			for (int f = 0; f < Toilets.Count; f++)
			{
				var c = Toilets[f].Count;
				for (int i = 0; i < c; i++)
				{
					var toilet = Toilets[f][i];
					if (!toilet.Entity.HasFeature<PollutionFeature>())
						continue;
					var inv = toilet.Entity.GetFeature<InventoryFeature>();
					var pollutionFeature = toilet.Entity.GetFeature<PollutionFeature>();
					pollutionFeature.pollution = inv.LoadProgress;
				}
			}
		}

		public override void UpdateStatistics()
		{
		}
	}
}