using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using System.Linq;

namespace Endciv
{
	public class UnitFactory : EntityFactory, IExiting
	{
		/*public static UnitFactory Instance { get; private set; }
		GridMap gridMap;
		NameGenerator nameGenerator;

		const string DummyObject = "dummy";
		public const float HumanSizeFactor = 1f;
		public const float GenderGenerationThreshold = 0.6f;
		public const float AdultGenerationThreshold = 0.5f;

		//Static Data
		public Dictionary<string, EntityStaticData> UnitData;
		public Dictionary<string, UnitPrefabStaticData> UnitPrefabData;

		//Views
		public Dictionary<string, Dictionary<int, GameObject>> UnitPrefabViews { get; private set; }		

		[SerializeField] CharacterMeshFactory characterMeshFactory;

#if UNITY_EDITOR 
		static Transform UnitRoot;
#endif
		SimpleEntityFactory resourceFactory;
		AISettings aISettings;

		public void Setup(GridMap gridMap, SimpleEntityFactory resourceFactory, SystemsManager systemsManager, AISettings aISettings)
		{
			base.Setup(systemsManager);
			this.aISettings = aISettings;
			this.resourceFactory = resourceFactory;
			this.gridMap = gridMap;
			if (Instance != null) Debug.LogError("Singleton UnitFactory already exists! ");
			Instance = this;

#if UNITY_EDITOR 
			if (UnitRoot == null)
			{
				UnitRoot = new GameObject("Units").transform;
				UnitRoot.SetParent(UnitRoot);
			}
#endif
			UnitData = StaticDataIO.Instance.GetData<EntityStaticData>("Units");
			UnitPrefabData = StaticDataIO.Instance.GetData<UnitPrefabStaticData>("UnitPrefabs");

			AddDummyContent();
			nameGenerator = new NameGenerator();

			ValidateUnitsPrefabReferences();
			LoadUnitPrefabViews();
		}

		internal EntityStaticData GetStaticData(string id)
		{
			if (!UnitData.ContainsKey(id))
			{
				Debug.LogError("Unit ID(" + id + ") not found.");
				return null;
			}
			return UnitData[id];
		}

		void AddDummyContent()
		{
			UnitPrefabStaticData dummy = new UnitPrefabStaticData();
			dummy.ModelID = DummyObject;
			dummy.name = DummyObject;
			UnitPrefabData.Add(dummy.ID, dummy);
		}

		/// <summary>
		/// Check if Prefab exists in UnitPrefab. Otherwise replace with dummy object.
		/// </summary>
		private void ValidateUnitsPrefabReferences()
		{
			foreach (var item in UnitData)
			{
				var Unit = item.Value;
				if (!UnitPrefabData.ContainsKey(Unit.PrefabID))
				{
					Debug.LogWarning("PrefabID (" + Unit.PrefabID + ") of Unit (" + Unit.ID + ") not found.");
					Unit.PrefabID = DummyObject;
					//Unit.GetFeature<UnitFeatureStaticData>().Prefab = UnitPrefabData[Unit.PrefabID];
				}
				//else
				//	Unit.GetFeature<UnitFeatureStaticData>().Prefab = UnitPrefabData[Unit.PrefabID];
			}
		}

		internal Transform GetModelObject(string modelID, int variationID = -1)
		{
			var views = UnitPrefabViews[modelID];
			if (!views.ContainsKey(variationID))
				variationID = views.Keys.ToArray()[Random.Range(0, views.Count)];

			GameObject viewModel = Instantiate(views[variationID]);
			viewModel.name = "model";

			return viewModel.transform;
		}

		/// <summary>
		/// Loads View game objects for each ModelID. Replace UnitPrefabData.ModelID with dummy object if not found.
		/// </summary>
		private void LoadUnitPrefabViews()
		{
			UnitPrefabViews = new Dictionary<string, Dictionary<int, GameObject>>(UnitPrefabData.Count);
			Dictionary<int, GameObject> tmpVariations = null;

			string path = "Units/";
			foreach (var item in UnitPrefabData)
			{
				tmpVariations = new Dictionary<int, GameObject>();
				var unitPrefab = item.Value;

				if (!UnitPrefabViews.ContainsKey(unitPrefab.ModelID))
				{
					var obj = Resources.Load<GameObject>(path + unitPrefab.ModelID);
					if (obj == null)
					{
						Debug.LogWarning("unitPrefab " + unitPrefab.ModelID + " could not be found. Use Dummy instead. In: " + path + unitPrefab.ModelID);
						unitPrefab.ModelID = DummyObject;

						obj = Resources.Load<GameObject>(path + DummyObject);
						if (obj == null) Debug.LogError("Dummy unitPrefab not found! " + path + DummyObject);
					}
					if (!UnitPrefabViews.ContainsKey(unitPrefab.ModelID))
					{
						string variationPath = path + unitPrefab.ModelID + "_";
						tmpVariations.Add(0, obj);

						//Find variations
						for (int i = 1; i < 20; i++)
						{
							obj = Resources.Load<GameObject>(variationPath + i);
							if (obj != null)
							{
								tmpVariations.Add(i, obj);
							}
						}
						UnitPrefabViews.Add(unitPrefab.ModelID, tmpVariations);
						//Debug.Log(logstring);
					}
				}
			}
		}

		public BaseEntity CreateInstance(string id, Vector3 pos, int faction, Guid? uid = null, ELivingBeingAge age = ELivingBeingAge.Undefined, ELivingBeingGender gender = ELivingBeingGender.Undefined)
		{
			if (gender == ELivingBeingGender.Undefined)
				gender = Random.value <= GenderGenerationThreshold ? ELivingBeingGender.Male : ELivingBeingGender.Female;
			if (age == ELivingBeingAge.Undefined)
				age = Random.value <= AdultGenerationThreshold ? ELivingBeingAge.Adult : ELivingBeingAge.Child;

			bool isnull = string.IsNullOrEmpty(id);
			if (isnull || !UnitData.ContainsKey(id))
			{
				Debug.LogError("Unit ID(" + (isnull ? "NULL" : id.ToString()) + ") not found.");
				return null;
			}

			//Create Entity
			var data = UnitData[id];
			var unitData = UnitPrefabData[data.PrefabID];   //data.PrefabID shall always be valid  

			//Create View - ModelID shall always be valid
			var modelID = unitData.GetGenderModel(gender, age);
			Transform viewModel = GetModelObject(modelID);
#if UNITY_EDITOR 
			BaseEntity entity = CreateBaseEntity(faction, data, systemsManager, viewModel.gameObject, UnitRoot);
#else
			BaseEntity entity = CreateBaseEntity(faction, data, systemsManager, viewModel.gameObject);
#endif
			if (uid != null)
				entity.SetEntityGuid(uid.Value);
			entity.GetFeature<EntityFeature>().View.transform.position = pos;

			//Generate Mesh
			var modularView = viewModel.GetComponent<ModularCharacterView>();
			float sizeVariation = HumanSizeFactor;

			switch (age)
			{
				case ELivingBeingAge.Child:
					if (gender == ELivingBeingGender.Male)
						sizeVariation *= unitData.childMaleSizes.GetRandom();
					else
						sizeVariation *= unitData.childFemaleSizes.GetRandom();
					break;
				case ELivingBeingAge.Adolecent:
				case ELivingBeingAge.Senior:
				case ELivingBeingAge.Adult:
					if (gender == ELivingBeingGender.Male)
						sizeVariation *= unitData.adultMaleSizes.GetRandom();
					else
						sizeVariation *= unitData.adultFemaleSizes.GetRandom();
					break;
			}
			viewModel.localScale = Vector3.one * sizeVariation;

			if (modularView != null)
				characterMeshFactory.GenerateModel(modularView, gender, age);

			SetupEntity(entity, data, gender, age);
			var unitFeature = entity.GetFeature<UnitFeature>();
			unitFeature.SetView(viewModel.GetComponent<UnitFeatureView>());
			if (unitFeature.View == null)
			{
				Debug.LogWarning("Unit model" + modelID + " had no view class. Adding one to prevent exceptions.");
				unitFeature.SetView(viewModel.gameObject.AddComponent<UnitFeatureView>());
			}

			//unitFeature.View.Setup(unitFeature, gridMap);

			if (id == "human") entity.GetFeature<EntityFeature>().EntityName =
					nameGenerator.GetRandomName(gender, false);

			var keys = entity.Features.Keys.ToArray();
			foreach(var key in keys)
			{
				entity.Features[key].AutoRun = true;
			}

			entity.Run();
			return entity;
		}

		void SetupEntity(BaseEntity entity, EntityStaticData data, ELivingBeingGender gender, ELivingBeingAge age)
		{
			entity.GetFeature<EntityFeature>().View.gameObject.name = "#" + entity.IDString + " - " + data.ID;

			var unitData = data.GetFeature<UnitFeatureStaticData>();
			if (unitData != null)
			{
				var feature = new UnitFeature();
				feature.Setup(entity);
				entity.AttachFeature(feature);
			}

			var gridAgent = data.GetFeature<GridAgentStaticData>();
			if (gridAgent != null)
			{
				var feature = new GridAgentFeature();
				feature.Setup(entity);
				entity.AttachFeature(feature);
			}

			var livingBeing = data.GetFeature<LivingBeingStaticData>();
			if (livingBeing != null)
			{
				var feature = new LivingBeingFeature();
				feature.Setup(entity);
				//feature.SetAgeGender(age, gender);
				entity.AttachFeature(feature);
			}

			var cattle = data.GetFeature<CattleStaticData>();
			if(cattle != null)
			{
				var feature = new CattleFeature();
				feature.Setup(entity);
				entity.AttachFeature(feature);
			}

			var item = data.GetFeature<ItemFeatureStaticData>();
			if (item != null)
			{
				var feature = new ItemFeature();
				feature.Setup(entity);
				entity.AttachFeature(feature);
			}

			//Features
			var inventory = data.GetFeature<InventoryStaticData>();
			if (inventory != null)
			{
				var feature = new InventoryFeature();
				feature.Setup(entity);
				feature.SetStatistics(InventorySystem.GetNewInventoryStatistics());
				entity.AttachFeature(feature);
			}

			//Add AI
			switch (unitData.unitType)
			{
				case EUnitType.Citizen:
					{
						var feature = new CitizenAIAgentFeature();
						feature.Setup(entity);
						feature.SetAIAgentSettings(Main.Instance.GameManager.gameMechanicSettings.aiSettings.citizenClasses[0]);
						entity.AttachFeature(feature);
					}					
					break;
				case EUnitType.Animal:
					{
						var feature = new AnimalAIAgentFeature();
						feature.Setup(entity);
						feature.SetAIAgentSettings(unitData.aiSettings);
						entity.AttachFeature(feature);						
					}
					
					break;
				case EUnitType.Immigrant:
					{
						var feature = new ImmigrantAIAgentFeature();
						feature.Setup(entity);
						feature.SetAIAgentSettings(Main.Instance.GameManager.gameMechanicSettings.aiSettings.citizenClasses[0]);
						entity.AttachFeature(feature);
					}					
					break;

				case EUnitType.Trader:
				default:
					{
						var feature = new TraderAIAgentFeature();
						feature.Setup(entity);
						feature.SetAIAgentSettings(unitData.aiSettings);
						entity.AttachFeature(feature);
					}					
					break;

				
			}
		}

		new public void OnExit()
		{
			base.OnExit();
			Instance = null;
#if UNITY_EDITOR 
			UnitRoot = null;
#endif
		}*/
	}
}