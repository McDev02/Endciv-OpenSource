namespace Endciv
{
	public class MoveToAction : AIAction<ActionSaveData>
	{
		public Location Destination;
		GridAgentFeature Agent;
		AITask task;
		string locationKey;

		public MoveToAction(Location destination, GridAgentFeature agent)
		{
			Agent = agent;
			if (destination != null)
				Destination = destination.GetCopy();//To prevent bugs we make a copy of the location as its currentPositionID value can cause issues
			task = null;
			locationKey = null;
		}

		public MoveToAction( GridAgentFeature agent, AITask task, string destinationKey)
		{
			Agent = agent;
			Destination = null;
			this.task = task;
			this.locationKey = destinationKey;
		}

		public override void Reset()
		{

		}

		public override void ApplySaveData(ActionSaveData data)
		{
			Status = (EStatus)data.status;
			OnStart();
		}

		public override ISaveable CollectData()
		{
			var data = new ActionSaveData();
			data.status = (int)Status;
			return data;
		}

		public override void OnStart()
		{
			Location loc = null;
			if (task != null && !string.IsNullOrEmpty(locationKey) && (loc = task.GetMemberValue<Location>(locationKey)) != null)
			{
				Destination = loc;
			}

			GridAgentSystem.SetNewGoal(Agent, Destination);
		}

		public override void Update()
		{
            var unitFeature = Agent.Entity.GetFeature<UnitFeature>();

            if (unitFeature.IsCarrying)
                unitFeature.View.SwitchAnimationState(EAnimationState.CarryWalking, Agent.CurrentSpeed * Agent.WalkingAnimationFactor);
			else
                unitFeature.View.SwitchAnimationState(EAnimationState.Walking, Agent.CurrentSpeed * Agent.WalkingAnimationFactor);
			if (Destination != Agent.Destination)  //Agent has new destination
			{
				Status = EStatus.Failure;
			}

			if (Agent.State == GridAgentSystem.EAgentState.Idle)   //Agent is not moving anymore Check if we reached the goal or not
			{
				//  Status = Agent.ReachedGoal ? EStatus.Success : EStatus.Failure;
				if (Agent.ReachedGoal)
					Status = EStatus.Success;
				else Status = EStatus.Failure;

			}
		}

#if UNITY_EDITOR
		public override void DrawUIDetails()
		{
			UnityEngine.GUILayout.Label("Agent.State: " + Agent.State.ToString());
		}
#endif
	}
}