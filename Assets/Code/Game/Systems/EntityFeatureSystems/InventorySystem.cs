using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;
using System.Linq;

namespace Endciv
{
	/// <summary>
	/// Controlls Inventories of Resources, Items and Food
	/// </summary>

	public class InventorySystem : EntityFeatureSystem<InventoryFeature>
	{
		public const int ChamberMainID = 0;
		public const int ChamberReservedID = 1;
		public const string ChamberMainName = "main";
		public const string ChamberReservedName = "reserved";
		public const float maxPileDistance = 20f;
		readonly float WasteMass;

		static SimpleEntityFactory entityFactory;

		public InventorySystem(int factions, FactoryManager factoryManager) : base(factions)
		{
			entityFactory = factoryManager.SimpleEntityFactory;         
			if (!entityFactory.EntityStaticData.ContainsKey(FactoryConstants.WasteID))
				Debug.LogError("Fatal error, waste resource not defined.");
			else
				WasteMass = entityFactory.GetStaticData<ItemFeatureStaticData>(FactoryConstants.WasteID).Mass;
		}

		public override void UpdateStatistics()
		{
		}

		public override void UpdateGameLoop()
		{
			//watch.Reset();
			//watch.Start();
			//Debug.Log("Update InventorySystem Loop - Features: " + Features.Count);
			for (int f = 0; f < FeaturesByFaction.Count; f++)
			{
				for (int i = 0; i < FeaturesByFaction[f].Count; i++)
				{
					var inventory = FeaturesByFaction[f][i];

					//Food decay
					var chambers = inventory.TotalChambers;
					for (int c = 0; c < chambers; c++)
					{
						var keys = inventory.ItemPoolByChambers[c].Keys.ToList();
						foreach (var key in keys)
						{
							var items = inventory.ItemPoolByChambers[c][key];
							foreach (var item in items)
							{
								if (!item.Entity.HasFeature<DurabilityFeature>())
									continue;
								var food = item.Entity.GetFeature<DurabilityFeature>();
								food.AddDurability(-GameConfig.Instance.FoodDecay);
								//Food lost all durability, remove from inventory and add waste in its place in main chamber
								if (food.Durability <= 0)
								{
									RemoveItem(inventory, item, c);
									var waste = entityFactory.CreateInstance(FactoryConstants.wasteOrganicID).GetFeature<ItemFeature>();
									waste.Quantity = 1;
									AddItem(inventory, waste, true, ChamberMainID);
									break;
								}
							}
						}
					}
				}
			}
			//watch.LogRoundMilliseconds("InventorySystem");

		}

		#region Chamber Methods   
		public static int GetChamberForItem(InventoryFeature inventory, ItemFeature item)
		{
			string id = item.Entity.StaticData.ID;
			for (int i = 0; i < inventory.Chambers.Count; i++)
			{
				if (!HasItems(inventory, id, 1, i))
					continue;
				if (inventory.ItemPoolByChambers[i][id].Contains(item))
					return i;
			}
			return -1;
		}

		public static ResourceStack[] GetChamberContentList(InventoryFeature inventory, int chamberID)
		{
			if (!ChamberExists(inventory, chamberID))
				return null;
			var dict = new Dictionary<string, int>();
			var pool = inventory.ItemPoolByChambers[chamberID];
			var keys = pool.Keys.ToList();
			foreach (var key in keys)
			{
				if (pool[key].Count <= 0)
					continue;
				int count = 0;
				if (pool[key][0].StaticData.IsStackable)
				{
					count = pool[key][0].Quantity;
				}
				else
				{
					count = pool[key].Count;
				}
				dict.Add(key, count);

			}
			return dict.ToResourceStackArray();
		}

		public static bool ChamberExists(InventoryFeature inventory, int id)
		{
			return id >= 0 && id < inventory.TotalChambers;
		}

		public static bool ChamberExists(InventoryFeature inventory, string chamberName)
		{
			return inventory.Chambers.Contains(chamberName);
		}

		public static void GenerateChamberIfNoneExists(InventoryFeature feature, int chamberID)
		{
			if (chamberID >= 0 && chamberID < feature.Chambers.Count)
				return;
			string chamberName = string.Empty;
			while (chamberName == string.Empty)
			{
				string key = UnityEngine.Random.Range(0, 1000).ToString() + DateTime.Now.ToString() + UnityEngine.Random.Range(0, 1000).ToString();
				if (feature.Chambers.Contains(key))
				{
					chamberName = string.Empty;
				}
				else
				{
					chamberName = key;
				}
			}
			AddChamber(feature, chamberName);
			feature.NotifyInventoryChanged();
		}

