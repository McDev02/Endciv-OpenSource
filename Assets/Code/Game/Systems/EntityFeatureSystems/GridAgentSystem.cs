using System;

namespace Endciv
{
	public class GridAgentSystem : EntityFeatureSystem<GridAgentFeature>
	{
		public enum EAgentState
		{
			Idle, HasNewGoal, WaitForPath,
#if USE_FLOWFIELDS
            WalkingFlowfield,
#endif
			WalkingWaypoint, DebugWalk
		}

		const string DebungWatch = "GridAgentSystem";
		PathfindingManager PathfindingManager;
		GridMap GridMap;

		static private GridAgentSystem instance;
		PartitionSystem partitionSystem;
		Vector3 ForwardVec;

		public GridAgentSystem(int factions, GridMap gridMap, PartitionSystem partitionSystem) : base(factions)
		{
			instance = this;
			this.partitionSystem = partitionSystem;
			GridMap = gridMap;
			PathfindingManager = PathfindingManager.Instance;
			DebugStatistics.RegisterNewStopwatch(DebungWatch, 1);
			ForwardVec = Vector3.forward;
		}
		public override void UpdateStatistics()
		{
		}

		public override void UpdateGameLoop()
		{
			//DebugStatistics.RestartWatch(DebungWatch);

			GridAgentFeature agent;

			for (int f = 0; f < FeaturesByFaction.Count; f++)
			{
				for (int i = 0; i < FeaturesByFaction[f].Count; i++)
				{
					agent = FeaturesByFaction[f][i];
					//agent.State = GridAgentFeature.EAgentState.DebugWalk;

					switch (agent.State)
					{
						case EAgentState.Idle:
							break;
						case EAgentState.HasNewGoal:
							GetNewPath(agent);
							break;
						case EAgentState.WaitForPath:
							if (agent.Job.IsReady)
							{
#if USE_FLOWFIELDS
                            agent.State = EAgentState.WalkingFlowfield;
                            agent.Unit.View.UpdateWaypoint();
#else
								int count = agent.Job.path.Count;
								Vector2[] path = new Vector2[count];
								for (int n = 0; n < count; n++)
								{
									path[n] = GridMap.View.GetTileWorldPosition(agent.Job.path[n]);
								}
								agent.State = EAgentState.WalkingWaypoint;
								agent.Destination.Positions = path;
								//Debug.Log("Set destination: " + agent.Destination.uid);
								agent.Destination.Type = Location.EDestinationType.Waypoint;
								agent.Entity.GetFeature<UnitFeature>().View.UpdateWaypoint();
#endif
							}
							//Wait for up to 10 real seconds
							else if (agent.waitingTime >= 10)
							{
								Debug.LogError("Unit (" + agent.Entity.GetFeature<EntityFeature>().EntityName + ") is waiting too long for path! Goal is reset.");
								agent.waitingTime = 0;
								SetNewGoal(agent, null);
							}
							agent.waitingTime += Main.unscaledDeltaTimeSafe;
							break;
#if USE_FLOWFIELDS
                    case EAgentState.WalkingFlowfield:
                        MoveAgent(agent, agent.Flowmap);
                        break;
#endif
						case EAgentState.WalkingWaypoint:
							MoveAgentOnWaypoint(agent);
							break;
						case EAgentState.DebugWalk:
							DebugMoveAgent(agent);
							break;
						default:
							break;
					}
				}
			}
			//var time = DebugStatistics.CountRoundAndStop(DebungWatch);
			////	var time = DebugStatistics.GetAverageTime(DebungWatch);
			//Debug.Log("GridAgent Loop: " + time.ToString("0.000") + "ms");

		}

		private void MoveAgentOnWaypoint(GridAgentFeature agent)
		{
			var destination = agent.Destination;
			//Skip movement as ID doesn't match
			if (destination.currentPositionID < 0 || destination.currentPositionID >= destination.Positions.Length)
			{
				agent.ReachedGoal = false;
				agent.State = EAgentState.Idle;
				return;
			}
			var target = destination.Positions[destination.currentPositionID];

			if (MoveAgent(agent, target, false))
			{
				destination.currentPositionID++;
				if (destination.currentPositionID >= destination.Positions.Length)
				{
					agent.Entity.GetFeature<UnitFeature>().View.HideWaypointPath();
					agent.ReachedGoal = true;
					agent.State = EAgentState.Idle;
				}
				else
					agent.Entity.GetFeature<UnitFeature>().View.UpdateWaypoint();
			}
		}

