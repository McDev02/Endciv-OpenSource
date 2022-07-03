using System.Collections.Generic;

namespace Endciv
{
    public class StoreResourcesTask : AITask
    {
        private List<ResourceStack> resources;
        private InventoryFeature targetFrom;
		private int chamberID;

        private const string sourceInventoryKey = "SourceInventory";
		private const string sourceChamberID = "SourceChamberID";
		private const string targetInventoryKey = "TargetInventory";		
        private const string unitInventoryKey = "UnitInventory";
        private const string transferedResourcesKey = "TransferedResources";
        private const string targetStorageKey = "TargetStorage";
        private const string finalResourcesKey = "FinalResources";
        private const string targetLocationKey = "TargetLocation";

        public StoreResourcesTask() { }
        public StoreResourcesTask(BaseEntity entity, List<ResourceStack> resources, InventoryFeature targetFrom, int inventoryChamberID) : base(entity)
        {
            this.resources = resources;
            this.targetFrom = targetFrom;
			chamberID = inventoryChamberID;
        }

        public override void Initialize()
        {
            //Store global members to dictionary
            SetMemberValue<InventoryFeature>(sourceInventoryKey, targetFrom);
			SetMemberValue<int>(sourceChamberID, chamberID);
            SetMemberValue<InventoryFeature>(unitInventoryKey, Entity.Inventory);
            SetMemberValue<List<ResourceStack>>(transferedResourcesKey, resources);            

            InitializeStates();

            //Initiate
            StateTree.SetState("ReserveResources");
        }

        public override void InitializeStates()
        {
            var targetFrom = GetMemberValue<InventoryFeature>(sourceInventoryKey);
			var chamberID = GetMemberValue<int>(sourceChamberID);
            var structure = targetFrom.Entity;
            StateTree = new BranchingStateMachine<AIActionBase>(5);
            var workshopLocation = new Location(structure,true);
			StateTree.AddState("ReserveResources", new ReserveItemsAction(this, transferedResourcesKey, sourceInventoryKey));
            //Move to workshop
            StateTree.AddNextState("MoveToSourceStructure", new MoveToAction(workshopLocation, Entity.GetFeature<GridAgentFeature>()));
            //Enter workshop
            StateTree.AddNextState("EnterSourceStructure", new EnterLeaveBuildingAction(workshopLocation, Entity.GetFeature<GridAgentFeature>(), true));
            //Pick up resources 
            StateTree.AddNextState("PickUpResources", new TransferResourcesAction(this as AITask, sourceInventoryKey, unitInventoryKey, transferedResourcesKey, false, chamberID, 0, ETransferDirection.Take), "LeaveSourceStructure", "LeaveSourceStructure");
            //Leave workshop
            StateTree.AddNextState("LeaveSourceStructure", new EnterLeaveBuildingAction(workshopLocation, Entity.GetFeature<GridAgentFeature>(), false));

            //Find storage that can accept one or all of the resources, store on global variables the following members:
            //Storage Location as "TargetLocation"
            //Storage Inventory as "TargetStorage"
            //Resources to bring specifically to this storage as "FinalResources"
            StateTree.AddNextState("FindStorage", new FindDepositStorageAction(Entity.GetFeature<GridAgentFeature>(), this, 1, transferedResourcesKey, targetLocationKey, targetStorageKey, finalResourcesKey));

            //Go to target storage
            StateTree.AddNextState("MoveToStorage", new MoveToAction( Entity.GetFeature<GridAgentFeature>(), this, targetLocationKey));
            //Enter storage building
            StateTree.AddNextState("EnterStorage", new EnterLeaveBuildingAction(null, Entity.GetFeature<GridAgentFeature>(), true, this, targetLocationKey));
            //Deposit resources to storage
            StateTree.AddNextState("PutDownResources", new TransferResourcesAction(this as AITask, unitInventoryKey, targetStorageKey, finalResourcesKey, true, 0, 0, ETransferDirection.Give), "LeaveStorage", "LeaveStorage");
            //Exit storage building
            StateTree.AddNextState("LeaveStorage", new EnterLeaveBuildingAction(null, Entity.GetFeature<GridAgentFeature>(), false, this, targetLocationKey));
            //Check if unit still have resources to transfer left, on success go to FindStorage State
            StateTree.AddNextState("CheckRemainingResources", new HasTransferedResourcesAction(Entity.Inventory, this, transferedResourcesKey), "FindStorage");
        }
    }
}