		public static int AddChamber(InventoryFeature feature, string chamberReservedName)
		{
			if (feature.Chambers.Contains(chamberReservedName))
			{
				return feature.Chambers.IndexOf(chamberReservedName);
			}
			feature.Chambers.Add(chamberReservedName);
			feature.TotalChambers++;

			feature.ItemPoolByChambers.Add(new Dictionary<string, List<ItemFeature>>());
			feature.NotifyInventoryChanged();
			return feature.Chambers.Count - 1;
		}

		public static void RemoveChamber(InventoryFeature feature, int id)
		{
			if (id < 0 || id > feature.Chambers.Count - 1)
			{
				Debug.Log("Chamber id " + id + " not registered.");
				return;
			}

			EmptyChamber(feature, id);

			//Removing Chamber
			feature.Chambers.RemoveAt(id);
			feature.ItemPoolByChambers.RemoveAt(id);
			feature.TotalChambers--;
			feature.NotifyInventoryChanged();
		}

		/// <summary>
		/// Clears a chamber and moves the resources ot hte main chamber instead
		/// </summary>
		public static void EmptyChamber(InventoryFeature feature, int id)
		{
			if (id < 1 || id > feature.Chambers.Count - 1)
			{
				Debug.Log("Chamber id " + id + " not registered or is main chamber.");
				return;
			}

			bool changed = false;
			var mainChamber = feature.ItemPoolByChambers[ChamberMainID];
			var oldChamber = feature.ItemPoolByChambers[id];
			var keys = oldChamber.Keys.ToList();
			foreach (var key in keys)
			{
				if (oldChamber[key][0].StaticData.IsStackable)
				{
					if (mainChamber.ContainsKey(key))
					{
						mainChamber[key][0].Quantity += oldChamber[key][0].Quantity;
					}
					else
					{
						mainChamber.Add(key, new List<ItemFeature>() { oldChamber[key][0] });
					}
				}
				else
				{
					if (mainChamber.ContainsKey(key))
					{
						mainChamber[key].AddRange(oldChamber[key]);
					}
					else
					{
						mainChamber.Add(key, oldChamber[key]);
					}
				}
				oldChamber.Remove(key);
				changed = true;
			}

			if (changed)
				feature.NotifyInventoryChanged();
		}

		public static void UnreserveInventory(InventoryFeature inventory)
		{
			EmptyChamber(inventory, ChamberReservedID);
#if _DEBUG
			CheckLoadForFailure(inventory);
#endif
		}
		#endregion

		private static void ChangeInventoryStatistics(InventoryFeature inventory, ItemFeatureStaticData data, int amount)
		{
			inventory.AddLoad(amount * data.Mass);
			var id = data.entity.ID;
			if (id == FactoryConstants.WaterID)
			{
				inventory.IsDirtyWater = true;

			}
			inventory.IsDirtyItems = true;
			inventory.TotalItems += amount;
			if (id == FactoryConstants.WasteID)
			{
				inventory.TotalWaste += amount;
			}
			if (id == FactoryConstants.wasteOrganicID)
			{
				inventory.TotalWasteOrganic += amount;
			}
			if (data.entity.HasFeature(typeof(ConsumableFeatureStaticData)))
			{
				inventory.IsDirtyFood = true;
				inventory.TotalFood += amount;
			}
			if (data.entity.HasFeature(typeof(ToolFeatureStaticData)))
			{
				inventory.IsDirtyTools = true;
				inventory.TotalTools += amount;
			}
			if (data.entity.HasFeature(typeof(WeaponFeatureStaticData)))
			{
				inventory.IsDirtyWeapons = true;
				inventory.TotalWeapons += amount;
			}
			inventory.NotifyInventoryChanged();
#if _DEBUG
			CheckLoadForFailure(inventory);
#endif
		}

#if _DEBUG
		public static void CheckLoadForFailure(InventoryFeature inventory)
		{
			int load = inventory.Load;

			int newload = 0;
			for (int j = 0; j < inventory.TotalChambers; j++)
			{
				var pool = inventory.ItemPool[j];
				var keys = pool.Keys.ToList();
				foreach(var key in keys)
				{
					var item = pool[key][0];
					if(item.StaticData.IsStackable)
					{
						newload += item.StaticData.Mass * item.Quantity;
					}
					else
					{
						newload += item.StaticData.Mass * pool[key].Count;
					}
				}				
			}
			//inventory.SetLoad(newload);

			if (load != newload)
				Debug.LogError($"Load is not consistent: {load.ToString()} / {newload.ToString()}");
			//else
			//	Debug.Log("Load is fine!");
		}
#endif

