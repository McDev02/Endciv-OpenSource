using UnityEngine;

namespace Endciv
{
	public class ConsumeFoodWaterAction : AIAction<ConsumeFoodWaterActionSaveData>
	{
		private float duration;
        private float timer;

        private FindFoodWaterTask task;
        private CitizenAIAgentFeature aiAgent;

        private string nutritionKey;
        private string waterKey;
        private string storageKey;

		public ConsumeFoodWaterAction(CitizenAIAgentFeature aiAgent, FindFoodWaterTask task, float duration, string nutritionKey, string waterKey, string storageKey)
		{
            this.aiAgent = aiAgent;
			this.duration = duration;
            this.nutritionKey = nutritionKey;
            this.waterKey = waterKey;
            this.storageKey = storageKey;
            this.task = task;
		}

		public override void Reset()
		{
			timer = duration;
		}

		public override void ApplySaveData(ConsumeFoodWaterActionSaveData data)
		{
			Status = (EStatus)data.status;
			timer = data.timer;
		}

		public override ISaveable CollectData()
		{
			var data = new ConsumeFoodWaterActionSaveData();
			data.status = (int)Status;
			data.timer = timer;
			return data;
		}

		public override void OnStart()
		{
			timer = duration;
		}

		public override void Update()
		{
            float nutrition = task.GetMemberValue<float>(nutritionKey);
            float water = task.GetMemberValue<int>(waterKey);
            if(nutrition <= 0 && water <= 0)
            {
                Status = EStatus.Success;
                return;
            }

			if (timer > 0)
			{
				timer -= Main.deltaTimeSafe;
				Status = EStatus.Running;
                return;
			}
            var storage = task.GetMemberValue<InventoryFeature>(storageKey);
            if(storage == null || storage.Entity == null)
            {
                Status = EStatus.Failure;
                return;
            }

            var being = aiAgent.Entity.GetFeature<LivingBeingFeature>();
            while(nutrition > 0)
            {
                var food = InventorySystem.WithdrawConsumable(storage, EConsumptionType.Food);
                if (food == null)
                    break;
				//Todo: This could cause the unit to drink like 100L of milk as it has low nutrition value.
                nutrition -= food.StaticData.Nutrition;
                if (water > 0)
                    water -= food.StaticData.Water;
                UnitSystem.ConsumeItem(being, food);
            }
            while(water > 0)
            {
				var drink = InventorySystem.WithdrawConsumable(storage, EConsumptionType.Drink);
				if (drink == null)
					break;
				//Todo: This could cause the unit to drink like 100L of milk as it has low nutrition value.
				water -= drink.StaticData.Water;
				if (nutrition > 0)
					nutrition -= drink.StaticData.Nutrition;
				UnitSystem.ConsumeItem(being, drink);				
            }
            task.SetMemberValue<float>(nutritionKey, nutrition);
            task.SetMemberValue<int>(waterKey, water);
            if(nutrition > 0 || water > 0)
            {
                Status = EStatus.Failure;
                return;
            }
            Status = EStatus.Success;                        
        }

#if UNITY_EDITOR
		public override void DrawUIDetails()
		{
			GUILayout.Label("ConsumeFood: " + timer.ToString("0.00") + " / " + duration.ToString("0.00"));
		}
#endif
	}
}