namespace Endciv
{
	public class UseShowerAction : AIAction<WaitActionSaveData>
	{
		AIAgentFeatureBase AIAgent;
		public float Duration;
		public float timer;
		GoToShowerTask task;

		public UseShowerAction(AIAgentFeatureBase aiAgent, GoToShowerTask task, float duration)
		{
			this.task = task;
			AIAgent = aiAgent;
			Duration = duration;
			timer = Duration;
		}

		public override void Reset()
		{
			timer = Duration;
		}

		public override void ApplySaveData(WaitActionSaveData data)
		{
			Status = (EStatus)data.status;
			Duration = data.duration;
			timer = data.timer;
			if (Status != EStatus.Success && Status != EStatus.Failure)
			{
				OnStart();
			}
		}

		public override ISaveable CollectData()
		{
			var data = new WaitActionSaveData();
			data.status = (int)Status;
			data.duration = Duration;
			data.timer = timer;
			return data;
		}


		public override void OnStart()
		{
			var shower = task.GetShower();
			if (shower != null)
			{
				var inventory = shower.Entity.GetFeature<InventoryFeature>();
				var amount = (int)(shower.StaticData.waterConsumption / GameConfig.WaterPortion);
	
				if (InventorySystem.HasItems(inventory, "water", amount))
				{
					if (InventorySystem.WithdrawItems(inventory, "water", amount) == null)
					{
						Status = EStatus.Failure;
					}
				}
			}
		}

		public override void Update()
		{
			if (timer > 0)
			{
				timer -= Main.deltaTimeSafe;
				Status = EStatus.Running;
			}
			else
			{
				Status = EStatus.Success;
			}
		}

#if UNITY_EDITOR
		public override void DrawUIDetails()
		{
			UnityEngine.GUILayout.Label("Use Shower action");
		}
#endif
	}
}