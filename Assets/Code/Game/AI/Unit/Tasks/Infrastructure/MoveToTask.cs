namespace Endciv
{
	public class MoveToTask : AITask, ISaveable, ILoadable<TaskSaveData>
	{
        private const string destinationKey = "Destination";
        private Location destination;

        public MoveToTask() { }
		public MoveToTask(BaseEntity entity, Location destination) : base(entity)
		{
            this.destination = destination;
		}

        public override void Initialize()
        {
            SetMemberValue<Location>(destinationKey, destination);
            InitializeStates();
            SetState("MoveTo");
        }

        public override void InitializeStates()
        {
            StateTree = new BranchingStateMachine<AIActionBase>(1);
            var dest = GetMemberValue<Location>(destinationKey);
            StateTree.AddState("MoveTo", new MoveToAction(dest, Entity.GetFeature<GridAgentFeature>()));
        }
    }
}