		internal override void RegisterFeature(GridAgentFeature feature)
		{
			base.RegisterFeature(feature);

			//Initialize
			var entityTransform = feature.Entity.GetFeature<EntityFeature>().View.transform;
			var wpos = entityTransform.position.To2D();
			feature.Entity.GetFeature<EntityFeature>().GridID = GridMap.View.SampleTileWorld(wpos);
		}

#if USE_FLOWFIELDS
        bool MoveAgent(GridAgentFeature agent, Velocity2[,] flowmap)
        {
            var entityTransform = agent.Entity.cachedTransform;
            var wpos = entityTransform.position.To2D();
            agent.Unit.GridID = GridMap.View.SampleTileWorld(wpos);

            var clampedID = GridMap.Grid.ClampID(agent.Unit.GridID);

            var targetPosition = GridMap.View.GetTileWorldPosition(agent.Destination.Index);
            Vector3 dir;
            if (clampedID == agent.Destination.Index)
            {
                dir = (targetPosition - wpos).To3D();
            }
            else if ((agent.Destination.Index - clampedID).Magnitude <= 1)
            {
                dir = (targetPosition - wpos).To3D();
            }
            else
            {
                var nodeVector = flowmap[clampedID.X, clampedID.Y];
                dir = new Vector3(nodeVector.X, 0, nodeVector.Y);
            }
            Vector3 curDir = entityTransform.forward;
            curDir.y = 0;

            //Calculate speed
            float speed = agent.CurrentSpeed * Main.deltaTime;

            //Steering
            dir.Normalize();
            if (dir != Vector3.zero)
            {
                //Reduce speed and increase sterring if not facing target
                var dirfactor = Mathf.Clamp01(Vector2.Dot(curDir.To2D(), dir.To2D()));
                speed *= dirfactor;
                Vector3 tempDir = new Vector3(dir.x, 0, dir.z);
                entityTransform.rotation = Quaternion.RotateTowards(entityTransform.rotation, Quaternion.LookRotation(tempDir), agent.CurrentSteering * (3 - 2 * dirfactor) * Time.deltaTime);
            }

            //Old
            //entityTransform.LookAt(entityTransform.position + Vector3.Lerp(curDir, dir, agent.CurrentSteering * Time.deltaTime), Vector3.up);

            bool goalReached = false;
            //Movement
            //float tolerance = 1;// agent.Destination.Type == Location.EDestinationType.Structure ? 20 : 1;
            //Check if Goal will be reached after movement
            if ((targetPosition - wpos).magnitude < DEST_REACHED_BIAS)// speed * tolerance)
            {
                agent.ReachedGoal = true;
                agent.State = EAgentState.Idle;
                goalReached = true;
                agent.Unit.View.HideWaypointPath();
            }
            //Move
            if (speed != 0) GridObjectSystem.UpdateEntity(agent.Entity);
            ForwardVec.z = speed;
            entityTransform.Translate(ForwardVec, Space.Self);

            Vector3 offset = Vector3.up * 0.2f;
            var posfrom = entityTransform.position + offset;
            Debug.DrawLine(posfrom, targetPosition.To3D(0.05f), Color.green);
            agent.Unit.View.UpdateWaypointPosition();
            return goalReached;
        }
#endif
		bool MoveAgent(GridAgentFeature agent, Vector2 target, bool reachGoal = true)
		{
			var entityTransform = agent.Entity.GetFeature<EntityFeature>().View.transform;
			var wpos = entityTransform.position.To2D();
			if (!Mathf.Approximately(0f, agent.pathNodeOffset))
				wpos += entityTransform.forward.To2D() * agent.pathNodeOffset;
			agent.Entity.GetFeature<EntityFeature>().GridID = GridMap.View.SampleTileWorld(wpos);

			//Get direction
			Vector3 dir = (target - wpos).normalized.To3D();

			Vector3 curDir = entityTransform.forward;
			curDir.y = 0;


			bool goalReached = false;
			//Movement
			float speed = agent.CurrentSpeed;

			//Adjust speed based on rotation
			float dirfactor = 1;
			if (dir != Vector3.zero)
			{
				//Reduce speed if not facing target
				dirfactor = Mathf.Clamp01(Vector2.Dot(curDir.To2D(), dir.To2D()));
				//Prevent unit from being stuck if aiming exalty 180° to target.
				//Yes this is entityTransform.right!!! Do not change!
				if (dirfactor < 0.01f) dir += entityTransform.right * 0.01f;
				speed *= dirfactor;
			}
			//Steering
			var steer = Mathf.Lerp(agent.CurrentSteeringMax, agent.CurrentSteeringMin, dirfactor);
			entityTransform.LookAt(entityTransform.position + Vector3.Lerp(curDir, dir, steer * Main.deltaTimeSafe), Vector3.up);  //* (3 - 2 * dirfactor)

			//float tolerance = 1;
			//Check if Goal will be reached after movement
			var bias = Mathf.Max(1, speed) * GridMapView.TileSize * agent.destinationReachedBias * Mathf.Sqrt(Mathf.Max(1, Time.timeScale));
			if ((target - wpos).magnitude < bias)
			{
				if (reachGoal)
				{
					agent.ReachedGoal = true;
					agent.State = EAgentState.Idle;
					agent.Entity.GetFeature<UnitFeature>().View.HideWaypointPath();
				}
				goalReached = true;
			}
			//Move
			if (speed != 0f) partitionSystem.UpdateEntity(agent.Entity);
			ForwardVec.z = speed * Main.deltaTimeSafe;
			entityTransform.Translate(ForwardVec, Space.Self);

			Vector3 offset = Vector3.up * 0.2f;
			Debug.DrawLine(entityTransform.position + offset, target.To3D(0.05f), Color.green);
			agent.Entity.GetFeature<UnitFeature>().View.UpdateWaypointPosition();
			return goalReached;
		}

