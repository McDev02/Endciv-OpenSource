namespace Endciv
{
    public class RequestNewWateringCropsAction : AIAction<ActionSaveData>
    {
        CitizenAIAgentFeature citizen;
        string indexKey;
        string locationKey;
        WateringTask task;
        FarmlandFeature farm;

        public RequestNewWateringCropsAction(CitizenAIAgentFeature citizen, WateringTask task, FarmlandFeature farm, string indexKey, string locationKey)
        {
            this.citizen = citizen;
            this.indexKey = indexKey;
            this.locationKey = locationKey;
            this.task = task;
            this.farm = farm;
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
            Vector2i index = new Vector2i();
            if (farm.HasUnwateredPlant(Main.Instance.GameManager.SystemsManager.AgricultureSystem.agricultureSettings.cropsWateringBuffer, out index))
            {
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