using System.Collections.Generic;
using UnityEngine;
namespace Endciv
{
	public class UserTool_Unit : UserTool
	{
		private UserToolsView userToolsView;

		private GameInputManager InputManager;
		private UserToolSystem PlacementSystem;
		private SimpleEntityFactory factory;
		private SystemsManager SystemsManager;
		private GridMap GridMap;
		private GridMapView GridMapView;
		Transform PreviewContainer;
		int BatchPlacement;
		float BatchRadius;
		MeshRenderer currentRectObject;

		//Key: Prefab.ModelID
		Dictionary<string, Transform> ModelViewPool;

		Transform CurrentObject;
		EntityStaticData CurrentStaticData;
		string currentObjectID;

		internal UserTool_Unit(UserToolSystem placementSystem, UserToolsView userToolsView, GameManager gameManager, SimpleEntityFactory factory)
		{
			this.userToolsView = userToolsView;

			PlacementSystem = placementSystem;
			InputManager = gameManager.gameInputManager;
			SystemsManager = gameManager.SystemsManager;
			this.factory = factory;
			GridMap = gameManager.GridMap;
			GridMapView = GridMap.View;
			PreviewContainer = new GameObject("PlaceTool_Unit Previews").transform;

			ModelViewPool = new Dictionary<string, Transform>();
			currentRectObject = Object.Instantiate(userToolsView.RectIndicatorPrefab);

			DoBeforeLeaving();
		}

		internal override void DoBeforeEntering()
		{
		}

		internal override void DoBeforeLeaving()
		{
			HidePreviewObject();
			currentObjectID = "";
			CurrentObject = null;
			CurrentStaticData = null;
			currentRectObject.gameObject.SetActive(false);
		}

		internal void UIPlaceUnitBatch(string id, int batch, ELivingBeingGender gender, ELivingBeingAge age, float batchRadius = 0)
		{
			UIPlaceUnit(id, gender, age);
			BatchPlacement = batch;
			BatchRadius = batchRadius;
			currentRectObject.gameObject.SetActive(true);
		}
		internal void UIPlaceUnit(string id, ELivingBeingGender gender, ELivingBeingAge age)
		{
			BatchPlacement = 0;
			currentObjectID = id;
			CurrentStaticData = factory.EntityStaticData[id];
			ShowPreviewObject(CurrentStaticData.ID);
			DoBeforeEntering();
			currentRectObject.gameObject.SetActive(true);
		}

		internal override void Process()
		{
			if (CurrentObject == null) return;

			//Position on Mouse
			if (!InputManager.Pointer1.enabled) return;
			Vector3 pos = InputManager.Pointer1.TerrainPosition;
			var cell = InputManager.Pointer1.GridIndex;

			const float pad = 0.15f;
			CurrentObject.transform.position = pos;
			currentRectObject.transform.position = pos - new Vector3(GridMapView.TileSize / 2f + pad, 0, GridMapView.TileSize / 2f + pad);
			currentRectObject.transform.localScale = new Vector3(GridMapView.TileSize + pad * 2, 1, GridMapView.TileSize + pad * 2);

			bool CanPlace = !GridMap.IsOccupied(cell, false);
#if DEV_MODE || UNITY_EDITOR
			if (Input.GetKey(KeyCode.LeftControl)) CanPlace = true;
#endif
			//Visualize
			Color color = CanPlace ? userToolsView.ValidColor : userToolsView.InvalidColor;
			currentRectObject.material.SetColor(userToolsView.ColorName, color);

			//Input
			if (CanPlace && Input.GetMouseButtonDown(0))
			{
				if (BatchPlacement > 1)
					CreateUnitBatch(currentObjectID, pos, SystemsManager.MainPlayerFaction, BatchPlacement);
				else
					CreateUnit(currentObjectID, pos, SystemsManager.MainPlayerFaction);
			}
		}


		private void GetNewObject() { }

		internal override void Stop()
		{
			DoBeforeLeaving();
		}


