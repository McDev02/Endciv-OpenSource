namespace Endciv
{
	public class ImmobileTask : AITask
	{
		private LivingBeingFeature being;

		public ImmobileTask(LivingBeingFeature being) : base(being.Entity)
		{
			this.being = being;
		}

		public override void Initialize()
		{
			InitializeStates();
			SetState("Rest");
		}

		public override void InitializeStates()
		{
			StateTree = new BranchingStateMachine<AIActionBase>(1);

			StateTree.AddState("Rest", new ImmobileAction(being));
		}
	}
}