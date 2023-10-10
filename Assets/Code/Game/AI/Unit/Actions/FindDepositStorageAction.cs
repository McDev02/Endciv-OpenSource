using System.Collections.Generic;
using System.Linq;


namespace Endciv
{
	public class FindDepositStorageAction : AIAction<ActionSaveData>
	{
		private GridAgentFeature agent;
		private AITask task;
		private int reserveID;

		private string transferedResourcesKey;
		private string targetLocationKey;
		private string targetStorageKey;
		private string finalResourcesKey;

		public FindDepositStorageAction(GridAgentFeature agent, AITask task, int reserveID, string transferedResourcesKey, string targetLocationKey, string targetStorageKey, string finalResourcesKey)
		{
			this.agent = agent;
			this.task = task;
			this.reserveID = reserveID;

			this.transferedResourcesKey = transferedResourcesKey;
			this.targetLocationKey = targetLocationKey;
			this.targetStorageKey = targetStorageKey;
			this.finalResourcesKey = finalResourcesKey;
		}

		public override void ApplySaveData(ActionSaveData data)
		{
			Status = (EStatus)data.status;
		}

		public override void Reset()
		{

		}

		public override ISaveable CollectData()
		{
			var data = new ActionSaveData();
			data.status = (int)Status;
			return data;
		}

		public override void OnStart()
		{
			Status = EStatus.Running;
		}

		public override void Update()
		{
			var manager = Main.Instance.GameManager.SystemsManager;

			InventoryFeature targetInventory = null;
			//Iterate all resources in the list until we have a storage that accepts at least one of them
			var resources = task.GetMemberValue<List<ResourceStack>>(transferedResourcesKey);
			if (resources == null)
			{
				Status = EStatus.Failure;
				return;
			}
			bool foundStorage = false;
			foreach (var resource in resources)
			{
				var data = Main.Instance.GameManager.Factories.SimpleEntityFactory.EntityStaticData[resource.ResourceID].
					GetFeature<ItemFeatureStaticData>();

				//Get a list of all storages that match
				var storages = manager.StorageSystem.GetAllStoragesAccepting(data, agent.FactionID);

				//No storages contain resource ID, move to next resource           
				if (storages.Length <= 0)
				{
					continue;
				}

				//Order storages by distance from unit and manipulated by priority
				var ordererdStorages = storages.OrderBy(x => (Vector3.Distance(x.Entity.GetFeature<EntityFeature>().View.transform.position, agent.Entity.GetFeature<EntityFeature>().View.transform.position) / x.Priority)).ToArray();

				//Potential storage is the closest from unit
				//Try the closest X storages
				int count = Mathf.Min(8, ordererdStorages.Length);
				for (int i = 0; i < count; i++)
				{
					var target = ordererdStorages[i].Entity.Inventory;

					//Amount to be transfered is the less between the needed, the available, and the storage capacity of the target
					int amount = Mathf.Min(resource.Amount, InventorySystem.GetItemCount(agent.Entity.Inventory, resource.ResourceID), InventorySystem.GetAddableAmount(target, resource.ResourceID));

					//Amount is 0, move to next resource
					if (amount <= 0)
						continue;

					//Found our inventory, break out of the loop
					targetInventory = target;
					foundStorage = true;
					break;
				}
				if (foundStorage)
					break;
			}

			if (targetInventory == null)
			{
				Status = EStatus.Failure;
				return;
			}


			List<ResourceStack> transferedResources = new List<ResourceStack>();
			foreach (var resource in resources)
			{
				int amount = Mathf.Min(resource.Amount, InventorySystem.GetItemCount(agent.Entity.Inventory, resource.ResourceID), InventorySystem.GetAddableAmount(targetInventory, resource.ResourceID));

				//Amount is 0, move to next resource
				if (amount <= 0)
					continue;

				//Attempt to reserve the resource, move to next resource if attempt failed
				if (reserveID > 0)
				{
					if (!InventorySystem.ReserveItems(agent.Entity.Inventory, resource.ResourceID, amount))
						continue;
				}
				//Attempt succeeded, add resource to array
				transferedResources.Add(new ResourceStack(resource.ResourceID, amount));
			}

			//Check if we could add anything to the array
			if (transferedResources.Count > 0)
			{
				//All required variables can have values, finish assigning and return to task
				task.SetMemberValue<InventoryFeature>(targetStorageKey, targetInventory);
				task.SetMemberValue<Location>(targetLocationKey, new Location(targetInventory.Entity, true));
				task.SetMemberValue<List<ResourceStack>>(finalResourcesKey, transferedResources);
				Status = EStatus.Success;
				return;
			}

			//No iteration reached success point, return failure
			Status = EStatus.Failure;
		}

#if UNITY_EDITOR
		public override void DrawUIDetails()
		{
		}
#endif
	}
}