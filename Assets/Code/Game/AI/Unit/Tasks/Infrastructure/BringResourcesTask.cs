using System.Collections.Generic;

namespace Endciv
{
	/// <summary>
	/// Unit picks up resources form one location and brings them to another.
	/// </summary>
	public class BringResourcesTask : AITask, ISaveable, ILoadable<TaskSaveData>
	{
		private List<ResourceStack> resources;
		private InventoryFeature target;
		private InventoryFeature source;

		private const string sourceInventoryKey = "SourceInventory";
		private const string targetInventoryKey = "TargetInventory";
		private const string unitInventoryKey = "UnitInventory";
		private const string transferedResourcesKey = "TransferedResources";
		private const bool GoInTarget = false;

		public BringResourcesTask() : base() { }
		public BringResourcesTask(BaseEntity unit, List<ResourceStack> resources, InventoryFeature source, InventoryFeature target, IAIJob job) 
			: base(unit, job, EWorkerType.Transporter)
		{
			this.resources = resources;
			this.source = source;
			this.target = target;
		}

		public override void Initialize()
		{
			SetMemberValue<InventoryFeature>(sourceInventoryKey, source);
			SetMemberValue<InventoryFeature>(targetInventoryKey, target);
			SetMemberValue<InventoryFeature>(unitInventoryKey, Entity.Inventory);
			SetMemberValue<List<ResourceStack>>(transferedResourcesKey, resources);			
			InitializeStates();
			SetState("ReserveResources");

		}

		public override void InitializeStates()
		{
			StateTree = new BranchingStateMachine<AIActionBase>(6);
			var source = GetMemberValue<InventoryFeature>(sourceInventoryKey);
			var target = GetMemberValue<InventoryFeature>(targetInventoryKey);
			Location sourceLocation = new Location(source.Entity, true);
			Location targetLocation = new Location(target.Entity, true);
			StateTree.AddState("ReserveResources", new ReserveItemsAction(this, transferedResourcesKey, sourceInventoryKey));
			StateTree.AddNextState("MoveToStorage", new MoveToAction(sourceLocation, Entity.GetFeature<GridAgentFeature>()));
			StateTree.AddNextState("EnterBuilding", new EnterLeaveBuildingAction(sourceLocation, Entity.GetFeature<GridAgentFeature>(), true));
			StateTree.AddNextState("PickUpResources", new TransferResourcesAction(this, sourceInventoryKey, unitInventoryKey, transferedResourcesKey, false, 0, 0, ETransferDirection.Take), "LeaveBuilding", "LeaveBuilding");
			StateTree.AddNextState("LeaveBuilding", new EnterLeaveBuildingAction(sourceLocation, Entity.GetFeature<GridAgentFeature>(), false));
			StateTree.AddNextState("MoveToTarget", new MoveToAction(targetLocation, Entity.GetFeature<GridAgentFeature>()));
			if (GoInTarget)
				StateTree.AddNextState("EnterTarget", new EnterLeaveBuildingAction(targetLocation, Entity.GetFeature<GridAgentFeature>(), true));
			if (GoInTarget)
				StateTree.AddNextState("PutDownResources", new TransferResourcesAction(this, unitInventoryKey, targetInventoryKey, transferedResourcesKey, true, 0, 0, ETransferDirection.Give), "LeaveTarget", "LeaveTarget");
			else
				StateTree.AddNextState("PutDownResources", new TransferResourcesAction(this, unitInventoryKey, targetInventoryKey, transferedResourcesKey, true, 0, 0, ETransferDirection.Give));
			if (GoInTarget)
				StateTree.AddNextState("LeaveTarget", new EnterLeaveBuildingAction(targetLocation, Entity.GetFeature<GridAgentFeature>(), false));
		}
	}
}