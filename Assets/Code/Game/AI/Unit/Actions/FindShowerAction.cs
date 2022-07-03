namespace Endciv
{
    public class FindShowerAction : AIAction<ActionSaveData>
    {
        private BaseEntity unit;
        private AITask task;
        private int reserveID;

        private string targetShowerKey;

        public FindShowerAction(BaseEntity unit, AITask task, string targetShowerKey)
        {
            this.unit = unit;
            this.task = task;
            this.targetShowerKey = targetShowerKey;
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
            var manager = Main.Instance.GameManager.SystemsManager;
            var shower = manager.UtilitySystem.GetBestShowerForAgent(unit);
            if (shower == null)
            {
                Status = EStatus.Failure;
                return;
            }
            task.SetMemberValue<Location>(targetShowerKey, new Location(shower, true));
            shower.GetFeature<UtilityFeature>().Occupants.Add(unit);
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