		void ShowPreviewObject(string id)
		{
			if (ModelViewPool.ContainsKey(id))
				CurrentObject = ModelViewPool[id];
			else
			{
				CurrentObject = factory.GetStaticData<UnitFeatureStaticData>(id).GetFeatureViewInstance().transform;
				CurrentObject.SetParent(PreviewContainer);
				ModelViewPool.Add(id, CurrentObject);
			}
			CurrentObject.gameObject.SetActive(true);
		}
		void HidePreviewObject()
		{
			if (CurrentObject != null)
				CurrentObject.gameObject.SetActive(false);

			currentRectObject.gameObject.SetActive(false);
		}

		internal void CreateUnitBatch(string id, Vector3 pos, int faction, int batch, ELivingBeingAge age = ELivingBeingAge.Undefined, ELivingBeingGender gender = ELivingBeingGender.Undefined)
		{
			for (int i = 0; i < batch; i++)
			{
				if (BatchRadius >= 0)
					CreateUnit(id, pos + (Random.insideUnitCircle * BatchRadius).To3D(), faction, age, gender);
				else
					CreateUnit(id, pos, faction, age, gender);
			}
		}

		internal BaseEntity CreateUnit(string id, Vector3 pos, int faction, ELivingBeingAge age = ELivingBeingAge.Undefined, ELivingBeingGender gender = ELivingBeingGender.Undefined)
		{
			var factoryParams = new FactoryParams();
			factoryParams.SetParams
				(
					new GridAgentFeatureParams()
					{
						Position = pos
					},
					new EntityFeatureParams()
					{
						FactionID = faction
					},
					new UnitFeatureParams()
					{
						Age = age,
						Gender = gender
					}
				);
			var obj = factory.CreateInstance(id, null, factoryParams);
			return obj;
		}

		internal TraderAIAgentFeature CreateTrader(string id, Vector3 pos, TraderStaticData traderData)
		{
			var factoryParams = new FactoryParams();
			factoryParams.SetParams
				(
					new GridAgentFeatureParams()
					{
						Position = pos
					},
					new EntityFeatureParams()
					{
						FactionID = SystemsManager.NeutralNpcFaction
					}
					//new UnitFeatureParams()
					//{
					//	Age = ELivingBeingAge.Adult,
					//	Gender = ELivingBeingGender.Male,
					//	UnitType = EUnitType.Trader
					//}
				);
			var obj = factory.CreateInstance(id, null, factoryParams);

			TraderAIAgentFeature trader = null;
			if (obj.HasFeature<TraderAIAgentFeature>())
			{
				trader = obj.GetFeature<TraderAIAgentFeature>();
				trader.traderData = traderData;
				trader.Entity.GetFeature<EntityFeature>().EntityName = traderData.TraderName;
			}
			else
				Debug.LogError($"Instantiated unit ({id}) is not a trader!");
			return trader;
		}

		internal ImmigrantAIAgentFeature CreateImmigrant(string id, Vector3 pos, ELivingBeingAge age = ELivingBeingAge.Undefined, ELivingBeingGender gender = ELivingBeingGender.Undefined)
		{
			var factoryParams = new FactoryParams();
			factoryParams.SetParams
				(
					new GridAgentFeatureParams()
					{
						Position = pos
					},
					new EntityFeatureParams()
					{
						FactionID = SystemsManager.NeutralNpcFaction
					},
					new UnitFeatureParams()
					{
						Age = age,
						Gender = gender,
						UnitType = EUnitType.Immigrant
					}
				);
			var unit = factory.CreateInstance(id, null, factoryParams);

			ImmigrantAIAgentFeature immigrant = null;
			if (unit.HasFeature<ImmigrantAIAgentFeature>())
			{
				immigrant = unit.GetFeature<ImmigrantAIAgentFeature>();
			}
			else
				Debug.LogError($"Instantiated unit ({id}) is not an immigrant!");
			return immigrant;
		}
	}
}