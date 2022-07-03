namespace Endciv
{
	[System.Serializable]
	public struct SheduleAIData
	{
		public bool hasConsumedFood;

		public bool lookedForShower;
		public bool lookedForToilet;
		public bool hadPause;

		public void Reset()
		{
			hasConsumedFood = false;
			lookedForShower = false;
			lookedForToilet = false;
			hadPause = false;
		}
	}

	public class CitizenAIAgentFeature :
		AIAgentFeature<CitizenAIAgentFeatureSaveData>,
		IUI3DController
	{
		public CitizenShedule shedule;
		public SheduleAIData lastSheduleData;
		public CitizenShedule.ESheduleType currentShedule;
		public CitizenClass citizenClass;

		public override void Setup(BaseEntity entity, FeatureParamsBase args = null)
		{
			base.Setup(entity);
			var being = Entity.GetFeature<LivingBeingFeature>();
			Occupation = being.age == ELivingBeingAge.Child ? EOccupation.None : EOccupation.Labour;
		}

		public override void SetAIAgentSettings(AiAgentSettings aiAgentSettings)
		{
			base.SetAIAgentSettings(aiAgentSettings);
			citizenClass = (CitizenClass)aiAgentSettings;

			ToiletNeed = new EntityNeed("Toilet", citizenClass.ToiletSatisfaction, 1.5f, 0);
			SettlementNeed = new EntityNeed("Settlement", citizenClass.SettlementSatisfaction, 1);
			HomehNeed = new EntityNeed("Home", citizenClass.HomeSatisfaction, 1);

			needsLookup.AddRange(new EntityNeed[] { ToiletNeed, SettlementNeed, HomehNeed });
			dailyReducedNeeds.AddRange(new EntityNeed[] { ToiletNeed });
		}

		//StaticData

		//Properties
		public EntityNeed FoodVariationNeed;
		public EntityNeed FoodQualityNeed;
		public EntityNeed ToiletNeed;
		public EntityNeed SettlementNeed;
		public EntityNeed HomehNeed;

		//Which occupation the agent has
		public EOccupation Occupation;

		//Properties
		private HousingFeature home;
		public HousingFeature Home
		{
			get
			{
				return home;
			}
			set
			{
				if (home != value)
				{
					home = value;
					OnHomePropertyChanged();
				}
			}
		}
		public bool HasHome { get { return Home != null; } }

		private void OnHomePropertyChanged()
		{
			if (HasHome)
			{
				SystemsManager.InfobarSystem.UnregisterEntity(EInfobarCategory.HomelessUnits, Entity);
			}
			else
			{
				SystemsManager.InfobarSystem.RegisterEntity(EInfobarCategory.HomelessUnits, Entity);
			}
			RefreshUI3D();
		}

		public void RefreshUI3D()
		{
			var entityFeature = Entity.GetFeature<EntityFeature>();
			if (entityFeature.FactionID != SystemsManager.MainPlayerFaction)
				return;
			if (home != null || !entityFeature.IsAlive)
			{
				Entity.NeedsInfo.RemoveImage(UI3DFactory.IconHomeless);
			}
			else
			{
				Entity.NeedsInfo.AddImage(UI3DFactory.IconHomeless);
			}
		}

		public override void Run(SystemsManager manager)
		{
			base.Run(manager);
			SystemsManager.HousingSystem.RegisterUnit(Entity);
			OnHomePropertyChanged();
		}

		public override void Stop()
		{
			base.Stop();
			SystemsManager.HousingSystem.DeregisterHome(Entity);
			SystemsManager.HousingSystem.DeregisterUnit(Entity);
			SystemsManager.InfobarSystem.UnregisterEntity(EInfobarCategory.HomelessUnits, Entity);
			Entity.NeedsInfo.RemoveImage(UI3DFactory.IconHomeless);
		}

		public override void OnFactionChanged(int oldFaction)
		{
			base.OnFactionChanged(oldFaction);
			SystemsManager.HousingSystem.DeregisterUnit(Entity, oldFaction);
			SystemsManager.HousingSystem.RegisterUnit(Entity);
			if (Entity.factionID == SystemsManager.MainPlayerFaction)
				OnHomePropertyChanged();
		}

		public override void Destroy()
		{
			SystemsManager.InfobarSystem.UnregisterEntity(EInfobarCategory.HomelessUnits, Entity);
			base.Destroy();
		}

		public override ISaveable CollectData()
		{
			var data = new CitizenAIAgentFeatureSaveData();
			if (CurrentTask != null)
			{
				data.taskData = CurrentTask.CollectData() as TaskSaveData;
			}

			data.occupation = (int)Occupation;
			data.lastSheduleAIData = lastSheduleData;
			if (FoodQualityNeed != null)
				data.foodQualityNeed = FoodQualityNeed.Value;
			if (FoodVariationNeed != null)
				data.foodVariationNeed = FoodVariationNeed.Value;
			if (ToiletNeed != null)
				data.toiletNeed = ToiletNeed.Value;
			if (CleaningNeed != null)
				data.cleaningNeed = CleaningNeed.Value;
			return data;
		}

		public override void ApplyData(CitizenAIAgentFeatureSaveData data)
		{
			Occupation = (EOccupation)data.occupation;
			lastSheduleData = data.lastSheduleAIData;
			if (FoodVariationNeed != null)
				FoodVariationNeed.Value = data.foodVariationNeed;
			if (FoodQualityNeed != null)
				FoodQualityNeed.Value = data.foodQualityNeed;
			if (ToiletNeed != null)
				ToiletNeed.Value = data.toiletNeed;
			if (CleaningNeed != null)
				CleaningNeed.Value = data.cleaningNeed;
		}
	}
}