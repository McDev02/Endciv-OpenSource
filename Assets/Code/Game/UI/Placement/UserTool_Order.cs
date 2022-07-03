using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Endciv
{
	public class UserTool_Order : UserTool
	{
		public enum EOrderType { Collection, Demolition }
		public EOrderType currentOrder;

		private UserToolSystem placementSystem;
		private SelectionRectController selectionRectObject;
		private GameInputManager gameInputManager;
		private Rect selectionRect;

		private List<BaseEntity> selectedEntities = new List<BaseEntity>();

		public UserTool_Order(UserToolSystem placementSystem, GameGUIController gameGUIController, GameInputManager gameInputManager)
		{
			this.placementSystem = placementSystem;
			selectionRectObject = gameGUIController.selectionRect;
			this.gameInputManager = gameInputManager;
			selectionRectObject.Hide();

			DoBeforeLeaving();
		}

		internal override void DoBeforeEntering()
		{
			selectedEntities.Clear();
		}

		internal override void DoBeforeLeaving()
		{
			selectedEntities.Clear();
			selectionRectObject.Hide();
			MouseCursorManager.Instance.SetCurrentCursor(MouseCursorManager.ECursorType.Default, MouseCursorManager.ECursorState.Normal);
		}

		public void Order(EOrderType type)
		{
			currentOrder = type;
			DoBeforeEntering();
		}

		internal override void Process()
		{
			switch (currentOrder)
			{
				case EOrderType.Collection:
					HandleGathering();
					break;
				case EOrderType.Demolition:
					HandleDeconstruction();
					break;
				default:
					break;
			}
		}

		void HandleDeconstruction()
		{
			selectionRectObject.Hide();
			if (gameInputManager.Pointer1 != null)
			{
				if (gameInputManager.Pointer1.isActive)
					MouseCursorManager.Instance.SetCurrentCursor(MouseCursorManager.ECursorType.Demolish, MouseCursorManager.ECursorState.Active);
				else
					MouseCursorManager.Instance.SetCurrentCursor(MouseCursorManager.ECursorType.Demolish, MouseCursorManager.ECursorState.Normal);

				bool alternate = Input.GetKey(KeyCode.LeftShift);

				if (gameInputManager.Pointer1.isDragging)
				{
					selectionRect = selectionRectObject.UpdateSelectionRect(gameInputManager.Pointer1.baseMousePos, gameInputManager.Pointer1.mousePos, alternate ? SelectionRectController.ESelectionRectMode.Normal : SelectionRectController.ESelectionRectMode.Deletion);
					MouseCursorManager.Instance.SetCurrentCursor(MouseCursorManager.ECursorType.Demolish, MouseCursorManager.ECursorState.Active);
				}
				else if (gameInputManager.Pointer1.releasedDrag)
				{
					selectionRect = selectionRectObject.UpdateSelectionRect(gameInputManager.Pointer1.baseMousePos, gameInputManager.Pointer1.mousePos, alternate ? SelectionRectController.ESelectionRectMode.Normal : SelectionRectController.ESelectionRectMode.Deletion);
					UpdateSelectionList();
					var sites = GetSelectedFeature<ConstructionFeature>();
					if (sites != null || sites.Count > 0)
					{
						for (int i = sites.Count - 1; i >= 0; i--)
						{
							var site = sites[i];

							if (alternate)
								ConstructionSystem.MarkForDemolition(site, false);
							else if (site.CurrentConstructionPoints <= 0)
								ConstructionSystem.DemolishConstructionSite(site);
							else
								ConstructionSystem.MarkForDemolition(site, true);


						}
					}
				}
			}
		}

		void HandleGathering()
		{
			selectionRectObject.Hide();
			if (gameInputManager.Pointer1 != null)
			{
				if (gameInputManager.Pointer1.isActive)
					MouseCursorManager.Instance.SetCurrentCursor(MouseCursorManager.ECursorType.Collect, MouseCursorManager.ECursorState.Active);
				else
					MouseCursorManager.Instance.SetCurrentCursor(MouseCursorManager.ECursorType.Collect, MouseCursorManager.ECursorState.Normal);

				bool alternate = Input.GetKey(KeyCode.LeftShift);

				if (gameInputManager.Pointer1.isDragging)
				{
					selectionRect = selectionRectObject.UpdateSelectionRect(gameInputManager.Pointer1.baseMousePos, gameInputManager.Pointer1.mousePos, alternate ? SelectionRectController.ESelectionRectMode.Deletion : SelectionRectController.ESelectionRectMode.Normal);
				}
				else if (gameInputManager.Pointer1.releasedDrag)
				{
					selectionRect = selectionRectObject.UpdateSelectionRect(gameInputManager.Pointer1.baseMousePos, gameInputManager.Pointer1.mousePos, alternate ? SelectionRectController.ESelectionRectMode.Deletion : SelectionRectController.ESelectionRectMode.Normal);
					UpdateSelectionList();
					List<ResourcePileFeature> piles = GetSelectedFeature<ResourcePileFeature>();
					if (piles != null || piles.Count > 0)
					{
						for (int i = 0; i < piles.Count; i++)
						{
							//Skip storage piles
							if (piles[i].ResourcePileType == ResourcePileSystem.EResourcePileType.StoragePile)
								continue;
							if (!alternate)
								ResourcePileSystem.MarkPileGathering(piles[i], true, true);
							else
							{
								if (piles[i].markedForCollection && piles[i].canCancelGathering)
									ResourcePileSystem.MarkPileGathering(piles[i], false, true);
							}
						}

					}
				}
			}
		}
		internal override void Stop()
		{
			DoBeforeLeaving();
		}


		void ProcessSelection()
		{
			selectionRectObject.Hide();
			if (gameInputManager.Pointer1 != null)
			{
				if (gameInputManager.Pointer1.isDragging)
				{
					selectionRect = selectionRectObject.UpdateSelectionRect(gameInputManager.Pointer1.baseMousePos, gameInputManager.Pointer1.mousePos, SelectionRectController.ESelectionRectMode.Normal);
					selectionRectObject.Show();
				}
				else if (gameInputManager.Pointer1.releasedDrag)
				{
					//OnSelectionEnd
				}
			}
		}


		private void UpdateSelectionList()
		{
			//TODO: Optimize and only seek grid and agent entities if possible
			var entities = Main.Instance.GameManager.SystemsManager.Entities.Values.ToArray();
			if (entities == null)
				return;
			selectedEntities.RemoveAll(x => x == null);
			foreach (var entity in entities)
			{
				if (!entity.HasFeature<EntityFeature>())
					continue;
				var view = entity.GetFeature<EntityFeature>().View;
				if (view == null)
					continue;
				var screenPos = gameInputManager.MainCamera.WorldToScreenPoint(view.transform.position);

				if (selectionRect.Contains(screenPos))
				{
					if (!selectedEntities.Contains(entity))
					{
						//Newly selected
						selectedEntities.Add(entity);
					}
				}
				else
				{
					if (selectedEntities.Contains(entity))
					{
						//RemoveSelection
						selectedEntities.Remove(entity);
					}
				}
			}

			//OnSelectionChanged 
		}

		public List<BaseEntity> GetSelected()
		{
			return selectedEntities.ToList();
		}

		public List<T> GetSelectedFeature<T>() where T : FeatureBase
		{
			T feature;
			List<T> features = new List<T>();
			for (int i = 0; i < selectedEntities.Count; i++)
			{
				if (selectedEntities[i].HasFeature<T>())
				{
					feature = selectedEntities[i].GetFeature<T>();
					features.Add(feature);
				}
			}
			return features;
		}
	}
}