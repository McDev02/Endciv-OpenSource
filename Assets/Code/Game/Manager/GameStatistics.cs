using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Endciv
{
	//Make non static
	public static class GameStatistics
	{
		public static TownStatistics MainTownStatistics = new TownStatistics();
		public static InventoryStatistics InventoryStatistics;
	}

	[System.Serializable]
	public abstract class StatisticsBase<T> where T : StatisticsBase<T>
	{
		public abstract bool Compare(T otherStatistics);


		protected bool compareStats(StringBuilder sb, string title, float newVal, float oldVal)
		{
			bool match = UnityEngine.Mathf.Approximately(newVal, oldVal);
			if (!match)
			{
				sb.Append($"{title} : {newVal } / { oldVal } (New / Old)");
				sb.AppendLine(" Missmatch!");
			}
			else
				sb.AppendLine($"{title} : {newVal } / { oldVal } (New / Old)");
			return match;
		}
	}

	[System.Serializable]
	public class TownStatistics : StatisticsBase<TownStatistics>, ISaveable
	{
		//People
		public int TotalPeople;
		public int TotalPeopleHomeless;
		public int TotalPeopleHoused;
		public int TotalUnitsDead;
		//Needs
		public float averageNeedMood;
		public float averageNeedHealth;
		public float averageNeedHunger;
		public float averageNeedThirst;
		public float averageNeedResting;
		public float averageNeedStress;
		public float averageNeedSettlement;
		public float averageNeedToilet;
		public float averageNeedCleaning;

		//Buildings
		public int TotalHomes;
		public int TotalHomeSpace;
		public int TotalHomeSpaceLeft;

		//Electricity
		public float TotalElectricityProduction;
		public float TotalElectricityConsumption;
		public float TotalElectricityBalance;
		public float TotalElectricityCapacity;
		public float TotalElectricityStored;

		public override bool Compare(TownStatistics stats)
		{
			bool match = true;
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("TownStatistics Comparison:");
			if (!compareStats(sb, "TotalPeople", TotalPeople, stats.TotalPeople)) match = false;
			if (!compareStats(sb, "TotalPeopleHomeless", TotalPeopleHomeless, stats.TotalPeopleHomeless)) match = false;
			if (!compareStats(sb, "TotalPeopleHoused", TotalPeopleHoused, stats.TotalPeopleHoused)) match = false;
			if (!compareStats(sb, "TotalUnitsDead", TotalUnitsDead, stats.TotalUnitsDead)) match = false;
			if (!compareStats(sb, "TotalHomes", TotalHomes, stats.TotalHomes)) match = false;
			if (!compareStats(sb, "TotalHomeSpace", TotalHomeSpace, stats.TotalHomeSpace)) match = false;
			if (!compareStats(sb, "TotalHomeSpaceLeft", TotalHomeSpaceLeft, stats.TotalHomeSpaceLeft)) match = false;
			UnityEngine.Debug.Log(sb.ToString());
			return match;
		}


		public ISaveable CollectData()
		{
			return this;
		}
	}

	[System.Serializable]
	public class InventoryStatistics : StatisticsBase<InventoryStatistics>, ISaveable
	{
		[System.NonSerialized]
		public Dictionary<string, int> Foods;
		[System.NonSerialized]
		public Dictionary<string, int> Items;
		[System.NonSerialized]
		public Dictionary<string, int> Tools;
		[System.NonSerialized]
		public Dictionary<string, int> Weapons;

		public int TotalItems;
		public int TotalFood;
		public int TotalTools;
		public int TotalWeapons;
		public int TotalWaste;
		public int TotalWasteOrganic;

		//Food
		public float Nutrition;
		public float ConsumableNutritionAvailable;
		public float NutritionAvailable;
		public int Water;
		public float ConsumableWater;
		public float ConsumableWaterAvailable;
		public int WaterAvailable;
		//public float Compost;

		public ISaveable CollectData()
		{
			return this;
		}

		public override bool Compare(InventoryStatistics stats)
		{
			bool match = true;
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("InventoryStatistics Comparison:");
			if (!compareStats(sb, "TotalTools", TotalTools, stats.TotalTools)) match = false;
			if (!compareStats(sb, "TotalItems", TotalItems, stats.TotalItems)) match = false;
			if (!compareStats(sb, "TotalFood", TotalFood, stats.TotalFood)) match = false;
			if (!compareStats(sb, "TotalWaste", TotalWaste, stats.TotalWaste)) match = false;
			if (!compareStats(sb, "TotalWeapons", TotalWeapons, stats.TotalWeapons)) match = false;
			if (!compareStats(sb, "Nutrition", Nutrition, stats.Nutrition)) match = false;
			if (!compareStats(sb, "NutritionAvailable", NutritionAvailable, stats.NutritionAvailable)) match = false;
			if (!compareStats(sb, "Water", Water, stats.Water)) match = false;
			if (!compareStats(sb, "WaterAvailable", WaterAvailable, stats.WaterAvailable)) match = false;
			UnityEngine.Debug.Log(sb.ToString());
			return match;
		}

		public InventoryStatistics()
		{
			TotalTools = 0;
			TotalWeapons = 0;
			TotalFood = 0;
			TotalItems = 0;
			TotalWaste = 0;

			Nutrition = 0;
			ConsumableNutritionAvailable = 0;
			NutritionAvailable = 0;
			Water = 0;
			ConsumableWater = 0;
			ConsumableWaterAvailable = 0;
			WaterAvailable = 0;
		}

		internal void Clear()
		{
			TotalFood = 0;
			TotalItems = 0;
			TotalWaste = 0;
			TotalTools = 0;
			TotalWeapons = 0;

			Nutrition = 0;
			ConsumableNutritionAvailable = 0;
			NutritionAvailable = 0;
			Water = 0;
			ConsumableWater = 0;
			ConsumableWaterAvailable = 0;
			WaterAvailable = 0;

			if (Tools != null)
			{
				var keys = Tools.Keys.ToArray();

				foreach (var key in keys)
				{
					Tools[key] = 0;
				}
			}

			if (Weapons != null)
			{
				var keys = Weapons.Keys.ToArray();

				foreach (var key in keys)
				{
					Weapons[key] = 0;
				}
			}

			if (Foods != null)
			{
				var keys = Foods.Keys.ToArray();

				foreach (var key in keys)
				{
					Foods[key] = 0;
				}
			}
			if (Items != null)
			{
				var keys = Items.Keys.ToArray();

				foreach (var key in keys)
				{
					Items[key] = 0;
				}
			}
		}

		public static InventoryStatistics operator -(InventoryStatistics left, InventoryStatistics right)
		{
			var entityFactory = Main.Instance.GameManager.Factories.SimpleEntityFactory;
			left.TotalTools -= right.TotalTools;
			left.TotalWeapons -= right.TotalWeapons;
			left.TotalFood -= right.TotalFood;
			left.TotalItems -= right.TotalItems;
			left.TotalWaste -= right.TotalWaste;

			//Food
			left.Nutrition -= right.Nutrition;
			left.NutritionAvailable -= right.NutritionAvailable;
			left.ConsumableNutritionAvailable -= right.ConsumableNutritionAvailable;
			left.Water -= right.Water;
			left.ConsumableWater -= right.ConsumableWater;
			left.ConsumableWaterAvailable -= right.ConsumableWaterAvailable;
			left.WaterAvailable -= right.WaterAvailable;

			string key;
			var ids = entityFactory.GetStaticDataIDList<ToolFeatureStaticData>();
			foreach (var id in ids)
			{
				try
				{
					left.Tools[id] -= right.Tools[id];
				}
				catch (System.Exception ex)
				{
					UnityEngine.Debug.LogError(ex.ToString());
				}
			}
			ids = entityFactory.GetStaticDataIDList<WeaponFeatureStaticData>();
			foreach (var id in ids)
			{
				try
				{
					left.Weapons[id] -= right.Weapons[id];
				}
				catch (System.Exception ex)
				{
					UnityEngine.Debug.LogError(ex.ToString());
				}
			}
			ids = entityFactory.GetStaticDataIDList<ConsumableFeatureStaticData>();
			foreach (var id in ids)
			{
				left.Foods[id] -= right.Foods[id];
			}
			ids = entityFactory.GetStaticDataIDList<ItemFeatureStaticData>();
			foreach (var id in ids)
			{
				left.Items[id] -= right.Items[id];
			}
			return left;
		}

		public static InventoryStatistics operator +(InventoryStatistics left, InventoryStatistics right)
		{
			var entityFactory = Main.Instance.GameManager.Factories.SimpleEntityFactory;
			left.TotalTools += right.TotalTools;
			left.TotalWeapons += right.TotalWeapons;
			left.TotalFood += right.TotalFood;
			left.TotalItems += right.TotalItems;
			left.TotalWaste += right.TotalWaste;

			//Food
			left.Nutrition += right.Nutrition;
			left.ConsumableNutritionAvailable += right.ConsumableNutritionAvailable;
			left.NutritionAvailable += right.NutritionAvailable;
			left.Water += right.Water;
			left.ConsumableWater += right.ConsumableWater;
			left.ConsumableWaterAvailable += right.ConsumableWaterAvailable;
			left.WaterAvailable += right.WaterAvailable;

			string key;
			var ids = entityFactory.GetStaticDataIDList<ToolFeatureStaticData>();
			foreach (var id in ids)
			{
				try
				{
					left.Tools[id] += right.Tools[id];
				}
				catch (System.Exception ex)
				{
					UnityEngine.Debug.LogError(ex.ToString());
				}
			}

			ids = entityFactory.GetStaticDataIDList<WeaponFeatureStaticData>();
			foreach (var id in ids)
			{
				try
				{
					left.Weapons[id] += right.Weapons[id];
				}
				catch (System.Exception ex)
				{
					UnityEngine.Debug.LogError(ex.ToString());
				}
			}

			ids = entityFactory.GetStaticDataIDList<ConsumableFeatureStaticData>();
			foreach (var id in ids)
			{
				left.Foods[id] += right.Foods[id];
			}

			ids = entityFactory.GetStaticDataIDList<ItemFeatureStaticData>();
			foreach (var id in ids)
			{
				left.Items[id] += right.Items[id];
			}
			return left;
		}

		public int GetItemCount(string id)
		{
			if (Items.ContainsKey(id))
				return Items[id];
			return 0;
		}
	}
}