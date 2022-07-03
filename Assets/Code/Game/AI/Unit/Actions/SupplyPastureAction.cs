using System.Collections.Generic;

namespace Endciv
{
	public class SupplyPastureAction : AIAction<TransferItemsActionSaveData>
	{
		private InventoryFeature from;
		private PastureFeature to;
		private List<ResourceStack> resources = null;
		private AITask task;

		private string fromInventoryKey;
		private string toPastureKey;
		private string resourcesKey;

		private int fromChamberID;
		private int toChamberID;		
		
		private bool canTransfer;

		public SupplyPastureAction(AITask task, string fromInventoryKey, string toPastureKey, string resourcesKey, int fromChamberID)
		{
			this.task = task;
			this.fromInventoryKey = fromInventoryKey;
			this.toPastureKey = toPastureKey;
			this.resourcesKey = resourcesKey;
			this.fromChamberID = fromChamberID;									
		}

		public override void Reset()
		{
			canTransfer = false;
		}

		public override void OnStart()
		{
			task.GetMemberValue<InventoryFeature>(fromInventoryKey).Entity.GetFeature<UnitFeature>().View.
					SwitchAnimationState(EAnimationState.PutDown, 1f, WaitForAnimation);
		}

		public override void ApplySaveData(TransferItemsActionSaveData data)
		{
			Status = (EStatus)data.status;
			canTransfer = data.canTransfer;
			if (!canTransfer)
				OnStart();
		}

		public override ISaveable CollectData()
		{
			var data = new TransferItemsActionSaveData();
			data.status = (int)Status;
			data.canTransfer = canTransfer;
			return data;
		}

		public override void Update()
		{
			if (!canTransfer)
			{
				Status = EStatus.Running;
				return;
			}
			from = task.GetMemberValue<InventoryFeature>(fromInventoryKey);
			to = task.GetMemberValue<PastureFeature>(toPastureKey);
			if (from == null || from.Entity == null || to == null || to.Entity == null)
			{
				Status = EStatus.Failure;
				return;
			}
			if (to.Entity.HasFeature<ConstructionFeature>() && to.Entity.GetFeature<ConstructionFeature>().MarkedForDemolition)
			{
				Status = EStatus.Failure;
				return;
			}
			var resources = task.GetMemberValue<List<ResourceStack>>(resourcesKey);
			if (resources == null)
			{
				resources = new List<ResourceStack>();
			}

			if (from == null || to == null || resources == null)
			{
				Status = EStatus.Failure;
				return;
			}

			for (int i = resources.Count - 1; i >= 0; i--)
			{
				var resource = resources[i];
				InventorySystem.UnreserveItems(from, resource.ResourceID, resource.Amount);
				InventorySystem.WithdrawItems(from, resource.ResourceID, resource.Amount);
				var data = Main.Instance.GameManager.Factories.SimpleEntityFactory.GetStaticData<ItemFeatureStaticData>(resource.ResourceID);
				if (data == null)
					continue;
				float water = 0;
				float food = 0;
				if(data.entity.HasFeature(typeof(ConsumableFeatureStaticData)))
				{
					var foodData = data.entity.GetFeature<ConsumableFeatureStaticData>();
					water = foodData.Water * resource.Amount;
					food = foodData.Nutrition * resource.Amount;
				}
				else if(resource.ResourceID == FactoryConstants.WaterID)
				{
					water = data.Mass * resource.Amount;
				}
				to.CurrentWater += water;
				to.CurrentNutrition += food;
			}
			Status = EStatus.Success;
		}

		private void WaitForAnimation(EAnimationState state)
		{
			if (state == EAnimationState.PutDown)
			{
				var inventory = task.GetMemberValue<InventoryFeature>(fromInventoryKey);
				canTransfer = true;
				inventory.Entity.GetFeature<UnitFeature>().View.UnregisterCallback(WaitForAnimation);
			}
		}

#if UNITY_EDITOR
		public override void DrawUIDetails()
		{
		}
#endif
	}
}