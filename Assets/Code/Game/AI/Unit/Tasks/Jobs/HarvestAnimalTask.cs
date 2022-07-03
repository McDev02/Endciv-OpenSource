namespace Endciv
{
	public class HarvestAnimalTask : AITask
	{
		public const string cattleKey = "CattleKey";
		private const string cattleLocationKey = "CattleLocation";
		private const string pastureLocationKey = "PastureLocation";		

		private PastureFeature pasture;
		private CattleFeature cattle;

		public HarvestAnimalTask() { }
		public HarvestAnimalTask(BaseEntity entity, PastureFeature pasture, CattleFeature cattle) : base(entity)
		{
			this.pasture = pasture;
			this.cattle = cattle;
		}

		public override void Initialize()
		{
			var location = new Location(pasture.Entity, true);
			SetMemberValue<Location>(pastureLocationKey, location);
			SetMemberValue<CattleFeature>(cattleKey, cattle);
			var cattleLocation = new Location(cattle.Entity.GetFeature<EntityFeature>().GridID);
			SetMemberValue<Location>(cattleLocationKey, cattleLocation);
			InitializeStates();
			SetState("MoveTo");
		}

		public override void InitializeStates()
		{
			var cattle = GetMemberValue<CattleFeature>(cattleKey);
			cattle.Entity.GetFeature<AnimalAIAgentFeature>().Stop();
			cattle.Entity.GetFeature<UnitFeature>().View.SwitchAnimationState(EAnimationState.Idle);

			StateTree = new BranchingStateMachine<AIActionBase>(5);
			StateTree.AddState("MoveTo", new MoveToAction(Entity.GetFeature<GridAgentFeature>(), this, pastureLocationKey));
			StateTree.AddNextState("EnterBuilding", new EnterLeaveBuildingAction(null, Entity.GetFeature<GridAgentFeature>(), true, this, pastureLocationKey));
			StateTree.AddNextState("MoveToAnimal", new MoveToAction(Entity.GetFeature<GridAgentFeature>(), this, cattleLocationKey));
			StateTree.AddNextState("HarvestAnimal", new HarvestAnimalAction(Entity, this, cattleKey), "ExitBuilding", "ExitBuilding");
			StateTree.AddNextState("ExitBuilding", new EnterLeaveBuildingAction(null, Entity.GetFeature<GridAgentFeature>(), false, this, pastureLocationKey));			
		}
	}

}
