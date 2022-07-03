namespace Endciv
{
	public class StayHomeTask : AITask
	{
		private GridMap gridMap;

		public StayHomeTask()
        {
            gridMap = Main.Instance.GameManager.GridMap;
        }

		public StayHomeTask(BaseEntity entity, GridMap gridMap) : base(entity)
		{
			this.gridMap = gridMap;
		}

		public override void Initialize()
		{
			InitializeStates();
			SetState("MoveTo");
		}

		public override void InitializeStates()
		{
            var home = Entity.GetFeature<CitizenAIAgentFeature>().Home.Entity;
            var gridAgent = Entity.GetFeature<GridAgentFeature>();
            var homeLocation = new Location(home, true);
            StateTree = new BranchingStateMachine<AIActionBase>(4);
            StateTree.AddState("MoveTo", new MoveToAction(homeLocation, gridAgent));
            StateTree.AddNextState("EnterBuilding", new EnterLeaveBuildingAction(homeLocation, gridAgent, true));
            StateTree.AddNextState("Be Home", new StayHomeSheduleAction(Entity.GetFeature<CitizenAIAgentFeature>()));
            StateTree.AddNextState("LeaveBuilding", new EnterLeaveBuildingAction(homeLocation, gridAgent, false));
        }		
	}
}