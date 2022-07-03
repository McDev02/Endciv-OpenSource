using System.Collections.Generic;

namespace Endciv
{
	/// <summary>
	/// Unit picks up resources form one location and brings them to target pasture.
	/// </summary>
	public class SupplyPastureTask : AITask, ISaveable, ILoadable<TaskSaveData>
	{
		private List<ResourceStack> resources;
		private InventoryFeature source;
		private PastureFeature target;		

		private const string sourceInventoryKey = "SourceInventory";
		private const string targetPastureKey = "TargetPasture";
		private const string unitInventoryKey = "UnitInventory";
		private const string transferedResourcesKey = "TransferedResources";
		private const bool GoInTarget = false;

		public SupplyPastureTask() : base() { }
		public SupplyPastureTask(BaseEntity unit, List<ResourceStack> resources, InventoryFeature source, PastureFeature target, IAIJob job)
			: base(unit, job, EWorkerType.Transporter)
		{
			this.resources = resources;
			this.source = source;
			this.target = target;
		}

		public override void Initialize()
		{
			SetMemberValue<InventoryFeature>(sourceInventoryKey, source);
			SetMemberValue<PastureFeature>(targetPastureKey, target);
			SetMemberValue<InventoryFeature>(unitInventoryKey, Entity.Inventory);
			SetMemberValue<List<ResourceStack>>(transferedResourcesKey, resources);			
			InitializeStates();
			SetState("ReserveResources");
		}

		public override void InitializeStates()
		{
			StateTree = new BranchingStateMachine<AIActionBase>(6);
			var source = GetMemberValue<InventoryFeature>(sourceInventoryKey);
			var target = GetMemberValue<PastureFeature>(targetPastureKey);
			Location sourceLocation = new Location(source.Entity, true);
			Location targetLocation = new Location(target.Entity, true);
			StateTree.AddState("ReserveResources", new ReserveItemsAction(this, transferedResourcesKey, sourceInventoryKey));
			StateTree.AddNextState("MoveToStorage", new MoveToAction(sourceLocation, Entity.GetFeature<GridAgentFeature>()));
			StateTree.AddNextState("EnterBuilding", new EnterLeaveBuildingAction(sourceLocation, Entity.GetFeature<GridAgentFeature>(), true));
			StateTree.AddNextState("PickUpResources", new TransferResourcesAction(this, sourceInventoryKey, unitInventoryKey, transferedResourcesKey, false, 0, 0, ETransferDirection.Take), "LeaveBuilding", "LeaveBuilding");
			StateTree.AddNextState("LeaveBuilding", new EnterLeaveBuildingAction(sourceLocation, Entity.GetFeature<GridAgentFeature>(), false));
			StateTree.AddNextState("MoveToTarget", new MoveToAction(targetLocation, Entity.GetFeature<GridAgentFeature>()));
			if (GoInTarget)
			{
				StateTree.AddNextState("EnterTarget", new EnterLeaveBuildingAction(targetLocation, Entity.GetFeature<GridAgentFeature>(), true));
				StateTree.AddNextState("PutDownResources", new SupplyPastureAction(this, unitInventoryKey, targetPastureKey, transferedResourcesKey, 0), "LeaveTarget", "LeaveTarget");
				StateTree.AddNextState("LeaveTarget", new EnterLeaveBuildingAction(targetLocation, Entity.GetFeature<GridAgentFeature>(), false));
			}				
			else
			{
				StateTree.AddNextState("PutDownResources", new SupplyPastureAction(this, unitInventoryKey, targetPastureKey, transferedResourcesKey, 0));
			}											
		}
	}
}