		#region General Methods
		public static void RecalculateLoad(InventoryFeature inventory)
		{
			int load = 0;
			var chambers = inventory.TotalChambers;
			for (int c = 0; c < chambers; c++)
			{
				var pool = inventory.ItemPoolByChambers[c];
				var keys = pool.Keys.ToList();
				foreach (var key in keys)
				{
					var item = pool[key][0];
					if (item.StaticData.IsStackable)
					{
						load += item.StaticData.Mass * item.Quantity;
					}
					else
					{
						load += item.StaticData.Mass * pool[key].Count;
					}
				}
			}
			inventory.SetLoad(load);
		}

		/// <summary>
		/// Returns the maximum amount that can be added based on capacity and mass
		/// </summary>
		private static int GetAddableAmount(float capacity, float mass)
		{
			return Mathf.FloorToInt(capacity / mass);
		}

		/// <summary>
		/// Returns the maximum amount that can be added of a Resource
		/// </summary>
		public static int GetAddableAmount(InventoryFeature inventory, string resourceID, int maxAmount = int.MaxValue)
		{
			var res = entityFactory.GetStaticData<ItemFeatureStaticData>(resourceID);
			return Mathf.Min(maxAmount, GetAddableAmount(inventory.CapacityLeft, res.Mass));
		}
		#endregion

		#region Item Methods
		public static bool CanAddItems(InventoryFeature inventory, string itemID, int amount)
		{
			var data = entityFactory.GetStaticData<ItemFeatureStaticData>(itemID);
			return inventory.CapacityLeft >= data.Mass * amount;
		}

		public static bool HasItems(InventoryFeature inventory, string itemID, int amount = 1, int chamberID = ChamberMainID)
		{
			if (chamberID < 0 || chamberID >= inventory.Chambers.Count)
				return false;
			if (!inventory.ItemPoolByChambers[chamberID].ContainsKey(itemID))
				return false;
			var item = entityFactory.GetStaticData<ItemFeatureStaticData>(itemID);
			if (item.IsStackable)
			{
				return inventory.ItemPoolByChambers[chamberID][itemID][0].Quantity >= amount;
			}
			else
			{
				return inventory.ItemPoolByChambers[chamberID][itemID].Count >= amount;
			}
		}

		public static int GetItemCount(InventoryFeature inventory, string itemID, int chamberID = ChamberMainID)
		{
			if (chamberID < 0 || chamberID >= inventory.Chambers.Count)
				return 0;
			if (!inventory.ItemPoolByChambers[chamberID].ContainsKey(itemID))
				return 0;
			var data = entityFactory.GetStaticData<ItemFeatureStaticData>(itemID);
			if (data.IsStackable)
			{
				return inventory.ItemPoolByChambers[chamberID][itemID][0].Quantity;
			}
			else
			{
				return inventory.ItemPoolByChambers[chamberID][itemID].Count;
			}
		}

		public static List<ItemFeature> WithdrawItems(InventoryFeature inventory, string itemID, int quantity = 1, int chamberID = ChamberMainID)
		{
			if (chamberID < 0 || chamberID >= inventory.Chambers.Count)
				return null;

			if (!inventory.ItemPoolByChambers[chamberID].ContainsKey(itemID))
				return null;
			List<ItemFeature> returnList = new List<ItemFeature>();
			var item = inventory.ItemPoolByChambers[chamberID][itemID][0];
			if (item.StaticData.IsStackable)
			{
				int amount = Mathf.Min(quantity, item.Quantity);
				if (amount <= 0)
					return null;
				if (amount == item.Quantity)
				{
					inventory.ItemPoolByChambers[chamberID].Remove(itemID);
					returnList.Add(item);
				}
				else
				{
					item.Quantity -= amount;
					var newItem = entityFactory.CreateInstance(itemID).GetFeature<ItemFeature>();
					newItem.Quantity = amount;
					returnList.Add(newItem);
				}
				ChangeInventoryStatistics(inventory, item.StaticData, -amount);
			}
			else
			{
				int total = inventory.ItemPoolByChambers[chamberID][itemID].Count;
				int amount = Mathf.Min(quantity, total);
				if (amount <= 0)
					return null;
				for (int j = 0; j < amount; j++)
				{
					returnList.Add(inventory.ItemPoolByChambers[chamberID][itemID][0]);
					inventory.ItemPoolByChambers[chamberID][itemID].RemoveAt(0);
				}
				if (inventory.ItemPoolByChambers[chamberID][itemID].Count <= 0)
				{
					inventory.ItemPoolByChambers[chamberID].Remove(itemID);
				}
				ChangeInventoryStatistics(inventory, item.StaticData, -amount);
			}
			return returnList;
		}

