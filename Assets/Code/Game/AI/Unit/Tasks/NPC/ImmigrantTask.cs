namespace Endciv
{
	public class ImmigrantTask : AITask
	{
		private const string destinationKey = "Destination";
		private Location destination;
		private GridMap gridMap;

		public ImmigrantTask()
		{
			gridMap = Main.Instance.GameManager.GridMap;
		}

		public ImmigrantTask(BaseEntity entity, Location destination, GridMap gridMap) : base(entity)
		{
			this.gridMap = gridMap;
			this.destination = destination;
		}

		public override void Initialize()
		{
			SetMemberValue<Location>(destinationKey, destination);
			InitializeStates();
			SetState("Move to Town");
		}

		public override void InitializeStates()
		{
			var immigrant = Entity.GetFeature<ImmigrantAIAgentFeature>();
			var destination = GetMemberValue<Location>(destinationKey);
			StateTree = new BranchingStateMachine<AIActionBase>(1);
			StateTree.AddState("Move to Town", new MoveToAction(Entity.GetFeature<GridAgentFeature>(), this, destinationKey), null, "Quit");
			StateTree.AddNextState("Setstate waiting", new CustomAction(
				() => { immigrant.State = EImmigrantState.Waiting; }, null));

			StateTree.AddNextState("Wait", new WaitConditionAction(new System.Func<bool>(
				() => { return immigrant.State != EImmigrantState.Waiting; })));			

			StateTree.AddNextState("Find destination", new CustomAction(() =>
			{
				Vector2i pos;
				gridMap.GetRandomPassablePositionOnEdge(out pos);
				SetMemberValue<Location>(destinationKey, new Location(pos));
			}, null), null, "Quit");

			StateTree.AddNextState("Move out of town", new MoveToAction(Entity.GetFeature<GridAgentFeature>(), this, destinationKey));
			StateTree.AddNextState("Quit", new CustomAction(
				() => { immigrant.Entity.Destroy(); }, null));
		}
	}

}
