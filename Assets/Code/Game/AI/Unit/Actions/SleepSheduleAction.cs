namespace Endciv
{
	public class SleepSheduleAction : AIAction<ActionSaveData>
	{
		CitizenAIAgentFeature AIAgent;

		public SleepSheduleAction(CitizenAIAgentFeature aiAgent)
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
			AIAgent.Entity.GetFeature<UnitFeature>().View.SwitchAnimationState(EAnimationState.Sleeping);

			//Reset labour of the workforce so we can rotate next day
			var citizenAI = AIAgent.Entity.GetFeature<CitizenAIAgentFeature>();
			if (citizenAI != null && citizenAI.Occupation > EOccupation.Labour)
			{
				var system = Main.Instance.GameManager.SystemsManager.AIAgentSystem.CitizenAISystem;
				system.ChangeOccupation(citizenAI, EOccupation.Labour);
			}
		}

		public override void Update()
		{
			var shedule = CitizenAISystem.GetCurrentUnitShedule(AIAgent);
			if (shedule != CitizenShedule.ESheduleType.Sleep)
			{

				if (AIAgent.Entity.HasFeature<LivingBeingFeature>())
				{
					var being = AIAgent.Entity.GetFeature<LivingBeingFeature>();
					UnitSystem.ResetDailyValues(being);
				}
				Status = EStatus.Success;
			}
			else
				Status = EStatus.Running;
		}

#if UNITY_EDITOR
		public override void DrawUIDetails()
		{
			UnityEngine.GUILayout.Label("Sleep shedule");
		}
#endif
	}
}