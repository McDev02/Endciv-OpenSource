namespace Endciv
{
    public class DropCorpseAction : AIAction<TransferItemsActionSaveData>
    {
        private AITask task;
        private CitizenAIAgentFeature aiAgent;
        private string corpseKey;
        private bool canTransfer;

        public DropCorpseAction(CitizenAIAgentFeature aiAgent, AITask task, string corpseKey)
        {
            this.aiAgent = aiAgent;
            this.task = task;
            this.corpseKey = corpseKey;
        }

        public override void OnStart()
        {
            aiAgent.Entity.GetFeature<UnitFeature>().View.
                SwitchAnimationState(EAnimationState.PutDown, 1f, WaitForAnimation);
        }

        public override void Reset()
        {
            canTransfer = false;
        }

        public override void ApplySaveData(TransferItemsActionSaveData data)
        {
            Status = (EStatus)data.status;
            canTransfer = data.canTransfer;
            if (!canTransfer)
                OnStart();

        }

        public override ISaveable CollectData()
        {
            var data = new TransferItemsActionSaveData();
            data.status = (int)Status;
            data.canTransfer = canTransfer;
            return data;
        }


        public override void Update()
        {
            if (!canTransfer)
            {
                Status = EStatus.Running;
                return;
            }
            var corpse = task.GetMemberValue<BaseEntity>(corpseKey);
            if (corpse == null)
            {
                Status = EStatus.Failure;
                return;
            }
            (task as BuryCorpseTask).ReleaseCorpse();
            Status = EStatus.Success;
        }

        private void WaitForAnimation(EAnimationState state)
        {
            if (state == EAnimationState.PutDown)
            {
                canTransfer = true;
                aiAgent.Entity.GetFeature<UnitFeature>().View.UnregisterCallback(WaitForAnimation);
            }

        }

#if UNITY_EDITOR
        public override void DrawUIDetails()
        {
        }
#endif
    }
}