namespace Endciv
{
    public class FindFoodWaterStorageAction : AIAction<ActionSaveData>
    {
        private GridAgentFeature agent;
        private FindFoodWaterTask task;

        private string targetLocationKey;
        private string targetStorageKey;
        private string requiredWaterKey;
        private string requiredNutritionKey;

        public FindFoodWaterStorageAction(GridAgentFeature agent, FindFoodWaterTask task, string requiredWaterKey, string requiredNutritionKey, string targetLocationKey, string targetStorageKey)
        {
            this.agent = agent;
            this.task = task;

            this.requiredNutritionKey = requiredNutritionKey;
            this.requiredWaterKey = requiredWaterKey;
            this.targetLocationKey = targetLocationKey;
            this.targetStorageKey = targetStorageKey;
        }

        public override void ApplySaveData(ActionSaveData data)
        {
            Status = (EStatus)data.status;
        }

        public override void Reset()
        {

        }

        public override ISaveable CollectData()
        {
            var data = new ActionSaveData();
            data.status = (int)Status;
            return data;
        }

        public override void OnStart()
        {
            Status = EStatus.Running;
        }

        public override void Update()
        {
            float nutrition = task.GetMemberValue<float>(requiredNutritionKey);
            int water = task.GetMemberValue<int>(requiredWaterKey);
            if(nutrition <= 0 && water <= 0)
            {
                Status = EStatus.Failure;
                return;
            }
            if(agent.Entity.HasFeature<CitizenAIAgentFeature>())
            {
                var citizen = agent.Entity.GetFeature<CitizenAIAgentFeature>();
                if (citizen.HasHome)
                {
                    if ((water > 0 && citizen.Home.Entity.Inventory.Statistics.WaterAvailable > 0) ||
                        (nutrition > 0 && citizen.Home.Entity.Inventory.Statistics.NutritionAvailable > 0))
                    {
                        var targetInventory = citizen.Home.Entity.Inventory;
                        task.SetMemberValue<InventoryFeature>(targetStorageKey, targetInventory);
                        task.SetMemberValue<Location>(targetLocationKey, new Location(targetInventory.Entity, true));
                        Status = EStatus.Success;
                        return;
                    }

                }
            }
            
            var manager = Main.Instance.GameManager.SystemsManager;

			if (water > 0 && StorageSystem.Statistics.WaterAvailable > 0)
			{
				var storage = manager.StorageSystem.GetClosestWaterStorage(agent);
				if (storage != null)
				{
					var targetInventory = storage.Inventory;
					task.SetMemberValue<InventoryFeature>(targetStorageKey, targetInventory);
					task.SetMemberValue<Location>(targetLocationKey, new Location(targetInventory.Entity, true));
					Status = EStatus.Success;
					return;
				}
			}
			if (nutrition > 0 && StorageSystem.Statistics.Nutrition > 0)
            {
                var storage = manager.StorageSystem.GetClosestFoodStorage(agent);
                if(storage != null)
                {
                    var targetInventory = storage.Inventory;
                    task.SetMemberValue<InventoryFeature>(targetStorageKey, targetInventory);
                    task.SetMemberValue<Location>(targetLocationKey, new Location(targetInventory.Entity, true));
                    Status = EStatus.Success;
                    return;
                }                                                
            }


            Status = EStatus.Failure;
            return;            
        }

#if UNITY_EDITOR
        public override void DrawUIDetails()
        {
        }
#endif
    }
}