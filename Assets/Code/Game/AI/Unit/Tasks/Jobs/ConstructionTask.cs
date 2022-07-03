namespace Endciv
{
	public class ConstructionTask : AITask
	{
		private const string constructionSiteKey = "ConstructionSite";
		private ConstructionFeature ConstructionSite;

		public ConstructionTask() { }

		public ConstructionTask(BaseEntity citizen, ConstructionFeature facility)
			: base(citizen, facility, EWorkerType.Worker)
		{
			ConstructionSite = facility;
		}

		public override void Initialize()
		{
			SetMemberValue<BaseEntity>(constructionSiteKey, ConstructionSite.Entity);
			InitializeStates();
			SetState("MoveTo");
		}

		public override void InitializeStates()
		{
			var home = GetMemberValue<BaseEntity>(constructionSiteKey);
			var gridAgent = Entity.GetFeature<GridAgentFeature>();
			StateTree = new BranchingStateMachine<AIActionBase>(2);
			var homeLocation = new Location(home, true);
			StateTree.AddState("MoveTo", new MoveToAction(homeLocation, gridAgent));
			StateTree.AddNextState("Construct", new ConstructionAction(Entity.GetFeature<CitizenAIAgentFeature>(), home.GetFeature<ConstructionFeature>(), this));
		}
	}
}