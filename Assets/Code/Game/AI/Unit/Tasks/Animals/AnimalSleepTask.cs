namespace Endciv
{
    public class AnimalSleepTask : AITask, ISaveable, ILoadable<TaskSaveData>
    {
        private const string distanceKey = "MaxDistance";
        private float maxDistance;
		private float duration;

		public AnimalSleepTask() { }
        public AnimalSleepTask(BaseEntity entity, float maxDistance,  MinMax waitTime) : base(entity)
        {
            this.maxDistance = maxDistance;
			duration= waitTime.GetRandom();
        }

        public override void Initialize()
        {
            SetMemberValue<float>(distanceKey, maxDistance);
            InitializeStates();
            if (StateTree.States.Count == 1)
            {
                SetState("Sleep");
            }
            else
            {
                SetState("MoveTo");
            }
        }

        public override void InitializeStates()
        {
            StateTree = new BranchingStateMachine<AIActionBase>(1);
            var dist = GetMemberValue<float>(distanceKey);
            Vector2i goal = Entity.GetFeature<EntityFeature>().GridID;
            if (!Main.Instance.GameManager.GridMap.Grid.GetRandomPassablePosition(Entity.GetFeature<EntityFeature>().GridID, dist, out goal))
            {
                StateTree.AddState("Sleep", new AnimalSleepAction(Entity.GetFeature<AnimalAIAgentFeature>(), duration));
            }
            else
            {
                var location = new Location(goal);
                StateTree.AddState("MoveTo", new MoveToAction(location, Entity.GetFeature<GridAgentFeature>()));
                StateTree.AddNextState("Sleep", new AnimalSleepAction(Entity.GetFeature<AnimalAIAgentFeature>(), duration));
            }
        }
    }
}