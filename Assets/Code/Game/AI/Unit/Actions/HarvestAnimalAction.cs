namespace Endciv
{
	public class HarvestAnimalAction : AIAction<HarvestAnimalActionSaveData>
	{
		private HarvestAnimalTask task;
		private BaseEntity citizen;
		private string cattleKey;

		private float progress = 0f;

		public HarvestAnimalAction(BaseEntity citizen, HarvestAnimalTask task, string cattleKey)
		{
			this.task = task;
			this.citizen = citizen;
			this.cattleKey = cattleKey;
		}

		public override void Reset()
		{
			progress = 0f;
		}

		public override void ApplySaveData(HarvestAnimalActionSaveData data)
		{
			Status = (EStatus)data.status;
			progress = data.progress;
			if (Status != EStatus.Failure && Status != EStatus.Success)
			{
				OnStart();
			}
		}

		public override ISaveable CollectData()
		{
			var data = new HarvestAnimalActionSaveData();
			data.progress = progress;
			data.status = (int)Status;
			return data;
		}

		public override void OnStart()
		{			
			citizen.GetFeature<UnitFeature>().View.
				SwitchAnimationState(EAnimationState.Working);
		}

		public override void Update()
		{
			var cattle = task.GetMemberValue<CattleFeature>(cattleKey);
			if(cattle == null)
			{
				Status = EStatus.Failure;
				return;
			}
			if (!cattle.Entity.GetFeature<EntityFeature>().IsAlive)
			{
				Status = EStatus.Failure;
				return;
			}
				
			if(cattle.ProducedGoods < 1f)
			{
				Status = EStatus.Success;
				return;
			}

			progress += 0.2f * Main.deltaTimeSafe;
			if (progress >= 1f)
			{
				var factory = Main.Instance.GameManager.Factories.SimpleEntityFactory;
				progress = 0f;				
				var data = factory.GetStaticData<ItemFeatureStaticData>(cattle.staticData.ProducedItem);
				if (citizen.Inventory.CapacityLeft < data.Mass)
				{
					Status = EStatus.Success;
					return;
				}
				cattle.ProducedGoods -= 1;
				var item = factory.CreateInstance(cattle.staticData.ProducedItem).GetFeature<ItemFeature>();
				item.Quantity = 1;
				InventorySystem.AddItem(citizen.Inventory, item, false);
				
				if(cattle.ProducedGoods < 1f)
				{
					Status = EStatus.Success;
					return;
				}

				if (citizen.Inventory.CapacityLeft < data.Mass)
				{
					Status = EStatus.Success;
					return;
				}
			}
			Status = EStatus.Running;
		}

#if UNITY_EDITOR
		public override void DrawUIDetails()
		{
			
		}

#endif
	}
}