using System;
using System.Collections.Generic;
using System.Linq;

namespace Endciv
{
	public class ResourcePileFeature :
		Feature<ResourcePileSaveData>,
		IViewController<ResourcePileView>
	{
		//Static Data
		public ResourcePileFeatureStaticData StaticData { get; private set; }

		public ResourcePileView View { get; private set; }

		public UI3DIconMark markIcon;

		public float collectingProgress;
		public float fullResources;
		public bool markedForCollection;
		public bool canCancelGathering = true;
		public BaseEntity overlappingConstructionSite = null;
		public CitizenAIAgentFeature assignedCollector = null;

		public int startResources;
		public List<ResourceStack> resources = new List<ResourceStack>();
		public ResourcePileSystem.EResourcePileType ResourcePileType { get; private set; }		

		public EStoragePolicy StoragePolicy
		{
			get
			{
				if (resources.Count > 0)
				{
					var data = Main.Instance.GameManager.Factories.SimpleEntityFactory.EntityStaticData[resources[0].ResourceID].
						GetFeature<ItemFeatureStaticData>();
					return data.Category;
				}
				else if(Entity.Inventory != null)
				{
					var resources = InventorySystem.GetChamberContentList(Entity.Inventory, 0);
					if (resources == null || resources.Length <= 0)
						return EStoragePolicy.AllGoods;
					var data = Main.Instance.GameManager.Factories.SimpleEntityFactory.EntityStaticData[resources.First().ResourceID].
						GetFeature<ItemFeatureStaticData>();
					return data.Category;
				}
				return EStoragePolicy.AllGoods;
			}
		}		

		public override void Setup(BaseEntity entity, FeatureParamsBase args = null)
		{
			base.Setup(entity);
			StaticData = Entity.StaticData.GetFeature<ResourcePileFeatureStaticData>();
			if(entity.StaticData.GetFeature<StoragePileFeatureStaticData>() == null)
			{
				ResourcePileType = ResourcePileSystem.EResourcePileType.ResourcePile;
				GenerateRandomResources();
			}			
			else
			{
				ResourcePileType = ResourcePileSystem.EResourcePileType.StoragePile;
			}
			
		}

		public override void Run(SystemsManager manager)
		{
			base.Run(manager);
			manager.ResourcePileSystem.RegisterFeature(this);			
		}

		public override void Stop()
		{
			base.Stop();
			SystemsManager.ResourcePileSystem.DeregisterFeature(this);
		}

		public override void OnFactionChanged(int oldFaction)
		{
			base.OnFactionChanged(oldFaction);
			SystemsManager.ResourcePileSystem.DeregisterFeature(this, oldFaction);
			SystemsManager.ResourcePileSystem.RegisterFeature(this);
		}

		void GenerateRandomResources()
		{
			foreach (var res in StaticData.Resources)
			{
				var amount = CivRandom.Range(res.minAmount, res.maxAmount);
				if (amount > 0)
				{
					resources.Add(new ResourceStack(res.resourceID, amount));
					startResources += amount;
				}
			}
		}

		#region IViewController

		public int CurrentViewID { get; set; }

		public void SetView(FeatureViewBase view)
		{
			View = (ResourcePileView)view;
		}

		public void ShowView()
		{
			if (View != null)
			{
				View.ShowHide(true);
			}
		}

		public void HideView()
		{
			if (View != null)
			{
				View.ShowHide(false);
			}
		}

		public void UpdateView()
		{
			if (View != null)
			{
				View.UpdateView();
			}
		}

		public void SelectView()
		{
			if (View != null)
			{
				View.OnViewSelected();
			}
		}

		public void DeselectView()
		{
			if (View != null)
			{
				View.OnViewDeselected();
			}
		}
		#endregion

		public override ISaveable CollectData()
		{
			var data = new ResourcePileSaveData();
			data.markedAsGathering = markedForCollection;
			data.canCancelGathering = canCancelGathering;
			data.collectionProgress = collectingProgress;
			data.fullResources = fullResources;
			if (overlappingConstructionSite != null)
				data.overlappingConstructionSiteID = overlappingConstructionSite.UID.ToString();
			else data.overlappingConstructionSiteID = string.Empty;
			if (assignedCollector != null)
				data.assignedGathererID = assignedCollector.Entity.UID.ToString();
			else
				data.assignedGathererID = string.Empty;
			data.resources = new List<ResourceSaveData>();
			data.startResources = startResources;
			foreach (var res in resources)
			{
				data.resources.Add(new ResourceSaveData(res.ResourceID, res.Amount));
			}
			return data;
		}

		public override void ApplyData(ResourcePileSaveData data)
		{
			if (data.markedAsGathering)
				ResourcePileSystem.MarkPileGathering(this, true, false);
			else
				markedForCollection = false;
			canCancelGathering = data.canCancelGathering;
			collectingProgress = data.collectionProgress;
			fullResources = data.fullResources;
			if (!string.IsNullOrEmpty(data.overlappingConstructionSiteID))
			{
				var uid = Guid.Parse(data.overlappingConstructionSiteID);
				if (Main.Instance.GameManager.SystemsManager.Entities.ContainsKey(uid))
					overlappingConstructionSite = Main.Instance.GameManager.SystemsManager.Entities[uid];
			}
			if (!string.IsNullOrEmpty(data.assignedGathererID))
			{
				var uid = Guid.Parse(data.assignedGathererID);
				if (Main.Instance.GameManager.SystemsManager.Entities.ContainsKey(uid))
					assignedCollector = Main.Instance.GameManager.SystemsManager.Entities[uid].GetFeature<CitizenAIAgentFeature>();
			}

			startResources = data.startResources;
			resources.Clear();
			foreach (var res in data.resources)
			{
				resources.Add(new ResourceStack(res.id, res.amount));
			}
		}
	}
}