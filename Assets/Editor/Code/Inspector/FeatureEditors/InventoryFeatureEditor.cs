using UnityEditor;
using UnityEngine;
using System.Linq;

namespace Endciv.Editor
{
    public class InventoryFeatureEditor : FeatureEditor<InventoryFeature>
    {
        private ItemFeature InventoryDeleteResource;
        private int InventoryDeleteChamber;

        private string InventoryResourceId = "";
        private int InventoryBatchAmount = 10;
		private int TargetChamber = 0;
        private bool[] showChamber = new bool[9];
        private SimpleEntityFactory ResourceFactory;

        public override void OnEnable()
        {
            ResourceFactory = Main.Instance.GameManager.Factories.SimpleEntityFactory;
        }

        public override void OnGUI()
        {            
            EditorHelper.ProgressBar(Feature.LoadProgress, EditorHelper.GetProgressBarTitle("Load", Feature.Load, Feature.MaxCapacity), 16);
            EditorGUILayout.LabelField("Chambers: " + Feature.TotalChambers, EditorStyles.boldLabel);

            //Tools
            //Add resources            
            InventoryBatchAmount = Mathf.Clamp(EditorGUILayout.IntField("Batches", InventoryBatchAmount), 1, 1000);
            InventoryResourceId = EditorGUILayout.TextField("ID", InventoryResourceId);
            TargetChamber = Mathf.Clamp(EditorGUILayout.IntField("Target Chamber", TargetChamber), 0, Feature.TotalChambers - 1);
			string id;
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Items"))
            {
				var ids = ResourceFactory.GetStaticDataIDList<ItemFeatureStaticData>();
				if (!string.IsNullOrEmpty(InventoryResourceId))
					id = InventoryResourceId;
				else
					id = ids.ElementAt(Random.Range(0, ids.Count));                
                int amount = InventorySystem.GetAddableAmount(Feature, id, InventoryBatchAmount);
				if (amount <= 0)
					return;
				var data = ResourceFactory.GetStaticData<ItemFeatureStaticData>(id);
				if(data.IsStackable)
				{
					var item = ResourceFactory.CreateInstance(id).GetFeature<ItemFeature>();
					item.Quantity = amount;
					InventorySystem.AddItem(Feature, item, false, TargetChamber);
				}
				else
				{
					for(int i = 0; i < amount; i++)
					{
						var item = ResourceFactory.CreateInstance(id).GetFeature<ItemFeature>();
						item.Quantity = 1;
						InventorySystem.AddItem(Feature, item, false, TargetChamber);
					}
				}
            }            
            EditorGUILayout.EndHorizontal();

            //Fill
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Fill"))
            {
                InventorySystem.FillInventory<ItemFeatureStaticData>(Feature);
            }
            if (GUILayout.Button("Fill Weapons"))
            {
                InventorySystem.FillInventory<WeaponFeatureStaticData>(Feature);
            }
			if (GUILayout.Button("Fill Tools"))
			{
				InventorySystem.FillInventory<ToolFeatureStaticData>(Feature);
			}
			if (GUILayout.Button("Fill Foods"))
            {
                InventorySystem.FillInventory<ConsumableFeatureStaticData>(Feature);
            }
            if (GUILayout.Button("Create Overflow"))
            {
				var ids = ResourceFactory.GetStaticDataIDList<ItemFeatureStaticData>();
				if (!string.IsNullOrEmpty(InventoryResourceId))
					id = InventoryResourceId;
				else
					id = ids.ElementAt(Random.Range(0, ids.Count));				
				var data = ResourceFactory.GetStaticData<ItemFeatureStaticData>(id);
				if(data.IsStackable)
				{
					var item = ResourceFactory.CreateInstance(id).GetFeature<ItemFeature>();
					item.Quantity = 255;
					InventorySystem.AddItem(Feature, item, true);
				}
				else
				{
					for(int i = 0; i < 255; i++)
					{
						var item = ResourceFactory.CreateInstance(id).GetFeature<ItemFeature>();
						item.Quantity = 1;
						InventorySystem.AddItem(Feature, item, true);
					}
				}
                
            }
            EditorGUILayout.EndHorizontal();

            //Remove
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear"))
            {
                //Clear all resources
                InventorySystem.ClearInventory(Feature);
            }
            EditorGUILayout.EndHorizontal();

            if (InventoryDeleteResource != null)
            {
                InventorySystem.WithdrawItems(Feature, InventoryDeleteResource.Entity.StaticData.ID, InventoryDeleteResource.Quantity);
            }
			InventoryDeleteResource = null;

            //Draw resources:
            DrawResourcesInChambers(Feature);
        }

        private void DrawResourcesInChambers(InventoryFeature context)
        {
            for (int c = 0; c < context.TotalChambers; c++)
            {
                if (c > 0) GUILayout.Space(12);

                EditorGUILayout.BeginHorizontal();
                if (c < 9) showChamber[c] = GUILayout.Toggle(showChamber[c], "");
                else GUILayout.Toggle(true, "");
                GUILayout.Label("Chamber #" + c.ToString() + ": " + context.Chambers[c], EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginVertical("Box");
                if (c >= 9 || showChamber[c])
                {
					if(context.ItemPoolByChambers[c].Count > 0)
					{
						var keys = context.ItemPoolByChambers[c].Keys.ToArray();
						foreach(var key in keys)
						{
							var items = context.ItemPoolByChambers[c][key];
							EditorGUILayout.BeginVertical("Box");
							foreach (var item in items)
							{
								DrawItemEntry(item, c);
							}
							EditorGUILayout.EndVertical();
						}
					}
                    
                }
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawItemEntry(ItemFeature item, int chamber)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(item.Entity.StaticData.ID + ": " + item.Quantity);			
			if(item.Entity.HasFeature<DurabilityFeature>())
			{
				var food = item.Entity.GetFeature<DurabilityFeature>();
				EditorHelper.ProgressBar(food.Durability, EditorHelper.GetProgressBarTitle("Durability", food.Durability, food.StaticData.maxDurability), 14);
			}
            if (GUILayout.Button("X", GUILayout.Width(28)))
            {
                InventoryDeleteResource = item;
                InventoryDeleteChamber = chamber;
            }
            EditorGUILayout.EndHorizontal();
        }        
    }

}
