using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	public class LivingBeingEditorController : MonoBehaviour
	{
		const float drinkPortion = 0.25f;
		const float eatPortion = 1f;

		UnitSystemConfig config;
		[SerializeField] EntityStaticData unitData;
		SimulationData lastData;
		const int DayLength = 60;
		const int Maxtime = 360 * DayLength;
		const float ConsumeTickFactor = 1f / DayLength;

		[SerializeField] Text drinkMaxLivingTime;
		[SerializeField] Text drinhHalfLivingTime;
		[SerializeField] Text drinkQuarterLivingTime;
		[SerializeField] Text drinkLowLivingTime;

		[SerializeField] Text[] livingTimeDrink;

		[SerializeField] Text eatMaxLivingTime;
		[SerializeField] Text eatHalfLivingTime;
		[SerializeField] Text eatQuarterLivingTime;
		[SerializeField] Text eatLowLivingTime;

		[SerializeField] Text[] livingTimeEat;

		[SerializeField] Text moodValue;
		[SerializeField] RectTransform needsList;
		[SerializeField] GUINeedEntry needEntryPrefab;
		GUINeedEntry[] needEntries;

		float timer;
		private void Start()
		{
			config = GameConfig.Instance.UnitSystemData;
			lastData = new SimulationData(unitData, 6, 6);
			var needs = lastData.unit.Entity.GetFirstAIFeature().needsLookup;

			needEntries = new GUINeedEntry[needs.Count];
			for (int i = 0; i < needEntries.Length; i++)
			{
				var need = needs[i];
				GUINeedEntry obj = Instantiate(needEntryPrefab, needsList, false);
				obj.Title.text = need.name;
				obj.slider.minValue = need.minValue;
				obj.slider.maxValue = need.maxValue;
				obj.slider.value = need.Value;
				needEntries[i] = obj;
			}
		}

		private void Update()
		{
			if (timer <= 0)
			{
				timer = 1;
				SimulateUnit(lastData);
				UpdateWindow(lastData);
			}
			timer -= Time.deltaTime;

			SimulateMood(lastData.unit.Entity.GetFirstAIFeature());
		}

		void SimulateUnit(SimulationData data)
		{
			int counter = 0;
			var being = data.unit;
			data.Reset();

            //Thirst
            var livingBeing = unitData.GetFeature<LivingBeingStaticData>();
			being.Thirst.maxValue = livingBeing.maxThirst.adultValue;
			being.Thirst.Value = livingBeing.maxThirst.adultValue;
			while (being.Thirst.Value > 0 && counter++ < Maxtime)
			{
				float consumption = Mathf.Lerp(1f, being.Thirst.Value * config.thirstConsumptionCenter, config.thirstConsumptionBalance) * livingBeing.consumeThirst.adultValue;
				being.Thirst.Value -= consumption * ConsumeTickFactor;
				data.drinkMaxLivingTime++;
				if (being.Thirst.Progress <= 0.5f)
					data.drinkHalfLivingTime++;
				if (being.Thirst.Progress <= 0.25f)
					data.drinkQuarterLivingTime++;
				if (being.Thirst.Progress <= 0.05f)
					data.drinkLowLivingTime++;
			}

			data.LivingTimeDrink[0] = SimulateDrinking(being, 1, out data.waterLevel[0]);
			data.LivingTimeDrink[1] = SimulateDrinking(being, 2, out data.waterLevel[1]);
			data.LivingTimeDrink[2] = SimulateDrinking(being, 3, out data.waterLevel[2]);
			data.LivingTimeDrink[3] = SimulateDrinking(being, 4, out data.waterLevel[3]);

			data.LivingTimeDrink[4] = SimulateDrinking(being, 8, out data.waterLevel[4]);
			data.LivingTimeDrink[5] = SimulateDrinking(being, 12, out data.waterLevel[5]);

			//Eating
			being.Hunger.maxValue = livingBeing.maxHunger.adultValue;
			being.Hunger.Value = livingBeing.maxHunger.adultValue;
			while (being.Hunger.Value > 0 && counter++ < Maxtime)
			{
				float consumption = Mathf.Lerp(1f, being.Hunger.Value * config.hungerConsumptionCenter, config.hungerConsumptionBalance) * livingBeing.consumeHunger.adultValue;
				being.Hunger.Value -= consumption * ConsumeTickFactor;
				data.eatMaxLivingTime++;
				if (being.Hunger.Progress <= 0.5f)
					data.eatHalfLivingTime++;
				if (being.Hunger.Progress <= 0.25f)
					data.eatQuarterLivingTime++;
				if (being.Hunger.Progress <= 0.05f)
					data.eatLowLivingTime++;
			}

			data.LivingTimeEat[0] = SimulateEating(being, 1, out data.nutritionLevel[0]);
			data.LivingTimeEat[1] = SimulateEating(being, 2, out data.nutritionLevel[1]);
			data.LivingTimeEat[2] = SimulateEating(being, 4, out data.nutritionLevel[2]);
			data.LivingTimeEat[3] = SimulateEating(being, 6, out data.nutritionLevel[3]);

			data.LivingTimeEat[4] = SimulateEating(being, 8, out data.nutritionLevel[4]);
			data.LivingTimeEat[5] = SimulateEating(being, 12, out data.nutritionLevel[5]);
		}

		int SimulateDrinking(LivingBeingFeature being, int portions, out float waterLevel)
		{
            var livingBeing = unitData.GetFeature<LivingBeingStaticData>();
            waterLevel = 0;
			int drinkCounts = 0;
			int counter = 0;
			int drinksRemain = portions;
			int dayCounter = 0;
			being.Thirst.Value = livingBeing.maxThirst.adultValue / 2f;
			while (being.Thirst.Value > 0 && counter++ < Maxtime)
			{
				if (being.Thirst.Progress <= 0.05f && drinksRemain > 0)
				{
					drinkCounts++;
					waterLevel += being.Thirst.Progress;
					drinksRemain--;
					being.Thirst.Value += drinkPortion;
				}

				float consumption = Mathf.Lerp(1f, being.Thirst.Value * config.thirstConsumptionCenter, config.thirstConsumptionBalance) * livingBeing.consumeThirst.adultValue;
				being.Thirst.Value -= consumption * ConsumeTickFactor;
				dayCounter++;

				if (dayCounter >= DayLength)
				{
					being.Thirst.Value += drinksRemain * drinkPortion;
					dayCounter = 0;
					drinksRemain = portions;
					drinkCounts++;
					waterLevel += being.Thirst.Progress;
				}
			}
			waterLevel = being.Thirst.Progress;
			//waterLevel /= drinkCounts;
			return counter;
		}

		int SimulateEating(LivingBeingFeature being, int portions, out float nutritionLevel)
		{
            var livingBeing = unitData.GetFeature<LivingBeingStaticData>();
            nutritionLevel = 0;
			int eatCounts = 0;
			int counter = 0;
			int mealsRemain = portions;
			int dayCounter = 0;
			being.Hunger.Value = livingBeing.maxHunger.adultValue / 2f;
			while (being.Hunger.Value > 0 && counter++ < Maxtime)
			{
				if (being.Hunger.Progress <= 0.05f && mealsRemain > 0)
				{
					eatCounts++;
					nutritionLevel += being.Hunger.Progress;
					mealsRemain--;
					being.Hunger.Value += eatPortion;
				}

				float consumption = Mathf.Lerp(1f, being.Hunger.Value * config.hungerConsumptionCenter, config.hungerConsumptionBalance) * livingBeing.consumeHunger.adultValue;
				being.Hunger.Value -= consumption * ConsumeTickFactor;
				dayCounter++;

				if (dayCounter >= DayLength)
				{
					being.Hunger.Value += mealsRemain * eatPortion;
					dayCounter = 0;
					mealsRemain = portions;
					eatCounts++;
					nutritionLevel += being.Hunger.Progress;
				}
			}
			nutritionLevel = being.Hunger.Progress;
			//nutritionLevel /= eatCounts;
			return counter;
		}
		void UpdateWindow(SimulationData data)
		{
			drinkMaxLivingTime.text = GetTimeString(data.drinkMaxLivingTime);
			drinhHalfLivingTime.text = GetTimeString(data.drinkHalfLivingTime);
			drinkQuarterLivingTime.text = GetTimeString(data.drinkQuarterLivingTime);
			drinkLowLivingTime.text = GetTimeString(data.drinkLowLivingTime);

			for (int i = 0; i < livingTimeDrink.Length; i++)
				livingTimeDrink[i].text = GetTimeString(data.LivingTimeDrink[i]) + $" (+{GetTimeString(data.LivingTimeDrink[i] - data.drinkHalfLivingTime)}) Water Level: {(data.waterLevel[i] * 100).ToString("0")}%";

			eatMaxLivingTime.text = GetTimeString(data.eatMaxLivingTime);
			eatHalfLivingTime.text = GetTimeString(data.eatHalfLivingTime);
			eatQuarterLivingTime.text = GetTimeString(data.eatQuarterLivingTime);
			eatLowLivingTime.text = GetTimeString(data.eatLowLivingTime);

			for (int i = 0; i < livingTimeEat.Length; i++)
				livingTimeEat[i].text = GetTimeString(data.LivingTimeEat[i]) + $" (+{GetTimeString(data.LivingTimeEat[i] - data.eatHalfLivingTime)}) Nutrition Level: {(data.nutritionLevel[i] * 100).ToString("0")}%";

		}

		void SimulateMood(AIAgentFeatureBase agent)
		{
			float val;
			for (int i = 0; i < needEntries.Length; i++)
			{
				val = needEntries[i].slider.value;
				agent.needsLookup[i].Value = val;

				needEntries[i].value.text = val.ToString("0.00");
				needEntries[i].mood.text = Mathf.Round(agent.needsLookup[i].Mood * 100).ToString();
			}

			AIAgentSystem.CalculateMood(agent);

			moodValue.text = Mathf.Round(agent.mood * 100).ToString();
		}

		string GetTimeString(float time)
		{
			var val = time / DayLength;
			if (val < 1)
				return $"{(int)(val * 24)} Hours";
			else return $"{val.ToString("0.0")} Days";
		}

		class SimulationData
		{
			public LivingBeingFeature unit;

			public int drinkMaxLivingTime;
			public int drinkHalfLivingTime;
			public int drinkQuarterLivingTime;
			public int drinkLowLivingTime;

			public int[] LivingTimeDrink;
			public float[] waterLevel;

			public int eatMaxLivingTime;
			public int eatHalfLivingTime;
			public int eatQuarterLivingTime;
			public int eatLowLivingTime;

			public int[] LivingTimeEat;
			public float[] nutritionLevel;

			public SimulationData(EntityStaticData data, int lengthDrink, int lengthEat)
			{
                var livingBeing = data.GetFeature<LivingBeingStaticData>();
                var entityTrans = new GameObject();
                var entityView = entityTrans.AddComponent<EntityFeatureView>();
                var entity = new BaseEntity();
                var entityFeature = new EntityFeature();
				entityFeature.Setup(entity);
                entity.AttachFeature(entityFeature);
                entityFeature.SetView(entityView);				
				entity.Initialize(data, null);
				unit = new LivingBeingFeature();
				unit.Setup(entity);
				unit.Entity.GetFeature<UnitFeature>().Age = ELivingBeingAge.Adult;
				unit.Entity.GetFeature<UnitFeature>().Gender = ELivingBeingGender.Male;
				entity.AttachFeature(unit);
				var feature = new AnimalAIAgentFeature();
				feature.Setup(entity);
				feature.SetAIAgentSettings(data.GetFeature<UnitFeatureStaticData>().aiSettings);
				entity.AttachFeature(feature);

				LivingTimeEat = new int[lengthEat];
				nutritionLevel = new float[lengthEat];
				waterLevel = new float[lengthDrink];
				LivingTimeDrink = new int[lengthDrink];

			}

			internal void Reset()
			{
				drinkMaxLivingTime = 0;
				drinkHalfLivingTime = 0;
				drinkQuarterLivingTime = 0;
				drinkLowLivingTime = 0;

				for (int i = 0; i < LivingTimeDrink.Length; i++)
				{
					LivingTimeDrink[i] = 0;
					waterLevel[i] = 0;
				}

				eatMaxLivingTime = 0;
				eatHalfLivingTime = 0;
				eatQuarterLivingTime = 0;
				eatLowLivingTime = 0;

				for (int i = 0; i < LivingTimeEat.Length; i++)
				{
					LivingTimeEat[i] = 0;
					nutritionLevel[i] = 0;
				}
			}
		}
	}
}