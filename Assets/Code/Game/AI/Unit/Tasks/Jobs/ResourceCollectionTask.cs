namespace Endciv
{
	public class ResourceCollectionTask : AITask
	{
		private const string unitInventoryKey = "UnitInventory";
		private const string nextPileKey = "NextPile";
		private const string nextLocationKey = "NextLocation";
		private const string targetStorageKey = "TargetStorage";
		private const string targetLocationKey = "TargetLocation";
		private const string transferedResourcesKey = "TransferedResources";
		private const string actionTypeKey = "ActionType";

		private ResourcePileFeature pile;
		private InventoryFeature targetInventory;

		public ResourceCollectionTask() { }
		public ResourceCollectionTask(BaseEntity unit, ResourcePileFeature resourcePile, InventoryFeature targetInventory)
			: base(unit)
		{
			pile = resourcePile;
			this.targetInventory = targetInventory;
		}

		protected override void OnFailure()
		{
			base.OnFailure();
			UnassignGatherer();
		}

		public void UnassignGatherer()
		{
			var pile = GetMemberValue<ResourcePileFeature>(nextPileKey);
			if (pile != null && pile.Entity != null)
				pile.assignedCollector = null;
		}

		public override void Initialize()
		{
			pile.assignedCollector = Entity.GetFeature<CitizenAIAgentFeature>();

			SetMemberValue<InventoryFeature>(unitInventoryKey, Entity.Inventory);
			SetMemberValue<ResourcePileFeature>(nextPileKey, pile);
			if (pile.ResourcePileType == ResourcePileSystem.EResourcePileType.ResourcePile)
			{
				SetMemberValue<ushort>(actionTypeKey, 0);
			}
			else if (pile.ResourcePileType == ResourcePileSystem.EResourcePileType.StoragePile)
			{
				SetMemberValue<ushort>(actionTypeKey, 1);
			}
			var pileLocation = new Location(pile.Entity, false);
			SetMemberValue<Location>(nextLocationKey, pileLocation);

			if (targetInventory != null)
			{
				SetMemberValue<InventoryFeature>(targetStorageKey, targetInventory);
				SetMemberValue<Location>(targetLocationKey, new Location(targetInventory.Entity, false));
			}
			InitializeStates();
			//Initiate
			StateTree.SetState("MoveToResourcePile");
		}

		public override void InitializeStates()
		{
			var inv = GetMemberValue<InventoryFeature>(targetStorageKey);
			if (inv != null)
				StateTree = new BranchingStateMachine<AIActionBase>(6);
			else
				StateTree = new BranchingStateMachine<AIActionBase>(3);

			var pile = GetMemberValue<ResourcePileFeature>(nextPileKey);
			int actionType = GetMemberValue<ushort>(actionTypeKey);
			//Move to workshop
			StateTree.AddState("MoveToResourcePile", new MoveToAction(Entity.GetFeature<GridAgentFeature>(), this, nextLocationKey));
			if (actionType == 0)
			{
				//Gather resource pile
				StateTree.AddNextState("GatherResources", new ResourcePileCollectionAction(Entity.GetFeature<CitizenAIAgentFeature>(), nextPileKey, this));
			}
			else
			{
				//Gather storage pile
				StateTree.AddNextState("GatherResources", new StoragePileCollectingAction(Entity.GetFeature<CitizenAIAgentFeature>(), nextPileKey, this));
			}

			if (inv != null)
			{
				//Go to target storage
				StateTree.AddNextState("MoveToStorage", new MoveToAction(Entity.GetFeature<GridAgentFeature>(), this, targetLocationKey));
				//Enter storage building
				StateTree.AddNextState("EnterStorage", new EnterLeaveBuildingAction(null, Entity.GetFeature<GridAgentFeature>(), true, this, targetLocationKey));
				//Deposit resources to storage
				StateTree.AddNextState("PutDownResources", new TransferResourcesAction(this as AITask, unitInventoryKey, targetStorageKey, transferedResourcesKey, true, 0, 0, ETransferDirection.Give), "LeaveStorage", "LeaveStorage");
				//Exit storage building
				StateTree.AddNextState("LeaveStorage", new EnterLeaveBuildingAction(null, Entity.GetFeature<GridAgentFeature>(), false, this, "TargetLocation"));
			}
			else
			{
				//Checks if we can gather more resource piles before stopping
				//Sets the next pile as "NextPile" on success
				//Sets the next location as "NextLocation" on success
				StateTree.AddNextState("GetResourcePiles", new RequestResourcePileAction(Entity.GetFeature<CitizenAIAgentFeature>(), this, nextPileKey, nextLocationKey, actionTypeKey), "MoveToResourcePile");
			}
		}		
	}
}
