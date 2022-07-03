namespace Endciv
{
    public class HarvestingTask : AgriculturingTask
    {
        public HarvestingTask() { }
        public HarvestingTask(BaseEntity farmer, FarmlandFeature farm, Vector2i plantIndex) 
			: base(farmer, farm, plantIndex) { }

        public override void Initialize()
        {
            base.Initialize();
            SetMemberValue<Vector2i>(plantIndexKey, plantIndex);
            SetMemberValue<Location>(plantLocationKey, farm.GetCropsLocation(plantIndex));
            farm.assignedFarmers[plantIndex.X, plantIndex.Y] = Entity.GetFeature<CitizenAIAgentFeature>();
            InitializeStates();
            //Initiate
            StateTree.SetState("MoveToFarm");
        }

        public override void InitializeStates()
        {
            StateTree = new BranchingStateMachine<AIActionBase>(3);
            var farmEntity = GetMemberValue<BaseEntity>(farmKey);
            //Move to Farm Structure
            StateTree.AddState("MoveToFarm", new MoveToAction(new Location( farmEntity, true ), Entity.GetFeature<GridAgentFeature>()));
            //[TODO] Move towards target crops
            StateTree.AddNextState("HarvestCrops", new HarvestingAction(Entity.GetFeature<CitizenAIAgentFeature>(), this, farmEntity.GetFeature<FarmlandFeature>()));
            //Loop back if there's more crops to harvest and we can carry at least 1 fruit from it
            StateTree.AddNextState("GetNextHarvestingCrops", new RequestNewHarvestingCropsAction(Entity.GetFeature<CitizenAIAgentFeature>(), this, farmEntity.GetFeature<FarmlandFeature>(), plantIndexKey, plantLocationKey), "HarvestCrops", null);
        }              
    }
}

