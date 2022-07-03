namespace Endciv
{
	public class ScavengingTask : AITask
	{
		protected const string expeditionKey = "Expedition";

		private ExpeditionFeature expeditionFeature;

		public ScavengingTask() { }
		public ScavengingTask(BaseEntity citizen, ExpeditionFeature expedition) : base(citizen)
		{
			expeditionFeature = expedition;			
		}

		public override void Initialize()
		{
			SetMemberValue<ExpeditionFeature>(expeditionKey, expeditionFeature);
			InitializeStates();
			SetState("Set State Attending");
		}

		public override void InitializeStates()
		{
			var gridAgent = Entity.GetFeature<GridAgentFeature>();
			var expedition = GetMemberValue<ExpeditionFeature>(expeditionKey);
			var aiAgent = Entity.GetFeature<CitizenAIAgentFeature>();
			StateTree = new BranchingStateMachine<AIActionBase>(4);

			StateTree.AddState("Set State Attending", new CustomAction(() => { expedition.SetState(aiAgent, AIGroupSystem.EAssigneeState.Attending); }, null));
			StateTree.AddNextState("Find provisions", new FindProvisionsForExpeditionAction(aiAgent, expedition, 3, 1), "Gather Togehter", "Gather Togehter");

			StateTree.AddNextState("Gather Togehter", new MoveToAction(expedition.gatherLocation, gridAgent));
			StateTree.AddNextState("Set State Ready", new CustomAction(
				() =>
				{
					expedition.SetState(aiAgent, AIGroupSystem.EAssigneeState.Ready);
				}, null));
			StateTree.AddNextState("Wait for start", new WaitConditionAction(new System.Func<bool>(
				() => { return expedition.state == ExpeditionFeature.EState.Started; })));
			StateTree.AddNextState("Move Outside", new MoveToAction(expedition.expeditionLocation, gridAgent));
			StateTree.AddNextState("On Expedition", new OnExpeditionAction(aiAgent, expedition));

			//Unit shall put stuff to the inventory autonomously as their inventory is full
		}
	}
}