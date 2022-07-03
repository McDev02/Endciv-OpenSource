using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	public class TradingWindow : GUIAnimatedPanel
	{
		GridMap gridMap;
		StorageSystem storageSystem;
		AgricultureSystem agricultureSystem;
		PastureSystem pastureSystem;

		[SerializeField]
		private TradingBaseListEntry[] listEntryPrefabs;
		private Dictionary<Type, TradingBaseListEntry> prefabLookup;

		TraderAIAgentFeature trader;

		[SerializeField] RectTransform listContainerMine;
		[SerializeField] RectTransform listContainerTheirs;
		[SerializeField] RectTransform listContainerIncoming;
		[SerializeField] RectTransform listContainerOutcoming;

		[SerializeField] Text txtTheirValue;
		[SerializeField] Text txtMineValue;

		[SerializeField] Button makeTradeButton;
		[SerializeField] Text makeTradeText;

		List<TradingBaseListEntry> myEntries = new List<TradingBaseListEntry>();
		List<TradingBaseListEntry> theirEntries = new List<TradingBaseListEntry>();
		List<TradingBaseListEntry> outgoingEntries = new List<TradingBaseListEntry>();
		List<TradingBaseListEntry> incomingEntries = new List<TradingBaseListEntry>();

		Dictionary<Type, List<TradingBaseListEntry>> entryPool = new Dictionary<Type, List<TradingBaseListEntry>>();

		private float theirValue;
		private float myValue;
		private bool isTradePossible;

		public void Setup(GridMap gridMap, StorageSystem storageSystem, TraderAIAgentFeature trader, AgricultureSystem agricultureSystem, PastureSystem pastureSystem)
		{
			this.gridMap = gridMap;
			this.storageSystem = storageSystem;
			this.agricultureSystem = agricultureSystem;
			this.pastureSystem = pastureSystem;
			InitializePool();

			var myResources = new Dictionary<string, int>();			
			var myCattle = new Dictionary<CattleStaticData, Dictionary<ELivingBeingAge, int>>();

			foreach (var storage in storageSystem.GetAllStorages(SystemsManager.MainPlayerFaction))
			{
				var resources = InventorySystem.GetChamberContentList(storage.Inventory, 0);
				foreach (var resource in resources)
				{
					if (!myResources.ContainsKey(resource.ResourceID))
					{
						myResources.Add(resource.ResourceID, resource.Amount);
					}
					else
					{
						myResources[resource.ResourceID] += resource.Amount;
					}
				}
			}
			foreach(var pasture in pastureSystem.FeaturesByFaction[SystemsManager.MainPlayerFaction])
			{
				foreach(var cattle in pasture.Cattle)
				{
					var staticData = cattle.staticData;
					var age = cattle.Entity.GetFeature<LivingBeingFeature>().age;
					if(!myCattle.ContainsKey(staticData))
					{
						myCattle.Add(staticData, new Dictionary<ELivingBeingAge, int>());
					}
					if(!myCattle[staticData].ContainsKey(age))
					{
						myCattle[staticData].Add(age, 0);
					}
					myCattle[staticData][age]++;
				}
			}
			RemoveAllEntries();

			foreach (var resource in myResources)
			{
				var entry = CreateNewEntry<TradingResourceListEntry>(listContainerMine);
				entry.Setup(resource.Value, resource.Key, this);				
				myEntries.Add(entry);
			}

			foreach(var cattleGroupPair in myCattle)
			{
				foreach(var cattlePair in cattleGroupPair.Value)
				{
					var entry = CreateNewEntry<TradingCattleListEntry>(listContainerMine);
					entry.Setup(cattlePair.Value, cattleGroupPair.Key.entity.ID, this, false, cattlePair.Key);					
					myEntries.Add(entry);
				}
			}

			this.trader = trader;
			if (trader != null && trader.traderData != null)
			{
				var list = trader.traderData.GenerateTradingList();
				foreach (var resource in list)
				{
					bool isCattle = Main.Instance.GameManager.Factories.SimpleEntityFactory.GetStaticDataIDList<UnitFeatureStaticData>().Contains(resource.Key);
					if(isCattle)
					{
						var entry = CreateNewEntry<TradingCattleListEntry>(listContainerTheirs);
						ELivingBeingAge age = ELivingBeingAge.Child;
						if (UnityEngine.Random.Range(0, 5) > 3)
							age = ELivingBeingAge.Adult;						
						entry.Setup(resource.Value, resource.Key, this, false, age);
						theirEntries.Add(entry);
					}
					else
					{
						var entry = CreateNewEntry<TradingResourceListEntry>(listContainerTheirs);
						entry.Setup(resource.Value, resource.Key, this);
						theirEntries.Add(entry);
					}
				}
			}
			EvaluateTrade();
		}

		private void InitializePool()
		{
			prefabLookup = new Dictionary<Type, TradingBaseListEntry>();
			foreach(var prefab in listEntryPrefabs)
			{
				prefabLookup.Add(prefab.GetType(), prefab);
			}		

			if (entryPool == null)
				entryPool = new Dictionary<Type, List<TradingBaseListEntry>>();

			var types = GetType().Assembly.GetTypes().Where(
				x => x.IsClass && typeof(TradingBaseListEntry).IsAssignableFrom(x)
				&& !x.IsAbstract);

			foreach (var type in types)
			{
				if(!entryPool.ContainsKey(type))
				{
					entryPool.Add(type, new List<TradingBaseListEntry>());
				}
			}
		}

		private void RemoveAllEntries()
		{			
			for (int i = myEntries.Count - 1; i >= 0; i--)
			{
				var entry = myEntries[i];
				entry.gameObject.SetActive(false);
				myEntries.RemoveAt(i);
				entryPool[entry.GetType()].Add(entry);
			}

			for (int i = theirEntries.Count - 1; i >= 0; i--)
			{
				var entry = theirEntries[i];
				entry.gameObject.SetActive(false);
				theirEntries.RemoveAt(i);
				entryPool[entry.GetType()].Add(entry);
			}

			for (int i = outgoingEntries.Count - 1; i >= 0; i--)
			{
				var entry = outgoingEntries[i];
				entry.gameObject.SetActive(false);
				outgoingEntries.RemoveAt(i);
				entryPool[entry.GetType()].Add(entry);
			}

			for (int i = incomingEntries.Count - 1; i >= 0; i--)
			{
				var entry = incomingEntries[i];
				entry.gameObject.SetActive(false);
				incomingEntries.RemoveAt(i);
				entryPool[entry.GetType()].Add(entry);
			}
			theirValue = 0;
			myValue = 0;
		}

		private T CreateNewEntry<T>(Transform parent) 
			where T : TradingBaseListEntry
		{
			return (T)CreateNewEntry(parent, typeof(T));			
		}

		private TradingBaseListEntry CreateNewEntry(Transform parent, Type type)
		{
			if (entryPool[type].Count > 0)
			{
				var entry = entryPool[type][0];
				entryPool[type].Remove(entry);
				entry.transform.SetParent(parent, false);
				entry.gameObject.SetActive(true);
				return entry;
			}
			else
			{
				return Instantiate(prefabLookup[type], parent, false);
			}
		}

		public void MakeTrade()
		{
			//Check current trade status before allowing trade to proceed.
			if (!isTradePossible) return;

			foreach(var entry in outgoingEntries)
			{
				entry.DestroyResources();
			}			

			if(incomingEntries == null)
			{
				OnClose();
				return;
			}

			foreach (var entry in incomingEntries)
			{
				entry.AcquireResources();
			}				
			
			trader.state = NpcSpawnSystem.ETraderState.Leaving;
			OnClose();
		}

		public void AddAllMine()
		{
			float val = 0f;
			for (int i = myEntries.Count - 1; i >= 0; i--)
			{
				var entry = myEntries[i];
				MoveResource(myEntries, outgoingEntries, listContainerOutcoming, entry, entry.currentAmount);
				val += entry.BaseValue * entry.currentAmount;
			}
			myValue += val;
			EvaluateTrade();
		}

		public void ClearAllMine()
		{
			for (int i = outgoingEntries.Count - 1; i >= 0; i--)
			{
				var entry = outgoingEntries[i];
				MoveResource(outgoingEntries, myEntries, listContainerMine, entry, entry.currentAmount);
			}
			myValue = 0f;
			EvaluateTrade();
		}

		public void AddAllTheirs()
		{
			float val = 0f;
			for (int i = theirEntries.Count - 1; i >= 0; i--)
			{
				var entry = theirEntries[i];
				MoveResource(theirEntries, incomingEntries, listContainerIncoming, entry, entry.currentAmount);
				val += entry.BaseValue * entry.currentAmount;
			}
			theirValue += val;
			EvaluateTrade();
		}

		public void ClearAllTheirs()
		{
			for (int i = incomingEntries.Count - 1; i >= 0; i--)
			{
				var entry = incomingEntries[i];
				MoveResource(incomingEntries, theirEntries, listContainerTheirs, entry, entry.currentAmount);
			}
			theirValue = 0f;
			EvaluateTrade();
		}

		public void MoveResources(int amount, TradingBaseListEntry entry)
		{
			List<TradingBaseListEntry> sourceList = null;
			List<TradingBaseListEntry> destinationList = null;
			RectTransform destinationRoot = null;
			float val = entry.BaseValue * amount;
			if (myEntries.Contains(entry))
			{
				sourceList = myEntries;
				destinationList = outgoingEntries;
				destinationRoot = listContainerOutcoming;
				myValue += val;
			}
			else if (outgoingEntries.Contains(entry))
			{
				sourceList = outgoingEntries;
				destinationList = myEntries;
				destinationRoot = listContainerMine;
				myValue -= val;
				if (myValue < 0f)
					myValue = 0f;
			}
			else if (theirEntries.Contains(entry))
			{
				sourceList = theirEntries;
				destinationList = incomingEntries;
				destinationRoot = listContainerIncoming;
				theirValue += val;
			}
			else if (incomingEntries.Contains(entry))
			{
				sourceList = incomingEntries;
				destinationList = theirEntries;
				destinationRoot = listContainerTheirs;
				theirValue -= val;
				if (theirValue < 0f)
					theirValue = 0f;
			}
			else
			{
				Debug.LogError("Unable to locate Trading Resource List Entry's source.");
				return;
			}
			MoveResource(sourceList, destinationList, destinationRoot, entry, amount);
			EvaluateTrade();
		}

		private void MoveResource(List<TradingBaseListEntry> sourceList, List<TradingBaseListEntry> destinationList, RectTransform destinationRoot, TradingBaseListEntry entry, int amount)
		{
			entry.currentAmount -= amount;
			string id = entry.id;
			if (entry.currentAmount <= 0)
			{
				sourceList.Remove(entry);
				entry.gameObject.SetActive(false);
				entryPool[entry.GetType()].Add(entry);
			}
			else
			{
				entry.UpdateValues();
			}
			var destinationEntry = destinationList.FirstOrDefault(x => x.Matches(entry));
			if (destinationEntry != null)
			{
				destinationEntry.currentAmount += amount;
				destinationEntry.UpdateValues();
			}
			else
			{
				destinationEntry = CreateNewEntry(destinationRoot, entry.GetType());
				destinationEntry.Setup(amount, id, this, false, entry.args);				
				destinationList.Add(destinationEntry);
			}
		}

		private void EvaluateTrade()
		{
			string valueText = LocalizationManager.GetText("#UI/Game/TradingWindow/TradeValue");
			txtMineValue.text = $"{valueText}: {myValue.ToString("0.##")}";
			txtTheirValue.text = $"{valueText}: {theirValue.ToString("0.##")}";

			isTradePossible = myValue > theirValue * trader.traderData.tradeSpread;

			makeTradeButton.interactable = isTradePossible;
			if (isTradePossible)
				makeTradeText.text = LocalizationManager.GetText("#UI/Game/TradingWindow/Messages/1");
			else if (myValue <= 1)
				makeTradeText.text = LocalizationManager.GetText("#UI/Game/TradingWindow/Messages/2");
			else if (myValue <= theirValue * trader.traderData.thresholdBad)
				makeTradeText.text = LocalizationManager.GetText("#UI/Game/TradingWindow/Messages/3");
			else if (myValue <= theirValue * trader.traderData.thresholdMed)
				makeTradeText.text = LocalizationManager.GetText("#UI/Game/TradingWindow/Messages/4");
			else
				makeTradeText.text = LocalizationManager.GetText("#UI/Game/TradingWindow/Messages/5");
		}
	}
}