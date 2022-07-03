namespace Endciv
{
    public class PlantingAction : AIAction<PlantingActionSaveData>
    {
        private PlantingTask task;
        private CitizenAIAgentFeature citizen;

        private float progress;
        private Vector2i currentPlantIndex;
        private FarmlandFeature farm;

        public PlantingAction(CitizenAIAgentFeature citizen, PlantingTask task, FarmlandFeature farm)
        {            
            this.task = task;
            this.citizen = citizen;
            this.farm = farm;
        }

        public override void Reset()
        {
            progress = 0;
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
            if(farm == null || farm.Entity == null || farm.cropModels[currentPlantIndex.X, currentPlantIndex.Y] == null)
            {
                Status = EStatus.Failure;
                return;
            }
            progress += 0.2f * Main.deltaTimeSafe;
            if(progress >= 1f)
            {
                farm.cropModels[currentPlantIndex.X, currentPlantIndex.Y].cropState = ECropState.Growing;
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