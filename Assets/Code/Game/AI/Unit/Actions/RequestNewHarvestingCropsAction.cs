﻿namespace Endciv
{
    public class RequestNewHarvestingCropsAction : AIAction<ActionSaveData>
    {
        CitizenAIAgentFeature citizen;
        string indexKey;
        string locationKey;
        HarvestingTask task;
        private FarmlandFeature farm;

        public RequestNewHarvestingCropsAction(CitizenAIAgentFeature citizen, HarvestingTask task, FarmlandFeature farm, string indexKey, string locationKey)
        {
            this.citizen = citizen;
            this.indexKey = indexKey;
            this.locationKey = locationKey;
            this.farm = farm;
            this.task = task;
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
            if (CitizenAISystem.GetCurrentUnitShedule(citizen) != CitizenShedule.ESheduleType.Work)
            {
                Status = EStatus.Failure;
                return;
            }
            Vector2i index = new Vector2i();
            if (farm.HasGrownPlant(out index))
            {
                var crops = farm.cropModels[index.X, index.Y];
                if(!InventorySystem.CanAddItems(citizen.Entity.Inventory, crops.staticData.fruit, 1))
                {
                    Status = EStatus.Failure;
                    return;
                }
                task.SetMemberValue<Vector2i>(indexKey, index);
                task.SetMemberValue<Location>(locationKey, farm.GetCropsLocation(index));
                farm.assignedFarmers[index.X, index.Y] = citizen;
                Status = EStatus.Success;
                return;
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