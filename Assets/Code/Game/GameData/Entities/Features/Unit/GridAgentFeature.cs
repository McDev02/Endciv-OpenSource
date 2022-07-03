using UnityEngine;

namespace Endciv
{
	/// <summary>
	/// Entity component that moves on the grid
	/// </summary>
	public class GridAgentFeature : Feature<GridAgentSaveData>
	{
		private Vector3? position;

		public override void Setup(BaseEntity entity, FeatureParamsBase args = null)
		{
			base.Setup(entity);
			staticData = Entity.StaticData.GetFeature<GridAgentStaticData>();
			WalkingAnimationFactor = 1f / staticData.WalkingAnimationSpeed;
			var featureParams = (GridAgentFeatureParams)args;
			if(featureParams != null)
			{
				position = featureParams.Position;
			}
			//CurrentSteering = staticData.SteeringSpeed;
			//pathNodeOffset = staticData.PathfindingCenterOffset;
			//CurrentSpeed = BaseSpeed;
		}

		//internal void SetNewGoal(Location destination)
		//{
		//    if (destination == null)
		//    {
		//        Destination = null;
		//        State = EAgentState.Idle;
		//        return;
		//    }
		//	Destination = destination;
		//	if (Destination.Type == Location.EDestinationType.Waypoint)
		//	{
		//		State = EAgentState.WalkingWaypoint;
		//		Unit.View.UpdateWaypoint();
		//	}
		//	else
		//		State = EAgentState.HasNewGoal;
		//}

		GridAgentStaticData staticData;

		//StaticData
		public float WalkingAnimationFactor { get; private set; }     //Factor to multiply to walking animation

		//Properties
		public float CurrentSteeringMin { get { return staticData.Steering.min; } }
		public float CurrentSteeringMax { get { return staticData.Steering.max; } }
		public float pathNodeOffset { get { return staticData.PathfindingCenterOffset; } }
		public float destinationReachedBias { get { return staticData.destinationReachedBias; } }
		public float openAreaMinimum { get { return staticData.openAreaMinimum; } }

		public float CurrentSpeed { get { return staticData.Speed * speedModifer; } }    //At which speed the entity moves currently
		public float speedModifer = 1;

		public PathfinderJob Job;
#if USE_FLOWFIELDS
		public Velocity2[,] Flowmap { get { return Job.flowMap; } }
#endif

		public bool ReachedGoal { get; internal set; }

		public Location Destination;
		public float waitingTime;

		public GridAgentSystem.EAgentState State;

		public override void Run(SystemsManager manager)
		{
			base.Run(manager);
			if(position != null)
			{
				Entity.GetFeature<EntityFeature>().View.transform.position = position.Value;
			}
			manager.GridAgentSystem.RegisterFeature(this);
		}

		public override void Stop()
		{
			base.Stop();
			SystemsManager.GridAgentSystem.DeregisterFeature(this);
		}

		public override void OnFactionChanged(int oldFaction)
		{
			base.OnFactionChanged(oldFaction);
			SystemsManager.GridAgentSystem.DeregisterFeature(this, oldFaction);
			SystemsManager.GridAgentSystem.RegisterFeature(this);
		}

		public override ISaveable CollectData()
		{
			var data = new GridAgentSaveData();
			if (Destination != null)
				data.destination = (LocationSaveData)Destination.CollectData();
			return data;
		}

		public override void ApplyData(GridAgentSaveData data)
		{
			//CurrentSteering = saveData.currentSteering;
			//CurrentSpeed = saveData.currentSpeed;
			if (data.destination != null)
			{
				Destination = data.destination.ToLocation();
			}
		}
	}
}