		public static bool AddItem(InventoryFeature inventory, ItemFeature item, bool dropOverflow, int chamberID = ChamberMainID)
		{
			if (item == null)
				return false;

			if (CanAddItems(inventory, item.Entity.StaticData.ID, item.Quantity))
			{
				GenerateChamberIfNoneExists(inventory, chamberID);
				if (!inventory.ItemPoolByChambers[chamberID].ContainsKey(item.Entity.StaticData.ID))
				{
					inventory.ItemPoolByChambers[chamberID].Add(item.Entity.StaticData.ID, new List<ItemFeature>());
				}
				if (item.StaticData.IsStackable)
				{
					if (inventory.ItemPoolByChambers[chamberID][item.Entity.StaticData.ID].Count <= 0)
					{
						inventory.ItemPoolByChambers[chamberID][item.Entity.StaticData.ID].Add(item);
					}
					else
					{
                        var existingItem = inventory.ItemPoolByChambers[chamberID][item.Entity.StaticData.ID][0];
                        if (item.Entity.HasFeature<PollutionFeature>())
                        {
                            var pollution = item.Entity.GetFeature<PollutionFeature>();
                            var oldPollution = existingItem.Entity.GetFeature<PollutionFeature>().pollution;
                            oldPollution *= existingItem.Quantity;
                            var newPollution = pollution.pollution;
                            newPollution *= item.Quantity;
                            existingItem.Entity.GetFeature<PollutionFeature>().pollution =
                                (oldPollution + newPollution) / (existingItem.Quantity + item.Quantity);
                        }
						existingItem.Quantity += item.Quantity;
					}
				}
				else
				{
					inventory.ItemPoolByChambers[chamberID][item.Entity.StaticData.ID].Add(item);
				}
				ChangeInventoryStatistics(inventory, item.StaticData, item.Quantity);
				return true;
			}
			else if (dropOverflow)
			{
				if (ResourcePileSystem.PlaceStoragePile(inventory.Entity, new List<ItemFeature>() { item }))
					return true;
				else
					return false;
			}
			else
			{
				return false;
			}

		}

		public static void RemoveItem(InventoryFeature inventory, ItemFeature item, int chamberID = ChamberMainID)
		{
			if (chamberID < 0 || chamberID >= inventory.Chambers.Count)
				return;
			if (item == null)
				return;
			var id = item.Entity.StaticData.ID;
			if (!inventory.ItemPoolByChambers[chamberID].ContainsKey(id))
				return;
			if (!inventory.ItemPoolByChambers[chamberID][id].Contains(item))
				return;
			int count = item.Quantity;
			var data = item.StaticData;
			inventory.ItemPoolByChambers[chamberID][id].Remove(item);
			item.Destroy();
			if (inventory.ItemPoolByChambers[chamberID][id].Count <= 0)
				inventory.ItemPoolByChambers[chamberID].Remove(id);
			ChangeInventoryStatistics(inventory, data, -count);
		}

