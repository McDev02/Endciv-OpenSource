using UnityEngine;
using System.ComponentModel;
using System;

namespace Endciv
{
	//public enum ELivingBeingCondition { Normal, Exhausted, Immobilized }
	public enum ELivingBeingAge { Undefined, Child, Adult }
	public enum ELivingBeingGender { Undefined, Male, Female }

	/// <summary>
	/// Unit which must eat and drink
	/// </summary>
	public class LivingBeingFeature :
		Feature<LivingBeingFeatureSaveData>,
		IUI3DController
	{
		public Action OnAgeCategoryChanged;

		public LivingBeingStaticData StaticData { get; private set; }

		public override void Setup(BaseEntity entity, FeatureParamsBase args = null)
		{
			base.Setup(entity);
			StaticData = Entity.StaticData.GetFeature<LivingBeingStaticData>();
		}

		//Properties
		public EntityProperty Hunger;
		public EntityProperty Thirst;
		public EntityNeed vitality;
		public float innards;
		public float meat;

		public ELivingBeingGender gender;
		public ELivingBeingAge age;

		private int ageDayCounter;
		public int AgeDayCounter
		{
			get
			{
				return ageDayCounter;
			}
			set
			{
				ageDayCounter = value;
				ELivingBeingAge currentAge = GetAgeCategory(value);
				if (currentAge != age)
				{
					age = currentAge;
					OnAgeCategoryChanged?.Invoke();
				}

			}
		}

		public int StartingAge { get; set; }
		public int AdultDay { get; set; }
		public int DeathDay { get; set; }

		//Shower seperate?

		/// <summary>
		/// Consumption per Day
		/// </summary>
		public float HungerConsumption { get; private set; }
		/// <summary>
		/// Consumption per Day
		/// </summary>
		public float ThirstConsumption { get; private set; }

		/// <summary>
		/// Consumed water of the day. Reset to 0 each morning.
		/// </summary>
		public float waterConsumed;
		/// <summary>
		/// Consumed nutrition of the day. Reset to 0 each morning.
		/// </summary>
		public float nutritionConsumed;

		public bool isGettingBurried;

		private void OnHungerChanged(object sender, PropertyChangedEventArgs args)
		{
			if (Entity.factionID != SystemsManager.MainPlayerFaction)
				return;
			var threshold = StaticData.HungerUrgencyThreshold;
			var entityFeature = Entity.GetFeature<EntityFeature>();
			if (Hunger.Progress <= threshold.min && entityFeature.IsAlive)
			{
				Entity.NeedsInfo.AddImage(UI3DFactory.IconHunger);
				SystemsManager.UnitSystem.RegisterHungryUnit(Entity);
				SystemsManager.InfobarSystem.RegisterEntity(EInfobarCategory.HungryUnits, Entity);
				if (Entity.HasFeature<CattleFeature>())
				{
					SystemsManager.InfobarSystem.RegisterEntity(EInfobarCategory.TroubledCattle, Entity);
				}
			}
			else
			{
				Entity.NeedsInfo.RemoveImage(UI3DFactory.IconHunger);
				SystemsManager.UnitSystem.UnregisterHungryUnit(Entity);
				SystemsManager.InfobarSystem.UnregisterEntity(EInfobarCategory.HungryUnits, Entity);
				SystemsManager.InfobarSystem.UnregisterEntity(EInfobarCategory.TroubledCattle, Entity);
			}
		}

		private void OnThirstChanged(object sender, PropertyChangedEventArgs args)
		{
			if (Entity.factionID != SystemsManager.MainPlayerFaction)
				return;
			var threshold = StaticData.ThirstUrgencyThreshold;
			var entityFeature = Entity.GetFeature<EntityFeature>();
			if (Thirst.Progress <= threshold.min && entityFeature.IsAlive)
			{
				Entity.NeedsInfo.AddImage(UI3DFactory.IconThirst);
				SystemsManager.UnitSystem.RegisterThirstyUnit(Entity);
				SystemsManager.InfobarSystem.RegisterEntity(EInfobarCategory.ThirstyUnits, Entity);
				if (Entity.HasFeature<CattleFeature>())
				{
					SystemsManager.InfobarSystem.RegisterEntity(EInfobarCategory.TroubledCattle, Entity);
				}
			}
			else
			{
				Entity.NeedsInfo.RemoveImage(UI3DFactory.IconThirst);
				SystemsManager.UnitSystem.UnregisterThirstyUnit(Entity);
				SystemsManager.InfobarSystem.UnregisterEntity(EInfobarCategory.ThirstyUnits, Entity);
				SystemsManager.InfobarSystem.UnregisterEntity(EInfobarCategory.TroubledCattle, Entity);
			}
		}

		public void RefreshUI3D()
		{
			OnHungerChanged(null, null);
			OnThirstChanged(null, null);
		}

		public override void Run(SystemsManager manager)
		{
			base.Run(manager);
			var unit = Entity.GetFeature<UnitFeature>();
			gender = unit.Gender;
			age = unit.Age;
			if (age == ELivingBeingAge.Child)
			{
				HungerConsumption *= 0.55f;
				ThirstConsumption *= 0.55f;
			}
			float salt = CivRandom.Range(0.9f, 1.1f);
			DeathDay = Mathf.RoundToInt(StaticData.lifeExpectancy * salt * Main.Instance.GameManager.gameConfig.YearDayLength);
			AdultDay = Mathf.RoundToInt(StaticData.adulthood * Main.Instance.GameManager.gameConfig.YearDayLength);
			StartingAge = Mathf.RoundToInt(StaticData.startingAge * Main.Instance.GameManager.gameConfig.YearDayLength);

			switch (age)
			{
				case ELivingBeingAge.Undefined:
				case ELivingBeingAge.Child:
					AgeDayCounter = Mathf.RoundToInt(CivRandom.Range(StartingAge, AdultDay * 0.8f));
					break;

				case ELivingBeingAge.Adult:
					AgeDayCounter = Mathf.RoundToInt(CivRandom.Range(AdultDay, DeathDay * 0.8f));
					break;
			}
			UpdateAge();
		}

		private ELivingBeingAge GetAgeCategory(int counter)
		{
			if (counter <= AdultDay)
				return ELivingBeingAge.Child;
			return ELivingBeingAge.Adult;
		}

		public override void Stop()
		{
			Entity.NeedsInfo.RemoveImage(UI3DFactory.IconThirst);
			Entity.NeedsInfo.RemoveImage(UI3DFactory.IconHunger);
			SystemsManager.UnitSystem.UnregisterHungryUnit(Entity);
			SystemsManager.UnitSystem.UnregisterThirstyUnit(Entity);
			SystemsManager.InfobarSystem.UnregisterEntity(EInfobarCategory.HungryUnits, Entity);
			SystemsManager.InfobarSystem.UnregisterEntity(EInfobarCategory.ThirstyUnits, Entity);
			SystemsManager.InfobarSystem.UnregisterEntity(EInfobarCategory.TroubledCattle, Entity);
			base.Stop();
		}

		public override void OnFactionChanged(int oldFaction)
		{
			base.OnFactionChanged(oldFaction);
			if (oldFaction == SystemsManager.MainPlayerFaction)
			{
				SystemsManager.UnitSystem.UnregisterHungryUnit(Entity);
				SystemsManager.UnitSystem.UnregisterThirstyUnit(Entity);
				SystemsManager.InfobarSystem.UnregisterEntity(EInfobarCategory.HungryUnits, Entity);
				SystemsManager.InfobarSystem.UnregisterEntity(EInfobarCategory.ThirstyUnits, Entity);
				SystemsManager.InfobarSystem.UnregisterEntity(EInfobarCategory.TroubledCattle, Entity);
			}
			if (Entity.factionID == SystemsManager.MainPlayerFaction)
			{
				OnHungerChanged(null, null);
				OnThirstChanged(null, null);
			}
		}

		//Called once per day from Unit System
		public void UpdateAge()
		{
			if (age < ELivingBeingAge.Adult)
			{
				//Children
				float percent = Mathf.InverseLerp(StartingAge, AdultDay, AgeDayCounter);

				float maxHunger = Mathf.Lerp(StaticData.maxHunger.childValue, StaticData.maxHunger.adultValue, percent);
				float currentHunger = (Hunger == null) ? (maxHunger / 2) : Hunger.Value;
				Hunger = new EntityProperty(maxHunger, currentHunger);
				Hunger.PropertyChanged += OnHungerChanged;
				OnHungerChanged(null, null);

				float maxThirst = Mathf.Lerp(StaticData.maxThirst.childValue, StaticData.maxThirst.adultValue, percent);
				float currentThirst = (Thirst == null) ? (maxThirst / 2) : Thirst.Value;
				Thirst = new EntityProperty(maxThirst, currentThirst);
				Thirst.PropertyChanged += OnThirstChanged;
				OnThirstChanged(null, null);

				float immobilize = Mathf.Lerp(StaticData.immobilizeStateThreshold.childValue, StaticData.immobilizeStateThreshold.adultValue, percent);
				float exhaust = Mathf.Lerp(StaticData.exhaustionStateThreshold.childValue, StaticData.exhaustionStateThreshold.adultValue, percent);
				float value = (vitality == null) ? 1 : vitality.Value;
				vitality = new EntityNeed("Vitality", new MinMax(immobilize, exhaust), 0);
				vitality.Value = value;

				HungerConsumption = Mathf.Lerp(StaticData.consumeHunger.childValue, StaticData.consumeHunger.adultValue, percent);
				ThirstConsumption = Mathf.Lerp(StaticData.consumeThirst.childValue, StaticData.consumeThirst.adultValue, percent);

				innards = Mathf.Lerp(StaticData.innards.childValue, StaticData.innards.adultValue, percent);
				meat = Mathf.Lerp(StaticData.meat.childValue, StaticData.meat.adultValue, percent);
			}
			else
			{
				//Adults
				float percent = Mathf.InverseLerp(AdultDay, DeathDay, AgeDayCounter);

				float maxHunger = Mathf.Lerp(StaticData.maxHunger.adultValue, StaticData.maxHunger.seniorValue, percent);
				float currentHunger = (Hunger == null) ? (maxHunger / 2) : Hunger.Value;
				Hunger = new EntityProperty(maxHunger, currentHunger);
				Hunger.PropertyChanged += OnHungerChanged;
				OnHungerChanged(null, null);

				float maxThirst = Mathf.Lerp(StaticData.maxThirst.adultValue, StaticData.maxThirst.seniorValue, percent);
				float currentThirst = (Thirst == null) ? (maxThirst / 2) : Thirst.Value;
				Thirst = new EntityProperty(maxThirst, currentThirst);
				Thirst.PropertyChanged += OnThirstChanged;
				OnThirstChanged(null, null);

				float immobilize = Mathf.Lerp(StaticData.immobilizeStateThreshold.adultValue, StaticData.immobilizeStateThreshold.seniorValue, percent);
				float exhaust = Mathf.Lerp(StaticData.exhaustionStateThreshold.adultValue, StaticData.exhaustionStateThreshold.seniorValue, percent);
				float value = (vitality == null) ? 1 : vitality.Value;
				vitality = new EntityNeed("Vitality", new MinMax(immobilize, exhaust), 0);
				vitality.Value = value;

				HungerConsumption = Mathf.Lerp(StaticData.consumeHunger.adultValue, StaticData.consumeHunger.seniorValue, percent);
				ThirstConsumption = Mathf.Lerp(StaticData.consumeThirst.adultValue, StaticData.consumeThirst.seniorValue, percent);

				innards = Mathf.Lerp(StaticData.innards.adultValue, StaticData.innards.seniorValue, percent);
				meat = Mathf.Lerp(StaticData.meat.adultValue, StaticData.meat.seniorValue, percent);
			}
		}

		public override void Destroy()
		{
			SystemsManager.InfobarSystem.UnregisterEntity(EInfobarCategory.HungryUnits, Entity);
			SystemsManager.InfobarSystem.UnregisterEntity(EInfobarCategory.ThirstyUnits, Entity);
			SystemsManager.InfobarSystem.UnregisterEntity(EInfobarCategory.TroubledCattle, Entity);
			base.Destroy();
		}

		public override ISaveable CollectData()
		{
			var data = new LivingBeingFeatureSaveData();
			data.gender = gender;
			data.age = age;

			if (Hunger != null)
				data.hunger = Hunger.Value;
			if (Thirst != null)
				data.thirst = Thirst.Value;
			data.hungerConsumption = HungerConsumption;
			data.thirstConsumption = ThirstConsumption;
			data.waterConsumed = waterConsumed;
			data.nutritionConsumed = nutritionConsumed;
			data.isGettingBurried = isGettingBurried;
			data.ageDayCounter = AgeDayCounter;
			data.childAge = StartingAge;
			data.deathAge = DeathDay;
			data.adultAge = AdultDay;
			return data;
		}

		public override void ApplyData(LivingBeingFeatureSaveData data)
		{
			gender = data.gender;
			age = data.age;

			if (Hunger != null)
				Hunger.Value = data.hunger;
			if (Thirst != null)
				Thirst.Value = data.thirst;
			HungerConsumption = data.hungerConsumption;
			ThirstConsumption = data.thirstConsumption;
			waterConsumed = data.waterConsumed;
			nutritionConsumed = data.nutritionConsumed;
			isGettingBurried = data.isGettingBurried;
			AgeDayCounter = data.ageDayCounter;
			StartingAge = data.childAge;
			DeathDay = data.deathAge;
			AdultDay = data.adultAge;
			UpdateAge();
		}
	}
}