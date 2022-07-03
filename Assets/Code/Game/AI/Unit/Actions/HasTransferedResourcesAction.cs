using System.Collections.Generic;

namespace Endciv
{
    public class HasTransferedResourcesAction : AIAction<ActionSaveData>
    {
        private AITask task;
        private InventoryFeature inventory;
        private string resourcesKey;

        public HasTransferedResourcesAction(InventoryFeature inventory, AITask task, string resourcesKey)
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
            var resources = task.GetMemberValue<List<ResourceStack>>(resourcesKey);
            if (resources == null || inventory == null)
            {
                Status = EStatus.Failure;
                return;
            }
           
            //Iterate all resources on specified global variable key
            for (int i = resources.Count - 1; i >= 0; i--)
            {
                var resource = resources[i];
                //Check if unit's inventory contains resource and how many
                int amount = InventorySystem.GetItemCount(inventory, resource.ResourceID);
                //Resource has been removed from inventory; remove from list, move to next iteration
                if (amount <= 0)
                {
                    resources.RemoveAt(i);
                    continue;
                }
                //Set resource count to actual resources carried unless unit carries more than that
                resource.Amount = UnityEngine.Mathf.Min(amount, resource.Amount);                
            }
            task.SetMemberValue<List<ResourceStack>>(resourcesKey, resources);
            //If Unit still has resources in inventory, return success, otherwise return failure
            if (resources.Count > 0)
                Status = EStatus.Success;
            else
                Status = EStatus.Failure;
        }

#if UNITY_EDITOR
        public override void DrawUIDetails()
        {
        }
#endif
    }
}