		public static bool ReserveItems(InventoryFeature inventory, string resID, int amount)
		{
			if (GetItemCount(inventory, resID) < amount)
				return false;
			var data = entityFactory.GetStaticData<ItemFeatureStaticData>(resID);
            float newPollution = 0f;
			if (data.IsStackable)
			{
                if(inventory.ItemPoolByChambers[0][resID][0].Entity.HasFeature<PollutionFeature>())
                {
                    newPollution = inventory.ItemPoolByChambers[0][resID][0].Entity.GetFeature<PollutionFeature>().pollution;
                }
				if (amount == inventory.ItemPoolByChambers[0][resID][0].Quantity)
				{
					inventory.ItemPoolByChambers[0].Remove(resID);
				}
				else
				{
					inventory.ItemPoolByChambers[0][resID][0].Quantity -= amount;
				}
				if (inventory.ItemPoolByChambers[1].ContainsKey(resID))
				{
                    var existingItem = inventory.ItemPoolByChambers[1][resID][0];
                    if (existingItem.Entity.HasFeature<PollutionFeature>())
                    {
                        newPollution *= amount;
                        var oldPollution = existingItem.Entity.GetFeature<PollutionFeature>().pollution;
                        oldPollution *= existingItem.Quantity;
                        existingItem.Entity.GetFeature<PollutionFeature>().pollution =
                            (oldPollution + newPollution) / (existingItem.Quantity + amount);
                    }
                    inventory.ItemPoolByChambers[1][resID][0].Quantity += amount;
				}
				else
				{
					var newItem = entityFactory.CreateInstance(resID).GetFeature<ItemFeature>();
					newItem.Quantity = amount;
					inventory.ItemPoolByChambers[1].Add(resID, new List<ItemFeature>() { newItem });
				}
			}
			else
			{
				if (!inventory.ItemPoolByChambers[1].ContainsKey(resID))
				{
					inventory.ItemPoolByChambers[1].Add(resID, new List<ItemFeature>());
				}
				for (int i = 0; i < amount; i++)
				{
					var item = inventory.ItemPoolByChambers[0][resID][0];
					inventory.ItemPoolByChambers[0][resID].RemoveAt(0);
					inventory.ItemPoolByChambers[1][resID].Add(item);
				}
				if (inventory.ItemPoolByChambers[0][resID].Count <= 0)
				{
					inventory.ItemPoolByChambers[0].Remove(resID);
				}
			}
			return true;

		}
		public static bool UnreserveItems(InventoryFeature inventory, string resID, int amount)
		{
			if (GetItemCount(inventory, resID, 1) < amount)
				return false;
			var data = entityFactory.GetStaticData<ItemFeatureStaticData>(resID);
            float newPollution = 0f;
            if (data.IsStackable)
			{
                if (inventory.ItemPoolByChambers[1][resID][0].Entity.HasFeature<PollutionFeature>())
                {
                    newPollution = inventory.ItemPoolByChambers[1][resID][0].Entity.GetFeature<PollutionFeature>().pollution;
                }
                if (amount == inventory.ItemPoolByChambers[1][resID][0].Quantity)
				{
					inventory.ItemPoolByChambers[1].Remove(resID);
				}
				else
				{
					inventory.ItemPoolByChambers[1][resID][0].Quantity -= amount;
				}
				if (inventory.ItemPoolByChambers[0].ContainsKey(resID))
				{
                    var existingItem = inventory.ItemPoolByChambers[0][resID][0];
                    if (existingItem.Entity.HasFeature<PollutionFeature>())
                    {
                        newPollution *= amount;
                        var oldPollution = existingItem.Entity.GetFeature<PollutionFeature>().pollution;
                        oldPollution *= existingItem.Quantity;
                        existingItem.Entity.GetFeature<PollutionFeature>().pollution =
                            (oldPollution + newPollution) / (existingItem.Quantity + amount);
                    }
                    inventory.ItemPoolByChambers[0][resID][0].Quantity += amount;
				}
				else
				{
					var newItem = entityFactory.CreateInstance(resID).GetFeature<ItemFeature>();
					newItem.Quantity = amount;
					inventory.ItemPoolByChambers[0].Add(resID, new List<ItemFeature>() { newItem });
				}
			}
			else
			{
				if (!inventory.ItemPoolByChambers[0].ContainsKey(resID))
				{
					inventory.ItemPoolByChambers[0].Add(resID, new List<ItemFeature>());
				}
				for (int i = 0; i < amount; i++)
				{
					var item = inventory.ItemPoolByChambers[1][resID][0];
					inventory.ItemPoolByChambers[1][resID].RemoveAt(0);
					inventory.ItemPoolByChambers[0][resID].Add(item);
				}
				if (inventory.ItemPoolByChambers[1][resID].Count <= 0)
				{
					inventory.ItemPoolByChambers[1].Remove(resID);
				}
			}

#if _DEBUG
			CheckLoadForFailure(inventory);
#endif
			return true;
		}

		public static bool TransferItems(InventoryFeature inventoryFrom, InventoryFeature inventoryTo, string id, int amount, bool dropOverflow, int chamberFrom, int chamberTo)
		{
			if (chamberFrom < 0 || chamberFrom >= inventoryFrom.Chambers.Count)
				return false;

			int checkAmount = GetAddableAmount(inventoryTo, id);
			if (!dropOverflow && checkAmount == 0)
				return false;

			if (!HasItems(inventoryFrom, id, amount))
				return false;
			var items = WithdrawItems(inventoryFrom, id, amount, chamberFrom);

			GenerateChamberIfNoneExists(inventoryTo, chamberTo);

			foreach (var item in items)
			{
				AddItem(inventoryTo, item, dropOverflow, chamberTo);
			}
			return true;
		}

