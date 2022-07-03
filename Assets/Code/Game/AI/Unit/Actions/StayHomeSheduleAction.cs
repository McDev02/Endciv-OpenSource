namespace Endciv
{
	public class StayHomeSheduleAction : AIAction<ActionSaveData>
	{
		CitizenAIAgentFeature AIAgent;

		public StayHomeSheduleAction(CitizenAIAgentFeature aiAgent)
		{
			AIAgent = aiAgent;
		}

		public override void Reset()
		{

		}

		public override void ApplySaveData(ActionSaveData data)
		{
			Status = (EStatus)data.status;
			if (Status != EStatus.Success && Status != EStatus.Failure)
			{
				OnStart();
			}
		}

		public override ISaveable CollectData()
		{
			var data = new ActionSaveData();
			data.status = (int)Status;
			return data;
		}


		public override void OnStart()
		{
			AIAgent.Entity.GetFeature<UnitFeature>().View.
                SwitchAnimationState(EAnimationState.Sleeping);
		}

		public override void Update()
		{
			var shedule = CitizenAISystem.GetCurrentUnitShedule(AIAgent);
			if (shedule != CitizenShedule.ESheduleType.Hometime)
			{
				Status = EStatus.Success;
			}
			else
				Status = EStatus.Running;
		}

#if UNITY_EDITOR
		public override void DrawUIDetails()
		{
			UnityEngine.GUILayout.Label("Stay Home shedule");
		}
#endif
	}
}