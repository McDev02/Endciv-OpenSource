namespace Endciv
{
    public class DemolitionAction : AIAction<DemolitionActionSaveData>
    {
        CitizenAIAgentFeature citizen;
        ConstructionFeature facility;
        public bool inDemolition = false;

        public DemolitionAction(CitizenAIAgentFeature citizen, ConstructionFeature facility)
        {
            this.citizen = citizen;
            this.facility = facility;
        }

        public override void Reset()
        {
            inDemolition = false;
        }

        public override void ApplySaveData(DemolitionActionSaveData data)
        {
            Status = (EStatus)data.status;
			inDemolition = data.inDemolition;
            if (inDemolition)
                OnStart();
        }

        public override ISaveable CollectData()
        {
            var data = new DemolitionActionSaveData();
            data.status = (int)Status;
            data.inDemolition = inDemolition;
            return data;
        }

        public override void OnStart()
        {
            if(!facility.MarkedForDemolition)
            {
                Status = EStatus.Failure;
                return;
            }
			inDemolition = true;
            ConstructionSystem.InitiateDemolition(facility);
            citizen.Entity.GetFeature<UnitFeature>().View.
                SwitchAnimationState(UnityEngine.Random.Range(0f, 1f) < 0.5f ? 
                EAnimationState.HammeringStanding : EAnimationState.HammeringKneeling);
        }

        public override void Update()
        {            
            //Facility destroyed?
            if (facility == null || facility.Entity == null)
            {
                Status = EStatus.Success;
				inDemolition = false;
                return;
            }
            if (!facility.MarkedForDemolition)
            {
                Status = EStatus.Failure;
                return;
            }
            //Facility Demolished
            if (facility.DemolitionProgress >= 1f)
            {
                Status = EStatus.Success;                
				inDemolition = false;
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