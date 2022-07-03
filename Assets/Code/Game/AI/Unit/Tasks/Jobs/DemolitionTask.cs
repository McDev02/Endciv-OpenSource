namespace Endciv
{
	public class DemolitionTask : AITask
	{
		private const string constructionSiteKey = "ConstructionSite";
		public ConstructionFeature ConstructionSite;

		public DemolitionTask() { }

		public DemolitionTask(BaseEntity citizen, ConstructionFeature facility)
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
			StateTree.AddNextState("Demolish", new DemolitionAction(Entity.GetFeature<CitizenAIAgentFeature>(), home.GetFeature<ConstructionFeature>()));
		}				
	}
}