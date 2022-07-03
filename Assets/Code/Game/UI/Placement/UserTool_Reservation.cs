using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Endciv
{
	public class UserTool_Reservation : UserTool
	{
		private GameManager gameManager;
		private UserToolSystem placementSystem;
		private UserToolsView userToolsView;
		private GameInputManager inputManager;

		private GridMap gridMap;
		private GridMapView gridMapView;

		MeshRenderer rectViewObject;

		Vector2 basePosition;
		Vector2i basePositionRect;
		TerrainView.ELayerView terrainLayerBeforePlacing;

		private List<BaseEntity> selectedEntities = new List<BaseEntity>();

		public UserTool_Reservation(UserToolSystem placementSystem, UserToolsView userToolsView, GameManager gameManager)
		{
			this.gameManager = gameManager;
			this.placementSystem = placementSystem;
			this.userToolsView = userToolsView;
			inputManager = gameManager.gameInputManager;
			gridMap = gameManager.GridMap;
			gridMapView = gridMap.View;

			var obj = UnityEngine.Object.Instantiate(userToolsView.RectIndicatorPrefab);
			obj.gameObject.SetActive(false);
			rectViewObject = obj;

			DoBeforeLeaving();
		}

		internal override void DoBeforeEntering()
		{
			selectedEntities.Clear();
			terrainLayerBeforePlacing = gameManager.TerrainManager.terrainView.LayerMode;
			gameManager.TerrainManager.terrainView.ShowLayerMap(TerrainView.ELayerView.Reserved);
			rectViewObject.gameObject.SetActive(true);
		}

		internal override void DoBeforeLeaving()
		{
			selectedEntities.Clear();
			gameManager.TerrainManager.terrainView.ShowLayerMap(terrainLayerBeforePlacing);
			rectViewObject.gameObject.SetActive(false);
			MouseCursorManager.Instance.SetCurrentCursor(MouseCursorManager.ECursorType.Default, MouseCursorManager.ECursorState.Normal);
		}

		public void Reservation()
		{
			MouseCursorManager.Instance.SetCurrentCursor(MouseCursorManager.ECursorType.Reservation, MouseCursorManager.ECursorState.Normal);

			DoBeforeEntering();
		}

		internal override void Process()
		{
			//Position on Mouse
			if (!inputManager.Pointer1.enabled)
			{
				return;
			}
			Vector2 pointerPos = inputManager.Pointer1.TerrainPosition.To2D();
			Vector2 pointerPos2 = inputManager.Pointer1.TerrainPositionBase.To2D();
			Vector2 pos;
			pos.x = (int)Mathf.Floor((pointerPos.x) * GridMapView.InvGridTileSize) + GridMapView.HalfTileSize;
			pos.y = (int)Mathf.Floor((pointerPos.y) * GridMapView.InvGridTileSize) + GridMapView.HalfTileSize;

			if (!inputManager.Pointer1.isDragging)
				basePosition = pos;

			//Rect
			int rectX = (int)Mathf.Floor((pointerPos.x) * GridMapView.InvTileSize * 0.5f + 0.5f) * 2 - 1;
			int rectY = (int)Mathf.Floor((pointerPos.y) * GridMapView.InvTileSize * 0.5f + 0.5f) * 2 - 1;
			int rectX2 = (int)Mathf.Floor((pointerPos2.x) * GridMapView.InvTileSize * 0.5f + 0.5f) * 2 - 1;
			int rectY2 = (int)Mathf.Floor((pointerPos2.y) * GridMapView.InvTileSize * 0.5f + 0.5f) * 2 - 1;

			RectBounds rect = new RectBounds(new Vector2i(rectX, rectY), new Vector2i(rectX2, rectY2));

			var min = CivMath.Min(basePosition, pos);
			var max = CivMath.Max(basePosition, pos);

#if USE_GRIDTILE
			rectViewObject.transform.position = min.To3D();
			var diff = Vector2.one + max - min;
			rectViewObject.transform.localScale = diff.To3D(1);

			//test
			rectViewObject.transform.position = gridMapView.GetPointWorldPosition(rect.Minimum).To3D();
			rectViewObject.transform.localScale = gridMapView.GetPointWorldPosition(rect.Size).To3D(1);
#else
			Not implemented
			rectViewObject.transform.position = GridMapView.GetPointWorldPosition(checkRect.Minimum).To3D() - new Vector3(padding, 0, padding);
#endif
			var alternate = Input.GetKey(KeyCode.LeftShift);

			if (inputManager.Pointer1.releasedDrag)
			{
				gridMap.SetReservation(rect, !alternate);
			}
		}

		internal override void Stop()
		{
			DoBeforeLeaving();
		}

	}
}