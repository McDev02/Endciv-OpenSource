namespace Endciv
{
    public class PredatorFindFoodTask : AITask, ISaveable, ILoadable<TaskSaveData>
    {
        private const string distanceKey = "MaxDistance";
        private float maxDistance;

        public PredatorFindFoodTask() { }
        public PredatorFindFoodTask(BaseEntity entity, float maxDistance) : base(entity)
        {
            this.maxDistance = maxDistance;
        }

        public override void Initialize()
        {
            SetMemberValue<float>(distanceKey, maxDistance);
            InitializeStates();
            if (StateTree.States.Count == 1)
            {
                SetState("Eat");
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
          

        }
    }
}