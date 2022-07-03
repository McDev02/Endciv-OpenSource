namespace Endciv
{
	public class AnimalEnterPastureTask : AITask
	{		
		private const string pastureLocationKey = "PastureLocation";

		private PastureFeature pasture;

		public AnimalEnterPastureTask() { }
		public AnimalEnterPastureTask(BaseEntity entity, PastureFeature pasture) : base(entity)
		{
			this.pasture = pasture;
		}

		public override void Initialize()
		{
			var location = new Location(pasture.Entity, true);
			SetMemberValue<Location>(pastureLocationKey, location);			
			InitializeStates();
			SetState("MoveTo");
		}

		public override void InitializeStates()
		{
			StateTree = new BranchingStateMachine<AIActionBase>(4);			
			StateTree.AddState("MoveTo", new MoveToAction(Entity.GetFeature<GridAgentFeature>(), this, pastureLocationKey));
			StateTree.AddNextState("EnterBuilding", new EnterLeaveBuildingAction(null, Entity.GetFeature<GridAgentFeature>(), true, this, pastureLocationKey));			
		}
	}
}