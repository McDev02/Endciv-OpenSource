namespace Endciv
{
	public class PastureMaintainanceTask : AITask
	{
		public const string pastureKey = "Pasture";
		private const string pastureLocationKey = "PastureLocation";

		private PastureFeature pasture;

		public PastureMaintainanceTask() { }
		public PastureMaintainanceTask(BaseEntity entity, PastureFeature pasture) : base(entity)
		{
			this.pasture = pasture;
		}

		public override void Initialize()
		{
			var location = new Location(pasture.Entity, true);
			SetMemberValue<Location>(pastureLocationKey, location);
			SetMemberValue<PastureFeature>(pastureKey, pasture);			
			InitializeStates();
			SetState("MoveTo");
		}

		public override void InitializeStates()
		{			
			StateTree = new BranchingStateMachine<AIActionBase>(4);
			StateTree.AddState("MoveTo", new MoveToAction(Entity.GetFeature<GridAgentFeature>(), this, pastureLocationKey));
			StateTree.AddNextState("EnterBuilding", new EnterLeaveBuildingAction(null, Entity.GetFeature<GridAgentFeature>(), true, this, pastureLocationKey));			
			StateTree.AddNextState("MaintainPasture", new MaintainPastureAction(Entity, this, pastureKey), "ExitBuilding", "ExitBuilding");
			StateTree.AddNextState("ExitBuilding", new EnterLeaveBuildingAction(null, Entity.GetFeature<GridAgentFeature>(), false, this, pastureLocationKey));
		}
	}

}
