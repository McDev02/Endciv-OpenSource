namespace Endciv
{
	/// <summary>
	/// Wait if condition returns false, finish when it returns true
	/// </summary>
	public class OnExpeditionAction : AIAction<WaitActionSaveData>
	{
		CitizenAIAgentFeature agent;
		ExpeditionFeature expedition;

		public OnExpeditionAction(CitizenAIAgentFeature agent, ExpeditionFeature expedition)
		{
			this.agent = agent;
			this.expedition = expedition;
		}
		public override void Reset()
		{
		}

		public override void ApplySaveData(WaitActionSaveData data)
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
			agent.Entity.HideView();
		}

		public override void Update()
		{
			if (expedition.state != ExpeditionFeature.EState.Finished)
			{
				Status = EStatus.Running;

				var being = agent.Entity.GetFeature<LivingBeingFeature>();
				if (being != null)
				{
					if (being.Hunger.Progress <= being.StaticData.HungerUrgencyThreshold.max * 2)
					{
						var food = InventorySystem.WithdrawConsumable(being.Entity.Inventory, EConsumptionType.Food);
						if (food != null)
							UnitSystem.ConsumeItem(being, food);
					}
					if (being.Thirst.Progress <= being.StaticData.ThirstUrgencyThreshold.max * 2)
					{
						var drink = InventorySystem.WithdrawConsumable(being.Entity.Inventory, EConsumptionType.Drink);
						if (drink != null)
							UnitSystem.ConsumeItem(being, drink);
					}
				}
			}
			else
			{
				//Quit and consume all provitions
				Status = EStatus.Success;

				var being = agent.Entity.GetFeature<LivingBeingFeature>();
				if (being != null)
				{
					int safeCounter = 100;
					while (safeCounter-- > 0)
					{
						var food = InventorySystem.WithdrawConsumable(being.Entity.Inventory, EConsumptionType.Food);
						if (food == null)
							break;
						UnitSystem.ConsumeItem(being, food);
					}
					safeCounter = 100;
					while (safeCounter-- > 0)
					{
						{
							var drink = InventorySystem.WithdrawConsumable(being.Entity.Inventory, EConsumptionType.Drink);
							if (drink == null)
								break;
							UnitSystem.ConsumeItem(being, drink);
						}
					}
				}
				agent.Entity.ShowView();
			}
		}

#if UNITY_EDITOR
		public override void DrawUIDetails()
		{
			UnityEngine.GUILayout.Label("Waiting for condition");
		}
#endif
	}
}