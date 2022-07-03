namespace Endciv
{
	public class GoToShowerTask : AITask
	{
		private const string showerLocationKey = "ShowerLocation";
		
		public UtilityFeature GetShower()
		{
			var loc = GetMemberValue<Location>(showerLocationKey);
			if (loc != null && loc.Structure != null)
			{
				var feature = loc.Structure.GetFeature<UtilityFeature>();
				return feature;
			}
			return null;
		}

		public GoToShowerTask() { }
		public GoToShowerTask(BaseEntity entity) : base(entity)
		{
		}

		public override void Initialize()
		{
			InitializeStates();
			SetState("FindShower");
		}

		public override void InitializeStates()
		{
			StateTree = new BranchingStateMachine<AIActionBase>(5);
			StateTree.AddState("FindShower", new FindShowerAction(Entity, this, showerLocationKey));
			StateTree.AddNextState("MoveTo", new MoveToAction(Entity.GetFeature<GridAgentFeature>(), this, showerLocationKey));
			StateTree.AddNextState("EnterBuilding", new EnterLeaveBuildingAction(null, Entity.GetFeature<GridAgentFeature>(), true, this, showerLocationKey));
			StateTree.AddNextState("UseShower", new UseShowerAction(Entity.GetFeature<CitizenAIAgentFeature>(), this, 5f));
			StateTree.AddNextState("LeaveBuilding", new EnterLeaveBuildingAction(null, Entity.GetFeature<GridAgentFeature>(), false, this, showerLocationKey));
		}
	}
}