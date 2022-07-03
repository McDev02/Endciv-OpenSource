namespace Endciv
{
    public class WateringTask : AgriculturingTask
    {
        public WateringTask() { }
        public WateringTask(BaseEntity farmer, FarmlandFeature farm, Vector2i plantIndex) : base(farmer, farm, plantIndex) { }

        public override void Initialize()
        {
            base.Initialize();
            //SetMemberValue<Location>("FarmEntrance", new Vector2i[] { new Vector2i( farm.Structure.GridObjectFeature.GridObjectData.EntrancePoints[0]) });
            SetMemberValue<Vector2i>(plantIndexKey, plantIndex);
            SetMemberValue<Location>(plantLocationKey, farm.GetCropsLocation(plantIndex));
            farm.assignedFarmers[plantIndex.X, plantIndex.Y] = Entity.GetFeature<CitizenAIAgentFeature>();
            InitializeStates();
            //Initiate
            StateTree.SetState("MoveToFarm");
        }

        public override void InitializeStates()
        {
            StateTree = new BranchingStateMachine<AIActionBase>(5);
            var farmEntity = GetMemberValue<BaseEntity>(farmKey);
            //Move to Farm Structure
            StateTree.AddState("MoveToFarm", new MoveToAction(new Location(farmEntity, true), Entity.GetFeature<GridAgentFeature>()));
            //TODO : FIX CROPS MOVEMENT
            //StateTree.AddState("MoveToCrops", new MoveToAction( Unit.Unit.GridAgent, this, plantLocationKey));            
            StateTree.AddNextState("WaterCrops", new WateringAction(Entity.GetFeature<CitizenAIAgentFeature>(), this, farmEntity.GetFeature<FarmlandFeature>()));
            //Loop back if there's more crops to plant
            //StateTree.AddNextState("GetNextPlantingCrops", new RequestNewPlantingCropsAction(Unit, this, plantIndexKey, plantLocationKey), "MoveToCrops", "ExitFarm");
            StateTree.AddNextState("GetNextWateringCrops", new RequestNewWateringCropsAction(Entity.GetFeature<CitizenAIAgentFeature>(), this, farmEntity.GetFeature<FarmlandFeature>(), plantIndexKey, plantLocationKey), "WaterCrops", null);
            //StateTree.AddState("ExitFarm", new MoveToAction( Unit.Unit.GridAgent, this, "FarmEntrance"));
        }
    }
}
