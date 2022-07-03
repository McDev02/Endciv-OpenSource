namespace Endciv
{
	public class ProductionTask : AITask
	{
		private const string facilityKey = "Facility";
		private ProductionFeature facility;

		public ProductionTask() { }
		public ProductionTask(BaseEntity citizen, ProductionFeature facility)
			: base(citizen, facility, EWorkerType.Worker)
		{
			this.facility = facility;
		}

		public override void Initialize()
		{
			SetMemberValue<BaseEntity>(facilityKey, facility.Entity);
			InitializeStates();
			SetState("MoveTo");
		}

		public override void InitializeStates()
		{
			var facility = GetMemberValue<BaseEntity>(facilityKey);
			var gridAgent = Entity.GetFeature<GridAgentFeature>();
			StateTree = new BranchingStateMachine<AIActionBase>(4);
			var homeLocation = new Location(facility, true);
			StateTree.AddState("MoveTo", new MoveToAction(homeLocation, gridAgent));
			StateTree.AddNextState("EnterBuilding", new EnterLeaveBuildingAction(homeLocation, gridAgent, true));
			StateTree.AddNextState("Produce", new ProductionAction(Entity.GetFeature<CitizenAIAgentFeature>(), facility.GetFeature<ProductionFeature>()), "LeaveBuilding", "LeaveBuilding");
			StateTree.AddNextState("LeaveBuilding", new EnterLeaveBuildingAction(homeLocation, gridAgent, false));
		}		
	}
}