		void DebugMoveAgent(GridAgentFeature agent)
		{
			var entityTransform = agent.Entity.GetFeature<EntityFeature>().View.transform;
			var wpos = entityTransform.position.To2D();
			agent.Entity.GetFeature<EntityFeature>().GridID = GridMap.View.SampleTileWorld(wpos);

			Vector3 dir = (entityTransform.forward + entityTransform.right * 0.2f).normalized;
			Vector3 curDir = entityTransform.forward;
			curDir.y = 0;

			//Steer
			entityTransform.LookAt(entityTransform.position + Vector3.Lerp(curDir, dir, agent.CurrentSteeringMin * Main.deltaTimeSafe), Vector3.up);

			//Movement
			float speed = agent.CurrentSpeed * Main.deltaTimeSafe;

			//Move
			ForwardVec.z = speed;
			entityTransform.Translate(ForwardVec, Space.Self);
		}

		internal static bool SetNewGoal(GridAgentFeature agent, Location destination)
		{
			if (destination != null && destination.Indecies != null)
			{
				if (destination.Type == Location.EDestinationType.Position || destination.Type == Location.EDestinationType.Waypoint)
				{
					var indicies = destination.Indecies;
					for (int i = 0; i < indicies.Length; i++)
					{
						if (!instance.GridMap.IsPassable(indicies[i]))
						{
							Debug.Log("Destination of " + agent.Entity.GetFeature<EntityFeature>().View.name + " (Index: " + i.ToString() + ") is not passable: " + indicies[i].ToString());
							SetNewGoal(agent, null);
							return false;
						}
					}
				}
			}
			if (destination == null)
			{
				agent.Destination = null;
				agent.ReachedGoal = false;
				if (agent.Job != null)
				{
					PathfindingManager.Instance.CancelPathfindingJob(agent.Job);
					agent.Job = null;
				}
				agent.State = EAgentState.Idle;
				return false;
			}
			agent.Destination = destination;
			if (agent.Destination.Type == Location.EDestinationType.Waypoint)
			{
				agent.State = EAgentState.WalkingWaypoint;
				agent.Entity.GetFeature<UnitFeature>().View.UpdateWaypoint();
			}
			else
				agent.State = EAgentState.HasNewGoal;
			return true;
		}

		private void GetNewPath(GridAgentFeature agent)
		{
			//if (!GridMap.IsPassable(agent.Unit.GridID))
			//{
			//	Debug.LogError("Unit origin of " + agent.Unit.name + " is not passable: " + agent.Unit.GridID.ToString());
			//	SetNewGoal(agent, null);
			//	return;
			//}
			agent.waitingTime = 0;
			if (agent.Destination != null)
			{
				if (agent.Destination.Type == Location.EDestinationType.Position || agent.Destination.Type == Location.EDestinationType.Waypoint)
				{
					var indicies = agent.Destination.Indecies;
					for (int i = 0; i < indicies.Length; i++)
					{
						if (!GridMap.IsPassable(indicies[i]))
						{
							Debug.LogError("Destination of " + agent.Entity.GetFeature<EntityFeature>().View.name + " is not passable: " + indicies[i].ToString());
							SetNewGoal(agent, null);
							return;
						}
					}
				}
			}
			agent.ReachedGoal = false;
			if (agent.Job != null)
			{
				PathfindingManager.Instance.CancelPathfindingJob(agent.Job);
				agent.Job = null;
			}

			if (PathfindingManager.Instance.GetPathfindingJob
				(
					agent.Destination,
					new Location(agent.Entity.GetFeature<EntityFeature>().GridID),
					EPathfindingMode.FirstOrigin,
					ref agent.Job,
					agent.openAreaMinimum
				))
			{
				agent.State = EAgentState.WaitForPath;
			}
			else
			{
				agent.State = EAgentState.Idle;
			}
		}
	}
}