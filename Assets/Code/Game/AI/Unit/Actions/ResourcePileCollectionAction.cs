using System.Collections.Generic;
using System.Linq;

namespace Endciv
{
	public class ResourcePileCollectionAction : AIAction<ResourcePileCollectionActionSaveData>
	{
		CitizenAIAgentFeature citizen;
		string pileKey;
		public float collectionProgress;
		private ResourceCollectionTask task;
		private ResourcePileFeature pile;

		public override void Reset()
		{
			collectionProgress = 0f;
		}

		public override void ApplySaveData(ResourcePileCollectionActionSaveData data)
		{
			Status = (EStatus)data.status;
			collectionProgress = data.collectionProgress;
			pile = task.GetMemberValue<ResourcePileFeature>(pileKey);
			if (Status != EStatus.Failure && Status != EStatus.Success)
			{
				citizen.Entity.GetFeature<UnitFeature>().View.
                    SwitchAnimationState(EAnimationState.Working);
			}
		}

		public override ISaveable CollectData()
		{
			var data = new ResourcePileCollectionActionSaveData();
			data.status = (int)Status;
			data.collectionProgress = collectionProgress;
			return data;
		}

		public ResourcePileCollectionAction(CitizenAIAgentFeature citizen, string pileKey, ResourceCollectionTask task)
		{
			this.citizen = citizen;
			this.pileKey = pileKey;
			this.task = task;
		}

		public override void OnStart()
		{
			pile = task.GetMemberValue<ResourcePileFeature>(pileKey);
			citizen.Entity.GetFeature<UnitFeature>().View.
                SwitchAnimationState(EAnimationState.Working);
		}

		public override void Update()
		{
			//Facility destroyed?
			if (pile == null || pile.resources == null || pile.resources.Count <= 0)
			{
				if (pile != null && pile.Entity != null)
				{
					pile.canCancelGathering = true;
					ResourcePileSystem.MarkPileGathering(pile, false);
					Main.Instance.GameManager.SystemsManager.ResourcePileSystem.UnregisterBlockingPile(pile);
					pile.Entity.Destroy();
				}
				Status = EStatus.Success;
				return;
			}

			var gatherSpeed = pile.ResourcePileType == ResourcePileSystem.EResourcePileType.ResourcePile ?
				 GameConfig.Instance.GeneralEconomyValues.GatheringSpeed : GameConfig.Instance.GeneralEconomyValues.StorageGatheringSpeed;

			collectionProgress += gatherSpeed * Main.deltaTime;
			if (collectionProgress >= 1)
			{
				collectionProgress = 0;
				//Get random resource from pile and remove it
				string id = string.Empty;
				ResourceStack stack = null;
				foreach (var res in pile.resources)
				{
					if (!InventorySystem.CanAddItems(citizen.Entity.Inventory, res.ResourceID, 1))
						continue;

					stack = res;
					id = res.ResourceID;
					break;
				}
				if (id == string.Empty)
				{
					Status = EStatus.Failure;
					return;
				}

				if (!InventorySystem.CanAddItems(citizen.Entity.Inventory, id, 1))
				{
					Status = EStatus.Success;
					return;
				}
				stack.Amount -= 1;
				if (stack.Amount <= 0)
					pile.resources.Remove(stack);
				//Create and transfer resource (stack.ResourceID) to citizen inventory
				var newResource = Main.Instance.GameManager.Factories.SimpleEntityFactory.CreateInstance(id).GetFeature<ItemFeature>();
				newResource.Quantity = 1;				
				InventorySystem.AddItem(citizen.Entity.Inventory, newResource, false, InventorySystem.ChamberReservedID);
				var resources = task.GetMemberValue<List<ResourceStack>>("TransferedResources");
				if (resources == null)
					resources = new List<ResourceStack>();
				var resource = resources.FirstOrDefault(x => x.ResourceID == stack.ResourceID);
				if (resource == null)
				{
					resources.Add(new ResourceStack(id, 1));
				}
				else
				{
					resource.Amount++;
				}
				task.SetMemberValue<List<ResourceStack>>("TransferedResources", resources);

				pile.Entity.UpdateView();
			}

			if (pile.resources.Count <= 0)
			{
				pile.canCancelGathering = true;
				ResourcePileSystem.MarkPileGathering(pile, false);
				Main.Instance.GameManager.SystemsManager.ResourcePileSystem.UnregisterBlockingPile(pile);
				pile.Entity.Destroy();
				Status = EStatus.Success;
				return;
			}
			else
				Status = EStatus.Running;
		}

#if UNITY_EDITOR
		public override void DrawUIDetails()
		{
			//UnityEngine.GUILayout.Label("Resting: " + Unit.Resting.Progress.ToString());
		}

#endif
	}
}