		public static List<ItemFeature> TransferItemsToChamber(InventoryFeature inventory, List<ItemFeature> itemStack, int chamberFrom, int chamberTo)
		{
			if (!ChamberExists(inventory, chamberFrom))
				return itemStack;

			GenerateChamberIfNoneExists(inventory, chamberTo);

			for (int i = itemStack.Count - 1; i >= 0; i--)
			{
				var item = itemStack[i];
				var id = item.Entity.StaticData.ID;
				if (!inventory.ItemPoolByChambers[chamberFrom].ContainsKey(id))
					continue;
				if (!inventory.ItemPoolByChambers[chamberFrom][id].Contains(item))
					continue;
				inventory.ItemPoolByChambers[chamberFrom][id].Remove(item);
				if (!inventory.ItemPoolByChambers[chamberTo].ContainsKey(id))
					inventory.ItemPoolByChambers[chamberTo].Add(id, new List<ItemFeature>());
				if (item.StaticData.IsStackable)
				{
					if (inventory.ItemPoolByChambers[chamberTo][id].Count <= 0)
					{
						inventory.ItemPoolByChambers[chamberTo][id].Add(item);
					}
					else
					{
						inventory.ItemPoolByChambers[chamberTo][id][0].Quantity += item.Quantity;
					}
				}
				else
				{
					inventory.ItemPoolByChambers[chamberTo][id].Add(item);
				}
				itemStack.RemoveAt(i);
			}
			return itemStack;
		}

		#endregion

