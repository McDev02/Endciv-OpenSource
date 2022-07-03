namespace Endciv
{
	public class RequestResourcePileAction : AIAction<ActionSaveData>
	{
		CitizenAIAgentFeature citizen;
		string resourcePileKey;
		string resourceLocationKey;
        string actionTypeKey;
		ResourceCollectionTask task;

		public RequestResourcePileAction(CitizenAIAgentFeature citizen, ResourceCollectionTask task, string resourcePileKey, string resourceLocationKey, string actionTypeKey)
		{
			this.citizen = citizen;
			this.resourcePileKey = resourcePileKey;
			this.resourceLocationKey = resourceLocationKey;
            this.actionTypeKey = actionTypeKey;
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
            if(CitizenAISystem.GetCurrentUnitShedule(citizen) != CitizenShedule.ESheduleType.Work)
            {
                Status = EStatus.Failure;
                return;
            }
			var pile = Main.Instance.GameManager.SystemsManager.ResourcePileSystem.GetNewResourcePile(citizen);
            if (pile == null || pile.Entity == null)
			{
				Status = EStatus.Failure;
				return;
			}
			else
			{
                int actionType = task.GetMemberValue<ushort>(actionTypeKey);
                if(actionType == 0 && pile.ResourcePileType != ResourcePileSystem.EResourcePileType.ResourcePile)
                {
                    Status = EStatus.Failure;
                    return;
                }
                if (actionType == 1 && pile.ResourcePileType != ResourcePileSystem.EResourcePileType.StoragePile)
                {
                    Status = EStatus.Failure;
                    return;
                }
                task.UnassignGatherer();                
				task.SetMemberValue<ResourcePileFeature>(resourcePileKey, pile);
				task.SetMemberValue<Location>(resourceLocationKey, new Location(pile.Entity, false));
				pile.assignedCollector = citizen;
				Status = EStatus.Success;
				return;
			}
		}

#if UNITY_EDITOR
		public override void DrawUIDetails()
		{

		}

#endif
	}
}