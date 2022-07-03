using System.Collections.Generic;
using System.Linq;

namespace Endciv
{
	public class DropItemsAction : AIAction<TransferItemsActionSaveData>
	{
		private AITask task;
		private InventoryFeature inventory;
		private string resourcesKey;
		private bool canTransfer;

		public DropItemsAction(InventoryFeature inventory, AITask task, string resourcesKey)
		{
			this.inventory = inventory;
			this.task = task;
			this.resourcesKey = resourcesKey;
		}

		public override void OnStart()
		{
			inventory.Entity.GetFeature<UnitFeature>().View.
				SwitchAnimationState(EAnimationState.PutDown, 1f, WaitForAnimation);
		}

		public override void Reset()
		{
			canTransfer = false;
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
			var resources = task.GetMemberValue<List<ResourceStack>>(resourcesKey);
			if (resources == null || inventory == null || resources.Count <= 0)
			{
				Status = EStatus.Failure;
				return;
			}
			foreach (var res in resources)
			{
				var startAmount = res.Amount;
				for (int i = 0; i < startAmount; i++)
				{
					var withdrawItems = InventorySystem.WithdrawItems(inventory, res.ResourceID, 1);
					if (withdrawItems != null)
						ResourcePileSystem.PlaceStoragePile(inventory.Entity, withdrawItems);										
					res.Amount--;
				}
			}
			task.SetMemberValue<List<ResourceStack>>(resourcesKey, resources);
			if (inventory.Load <= 0)
				Status = EStatus.Success;
			else
				Status = EStatus.Failure;
		}

		private void WaitForAnimation(EAnimationState state)
		{
			if (state == EAnimationState.PutDown)
			{
				canTransfer = true;
				var unitFeature = inventory.Entity.GetFeature<UnitFeature>();
				unitFeature.View.UnregisterCallback(WaitForAnimation);
			}

		}

#if UNITY_EDITOR
		public override void DrawUIDetails()
		{
		}
#endif
	}
}