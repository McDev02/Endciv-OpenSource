namespace Endciv
{
    public class FindToiletAction : AIAction<ActionSaveData>
    {
        private BaseEntity unit;
        private AITask task;
        private int reserveID;

        private string targetToiletKey;        

        public FindToiletAction(BaseEntity unit, AITask task, string targetToiletKey)
        {
            this.unit = unit;
            this.task = task;
            this.targetToiletKey = targetToiletKey;
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
            var toilet = manager.UtilitySystem.GetBestToiletForAgent(unit);
            if (toilet == null)
            {
                Status = EStatus.Failure;
                return;
            }
            task.SetMemberValue<Location>(targetToiletKey, new Location(toilet, true));
            toilet.GetFeature<UtilityFeature>().Occupants.Add(unit);
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