		public static ConsumableFeature WithdrawConsumable(InventoryFeature inventory, EConsumptionType consumptionType, int chamberID = ChamberMainID)
		{
			var keys = inventory.ItemPoolByChambers[chamberID].Keys.ToArray();
			foreach (var key in keys)
			{
				var item = inventory.ItemPoolByChambers[chamberID][key][0];
				if (!item.Entity.HasFeature<ConsumableFeature>())
					continue;
                if (CitizenAISystem.consumableFilter.Contains(item.Entity.StaticData.ID))
                    continue;
				var food = item.Entity.GetFeature<ConsumableFeature>();
				if (food.StaticData.ConsumptionType == consumptionType)
				{
					var returnedItems = WithdrawItems(inventory, key, 1, chamberID);
					if (returnedItems != null && returnedItems.Count > 0)
					{
						return returnedItems[0].Entity.GetFeature<ConsumableFeature>();
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Returns load percentage (0~1) for each Storage Policy
		/// Dictionary values sum must always be within 0-1 range.
		/// </summary>
		/// <returns></returns>
		public static Dictionary<EStoragePolicy, float> CalculateLoadByPolicy(InventoryFeature inventory)
		{
			Dictionary<EStoragePolicy, float> loadByPolicyTable
				= new Dictionary<EStoragePolicy, float>();
			foreach (var chamber in inventory.ItemPoolByChambers)
			{
				var keys = chamber.Keys.ToArray();
				foreach (var key in keys)
				{
					var data = entityFactory.EntityStaticData[key].GetFeature<ItemFeatureStaticData>();
					if (!loadByPolicyTable.ContainsKey(data.Category))
					{
						loadByPolicyTable.Add(data.Category, 0);
					}
					if (data.IsStackable)
					{
						loadByPolicyTable[data.Category] += chamber[key][0].Quantity * data.Mass;
					}
					else
					{
						loadByPolicyTable[data.Category] += chamber[key].Count * data.Mass;
					}
				}
			}
			var categories = new List<EStoragePolicy>(loadByPolicyTable.Keys);
			foreach (var key in categories)
			{
				loadByPolicyTable[key] /= inventory.MaxCapacity;
			}
			return loadByPolicyTable;
		}

		public static void DropInventoryToTheFloor(InventoryFeature inventory)
		{
			foreach (var chamber in inventory.ItemPoolByChambers)
			{
				var keys = chamber.Keys.ToArray();
				foreach (var key in keys)
				{
					ResourcePileSystem.PlaceStoragePile(inventory.Entity, chamber[key]);
				}
				chamber.Clear();
			}
			inventory.SetLoad(0);
		}

		public static void DropInventoryToTheFloorAsync(InventoryFeature inventory)
		{
			CoroutineRunner.StartCoroutine(DropInventoryToTheFloorCoroutine(inventory));
		}

		private static IEnumerator DropInventoryToTheFloorCoroutine(InventoryFeature inventory)
		{
			var items = new Dictionary<string, List<ItemFeature>>();
			string[] keys;
			foreach (var chamber in inventory.ItemPoolByChambers)
			{
				keys = chamber.Keys.ToArray();
				foreach (var key in keys)
				{
					if (!items.ContainsKey(key))
					{
						items.Add(key, chamber[key]);
						continue;
					}
					var data = entityFactory.GetStaticData<ItemFeatureStaticData>(key);
					if (data.IsStackable)
					{
						items[key][0].Quantity += chamber[key][0].Quantity;
					}
					else
					{
						items[key].AddRange(chamber[key]);
					}
				}
				chamber.Clear();
			}
			inventory.SetLoad(0);
			Vector2i pos = inventory.Entity.GetFeature<GridObjectFeature>().GridObjectData.Rect.Centeri;
			yield return null;
			keys = items.Keys.ToArray();
			foreach (var key in keys)
			{
				ResourcePileSystem.PlaceStoragePile(pos, items[key]);
				yield return null;
			}
		}

		/// <summary>
		/// Hack n dirty inventory clearing
		/// </summary>
		/// <param name="context"></param>
		public static void ClearInventory(InventoryFeature inventory)
		{
			foreach (var chamber in inventory.ItemPoolByChambers)
			{
				chamber.Clear();
			}
			inventory.TotalFood = 0;
			inventory.TotalItems = 0;
			inventory.TotalTools = 0;
			inventory.TotalWeapons = 0;
			inventory.TotalWaste = 0;
			inventory.TotalWasteOrganic = 0;
			inventory.SetLoad(0);
		}

		public static void FillInventory<T>(InventoryFeature inventory)
			where T : FeatureStaticDataBase
		{
			int count = 1000;
			var ids = entityFactory.GetStaticDataIDList<T>();
			while (count > 0 && inventory.CapacityLeft > 0)
			{
				var id = ids.ElementAt(UnityEngine.Random.Range(0, ids.Count));
				if (CanAddItems(inventory, id, 1))
				{
					var item = entityFactory.CreateInstance(id).GetFeature<ItemFeature>();
					item.Quantity = 1;
					AddItem(inventory, item, false);
				}
				else count--;
			}
		}

		public static InventoryStatistics GetNewInventoryStatistics()
		{
			InventoryStatistics statistics = new InventoryStatistics();

			//Weapons
			var ids = entityFactory.GetStaticDataIDList<WeaponFeatureStaticData>();
			Dictionary<string, int> materials = new Dictionary<string, int>(ids.Count);
            foreach(var id in ids)			
				materials.Add(id, 0);
			statistics.Weapons = materials;
			//Tools
			ids = entityFactory.GetStaticDataIDList<ToolFeatureStaticData>();
			materials = new Dictionary<string, int>(ids.Count);
			foreach(var id in ids)
				materials.Add(id, 0);
			statistics.Tools = materials;
			//Food
			ids = entityFactory.GetStaticDataIDList<ConsumableFeatureStaticData>();
			materials = new Dictionary<string, int>(ids.Count);
			foreach(var id in ids)
				materials.Add(id, 0);
			statistics.Foods = materials;
			//Items
			ids = entityFactory.GetStaticDataIDList<ItemFeatureStaticData>();
			materials = new Dictionary<string, int>(ids.Count);
			foreach(var id in ids)
				materials.Add(id, 0);
			statistics.Items = materials;

			return statistics;
		}

        public static void UpdateInventoryStatistics(InventoryFeature inventory, InventoryStatistics systemStatistics)
        {
            var stats = inventory.Statistics;
            if (inventory.IsDirtyItems)
            {
                inventory.IsDirtyItems = false;

                stats.TotalItems = inventory.TotalItems;
                stats.TotalWaste = inventory.TotalWaste;
                stats.TotalWasteOrganic = inventory.TotalWasteOrganic;
                string key;
                int value = 0;
                var ids = entityFactory.GetStaticDataIDList<ItemFeatureStaticData>();
                foreach(var id in ids)
                {                    
                    value = 0;
                    int j = 0;
                    {
                        if (inventory.ItemPoolByChambers[j].ContainsKey(id))
                        {
                            var data = entityFactory.GetStaticData<ItemFeatureStaticData>(id);
                            if (data.IsStackable)
                            {
                                value += inventory.ItemPoolByChambers[j][id][0].Quantity;
                            }
                            else
                            {
                                value += inventory.ItemPoolByChambers[j][id].Count;
                            }

                        }
                    }
                    stats.Items[id] = value;
                }
            }
            if (inventory.IsDirtyFood)
            {
                inventory.IsDirtyFood = false;

                stats.ConsumableWater = 0;
                stats.ConsumableWaterAvailable = 0;
                stats.ConsumableNutritionAvailable = 0;
                stats.NutritionAvailable = 0;
                stats.Nutrition = 0;

                string key;
                int value = 0;
                var ids = entityFactory.GetStaticDataIDList<ConsumableFeatureStaticData>();
                foreach(var id in ids)
                {
                    value = 0;
                    int chamberID = 0;  //Main chamber only
                    {
                        if (inventory.ItemPoolByChambers[chamberID].ContainsKey(id))
                        {
                            var data = entityFactory.GetStaticData<ItemFeatureStaticData>(id);
                            if (data.IsStackable)
                            {
                                var itm = inventory.ItemPoolByChambers[chamberID][id][0];
                                var q = itm.Quantity;
                                value += q;
                                var consumable = itm.Entity.GetFeature<ConsumableFeature>();
                                if (consumable != null)
                                {
                                    switch(consumable.StaticData.ConsumptionType)
                                    {
                                        case EConsumptionType.Drink:
                                            if (!CitizenAISystem.consumableFilter.Contains(data.entity.ID))
                                            {
                                                stats.ConsumableWater += q * consumable.StaticData.Water;
                                                if (chamberID != inventory.ReservedChamberID)
                                                {
                                                    stats.ConsumableWaterAvailable += q * consumable.StaticData.Water;
                                                }
                                            }                                                
                                            break;

                                        case EConsumptionType.Food:
                                            stats.Nutrition += q * consumable.StaticData.Nutrition;
                                            if (chamberID != inventory.ReservedChamberID)
                                            {
                                                stats.NutritionAvailable += q * consumable.StaticData.Nutrition;
                                                if (!CitizenAISystem.consumableFilter.Contains(data.entity.ID))
                                                {
                                                    stats.ConsumableNutritionAvailable += q * consumable.StaticData.Nutrition;
                                                }
                                            }
                                            break;
                                    }                                                                            
                                }
                            }
                            else
                            {
                                var list = inventory.ItemPoolByChambers[chamberID][id];
                                var itm = list[0];
                                var q = list.Count;
                                if (q > 0)
                                {
                                    value += q;
                                    var consumable = itm.Entity.GetFeature<ConsumableFeature>();
                                    if (consumable != null)
                                    {
                                        stats.Nutrition += q * consumable.StaticData.Nutrition;
                                        if (chamberID != inventory.ReservedChamberID)
                                        {
                                            stats.NutritionAvailable += q * consumable.StaticData.Nutrition;
                                            if (!CitizenAISystem.consumableFilter.Contains(data.entity.ID))
                                            {
                                                stats.ConsumableNutritionAvailable += q * consumable.StaticData.Nutrition;
                                            }
                                        }
                                            
                                    }
                                }
                            }
                        }
                    }
                    stats.Foods[id] = value;
                    stats.TotalFood = inventory.TotalFood;
                }
            }

            if (inventory.IsDirtyTools)
            {
                inventory.IsDirtyTools = false;

                string key;
                int value = 0;
                var ids = entityFactory.GetStaticDataIDList<ToolFeatureStaticData>();
                foreach(var id in ids)
                {
                    value = 0;
                    int j = 0;
                    {
                        if (inventory.ItemPoolByChambers[j].ContainsKey(id))
                        {
                            var data = entityFactory.GetStaticData<ItemFeatureStaticData>(id);
                            if (data.IsStackable)
                            {
                                value += inventory.ItemPoolByChambers[j][id][0].Quantity;
                            }
                            else
                            {
                                value += inventory.ItemPoolByChambers[j][id].Count;
                            }

                        }
                    }
                    stats.Tools[id] = value;
                    stats.TotalTools = inventory.TotalTools;
                }
            }

            if (inventory.IsDirtyWeapons)
            {
                inventory.IsDirtyWeapons = false;

                int value = 0;
                var ids = entityFactory.GetStaticDataIDList<WeaponFeatureStaticData>();
                foreach(var id in ids)
                {
                    value = 0;
                    int j = 0;
                    {
                        if (inventory.ItemPoolByChambers[j].ContainsKey(id))
                        {
                            var data = entityFactory.GetStaticData<ItemFeatureStaticData>(id);
                            if (data.IsStackable)
                            {
                                value += inventory.ItemPoolByChambers[j][id][0].Quantity;
                            }
                            else
                            {
                                value += inventory.ItemPoolByChambers[j][id].Count;
                            }

                        }
                    }
                    stats.Weapons[id] = value;
                    stats.TotalWeapons = inventory.TotalWeapons;
                }
            }

            if (inventory.IsDirtyWater)
            {
                inventory.IsDirtyWater = false;
                stats.Water = 0;
                stats.WaterAvailable = 0;
                if (inventory.ItemPoolByChambers[0].ContainsKey(FactoryConstants.WaterID))
                {
                    stats.Water += inventory.ItemPoolByChambers[0][FactoryConstants.WaterID][0].Quantity;
                    stats.WaterAvailable += inventory.ItemPoolByChambers[0][FactoryConstants.WaterID][0].Quantity;
                }
            }
            systemStatistics += stats;
        }
    }
}