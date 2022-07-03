namespace Endciv
{
	public class SleepOutsideTask : AITask
	{
        private const string distanceKey = "MaxDistance";
		private float maxDistance;
		private GridMap gridMap;

		public SleepOutsideTask()
        {
            gridMap = Main.Instance.GameManager.GridMap;
        }

		public SleepOutsideTask(BaseEntity entity, float maxDistance, GridMap gridMap) : base(entity)
		{
			this.maxDistance = maxDistance;
			this.gridMap = gridMap;
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
			if (!gridMap.IsPassable(goal))
			{
				if (!gridMap.FindClosestEmptyTile(goal, 9999, out goal))
				{
					UnityEngine.Debug.LogError("No empy tile found!");
					StateTree.AddNextState("Sleep", new SleepSheduleAction(Entity.GetFeature<CitizenAIAgentFeature>()));
				}
				else
				{
					//Walk to next passable tile
					var location = new Location(gridMap.View.GetTileWorldPosition(goal).To3D());
					StateTree.AddState("MoveTo", new MoveToAction(location, Entity.GetFeature<GridAgentFeature>()));
					StateTree.AddNextState("Sleep", new SleepSheduleAction(Entity.GetFeature<CitizenAIAgentFeature>()));
				}
			}
			else
			{
				if (!gridMap.Grid.GetRandomPassablePosition(Entity.GetFeature<EntityFeature>().GridID, dist, out goal))
				{
					//No possition found, kill roam task? Causes Endless loop.
					//Also this is called for no reason it seems, maybe Grid.GetRandomPassablePosition is wrong?
					//UnityEngine.Debug.LogError("No path found, what to do?");
					UnityEngine.Debug.Log("New roaming task failed!");
					StateTree.AddNextState("Sleep", new SleepSheduleAction(Entity.GetFeature<CitizenAIAgentFeature>()));
				}
				else
				{
					var location = new Location(goal);
					StateTree.AddState("MoveTo", new MoveToAction(location, Entity.GetFeature<GridAgentFeature>()));
					StateTree.AddNextState("Sleep", new SleepSheduleAction(Entity.GetFeature<CitizenAIAgentFeature>()));
				}
			}
		}		
	}
}