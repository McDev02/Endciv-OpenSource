namespace Endciv
{
    public class WasteGatheringTask : AITask
    {
        public const string targetLocationKey = "TargetLocation";        

        private Location targetLocation;        

        public WasteGatheringTask() { }
        public WasteGatheringTask(BaseEntity unit, Location targetLocation) : base(unit)
        {
            this.targetLocation = targetLocation;            
        }

        public override void Initialize()
        {
            Main.Instance.GameManager.SystemsManager.WasteSystem.RegisterTile(targetLocation.Index);
            SetMemberValue<Location>(targetLocationKey, targetLocation);            
            InitializeStates();
            //Initiate
            StateTree.SetState("MoveToWaste");
        }

        public override void InitializeStates()
        {
            StateTree = new BranchingStateMachine<AIActionBase>(3);

            //Move to location
            StateTree.AddState("MoveToWaste", new MoveToAction( Entity.GetFeature<GridAgentFeature>(), this, targetLocationKey));
            
            //Gather waste        
            StateTree.AddNextState("GatherWaste", new GatherWasteAction(Entity.GetFeature<CitizenAIAgentFeature>(), targetLocationKey, this));

            //Checks if we can gather more waste before stopping
            //Sets the next location as targetLocation on success            
            StateTree.AddNextState("CheckNewTile", new RequestWasteTileAction(Entity.GetFeature<CitizenAIAgentFeature>(), this, targetLocationKey), "MoveToWaste");

        }        
    }
}
