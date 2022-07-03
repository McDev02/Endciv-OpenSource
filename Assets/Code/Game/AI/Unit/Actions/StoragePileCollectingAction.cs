using System.Collections.Generic;
using System.Linq;

namespace Endciv
{
    public class StoragePileCollectingAction : AIAction<ResourcePileCollectionActionSaveData>
    {
        CitizenAIAgentFeature citizen;
        ResourcePileFeature pile;
        string pileKey;
        public float collectionProgress;
        private ResourceCollectionTask task;

        public StoragePileCollectingAction(CitizenAIAgentFeature citizen, string pileKey, ResourceCollectionTask task)
        {
            this.citizen = citizen;
            this.pileKey = pileKey;
            this.task = task;
        }

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

        public override void OnStart()
        {
            pile = task.GetMemberValue<ResourcePileFeature>(pileKey);
            citizen.Entity.GetFeature<UnitFeature>().View.
                SwitchAnimationState(EAnimationState.Working);
        }

        public override void Update()
        {
            //Facility destroyed?
            if (pile == null || pile.Entity == null || pile.Entity.Inventory == null)
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

            collectionProgress += ResourcePileSystem.PickupSpeed * Main.deltaTime;
            if (collectionProgress >= 1)
            {
                collectionProgress = 0;
				//Get random resource from pile and remove it
				var keys = pile.Entity.Inventory.ItemPoolByChambers[0].Keys.ToArray();
				string id = string.Empty;
				foreach (var key in keys)
				{
					if (!InventorySystem.CanAddItems(citizen.Entity.Inventory, key, 1))
					{
						continue;
					}
					id = key;
					break;
				}
                                
                if (id == string.Empty)
                {
                    Status = EStatus.Failure;
                    return;
                }
                    
                if (!InventorySystem.TransferItems(pile.Entity.Inventory, citizen.Entity.Inventory, id, 1, false, 0, InventorySystem.ChamberReservedID))
                {
                    Status = EStatus.Success;
                    return;
                }


                var resources = task.GetMemberValue<List<ResourceStack>>("TransferedResources");
                if (resources == null)
                    resources = new List<ResourceStack>();
                var resource = resources.FirstOrDefault(x => x.ResourceID == id);
                if (resource == null)
                {
                    resources.Add(new ResourceStack(id, 1));
                }
                else
                {
                    resource.Amount++;
                }
                task.SetMemberValue<List<ResourceStack>>("TransferedResources", resources);

            }
            if (UnityEngine.Mathf.Approximately(pile.Entity.Inventory.Load,0f))
            {
                pile.canCancelGathering = true;
                ResourcePileSystem.MarkPileGathering(pile, false,false);
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