using System.Collections.Generic;

namespace Endciv
{
	public enum ETransferDirection
	{
		Take,
		Give
	}

	public class TransferResourcesAction : AIAction<TransferItemsActionSaveData>
	{
		private InventoryFeature from;
		private InventoryFeature to;
		private List<ResourceStack> resources = null;
		private AITask task;

		private string fromInventoryKey;
		private string toInventoryKey;
		private string resourcesKey;

		private int fromChamberID;
		private int toChamberID;
		private bool canOverflow;

		private ETransferDirection transferDirection;
		private bool canTransfer;

		public TransferResourcesAction(AITask task, string fromInventoryKey, string toInventoryKey, string resourcesKey, bool canOverflow, int fromChamberID, int toChamberID, ETransferDirection transferDirection)
		{
			this.task = task;
			this.fromInventoryKey = fromInventoryKey;
			this.toInventoryKey = toInventoryKey;
			this.resourcesKey = resourcesKey;
			this.fromChamberID = fromChamberID;
			this.toChamberID = toChamberID;
			this.canOverflow = canOverflow;
			this.transferDirection = transferDirection;
		}

		public override void Reset()
		{
			canTransfer = false;
		}

		public override void OnStart()
		{
			if (transferDirection == ETransferDirection.Give)
				task.GetMemberValue<InventoryFeature>(fromInventoryKey).Entity.GetFeature<UnitFeature>().View.
                    SwitchAnimationState(EAnimationState.PutDown, 1f, WaitForAnimation);
			else
				task.GetMemberValue<InventoryFeature>(toInventoryKey).Entity.GetFeature<UnitFeature>().View.
                    SwitchAnimationState(EAnimationState.PickUp, 1f, WaitForAnimation);
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
			to = task.GetMemberValue<InventoryFeature>(toInventoryKey);
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
			bool hasTransfered = false;

			for (int i = resources.Count - 1; i >= 0; i--)
			{
				var resource = resources[i];
				InventorySystem.UnreserveItems(from, resource.ResourceID, resource.Amount);
				if (!InventorySystem.TransferItems(from, to, resource.ResourceID, resource.Amount, canOverflow, fromChamberID, toChamberID))
				{
					UnityEngine.Debug.LogWarning($"Could not transfer: {resource.ResourceID}");
					continue;
				}
				hasTransfered = true;
			}
			if (hasTransfered)
				Status = EStatus.Success;
			else
				Status = EStatus.Failure;
		}

		private void WaitForAnimation(EAnimationState state)
		{
			if ((state == EAnimationState.PutDown && transferDirection == ETransferDirection.Give) ||
				(state == EAnimationState.PickUp && transferDirection == ETransferDirection.Take))
			{
				var inventory = (transferDirection == ETransferDirection.Give) ? task.GetMemberValue<InventoryFeature>(fromInventoryKey) : task.GetMemberValue<InventoryFeature>(toInventoryKey);
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