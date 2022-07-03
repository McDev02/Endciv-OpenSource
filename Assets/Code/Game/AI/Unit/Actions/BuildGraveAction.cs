namespace Endciv
{
    public class BuildGraveAction : AIAction<BuildGraveActionSaveData>
    {
        private AITask task;
        private CitizenAIAgentFeature aiAgent;
        private string corpseKey;
        private string graveyardKey;
        private string graveplotIDKey;
        private int graveID = -1;

        public BuildGraveAction(CitizenAIAgentFeature aiAgent, AITask task, string corpseKey, string graveyardKey, string graveplotIDKey)
        {
            this.aiAgent = aiAgent;
            this.task = task;
            this.corpseKey = corpseKey;
            this.graveyardKey = graveyardKey;
            this.graveplotIDKey = graveplotIDKey;
        }

        public override void OnStart()
        {
            
        }

        public override void Reset()
        {

        }

        public override void ApplySaveData(BuildGraveActionSaveData data)
        {
            Status = (EStatus)data.status;
            graveID = data.graveID;
        }

        public override ISaveable CollectData()
        {
            var data = new BuildGraveActionSaveData();
            data.status = (int)Status;
            data.graveID = graveID;
            return data;
        }


        public override void Update()
        {                        
            var graveyard = task.GetMemberValue<GraveyardFeature>(graveyardKey);
            if (graveyard == null)
            {
                Status = EStatus.Failure;
                return;
            }
            if(graveID == -1)
            {
                graveID = task.GetMemberValue<int>(graveplotIDKey);
                GraveyardSystem.BuryDeceased(graveyard, graveID);
                aiAgent.Entity.GetFeature<UnitFeature>().View.SwitchAnimationState(EAnimationState.Working, 1f);
            }
                

            if(graveID == -1)
            {
                Status = EStatus.Failure;
                return;
            }

            float progress = graveyard.AddConstructionPoints(graveID, Main.deltaTimeSafe * 0.1f);
            if(progress > 1f)
            {
                var corpse = task.GetMemberValue<BaseEntity>(corpseKey);
                if (corpse == null)
                {
                    Status = EStatus.Failure;
                    return;
                }
                Main.Instance.GameManager.SystemsManager.EntitySystem.DestroyEntity(corpse);
                Status = EStatus.Success;
                return;
            }
            Status = EStatus.Running;
            
        }        

#if UNITY_EDITOR
        public override void DrawUIDetails()
        {
        }
#endif
    }
}