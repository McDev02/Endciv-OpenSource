using System.Collections.Generic;
using System.Linq;

namespace Endciv
{
    public class HasRemainingResourcesAction : AIAction<ActionSaveData>
    {
        private AITask task;
        private InventoryFeature inventory;
        private string resourcesKey;

        public HasRemainingResourcesAction(InventoryFeature inventory, AITask task, string resourcesKey)
        {
            this.inventory = inventory;
            this.task = task;
            this.resourcesKey = resourcesKey;
        }

        public override void Reset()
        {
            
        }

        public override void ApplySaveData(ActionSaveData data)
        {
            Status = (EStatus)data.status;
        }

        public override ISaveable CollectData()
        {
            var data = new ActionSaveData();
            data.status = (int)Status;
            return data;
        }

        public override void OnStart()
        {

        }

        public override void Update()
        {
            if (inventory == null || inventory.Load<=0)
            {
                Status = EStatus.Failure;
                return;
            }
            
            //Put all items on main chamber
            InventorySystem.UnreserveInventory(inventory);

			//Generate a list on the task variables with all items in the inventory
			var resources = InventorySystem.GetChamberContentList(inventory, 0).ToList();            
            task.SetMemberValue<List<ResourceStack>>(resourcesKey, resources);
            if (resources.Count <= 0)
            {
                Status = EStatus.Failure;
                return;
            }
            Status = EStatus.Success;
            return;

        }

#if UNITY_EDITOR
        public override void DrawUIDetails()
        {
        }
#endif
    }
}