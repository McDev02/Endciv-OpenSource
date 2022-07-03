namespace Endciv
{
	public class RoamingTask : AITask
	{
		private const string distanceKey = "MaxDistance";
		private float maxDistance;
		private float waitTime;
		private GridMap gridMap;

		public RoamingTask()
		{
			gridMap = Main.Instance.GameManager.GridMap;
		}

		public RoamingTask(BaseEntity unit, float maxDistance, MinMax waitTime, GridMap gridMap) : base(unit)
		{
			this.maxDistance = maxDistance;
			this.gridMap = gridMap;
			this.waitTime = waitTime.GetRandom();
		}

		public override void Initialize()
		{
			SetMemberValue<float>(distanceKey, maxDistance);
			InitializeStates();
			if (StateTree.States.Count == 1)
			{
				SetState("Wait");
			}
			else
			{
				SetState("MoveTo");
			}
		}

		public override void InitializeStates()
		{
			StateTree = new BranchingStateMachine<AIActionBase>(1);
			var dist = GetMemberValue<float>(distanceKey);
			Vector2i goal = Entity.GetFeature<EntityFeature>().GridID;

			//If current place of the unit is not passable, find the clossest passable location
			if (!gridMap.IsPassable(goal))
			{
				if (!gridMap.FindClosestEmptyTile(goal, 9999, out goal))
				{
					UnityEngine.Debug.LogError("No empy tile found!");
					StateTree.AddState("Wait", new WaitAction(waitTime));
				}
				else
				{
					//Walk to next passable tile
					var location = new Location(gridMap.View.GetTileWorldPosition(goal));
					StateTree.AddState("MoveTo", new MoveToAction(location, Entity.GetFeature<GridAgentFeature>()));
					StateTree.AddNextState("Wait", new WaitAction(waitTime));
				}
			}
			//Find roaming target
			else
			{
				if (gridMap.Grid.GetRandomPassablePosition(Entity.GetFeature<EntityFeature>().GridID, dist / 3f, dist, out goal))
				{
					var location = new Location(goal);
					StateTree.AddState("MoveTo", new MoveToAction(location, Entity.GetFeature<GridAgentFeature>()));
					StateTree.AddNextState("Wait", new WaitAction(waitTime));
				}
				else
				{
					//No position found, kill roam task? Causes Endless loop.
					//Also this is called for no reason it seems, maybe Grid.GetRandomPassablePosition is wrong?
					//UnityEngine.Debug.LogError("No path found, what to do?");
					UnityEngine.Debug.Log("New roaming task failed!");
					StateTree.AddState("Wait", new WaitAction(waitTime));
				}
			}
		}
	}
}