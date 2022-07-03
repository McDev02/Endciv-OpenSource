using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

namespace Endciv
{
	/// <summary>
	/// Provides an stack of items for an entity
	/// </summary>
	public class InventoryFeature : Feature<InventoryFeatureSaveData>
	{
		public Action<InventoryFeature> OnInventoryChanged;

		public InventoryFeature()
		{
			Chambers = new List<string>();
			ItemPoolByChambers = new List<Dictionary<string, List<ItemFeature>>>();
			InventorySystem.AddChamber(this, InventorySystem.ChamberMainName);
			ReservedChamberID = InventorySystem.AddChamber(this, InventorySystem.ChamberReservedName);
		}

		public override void Setup(BaseEntity entity, FeatureParamsBase args = null)
		{
			base.Setup(entity);
			var data = Entity.StaticData.GetFeature<InventoryStaticData>();
			if(data != null)
			{
				staticData = data;
				SetStatistics(InventorySystem.GetNewInventoryStatistics());
			}				
		}

		public void SetStatistics(InventoryStatistics statistics)
		{
			Statistics = statistics;
			MaxCapacity = (int)(staticData.MaxCapacity * GameConfig.Instance.StorageCapacityMultipler);
			//Handle flexible size
			if (Entity.HasFeature<StructureFeature>())
			{
				var structure = Entity.GetFeature<StructureFeature>();
				if (structure.Entity.StaticData.GetFeature<GridObjectFeatureStaticData>().GridIsFlexible)
					MaxCapacity *= Entity.GetFeature<GridObjectFeature>().GridObjectData.Rect.Area;
			}
		}

		public InventoryStaticData staticData;

		public int ReservedChamberID { get; private set; }
		public int MaxCapacity { get; private set; }
		public bool Disabled { get; set; }
		public float UserDefinedPriority { get; set; }
		public float Priority { get { return UserDefinedPriority; } }
		public int CapacityLeft { get { return MaxCapacity - Load; } }
		public int Load { get; private set; }
		public float LoadProgress { get { return MaxCapacity <= 0 ? 0 : Load / (float)MaxCapacity; } }

		public int TotalChambers;
		public List<string> Chambers;

		//Chamber / ItemID / Features
		public List<Dictionary<string, List<ItemFeature>>> ItemPoolByChambers;

		public int TotalWeapons { get; internal set; }
		public int TotalTools { get; internal set; }
		public int TotalFood { get; internal set; }
		public int TotalItems { get; internal set; }
		public int TotalWaste { get; internal set; }
		public int TotalWasteOrganic { get; internal set; }

		public bool IsDirtyWeapons { get; internal set; }
		public bool IsDirtyTools { get; internal set; }
		public bool IsDirtyFood { get; internal set; }
		public bool IsDirtyWater { get; internal set; }
		public bool IsDirtyItems { get; internal set; }		

		public InventoryStatistics Statistics;

		public void SetLoad(int load)
		{
			if (MaxCapacity < Load)
				Debug.LogError("Load greater than Capacity! " + Load.ToString() + " / " + CapacityLeft.ToString());
			else if (Load < 0)
				Debug.LogError("Load below Zero! " + Load.ToString() + " / " + CapacityLeft.ToString());
			Load = load;
		}

		public void SetMaxCapacity(int capacity)
		{
			MaxCapacity = capacity;
		}

		public void AddLoad(int load)
		{
			Load += load;
			if (MaxCapacity < Load)
				Debug.LogError("Load greater than Capacity! " + Load.ToString() + " / " + MaxCapacity.ToString());
			else if (Load < 0)
				Debug.LogError("Load below Zero! " + Load.ToString() + " / " + MaxCapacity.ToString());

		}		

		public override void Run(SystemsManager manager)
		{
			base.Run(manager);
			manager.ResourceSystem.RegisterFeature(this);
		}

		public override void Stop()
		{
			base.Stop();
			SystemsManager.ResourceSystem.DeregisterFeature(this);
			InventorySystem.DropInventoryToTheFloor(this);
		}

		public override void OnFactionChanged(int oldFaction)
		{
			base.OnFactionChanged(oldFaction);
			SystemsManager.ResourceSystem.DeregisterFeature(this, oldFaction);
			SystemsManager.ResourceSystem.RegisterFeature(this);
		}

		#region Save Data
		public override ISaveable CollectData()
		{
			var inventoryData = new InventoryFeatureSaveData();
			inventoryData.Chambers = new List<InventoryFeatureSaveData.Chamber>();
			for (int i = 0; i < Chambers.Count; i++)
			{
				var chamber = new InventoryFeatureSaveData.Chamber();
				chamber.items = new List<EntitySaveData>();

				chamber.chamberName = Chambers[i];
				chamber.chamberID = i;
				var keys = ItemPoolByChambers[i].Keys.ToArray();
				foreach (var key in keys)
				{
					var items = ItemPoolByChambers[i][key];
					foreach (var item in items)
					{
						chamber.items.Add((EntitySaveData)item.Entity.CollectData());
					}
				} 				
				inventoryData.Chambers.Add(chamber);
			}
			return inventoryData;
		}

		public override void ApplyData(InventoryFeatureSaveData data)
		{
			var factory = Main.Instance.GameManager.Factories.SimpleEntityFactory;
			
			InventorySystem.ClearInventory(this);
			foreach (var chamber in data.Chambers)
			{
				if (!InventorySystem.ChamberExists(this, chamber.chamberID))
				{
					InventorySystem.AddChamber(this, chamber.chamberName);
				}	
				foreach(var item in chamber.items)
				{
					var itm = factory.CreateInstance(item.id, item.UID);
					itm.ApplySaveData(item);
					InventorySystem.AddItem(this, itm.GetFeature<ItemFeature>(), false, chamber.chamberID);
				}					
			}
		}
		#endregion

		public void NotifyInventoryChanged()
		{
			OnInventoryChanged?.Invoke(this);
		}		
	}
}