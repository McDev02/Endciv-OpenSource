namespace Endciv
{
    public class EmptyInventoryTask : AITask, ISaveable, ILoadable<TaskSaveData>
    {
        private const string unitInventoryKey = "UnitInventory";
        private const string transferedResourcesKey = "TransferedResources";
        private const string targetLocationKey = "TargetLocation";
        private const string targetStorageKey = "TargetStorage";
        private const string finalResourcesKey = "FinalResources";

        public EmptyInventoryTask() {}
        public EmptyInventoryTask(BaseEntity unit) : base(unit)
        {
       
        }

        public override void Initialize()
        {
            //Store global members to dictionary
            SetMemberValue<InventoryFeature>(unitInventoryKey, Entity.Inventory);

            InitializeStates();
            
            //Initiate
            StateTree.SetState("CheckResources");
        }

        public override void InitializeStates()
        {
            StateTree = new BranchingStateMachine<AIActionBase>(8);

            //Check for remaining resources. On failure end task, on success move to next state
            StateTree.AddState("CheckResources", new HasRemainingResourcesAction(Entity.Inventory, this, transferedResourcesKey));

            //Find storage that can accept one or all of the resources, store on global variables the following members:
            //Storage Location as "TargetLocation"
            //Storage Inventory as "TargetStorage"
            //Resources to bring specifically to this storage as "FinalResources"
            //On Failure drop all items on the ground
            StateTree.AddNextState("FindStorage", new FindDepositStorageAction(Entity.GetFeature<GridAgentFeature>(), this, 0, transferedResourcesKey, targetLocationKey, targetStorageKey, finalResourcesKey), "MoveToStorage", "DropItems");
            //Triggers on FindStorage Failure
            StateTree.AddState("DropItems", new DropItemsAction(Entity.Inventory, this, transferedResourcesKey));

            //Go to target storage
            StateTree.AddState("MoveToStorage", new MoveToAction( Entity.GetFeature<GridAgentFeature>(), this, targetLocationKey));
            //Enter storage building
            StateTree.AddNextState("EnterStorage", new EnterLeaveBuildingAction(null, Entity.GetFeature<GridAgentFeature>(), true, this, targetLocationKey));
            //Deposit resources to storage
            StateTree.AddNextState("PutDownResources", new TransferResourcesAction(this as AITask, unitInventoryKey, targetStorageKey, finalResourcesKey, true, 0, 0, ETransferDirection.Give), "LeaveStorage", "LeaveStorage");
            //Exit storage building
            StateTree.AddNextState("LeaveStorage", new EnterLeaveBuildingAction(null, Entity.GetFeature<GridAgentFeature>(), false, this, targetLocationKey));

            //Check if unit still have resources to transfer left, on success go to FindStorage State
            StateTree.AddNextState("CheckRemainingResources", new HasRemainingResourcesAction(Entity.Inventory, this, transferedResourcesKey), "FindStorage");

        }
    }
}