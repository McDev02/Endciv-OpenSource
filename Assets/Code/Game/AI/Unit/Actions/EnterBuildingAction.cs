using UnityEngine;

namespace Endciv
{
	public class EnterLeaveBuildingAction : AIAction<EnterLeaveBuildingActionSaveData>
	{
		public BaseEntity Structure;
		WaypointPath Waypoints;
		public bool EnterOrLeave;
		GridAgentFeature Agent;
		Location Destination;
		bool GoToEntrancePoint;
		AITask task;
		string locationKey;
		Location location;

		public EnterLeaveBuildingAction(Location location, GridAgentFeature agent, bool enterOrLeave, AITask task = null, string locationKey = "")
		{
			Agent = agent;
			EnterOrLeave = enterOrLeave;
			this.task = task;
			this.locationKey = locationKey;
			this.location = location;
		}

		public override void Reset()
		{
			GoToEntrancePoint = false;
		}

		public override void ApplySaveData(EnterLeaveBuildingActionSaveData data)
		{
			Status = (EStatus)data.status;
			if (data.destination == null)
				OnStart();
			else
			{
				if (task != null && !string.IsNullOrEmpty(locationKey) && task.GetMemberValue<Location>(locationKey) != null)
				{
					location = task.GetMemberValue<Location>(locationKey);
				}
				Structure = location.Structure;
				if (!Structure.GetFeature<StructureFeature>().View.CanBeEntered)
				{
					Status = EStatus.Success;
					return;
				}
				Waypoints = Structure.GetFeature<StructureFeature>().View.path;
				Destination = data.destination.ToLocation();
				GoToEntrancePoint = data.goToEntrancePoint;
				GridAgentSystem.SetNewGoal(Agent, Destination);
				var unitFeature = Agent.Entity.GetFeature<UnitFeature>();
				//Turn View on if Leaving
				if (!EnterOrLeave && Structure.GetFeature<StructureFeature>().View.hideViewOnEnter)
					Agent.Entity.ShowView();
				if (unitFeature.IsCarrying)
				{
					unitFeature.View.SwitchAnimationState(EAnimationState.CarryWalking);
				}
				else
				{
					unitFeature.View.SwitchAnimationState(EAnimationState.Walking);
				}

			}
		}

		public override ISaveable CollectData()
		{
			var data = new EnterLeaveBuildingActionSaveData();
			if (Destination != null)
				data.destination = Destination.CollectData() as LocationSaveData;
			data.goToEntrancePoint = GoToEntrancePoint;
			data.status = (int)Status;
			return data;
		}

		public override void OnStart()
		{
			if (task != null && !string.IsNullOrEmpty(locationKey) && task.GetMemberValue<Location>(locationKey) != null)
			{
				location = task.GetMemberValue<Location>(locationKey);
			}
			Structure = location.Structure;
			if (!Structure.GetFeature<StructureFeature>().View.CanBeEntered)
			{
				Status = EStatus.Success;
				return;
			}
			Waypoints = Structure.GetFeature<StructureFeature>().View.path;
			Vector2[] points = new Vector2[Waypoints.points.Length];
			int j;
			for (int i = 0; i < Waypoints.points.Length; i++)
			{
				j = EnterOrLeave ? i : (Waypoints.points.Length - i - 1);
				points[i] = Waypoints.GetWorldPoint(j).To2D();
			}

			Destination = new Location(points);
			GridAgentSystem.SetNewGoal(Agent, Destination);
			var unitFeature = Agent.Entity.GetFeature<UnitFeature>();
			//Turn View on if Leaving
			if (!EnterOrLeave && Structure.GetFeature<StructureFeature>().View.hideViewOnEnter)
				Agent.Entity.ShowView();
			if (unitFeature.IsCarrying)
			{
				unitFeature.View.SwitchAnimationState(EAnimationState.CarryWalking);
			}
			else
			{
				unitFeature.View.SwitchAnimationState(EAnimationState.Walking);
			}
		}

		public override void Update()
		{
			if (!Structure.GetFeature<StructureFeature>().View.CanBeEntered)
			{
				Status = EStatus.Success;
				return;
			}
			if (Destination != Agent.Destination)
			{
				Status = EStatus.Failure;
				return;
			}

			if (Agent.State == GridAgentSystem.EAgentState.Idle)   //Agent is not moving anymore Check if we reached the goal or not
			{
				if (Agent.ReachedGoal)
				{
					//Turn View off if Entered
					if (EnterOrLeave)
					{
						if (Structure.GetFeature<StructureFeature>().View.hideViewOnEnter)
							Agent.Entity.HideView();
						Status = EStatus.Success;
						return;
					}
					//When Leaving
					else
					{
						//Find closest Entrance point and walk there
						var entrancePoints = Structure.GetFeature<GridObjectFeature>().GridObjectData.EntrancePoints;
						float closestDiff = float.MaxValue;
						Vector2i closestCell = Agent.Entity.GetFeature<EntityFeature>().GridID;
						if (!GoToEntrancePoint && entrancePoints.Length > 0)
						{
							for (int i = 0; i < entrancePoints.Length; i++)
							{
								var diff = (Agent.Entity.GetFeature<EntityFeature>().GridID - entrancePoints[i]).Magnitude;
								if (diff <= closestDiff)
								{
									closestCell = entrancePoints[i];
								}
							}
							GoToEntrancePoint = true;
							Destination = new Location(closestCell);
							GridAgentSystem.SetNewGoal(Agent, Destination);
							Status = EStatus.Running;
						}
						else
						{
							Status = EStatus.Success;
							return;
						}

					}
				}
				else
				{
					Status = EStatus.Failure;
					//In case we cause a bug we better enable the view of the unit
					Agent.Entity.ShowView();
					return;
				}
			}
		}

#if UNITY_EDITOR
		public override void DrawUIDetails()
		{
			GUILayout.Label("Agent.State: " + Agent.State.ToString());
		}
#endif
	}
}