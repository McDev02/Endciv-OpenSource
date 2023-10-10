using System.Collections.Generic;


namespace Endciv
{
    public class GatherWasteAction : AIAction<GatherWasteActionSaveData>
    {
        CitizenAIAgentFeature citizen;
        string tileKey;
        private int currentIndex;
        private float timer;
        private WasteGatheringTask task;
        private List<Vector2i> tiles;
        private GridMap gridMap;

        public override void Reset()
        {
            currentIndex = 0;
            timer = 0f;
            tiles = null;
        }

        public override void ApplySaveData(GatherWasteActionSaveData data)
        {
            Status = (EStatus)data.status;
            currentIndex = data.currentIndex;
            timer = data.timer;            
            if (Status != EStatus.Failure && Status != EStatus.Success)
            {
                citizen.Entity.GetFeature<UnitFeature>().View.
                    SwitchAnimationState(EAnimationState.Working);
            }
        }

        public override ISaveable CollectData()
        {
            var data = new GatherWasteActionSaveData();
            data.status = (int)Status;
            data.currentIndex = currentIndex;
            data.timer = timer;
            return data;
        }

        public GatherWasteAction(CitizenAIAgentFeature citizen, string tileKey, WasteGatheringTask task)
        {
            this.citizen = citizen;
            this.tileKey = tileKey;
            this.task = task;
        }

        public override void OnStart()
        {
            citizen.Entity.GetFeature<UnitFeature>().View.
                SwitchAnimationState(EAnimationState.Working);
        }

        public override void Update()
        {
            if (currentIndex >= 9)
            {
                Status = EStatus.Success;
                return;
            }
            if (!InventorySystem.CanAddItems(citizen.Entity.Inventory, FactoryConstants.WasteID, 1))
            {
                Status = EStatus.Failure;
                return;
            }            
            if(tiles == null)
            {
                var location = task.GetMemberValue<Location>(tileKey);
                tiles = Main.Instance.GameManager.GridMap.partitionSystem.GetAdjacentTiles(location.Index);
            }
            if(gridMap == null)
            {
                gridMap = Main.Instance.GameManager.GridMap;
            }
            while(currentIndex < 9 && !gridMap.IsTileInGrid(tiles[currentIndex]))
            {
                currentIndex++;
            }
            if (currentIndex >= 9)
            {
                Status = EStatus.Success;
                return;
            }
            var currentTile = tiles[currentIndex];
            while(currentIndex < 9 && gridMap.Data.waste[currentTile.X, currentTile.Y] <= 0f)
            {
                currentIndex++;
                currentTile = tiles[currentIndex];
            }
            if (currentIndex >= 9)
            {
                Status = EStatus.Success;
                return;
            }
            timer += GameConfig.Instance.GeneralEconomyValues.GatheringSpeed * Main.deltaTimeSafe;
            if(timer >= 1f)
            {
                timer = 0f;
                var val = gridMap.Data.waste[currentTile.X, currentTile.Y];
                gridMap.Data.waste[currentTile.X, currentTile.Y] -= Mathf.Max(0, Random.Range(val, val * 2f));
				var waste = Main.Instance.GameManager.Factories.SimpleEntityFactory.CreateInstance(FactoryConstants.WasteID).GetFeature<ItemFeature>();
				waste.Quantity = 1;
                InventorySystem.AddItem(citizen.Entity.Inventory, waste, false, InventorySystem.ChamberMainID);
                if(gridMap.Data.waste[currentTile.X, currentTile.Y] <= 0f)
                {
                    currentIndex++;
                }
                Main.Instance.GameManager.TerrainManager.terrainView.UpdateTerrainSplatMap();
            }
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