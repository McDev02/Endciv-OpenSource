namespace Endciv
{
    public class RequestWasteTileAction : AIAction<ActionSaveData>
    {
        CitizenAIAgentFeature citizen;
        string tileLocationKey;
        WasteGatheringTask task;

        public RequestWasteTileAction(CitizenAIAgentFeature citizen, WasteGatheringTask task, string tileLocationKey)
        {
            this.citizen = citizen;
            this.tileLocationKey = tileLocationKey;
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
            var tile = Main.Instance.GameManager.SystemsManager.WasteSystem.GetNewWasteTile(citizen);
            if (tile == null)
            {
                Status = EStatus.Failure;
                return;
            }
            var currentLocation = task.GetMemberValue<Location>(tileLocationKey);
            Main.Instance.GameManager.SystemsManager.WasteSystem.UnregisterTile(currentLocation.Index);
            Main.Instance.GameManager.SystemsManager.WasteSystem.RegisterTile(tile.Value);
            task.SetMemberValue<Location>(tileLocationKey, new Location(tile.Value));
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