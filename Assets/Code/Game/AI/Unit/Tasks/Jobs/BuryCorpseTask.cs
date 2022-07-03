namespace Endciv
{
    public class BuryCorpseTask : AITask
    {
        public const string corpseKey = "Corpse";
        public const string graveyardKey = "Graveyard";
        private const string corpseLocationKey = "CorpseLocation";
        private const string graveyardLocationKey = "GraveyardLocation";
        public const string gravePlotIDKey = "GravePlotID";

        private BaseEntity corpse;
        private GraveyardFeature graveyard;
        private int gravePlotID;

        public BuryCorpseTask() { }
        public BuryCorpseTask(BaseEntity unit, BaseEntity corpse, GraveyardFeature graveyard, int gravePlotID) : base(unit)
        {
            this.corpse = corpse;
            this.graveyard = graveyard;
            this.gravePlotID = gravePlotID;
        }

        protected override void OnFailure()
        {
            base.OnFailure();
            ReleaseCorpse();
            var corpseEntity = GetMemberValue<BaseEntity>(corpseKey);
            if (corpseEntity == null)
                return;            
            if (!corpseEntity.HasFeature<LivingBeingFeature>())
                return;
            var feature = corpseEntity.GetFeature<LivingBeingFeature>();
            feature.isGettingBurried = false;

            var plotID = GetMemberValue<int>(gravePlotIDKey);
            var graveyard = GetMemberValue<GraveyardFeature>(graveyardKey);
            if(graveyard != null && plotID > -1)
            {
                graveyard.UnreserveGravePlot(plotID);
            }
        }

        public void ReleaseCorpse ()
        {
            var corpseEntity = GetMemberValue<BaseEntity>(corpseKey);
            if (corpseEntity == null)
                return;            
            corpseEntity.ShowView();
            corpseEntity.GetFeature<UnitFeature>().MoveToPosition
                (Entity.GetFeature<EntityFeature>().GridID);
        }

        public override void Initialize()
        {
            corpse.GetFeature<LivingBeingFeature>().isGettingBurried = true;
            var location = new Location(corpse.GetFeature<EntityFeature>().GridID);
            SetMemberValue<Location>(corpseLocationKey, location);
            SetMemberValue<BaseEntity>(corpseKey, corpse);
            SetMemberValue<GraveyardFeature>(graveyardKey, graveyard);
            SetMemberValue<int>(gravePlotIDKey, gravePlotID);
            var graveyardLocation = new Location(graveyard.GetPlotCoordinates(gravePlotID));
            SetMemberValue<Location>(graveyardLocationKey, graveyardLocation);
            InitializeStates();
            //Initiate
            StateTree.SetState("MoveToCorpse");
        }

        public override void InitializeStates()
        {
            StateTree = new BranchingStateMachine<AIActionBase>(5);
            var aiAgent = Entity.GetFeature<CitizenAIAgentFeature>();
            //Move to Corpse
            StateTree.AddState("MoveToCorpse", new MoveToAction(Entity.GetFeature<GridAgentFeature>(), this, corpseLocationKey));
            //Pick up corpse
            StateTree.AddNextState("PickupCorpse", new PickupCorpseAction(aiAgent, this, corpseKey));
            //Move to graveyard
            StateTree.AddNextState("MoveToGraveyard", new MoveToAction( Entity.GetFeature<GridAgentFeature>(), this, graveyardLocationKey));
            //Drop corpse
            StateTree.AddNextState("DropCorpse", new DropCorpseAction(aiAgent, this, corpseKey));
            //Build grave      
            StateTree.AddNextState("BuryCorpse", new BuildGraveAction(aiAgent, this, corpseKey, graveyardKey, gravePlotIDKey));
        }               
    }
}
