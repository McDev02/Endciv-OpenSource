/// <summary>
/// This is a copy of the roaming task, used only for debug purposes to identify FindFoodState
/// </summary>
/// 
namespace Endciv
{
    public class FindFoodWaterTask : AITask
    {
        private const string nutritionKey = "Nutrition";
        private const string waterKey = "Water";
        private const string storageKey = "NextStorage";
        private const string locationKey = "NextLocation";

        private float nutrition;
        private int water;

		public FindFoodWaterTask() { }
        public FindFoodWaterTask(BaseEntity entity, float nutrition, int water) : base(entity)
        {
            this.nutrition = nutrition;
            this.water = water;
        }

        public override void Initialize()
        {
            SetMemberValue<float>(nutritionKey, nutrition);
            SetMemberValue<int>(waterKey, water);
            InitializeStates();
            SetState("FindStorage");            
        }

        public override void InitializeStates()
        {
            StateTree = new BranchingStateMachine<AIActionBase>(3);
            
            //Move to workshop
            StateTree.AddState("FindStorage", new FindFoodWaterStorageAction(Entity.GetFeature<GridAgentFeature>(), this, waterKey, nutritionKey, locationKey, storageKey));
            StateTree.AddNextState("MoveToStorage", new MoveToAction(Entity.GetFeature<GridAgentFeature>(), this, locationKey));
            StateTree.AddNextState("SatisfyNeeds", new ConsumeFoodWaterAction(Entity.GetFeature<CitizenAIAgentFeature>(), this, 3f, nutritionKey, waterKey, storageKey), null, "FindStorage");            
        }        
    }   
}