namespace Endciv
{
    public class WateringAction : AIAction<PlantingActionSaveData>
    {
        private WateringTask task;
        private CitizenAIAgentFeature citizen;

        private float progress;
        private Vector2i currentPlantIndex;
        private FarmlandFeature farm;

        public WateringAction(CitizenAIAgentFeature citizen, WateringTask task, FarmlandFeature farm)
        {
            this.task = task;
            this.citizen = citizen;
            this.farm = farm;
        }

        public override void Reset()
        {
            progress = 0f;
        }

        public override void ApplySaveData(PlantingActionSaveData data)
        {
            Status = (EStatus)data.status;
            progress = data.progress;
            if (Status != EStatus.Failure && Status != EStatus.Success)
            {
                OnStart();
            }
        }

        public override ISaveable CollectData()
        {
            var data = new PlantingActionSaveData();
            data.status = (int)Status;
            data.progress = progress;
            return data;
        }

        public override void OnStart()
        {
            currentPlantIndex = task.GetMemberValue<Vector2i>("NextPlantIndex");
            citizen.Entity.GetFeature<UnitFeature>().View.
                SwitchAnimationState(EAnimationState.Working);
        }

        public override void Update()
        {
            if (farm == null || farm.Entity == null || farm.cropModels[currentPlantIndex.X, currentPlantIndex.Y] == null)
            {
                Status = EStatus.Failure;
                return;
            }
            progress += 0.2f * Main.deltaTimeSafe;
            if (progress >= 1f)
            {
                var crop = farm.cropModels[currentPlantIndex.X, currentPlantIndex.Y];
                if (crop == null)
                {
                    Status = EStatus.Failure;
                    return;
                }
                int itemCount = InventorySystem.GetItemCount(farm.Entity.Inventory, FactoryConstants.WaterID);
                if(itemCount <= 0)
                {
                    Status = EStatus.Failure;
                    return;
                }
                itemCount = UnityEngine.Mathf.Clamp(itemCount, 1, 3);
                for(int i = 0; i < itemCount; i++)
                {
                    if (crop.humidity.Progress >= 1f)
                        break;
                    InventorySystem.WithdrawItems(farm.Entity.Inventory, FactoryConstants.WaterID, 1);
                    crop.humidity.Value += GameConfig.WaterPortion;
                }                                
                farm.assignedFarmers[currentPlantIndex.X, currentPlantIndex.Y] = null;
                Status = EStatus.Success;
                return;
            }
            Status = EStatus.Running;
        }

#if UNITY_EDITOR
        public override void DrawUIDetails()
        {
            //UnityEngine.GUILayout.Label("Resting: " + Unit.Resting.Progress.ToString());
        }

#endif
    }
}