namespace Endciv
{
	public class RestHomeTask : AITask
	{
		private GridMap gridMap;

		public RestHomeTask()
        {
            gridMap = Main.Instance.GameManager.GridMap;
        }

		public RestHomeTask(BaseEntity entity, GridMap gridMap) : base(entity)
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
            StateTree.AddNextState("Sleep", new SleepSheduleAction(Entity.GetFeature<CitizenAIAgentFeature>()));
            StateTree.AddNextState("LeaveBuilding", new EnterLeaveBuildingAction(homeLocation, gridAgent, false));
        }		
	}
}