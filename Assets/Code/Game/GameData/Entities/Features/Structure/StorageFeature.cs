//Enum operates as bitmask
using System.Collections.Generic;
using System;

namespace Endciv
{
	public enum EStoragePolicy
	{
		Water = 1 << 0,
		Scrapyard = 1 << 1,
		Food = 1 << 2,
		Good = 1 << 3,
		Waste = 1 << 4,
		OrganicWaste = 1 << 5,
		AllGoods = Waste | Scrapyard | Food | Good
	}

	/// <summary>
	/// Allows structure to function like a storage or warehouse.
	/// </summary>
	public class StorageFeature : Feature<StorageFeatureSaveData>,
		IViewController<StorageAreaView>
	{
		public Action<StorageFeature> OnPolicyChanged;

		public StorageAreaView View { get; private set; }

		public override void Setup(BaseEntity entity, FeatureParamsBase args = null)
		{
			base.Setup(entity);
			StaticData = Entity.StaticData.GetFeature<StorageStaticData>();
			policy = StaticData.Policy;
		}

		//Static Data
		public StorageStaticData StaticData;

		//Properties
		private EStoragePolicy m_Policy;
		public EStoragePolicy policy
		{
			get
			{
				return m_Policy;
			}
			set
			{
				if (m_Policy != value)
				{
					m_Policy = value;
					OnPolicyChanged?.Invoke(this);
				}
			}

		}
				
		public float Priority { get { return StaticData.Priority; } }		
		public InventoryFeature Inventory { get; private set; }		

		public override void Run(SystemsManager manager)
		{
			base.Run(manager);
			manager.StorageSystem.RegisterFeature(this);
			Inventory = Entity.Inventory;
		}

		public override void Stop()
		{
			base.Stop();
			SystemsManager.StorageSystem.DeregisterFeature(this);
		}

		public override void OnFactionChanged(int oldFaction)
		{
			base.OnFactionChanged(oldFaction);
			SystemsManager.StorageSystem.DeregisterFeature(this, oldFaction);
			SystemsManager.StorageSystem.RegisterFeature(this);
		}		

		public override ISaveable CollectData()
		{
			var saveData = new StorageFeatureSaveData();
			saveData.storagePolicy = (int)policy;
			return saveData;
		}

		public override void ApplyData(StorageFeatureSaveData data)
		{
			policy = (EStoragePolicy)data.storagePolicy;
		}

		public int CurrentViewID { get; set; }

		public void SetView(FeatureViewBase view)
		{
			View = (StorageAreaView)view;
		}

		public void ShowView()
		{
			if (View != null)
				View.ShowHide(true);
		}

		public void HideView()
		{
			if (View != null)
				View.ShowHide(false);
		}

		public void UpdateView()
		{
			if (View != null)
				View.UpdateView();
		}

		public void SelectView()
		{
			if (View != null)
				View.OnViewSelected();
		}

		public void DeselectView()
		{
			if (View != null)
				View.OnViewDeselected();
		}
	}
}