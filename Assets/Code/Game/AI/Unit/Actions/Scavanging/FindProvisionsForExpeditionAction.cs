namespace Endciv
{
	/// <summary>
	/// Wait if condition returns false, finish when it returns true
	/// </summary>
	public class FindProvisionsForExpeditionAction : AIAction<ActionSaveData>
	{
		CitizenAIAgentFeature agent;
		ExpeditionFeature expedition;

		private float nutrition;
		private float water;

		public FindProvisionsForExpeditionAction(CitizenAIAgentFeature agent, ExpeditionFeature expedition, float nutrition, float water)
		{
			this.agent = agent;
			this.expedition = expedition;

			this.nutrition = nutrition;
			this.water = water;
		}

		public override void Reset()
		{
		}

		public override void ApplySaveData(ActionSaveData data)
		{
			Status = (EStatus)data.status;
		}

		public override ISaveable CollectData()
		{
			var data = new WaitActionSaveData();
			data.status = (int)Status;
			return data;
		}

		public override void OnStart()
		{
			//Get resources

			var manager = Main.Instance.GameManager.SystemsManager;
			var storages = manager.StorageSystem.FeaturesByFaction[agent.FactionID];

			var inventory = agent.Entity.GetFeature<InventoryFeature>();

			bool hasWeapon = false;
			var weaponList = new string[] { "weapon_t1", "weapon_t2" };
			foreach (var storage in storages)
			{
				if (hasWeapon && nutrition <= 0 && water <= 0)
					break;

				while (nutrition > 0)
				{
					var food = InventorySystem.WithdrawConsumable(storage.Inventory, EConsumptionType.Food);
					if (food == null)
						break;
					nutrition -= food.StaticData.Nutrition;
					if (water > 0)
						water -= food.StaticData.Water;
					InventorySystem.AddItem(inventory, food.Entity.GetFeature<ItemFeature>(), false);
				}
				while (water > 0)
				{
					var drink = InventorySystem.WithdrawConsumable(storage.Inventory, EConsumptionType.Drink);
					if (drink == null)
						break;
					water -= drink.StaticData.Water;
					if (nutrition > 0)
						nutrition -= drink.StaticData.Nutrition;
					InventorySystem.AddItem(inventory, drink.Entity.GetFeature<ItemFeature>(), false);					
				}

				if (!hasWeapon)
				{
					for (int i = 0; i < weaponList.Length; i++)
					{
						if (InventorySystem.HasItems(storage.Inventory, weaponList[i]))
						{
							if (InventorySystem.TransferItems(storage.Inventory, inventory, weaponList[i], 1, false, 0, 0))
							{
								hasWeapon = true;
								break;
							}
						}
					}
				}
			}
		}

		public override void Update()
		{
			Status = EStatus.Success;
		}

#if UNITY_EDITOR
		public override void DrawUIDetails()
		{
			UnityEngine.GUILayout.Label("Waiting for condition");
		}
#endif
	}
}