using System.Collections.Generic;

namespace Endciv
{
	public class ReserveItemsAction : AIAction<ActionSaveData>
	{
		private string resourcesKey;
		private string targetInventoryKey;
		private AITask task;

		public ReserveItemsAction(AITask task, string resourcesKey, string targetInventoryKey)
		{
			this.task = task;
			this.resourcesKey = resourcesKey;
			this.targetInventoryKey = targetInventoryKey;
		}

		public override void OnStart()
		{

		}

		public override void Reset()
		{

		}

		public override void Update()
		{
			var list = task.GetMemberValue<List<ResourceStack>>(resourcesKey);
			if(list == null)
			{
				Status = EStatus.Failure;
				return;
			}
			var inventory = task.GetMemberValue<InventoryFeature>(targetInventoryKey);
			if(inventory == null)
			{
				Status = EStatus.Failure;
				return;
			}
			bool hasReserved = false;
			foreach(var stack in list)
			{
				if(InventorySystem.ReserveItems(inventory, stack.ResourceID, stack.Amount))
				{
					hasReserved = true;
				}
			}
			if(!hasReserved)
			{
				Status = EStatus.Failure;
				return;
			}
			Status = EStatus.Success;
		}

		public override ISaveable CollectData()
		{
			var saveData = new ActionSaveData();
			saveData.status = (int)Status;
			return saveData;
		}

		public override void ApplySaveData(ActionSaveData data)
		{
			Status = (EStatus)data.status;
		}

#if UNITY_EDITOR
		public override void DrawUIDetails()
		{
			
		}
#endif
	}

}
