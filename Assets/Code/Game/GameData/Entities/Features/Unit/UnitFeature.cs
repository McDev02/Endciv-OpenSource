using UnityEngine;

namespace Endciv
{
	public class UnitFeature :
		Feature<UnitFeatureSaveData>,
		IViewController<UnitFeatureView>
	{
		public UnitFeatureView View { get; private set; }

		public UnitFeatureStaticData StaticData { get; private set; }

		public ELivingBeingAge Age { get; set; }
		public ELivingBeingGender Gender { get; set; }
		public EUnitType UnitType { get; set; }

		public bool IsCarrying
		{
			get
			{
				var inventory = Entity.GetFeature<InventoryFeature>();
				return inventory != null && inventory.Load > 0;
			}
		}

		public override void Setup(BaseEntity entity, FeatureParamsBase args = null)
		{
			base.Setup(entity);
			StaticData = Entity.StaticData.GetFeature<UnitFeatureStaticData>();

			UnitType = StaticData.unitType;

			var featureParams = (UnitFeatureParams)args;
			if (featureParams != null)
			{
				Age = featureParams.Age;
				Gender = featureParams.Gender;
				UnitType = featureParams.UnitType;
			}
			
			if (Gender == ELivingBeingGender.Undefined)
				Gender = Random.value <= UnitSystem.GenderGenerationThreshold ? ELivingBeingGender.Male : ELivingBeingGender.Female;
			if (Age == ELivingBeingAge.Undefined)
				Age = Random.value <= UnitSystem.AdultGenerationThreshold ? ELivingBeingAge.Adult : ELivingBeingAge.Child;
			if (Entity.StaticData.ID == "human")
			{
				Entity.GetFeature<EntityFeature>().EntityName =
					new NameGenerator().GetRandomName(Gender, false);
			}
		}

		//Properties        
		public int equippedChamberID { get; private set; }

		public void MoveToPosition(Vector2i gridPosition)
		{
			var worldPos = Main.Instance.GameManager.GridMap.View.GetTileWorldPosition(gridPosition).To3D(Entity.GetFeature<EntityFeature>().View.transform.position.y);
			Entity.GetFeature<EntityFeature>().View.transform.position = worldPos;
			Entity.GetFeature<EntityFeature>().GridID = gridPosition;
		}

		public void MoveToPosition(Vector3 worldPosition)
		{
			Entity.GetFeature<EntityFeature>().View.transform.position = worldPosition;
			Entity.GetFeature<EntityFeature>().GridID =
				Main.Instance.GameManager.GridMap.View.SamplePointWorld(worldPosition.To2D());
		}

		public override ISaveable CollectData()
		{
			var data = new UnitFeatureSaveData();
			data.unitType = UnitType;
			data.isVisible = View.IsVissible;
			return data;
		}

		public override void ApplyData(UnitFeatureSaveData data)
		{
			UnitType = data.unitType;
			View.ShowHide(data.isVisible);
		}

		public override void Run(SystemsManager manager)
		{
			base.Run(manager);
			//Add AI
			switch (UnitType)
			{
				case EUnitType.Citizen:
					{
						var feature = new CitizenAIAgentFeature();
						feature.Setup(Entity);
						feature.SetAIAgentSettings(Main.Instance.GameManager.gameMechanicSettings.aiSettings.citizenClasses[0]);
						Entity.AttachFeature(feature);
					}
					break;

				case EUnitType.Animal:
					{
						var feature = new AnimalAIAgentFeature();
						feature.Setup(Entity);
						feature.SetAIAgentSettings(StaticData.aiSettings);
						Entity.AttachFeature(feature);
					}

					break;

				case EUnitType.Trader:
					{
						var feature = new TraderAIAgentFeature();
						feature.Setup(Entity);
						feature.SetAIAgentSettings(StaticData.aiSettings);
						Entity.AttachFeature(feature);
					}
					break;

				case EUnitType.Immigrant:
					{
						var feature = new ImmigrantAIAgentFeature();
						feature.Setup(Entity);
						feature.SetAIAgentSettings(StaticData.aiSettings);
						Entity.AttachFeature(feature);
					}
					break;
			}
			equippedChamberID = InventorySystem.AddChamber(Entity.GetFeature<InventoryFeature>(), "Equipped");
			SystemsManager.UnitSystem.RegisterUnit(Entity);
		}


		internal void ConvertImmigrantToCitizen()
		{
			if (UnitType != EUnitType.Immigrant)
			{
				Debug.LogError($"Unit that should be an immigrant is: {UnitType}");
				return;
			}

			UnitType = EUnitType.Citizen;
			Entity.RemoveFeature<ImmigrantAIAgentFeature>();

			var feature = new CitizenAIAgentFeature();
			feature.Setup(Entity);
			feature.SetAIAgentSettings(Main.Instance.GameManager.gameMechanicSettings.aiSettings.citizenClasses[0]);
			Entity.AttachFeature(feature);
		}

		public override void Stop()
		{
			SystemsManager.UnitSystem.DeregisterUnit(Entity);
			View.SwitchAnimationState(EAnimationState.Sleeping);
			Entity.NeedsInfo.AddImage(UI3DFactory.IconDeath);
			SystemsManager.InfobarSystem.RegisterEntity(EInfobarCategory.DeadUnits, Entity);
			base.Stop();
		}

		public override void Destroy()
		{
			SystemsManager.UnitSystem.DeregisterCorpse(Entity, Entity.factionID);
			SystemsManager.InfobarSystem.UnregisterEntity(EInfobarCategory.DeadUnits, Entity);
			base.Destroy();
		}

		public override void OnFactionChanged(int oldFaction)
		{
			base.OnFactionChanged(oldFaction);
			SystemsManager.UnitSystem.OnFactionChanged(Entity, oldFaction);
		}

		#region IViewController

		public int CurrentViewID { get; set; }

		public void SetView(FeatureViewBase view)
		{
			View = (UnitFeatureView)view;
		}

		public void ShowView()
		{
			if (View != null)
			{
				View.ShowHide(true);
			}
		}

		public void HideView()
		{
			if (View != null)
			{
				View.ShowHide(false);
			}
		}

		public void UpdateView()
		{
			if (View != null)
			{
				View.UpdateView();
			}
		}

		public void SelectView()
		{
			if (View != null)
			{
				View.OnViewSelected();
			}
		}

		public void DeselectView()
		{
			if (View != null)
			{
				View.OnViewDeselected();
			}
		}
		#endregion
	}
}