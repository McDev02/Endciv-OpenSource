using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Endciv
{
	public class UserTool_GridObject : UserTool
	{
		private GameManager gameManager;
        private PlacementInfoUIController placementInfo;
		private UserInfo userInfo;
		private GameInputManager inputManager;
		private UserToolSystem placementSystem;
		private SimpleEntityFactory factory;
		private SystemsManager systemsManager;
		private GridMap gridMap;
		private GridMapView gridMapView;
		private PartitionSystem partitionSystem;
		private UserToolsView userToolsView;

		StringBuilder stringBuilder = new StringBuilder();
		public Action<string> OnBuildingPlaced;

		List<MeshRenderer> rectViewObjects;
		List<MeshRenderer> entranceViewObjects;
		List<MeshRenderer> radiusViewObjects;

		Transform PreviewContainer;
		RectBounds tmpRect;

		TerrainView.ELayerView terrainLayerBeforePlacing;

		public enum EObjectType { Structure, ResourcePile }
		public EObjectType currentObjectType;
		private int currentViewID = -1;

		//Key: Prefab.ModelID
		Dictionary<string, Dictionary<int, Transform>> ModelViewPool;

		Transform CurrentObject;
		EntityStaticData CurrentStaticData;
		string currentObjectID;
		GridObjectData CurrentGridObjectData;

		private List<BaseEntity> overlappingStructures = new List<BaseEntity>();

		internal UserTool_GridObject(UserToolSystem placementSystem, UserToolsView userToolsView, GameManager gameManager, SimpleEntityFactory factory)
		{
			this.gameManager = gameManager;
			this.userToolsView = userToolsView;
			this.placementSystem = placementSystem;
			inputManager = gameManager.gameInputManager;
			systemsManager = gameManager.SystemsManager;
            placementInfo = gameManager.GameGUIController.placementInfoUIController;
			userInfo = gameManager.GameGUIController.userInfo;

			this.factory = factory;
			gridMap = gameManager.GridMap;
			gridMapView = gridMap.View;
			partitionSystem = gameManager.SystemsManager.PartitionSystem;
			PreviewContainer = new GameObject("PlaceTool_GridObject Previews").transform;

			ModelViewPool = new Dictionary<string, Dictionary<int, Transform>>();

			rectViewObjects = new List<MeshRenderer>(5);
			for (int i = 0; i < rectViewObjects.Capacity; i++)
			{
				var obj = UnityEngine.Object.Instantiate(userToolsView.RectIndicatorPrefab);
				obj.gameObject.SetActive(false);
				rectViewObjects.Add(obj);
			}
			entranceViewObjects = new List<MeshRenderer>(4);
			for (int i = 0; i < entranceViewObjects.Capacity; i++)
			{
				var obj = UnityEngine.Object.Instantiate(userToolsView.EntranceIndicatorPrefab);
				obj.gameObject.SetActive(false);
				entranceViewObjects.Add(obj);
			}
			radiusViewObjects = new List<MeshRenderer>(4);
			for (int i = 0; i < radiusViewObjects.Capacity; i++)
			{
				var obj = UnityEngine.Object.Instantiate(userToolsView.RadiusIndicatorPrefab);
				obj.gameObject.SetActive(false);
				radiusViewObjects.Add(obj);
			}
			DoBeforeLeaving();
		}

		internal override void DoBeforeEntering()
		{
			if (CurrentStaticData == null)
				return;
			GridObjectFeatureStaticData prefab = null;
			if (CurrentStaticData.GetFeature<GridObjectFeatureStaticData>() != null)
			{
				prefab = CurrentStaticData.GetFeature<GridObjectFeatureStaticData>();
			}			
			else
			{
				return;
			}
			CurrentGridObjectData = new GridObjectData();
#if UNITY_EDITOR
			prefab.Init();
#endif
			CurrentGridObjectData.CopyFrom(prefab.GridObjectData);
			ShowPreviewObject(prefab.entity.ID, -1);

			CurrentObject.transform.rotation = DirectionHelper.GetRotation(CurrentGridObjectData.Direction);

			terrainLayerBeforePlacing = gameManager.TerrainManager.terrainView.LayerMode;
			ShowBuildingSpecificMapLayer();
            //placementInfo.Run(CurrentStaticData, CurrentGridObjectData);
		}

		internal override void DoBeforeLeaving()
		{
            placementInfo.Stop();
			userInfo.OnClose();

			HidePreviewObject();
			currentObjectID = "";
			CurrentObject = null;
			CurrentStaticData = null;
			for (int i = 0; i < rectViewObjects.Count; i++)
			{
				rectViewObjects[i].gameObject.SetActive(false);
			}
			for (int i = 0; i < entranceViewObjects.Count; i++)
			{
				entranceViewObjects[i].gameObject.SetActive(false);
			}
			for (int i = 0; i < radiusViewObjects.Count; i++)
			{
				radiusViewObjects[i].gameObject.SetActive(false);
			}

			gameManager.TerrainManager.terrainView.ShowLayerMap(terrainLayerBeforePlacing);
		}

		void ShowBuildingSpecificMapLayer()
		{
			var utility = CurrentStaticData.GetFeature<UtilityStaticData>();
			var mining = CurrentStaticData.GetFeature<MiningStaticData>();
			if (utility != null)
			{
				if (utility.type == EUtilityType.Toilet)
					gameManager.TerrainManager.terrainView.ShowLayerMap(TerrainView.ELayerView.GroundWater);
			}
			else if (mining != null)
			{
				if (mining.miningType == EMiningType.Groundwater)
					gameManager.TerrainManager.terrainView.ShowLayerMap(TerrainView.ELayerView.GroundWater);
			}
			else
			{
				gameManager.TerrainManager.terrainView.ShowLayerMap(TerrainView.ELayerView.Reserved);
			}
		}

		internal void UIPlaceStructure(string id)
		{
			currentObjectType = EObjectType.Structure;
			currentObjectID = id;
			CurrentStaticData = factory.EntityStaticData[id];
			if (CurrentStaticData == null) return;

			DoBeforeEntering();
		}

		internal void CycleNextView()
		{
			if (CurrentObject == null || !CurrentObject.gameObject.activeInHierarchy)
				return;
			if (CurrentStaticData == null)
				return;
			if (currentObjectType != EObjectType.Structure)
				return;
			HidePreviewObject();
			var structureData = CurrentStaticData.GetFeature<StructureFeatureStaticData>();
			ShowPreviewObject(structureData.entity.ID,
				structureData.GetNextViewID(currentViewID));
		}

		internal void UIPlaceResourcePile(string id)
		{
			currentObjectType = EObjectType.ResourcePile;
			currentObjectID = id;
			CurrentStaticData = factory.EntityStaticData[id];
			DoBeforeEntering();
		}

		internal override void Process()
		{
			stringBuilder.Clear();

			if (CurrentObject == null)
				return;
			if (inputManager.GetActionDown("CycleModel"))
			{
				CycleNextView();
			}

			//AdjustRotation
			HandleRotationInput();

			//Position on Mouse
			if (!inputManager.Pointer1.enabled)
			{
				return;
			}

			Vector3 pos = inputManager.Pointer1.TerrainPosition;

			bool CanPlace = true;

			GridObjectFeatureStaticData prefab = null;
			if (CurrentStaticData.GetFeature<GridObjectFeatureStaticData>() != null)
			{
				prefab = CurrentStaticData.GetFeature<GridObjectFeatureStaticData>();
			}			
			else
			{
				return;
			}
			if (prefab.GridIsFlexible)
				CanPlace = HandleFlexibleGrid(inputManager.Pointer1);
			else
				HandleFixedGridPosition(pos);

			HandleWells();
			HandlePollutionSources();
			HandleGraveyards();

			RectBounds checkRect = CurrentGridObjectData.Rect;
			RectBounds checkOuterRect = CurrentGridObjectData.Rect;
			RectBounds fullRect = new RectBounds(checkRect.Minimum, checkRect.Maximum);
			if (CurrentGridObjectData.EdgeIsWall)
				checkRect.Extend(-1);
			for (int i = 0; i < CurrentGridObjectData.EntrancePoints.Length; i++)
				fullRect.Merge(CurrentGridObjectData.EntrancePoints[i]);

			if (gridMap.IsReserved(checkRect) && !Input.GetKey(KeyCode.LeftShift))
				CanPlace = false;

			overlappingStructures.Clear();

			var objectsInRect = partitionSystem.GetStructuresInRect(fullRect, true);

			for (int i = 0; i < objectsInRect.Count; i++)
			{
				var obj = objectsInRect[i];
				var otherRect = obj.GetFeature<GridObjectFeature>().GridObjectData.Rect;
				if (checkRect.Intersects(otherRect))
				{
					if (obj.HasFeature<StructureFeature>())
					{
						CanPlace = false;
						break;
					}
					if (!overlappingStructures.Contains(obj))
						overlappingStructures.Add(obj);
					continue;
				}
				//Check active structure entrance points
				for (int e = 0; e < CurrentGridObjectData.EntrancePoints.Length; e++)
				{
					if (otherRect.Contains(CurrentGridObjectData.EntrancePoints[e]))
					{
						if (obj.HasFeature<StructureFeature>())
						{
							CanPlace = false;
							break;
						}
						if (!overlappingStructures.Contains(obj))
							overlappingStructures.Add(obj);
						break;
					}
				}

				//Check existing structure entrance points
				for (int e = 0; e < obj.GetFeature<GridObjectFeature>().GridObjectData.EntrancePoints.Length; e++)
				{
					if (checkOuterRect.Contains(obj.GetFeature<GridObjectFeature>().GridObjectData.EntrancePoints[e]))
					{
						if (obj.HasFeature<StructureFeature>())
						{
							CanPlace = false;
							break;
						}
						if (!overlappingStructures.Contains(obj))
							overlappingStructures.Add(obj);
						break;
					}
				}
				if (!CanPlace) break;
			}

			if (stringBuilder.Length > 0)
			{
				if (!userInfo.IsVisible)
					userInfo.OnOpen();

				userInfo.SetText(stringBuilder.ToString().TrimEnd('\n'));
				userInfo.SetPosition(inputManager.Pointer1.TerrainPosition, 2);
			}
			else if (userInfo.IsVisible)
				userInfo.OnClose();
			UpdateRectViewObjects(CanPlace);
			UpdateEntranceViewObjects();

			//Input
			if (CanPlace && inputManager.Pointer1.releasedDrag)
			{
				BaseEntity entity = null;
				if (currentObjectType == EObjectType.Structure)
				{
					bool placeConstruction = true;
#if DEV_MODE || UNITY_EDITOR
					if (Input.GetKey(KeyCode.LeftControl)) placeConstruction = false;
#endif
					CreateStructure(currentObjectID, SystemsManager.MainPlayerFaction, CurrentGridObjectData, overlappingStructures.Count > 0 || placeConstruction, out entity, null);

					Main.Instance.GameManager.SystemsManager.NotificationSystem.IncreaseInteger("totalStructuresPlaced");
					Main.Instance.GameManager.SystemsManager.NotificationSystem.IncreaseInteger($"totalPlaced_{currentObjectID}");

					OnBuildingPlaced?.Invoke(currentObjectID);
					var structureData = CurrentStaticData.GetFeature<StructureFeatureStaticData>();
					var id = structureData.GetRandomViewID();
					ShowPreviewObject(CurrentStaticData.ID, id);
				}
				else if (currentObjectType == EObjectType.ResourcePile)
				{
					CreateResourcePile(currentObjectID, CurrentGridObjectData, out entity, null);
				}
				if (entity != null)
				{
					for (int i = overlappingStructures.Count - 1; i >= 0; i--)
					{
						var structure = overlappingStructures[i];
						ResourcePileFeature pile = structure.GetFeature<ResourcePileFeature>();
						ResourcePileSystem.MarkPileGathering(pile, true, true);
						pile.canCancelGathering = false;
						pile.overlappingConstructionSite = entity;
						entity.GetFeature<ConstructionFeature>().BlockingResourcePiles.Add(pile);
						systemsManager.ResourcePileSystem.RegisterBlockingPile(pile);
					}
				}
			}
		}

		void HandlePollutionSources()
		{
			var utility = CurrentStaticData.GetFeature<UtilityStaticData>();
			if (utility != null && utility.type == EUtilityType.Toilet)
			{
				//Show radius?
			}
		}

		void HandleGraveyards()
		{
			var graveyard = CurrentStaticData.GetFeature<GraveyardStaticData>();
			if (graveyard != null)
			{
				var graves = GraveyardSystem.GetGraveyardSpots(CurrentGridObjectData);                
				stringBuilder.AppendLine($"{LocalizationManager.GetText("#UI/Game/UserTool/Graves")}: {graves}");
			}
		}

		void HandleWells()
		{
			var miningSystem = gameManager.SystemsManager.MiningSystem;

			var mining = CurrentStaticData.GetFeature<MiningStaticData>();
			if (mining != null && mining.miningType == EMiningType.Groundwater)
			{
				var rect = CurrentGridObjectData.Rect;
				var radiusObject = radiusViewObjects[0];
				var radiusScale = mining.radius * GridMapView.GridTileSize;
				radiusObject.transform.localScale = new Vector3(radiusScale, 1, radiusScale);
				radiusObject.transform.position = gridMap.View.LocalToWorld(rect.Center).To3D();
				radiusObject.gameObject.SetActive(true);

				var wellData = miningSystem.CalculateGain(mining, rect);

				var config = GameConfig.Instance.GeneralSystemsData;

				stringBuilder.AppendLine($"{LocalizationManager.GetText("#UI/Game/UserTool/Efficiency")}: {(int)(100 * wellData.efficientcy)}%");
				//stringBuilder.Append($"{LocalizationManager.GetText("#UI/Game/UserTool/Pollution")}: {(int)(wellData.pollution * 100)}%");
			}

			int radiusObjectID = 1;

			//Show radius of other Wells
			var features = miningSystem.FeaturesByFaction[SystemsManager.MainPlayerFaction];
			for (int i = 0; i < features.Count; i++)
			{
				var well = features[i];
				if (well.StaticData.miningType != EMiningType.Groundwater)
					continue;
				if (!well.Entity.HasFeature<GridObjectFeature>())
					continue;
				var wellRect = well.Entity.GetFeature<GridObjectFeature>().GridObjectData.Rect;
							   
				MeshRenderer radiusObject;
				if (radiusViewObjects.Count <= radiusObjectID)
				{
					radiusObject = UnityEngine.Object.Instantiate(userToolsView.RadiusIndicatorPrefab);
					radiusObject.gameObject.SetActive(false);
					radiusViewObjects.Add(radiusObject);
				}
				else
					radiusObject = radiusViewObjects[radiusObjectID++];

				//Apply scale
				var radiusScale = well.StaticData.radius * GridMapView.GridTileSize;
				radiusObject.transform.localScale = new Vector3(radiusScale, 1, radiusScale);
				radiusObject.transform.position = gridMap.View.LocalToWorld(wellRect.Center).To3D();
				radiusObject.gameObject.SetActive(true);

				//Disable other radius objects
				for (int j = radiusObjectID; j < radiusViewObjects.Count; j++)
				{
					radiusViewObjects[j].gameObject.SetActive(false);
				}
			}
		}

		protected void HandleFixedGridPosition(Vector3 pos)
		{
			GridObjectFeatureStaticData prefab = null;
			if (CurrentStaticData.GetFeature<GridObjectFeatureStaticData>() != null)
			{
				prefab = CurrentStaticData.GetFeature<GridObjectFeatureStaticData>();
			}			
			else
			{
				return;
			}

			int offsetX = prefab.GridOffsetX;
			int offsetY = prefab.GridOffsetY;
			//if (CurrentGridObjectData.EdgeIsWall)
			//{
			//	offsetX += 1;
			//	offsetY += 1;
			//}
#if USE_GRIDTILE
			//int x = (int)Mathf.Floor(pos.x * GridMapView.InvGridTileSize - offetX * 0.5f - CurrentGridObjectData.Rect.HalfWidth * GridMapView.GridTileFactor) * 2 + 1 + offetX;
			//int y = (int)Mathf.Floor(pos.z * GridMapView.InvGridTileSize - offetY * 0.5f - CurrentGridObjectData.Rect.HalfLength * GridMapView.GridTileFactor) * 2 + 1 + offetY;

			int x = (int)Mathf.Floor(pos.x * GridMapView.InvGridTileSize - CurrentGridObjectData.Rect.HalfWidth * GridMapView.GridTileFactor - offsetX * 0.5f) * 2 + 1 + offsetX;
			int y = (int)Mathf.Floor(pos.z * GridMapView.InvGridTileSize - CurrentGridObjectData.Rect.HalfLength * GridMapView.GridTileFactor - offsetY * 0.5f) * 2 + 1 + offsetY;
#else
			int x = (int)Mathf.Floor(pos.x * GridMapView.InvTileSize + 0.5f - CurrentGridObjectData.Rect.HalfWidth);
			int y = (int)Mathf.Floor(pos.z * GridMapView.InvTileSize + 0.5f -  CurrentGridObjectData.Rect.HalfLength);
#endif
			CurrentGridObjectData = OffsetGridObjectData(x, y, CurrentGridObjectData);

			var center = CurrentGridObjectData.Rect.Center * GridMapView.TileSize;
			pos.x = center.x;
			pos.z = center.y;

			CurrentObject.transform.position = pos;
		}

		protected bool HandleFlexibleGrid(GameInputManager.Pointer pointer)
		{
			GridObjectFeatureStaticData prefab = null;
			if (CurrentStaticData.GetFeature<GridObjectFeatureStaticData>() != null)
			{
				prefab = CurrentStaticData.GetFeature<GridObjectFeatureStaticData>();
			}			
			else
			{
				return false;
			}
			Vector2i pos1, pos2;
			pos1.X = (int)Mathf.Floor(pointer.TerrainPosition.x * GridMapView.InvGridTileSize - 0.5f) * 2 + 1;
			pos1.Y = (int)Mathf.Floor(pointer.TerrainPosition.z * GridMapView.InvGridTileSize - 0.5f) * 2 + 1;
			Vector2i pos = pos1;
			Vector2i size = Vector2i.One;

			if (pointer.isDragging || pointer.releasedDrag)
			{
				pos2.X = (int)Mathf.Floor(pointer.TerrainPositionBase.x * GridMapView.InvGridTileSize - 0.5f) * 2 + 1;
				pos2.Y = (int)Mathf.Floor(pointer.TerrainPositionBase.z * GridMapView.InvGridTileSize - 0.5f) * 2 + 1;

				pos = CivMath.Min(pos1, pos2);
				var diff = CivMath.Max(pos1, pos2) - pos;
				size = CivMath.Max(size, diff + Vector2i.One);
			}

			CurrentGridObjectData.Rect = new RectBounds(pos.X, pos.Y, size.X, size.Y);
			prefab.SizeX = size.X;
			prefab.SizeX = size.Y;

			var center = CurrentGridObjectData.Rect.Center * GridMapView.TileSize;
			CurrentObject.transform.position = center.To3D();
			var view = CurrentObject.GetComponent<FlexibleStructureView>();
			view.flexibleBase.localScale = new Vector3(size.X, 1, size.Y) * GridMapView.GridTileFactor;

			Vector2 gridSize;
			gridSize.x = (size.X + 1) * GridMapView.GridTileFactor;
			gridSize.y = (size.Y + 1) * GridMapView.GridTileFactor;

			bool isValid = pointer.isDragging || pointer.releasedDrag;
			if (isValid)
			{
				string message = null;
				stringBuilder.AppendLine($"{size.X}x{size.Y}");

				var area = gridSize.x * gridSize.y;
				if (gridSize.x < prefab.FlexibleSize.min || gridSize.y < prefab.FlexibleSize.min || area < prefab.FlexibleArea.min)
				{
					message = "Too small";
					isValid = false;
				}
				else if (gridSize.x > prefab.FlexibleSize.max || gridSize.y > prefab.FlexibleSize.max || area > prefab.FlexibleArea.max)
				{
					message = "Too big";
					isValid = false;
				}
				if (message != null)
				{					
					stringBuilder.AppendLine(message);
				}
			}

			return isValid;
		}

		protected void HandleRotationInput()
		{
			GridObjectFeatureStaticData prefab = null;
			if (CurrentStaticData.GetFeature<GridObjectFeatureStaticData>() != null)
			{
				prefab = CurrentStaticData.GetFeature<GridObjectFeatureStaticData>();
			}			
			else
			{
				return;
			}
			if (inputManager.GetActionDown("RotateBuildingCCW"))
			{
				CurrentGridObjectData.Direction = DirectionHelper.RotateCounterClockwise(CurrentGridObjectData.Direction);
				CurrentObject.transform.rotation = DirectionHelper.GetRotation(CurrentGridObjectData.Direction);
			}
			else if (inputManager.GetActionDown("RotateBuildingCW"))
			{
				CurrentGridObjectData.Direction = DirectionHelper.RotateClockwise(CurrentGridObjectData.Direction);
				CurrentObject.transform.rotation = DirectionHelper.GetRotation(CurrentGridObjectData.Direction);
			}
			CurrentGridObjectData = GetRotatedGridObjectData(CurrentGridObjectData.Direction, prefab, CurrentGridObjectData);
		}

		GridObjectData GetRotatedGridObjectData(EDirection dir, GridObjectFeatureStaticData data, GridObjectData gridObjectData)
		{
			gridObjectData.Direction = dir;
			gridObjectData.Rect.Set(0, 0, data.GridObjectData.Rect.Width, data.GridObjectData.Rect.Length);
			if ((int)dir % 2 == 1) gridObjectData.Rect.Swap();

			for (int i = 0; i < data.GridObjectData.EntrancePoints.Length; i++)
			{
				var point = gridObjectData.EntrancePoints[i];
				point = data.GridObjectData.EntrancePoints[i];
				int tmp;
				switch (dir)
				{
					case EDirection.East:
						tmp = point.X;
						point.X = point.Y;
						point.Y = data.GridObjectData.Rect.Width - 1 - tmp;
						break;
					case EDirection.South:
						point.X = data.GridObjectData.Rect.Width - 1 - point.X;
						point.Y = data.GridObjectData.Rect.Length - 1 - point.Y;
						break;
					case EDirection.West:
						tmp = point.X;
						point.X = data.GridObjectData.Rect.Length - 1 - point.Y;
						point.Y = tmp;
						break;
				}
				gridObjectData.EntrancePoints[i] = point;
			}
			return gridObjectData;
		}

		GridObjectData OffsetGridObjectData(int x, int y, GridObjectData gridObjectData, bool edgeOverlap = false)
		{
			gridObjectData.Rect.Translate(x, y);
			for (int i = 0; i < gridObjectData.EntrancePoints.Length; i++)
			{
				var point = gridObjectData.EntrancePoints[i];
				point.X += x;
				point.Y += y;
				gridObjectData.EntrancePoints[i] = point;
			}
			return gridObjectData;
		}

		private void GetNewObject() { }

		void UpdateRectViewObjects(bool canPlace)
		{
			int structures = overlappingStructures.Count + 1;
			int count = Mathf.Max(structures, rectViewObjects.Count);
			GridObjectFeatureStaticData prefab = null;
			if (CurrentStaticData.GetFeature<GridObjectFeatureStaticData>() != null)
			{
				prefab = CurrentStaticData.GetFeature<GridObjectFeatureStaticData>();
			}			
			else
			{
				return;
			}
			for (int i = 0; i < count; i++)
			{
				MeshRenderer rect;
				if (i >= rectViewObjects.Count)
					rectViewObjects.Add(UnityEngine.Object.Instantiate(userToolsView.RectIndicatorPrefab));
				rect = rectViewObjects[i];


				if (i == 0)
				{
					Color color = userToolsView.InvalidColor;
					if (canPlace)
					{
						if (overlappingStructures.Count == 0)
							color = userToolsView.ValidColor;
						else
							color = userToolsView.PartialColor;
					}

					var checkRect = CurrentGridObjectData.Rect;

					float padding = prefab.VisualPadding * GridMapView.GridTileSize;
					if (CurrentGridObjectData.EdgeIsWall)
						padding -= 0.25f;
#if USE_GRIDTILE
					rect.transform.position = gridMapView.GetPointWorldPosition(checkRect.Minimum).To3D() - new Vector3(padding, 0, padding);
#else
					rect.transform.position = GridMapView.GetPointWorldPosition(checkRect.Minimum).To3D() - new Vector3(padding, 0, padding);
#endif
					rect.transform.localScale = new Vector3(
						checkRect.Width * GridMapView.TileSize + padding * 2,
						1,
						checkRect.Length * GridMapView.TileSize + padding * 2);

					rect.material.SetColor(userToolsView.ColorName, color);
					rect.gameObject.SetActive(true);
				}
				else if (i < structures)
				{
					var structure = overlappingStructures[i - 1];
					var checkRect = structure.GetFeature<GridObjectFeature>().GridObjectData.Rect;
					float padding = 0;
					if (structure.HasFeature<GridObjectFeature>())
					{
						padding = structure.GetFeature<GridObjectFeature>().StaticData.VisualPadding * GridMapView.GridTileSize;
					}
#if USE_GRIDTILE
					rect.transform.position = gridMapView.GetPointWorldPosition(checkRect.Minimum).To3D() - new Vector3(padding, 0, padding);
#else
					rect.transform.position = GridMapView.GetPointWorldPosition(checkRect.Minimum).To3D() - new Vector3(padding, 0, padding);
#endif
					rect.transform.localScale = new Vector3(
						checkRect.Width * GridMapView.TileSize + padding * 2,
						1,
						checkRect.Length * GridMapView.TileSize + padding * 2);
					bool blocked = structure.HasFeature<StructureFeature>();
					rect.material.SetColor(userToolsView.ColorName, blocked ? userToolsView.InvalidColor : userToolsView.PartialColor);
					rect.gameObject.SetActive(true);
				}
				else
				{
					rect.gameObject.SetActive(false);
				}
			}
			for (int i = count; i < rectViewObjects.Count; i++)
			{
				rectViewObjects[i].gameObject.SetActive(false);
			}
		}

		void UpdateEntranceViewObjects()
		{
			int i = 0;
			for (i = 0; i < CurrentGridObjectData.EntrancePoints.Length; i++)
			{
				var point = CurrentGridObjectData.EntrancePoints[i];

				MeshRenderer obj;
				if (i >= entranceViewObjects.Count)
					entranceViewObjects.Add(UnityEngine.Object.Instantiate(userToolsView.EntranceIndicatorPrefab));
				obj = entranceViewObjects[i];

				obj.transform.position = gridMapView.GetTileWorldPosition(point).To3D();
				obj.gameObject.SetActive(true);
			}
			for (int j = i; j < entranceViewObjects.Count; j++)
			{
				entranceViewObjects[j].gameObject.SetActive(false);
			}
		}

		internal override void Stop()
		{
			DoBeforeLeaving();
		}

		void ShowPreviewObject(string modelID, int prefabID)
		{
			HidePreviewObject();
			var structure = factory.GetStaticData<StructureFeatureStaticData>(modelID);
			if (prefabID == -1)
				prefabID = structure.GetRandomViewID();
			if (ModelViewPool.ContainsKey(modelID) && ModelViewPool[modelID] != null && ModelViewPool[modelID].ContainsKey(prefabID))
			{
				CurrentObject = ModelViewPool[modelID][prefabID];
				currentViewID = prefabID;
			}
			else
			{
				if (currentObjectType == EObjectType.Structure)
				{
					CurrentObject = structure.GetFeatureViewInstance(prefabID).transform;
					currentViewID = prefabID;
				}

				else if (currentObjectType == EObjectType.ResourcePile)
				{
					CurrentObject = factory.GetStaticData<ResourcePileFeatureStaticData>(modelID).GetFeatureViewInstance().transform;					
				}
					
				CurrentObject.SetParent(PreviewContainer);
				if (!ModelViewPool.ContainsKey(modelID))
					ModelViewPool.Add(modelID, new Dictionary<int, Transform>());
				ModelViewPool[modelID].Add(prefabID, CurrentObject);
			}
			CurrentObject.transform.rotation = DirectionHelper.GetRotation(CurrentGridObjectData.Direction);
			CurrentObject.gameObject.SetActive(true);
		}

		void HidePreviewObject()
		{
			if (CurrentObject != null)
				CurrentObject.gameObject.SetActive(false);
		}

		void SetupTransformation(BaseEntity obj, GridObjectData data)
		{
			//Copy grid data reference
			var gridObjectFeature = obj.GetFeature<GridObjectFeature>();
			if (gridObjectFeature.GridObjectData == null) gridObjectFeature.GridObjectData = new GridObjectData();
			gridObjectFeature.GridObjectData.CopyFrom(data);
			obj.GetFeature<EntityFeature>().View.transform.rotation = DirectionHelper.GetRotation(data.Direction);

			var pos = gridObjectFeature.GridObjectData.Rect.Center.To3D() * GridMapView.TileSize;
			obj.GetFeature<EntityFeature>().View.transform.position = pos;
			obj.GetFeature<EntityFeature>().GridID = gridObjectFeature.GridObjectData.Rect.Minimum;
		}

		internal bool CreateStructure(string id, int faction, Vector2i position, EDirection direction, bool asConstruction, out BaseEntity entity, Guid? uid = null, bool updateImmediately = true, GridObjectData gridData = null)
		{
			entity = null;
			CurrentStaticData = factory.EntityStaticData[id];
			if (CurrentStaticData == null || CurrentStaticData.GetFeature<GridObjectFeatureStaticData>() == null) return false;
			var currentGridObjectData = gridData;
			if (currentGridObjectData == null)
			{
				currentGridObjectData = new GridObjectData();
				var feature = CurrentStaticData.GetFeature<StructureFeatureStaticData>();
				var prefab = CurrentStaticData.GetFeature<GridObjectFeatureStaticData>();
				currentGridObjectData.CopyFrom(prefab.GridObjectData);
				currentGridObjectData = GetRotatedGridObjectData(direction, prefab, currentGridObjectData);
				currentGridObjectData = OffsetGridObjectData(position.X, position.Y, currentGridObjectData);
			}
			return CreateStructure(id, faction, currentGridObjectData, asConstruction, out entity, uid, updateImmediately);
		}

		internal bool CreateStructure(string id, int faction, GridObjectData gridObjectData, bool asConstruction, out BaseEntity entity, Guid? uid, bool updateImmediately = true)
		{
			entity = null;
			if (!gridMap.Grid.IsInRange(gridObjectData.Rect)) return false;
			var factoryParams = new FactoryParams();
			factoryParams.SetParams
				(
					new EntityFeatureParams()
					{
						FactionID = faction
					},
					new GridObjectFeatureParams()
					{
						GridObjectData = gridObjectData
					},
					new ConstructionFeatureParams()
					{
						AsConstruction = asConstruction
					},
					new StructureFeatureParams()
					{
						CurrentViewID = currentViewID
					}
				);
			entity = factory.CreateInstance(id, uid.ToString(), factoryParams);

			SetupTransformation(entity, gridObjectData);
			Main.Instance.GameManager.SystemsManager.PartitionSystem.RegisterStructure(entity, updateImmediately);

			return true;
		}

		internal bool CreateResourcePile(string id, Vector2i position, EDirection direction, out BaseEntity entity, Guid? uid, bool updateImmediately = true)
		{
			var currentStaticData = factory.EntityStaticData[id];
			var currentGridObjectData = new GridObjectData();
			currentGridObjectData.CopyFrom(currentStaticData.GetFeature<GridObjectFeatureStaticData>().GridObjectData);
			currentGridObjectData.Rect.Translate(position.X, position.Y);
			currentGridObjectData = GetRotatedGridObjectData(direction, currentStaticData.GetFeature<GridObjectFeatureStaticData>(), currentGridObjectData);
			currentGridObjectData = OffsetGridObjectData(position.X, position.Y, currentGridObjectData);

			return CreateResourcePile(id, currentGridObjectData, out entity, uid, updateImmediately);
		}

		internal bool CreateResourcePile(string id, GridObjectData data, out BaseEntity entity, Guid? uid, bool updateImmediately = true)
		{
			entity = null;
			if (!gridMap.Grid.IsInRange(data.Rect)) return false;
			var factoryParams = new FactoryParams();
			factoryParams.SetParams
				(
					new EntityFeatureParams()
					{
						FactionID = SystemsManager.NeutralNpcFaction
					},
					new GridObjectFeatureParams()
					{
						GridObjectData = data
					}					
				);
			entity = factory.CreateInstance(id, uid.ToString(), factoryParams);
			SetupTransformation(entity, data);
			Main.Instance.GameManager.SystemsManager.PartitionSystem.RegisterStructure(entity, updateImmediately);
			return true;
		}		
	}
}