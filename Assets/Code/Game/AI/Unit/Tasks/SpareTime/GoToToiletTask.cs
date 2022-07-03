namespace Endciv
{
	public class GoToToiletTask : AITask
	{
		private const string toiletLocationKey = "ToiletLocation";

		public UtilityFeature GetToilet()
		{
			var loc = GetMemberValue<Location>(toiletLocationKey);
			if (loc != null && loc.Structure != null)
			{
				var feature = loc.Structure.GetFeature<UtilityFeature>();
				return feature;
			}
			return null;
		}

		public GoToToiletTask() { }
		public GoToToiletTask(BaseEntity entity) : base(entity)
		{
		}

		public override void Initialize()
		{
			InitializeStates();
			SetState("FindToilet");
		}

		public override void InitializeStates()
		{
			StateTree = new BranchingStateMachine<AIActionBase>(5);
			StateTree.AddState("FindToilet", new FindToiletAction(Entity, this, toiletLocationKey));
			StateTree.AddNextState("MoveTo", new MoveToAction(Entity.GetFeature<GridAgentFeature>(), this, toiletLocationKey));
			StateTree.AddNextState("EnterBuilding", new EnterLeaveBuildingAction(null, Entity.GetFeature<GridAgentFeature>(), true, this, toiletLocationKey));
			StateTree.AddNextState("UseToilet", new UseToiletAction(Entity.GetFeature<CitizenAIAgentFeature>(), this, 5f));
			StateTree.AddNextState("LeaveBuilding", new EnterLeaveBuildingAction(null, Entity.GetFeature<GridAgentFeature>(), false, this, toiletLocationKey));
		}		
	}
}