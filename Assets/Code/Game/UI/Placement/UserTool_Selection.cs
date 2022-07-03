using UnityEngine;

namespace Endciv
{
	public class UserTool_Selection : UserTool
	{
		private GameInputManager gameInputManager;

		public BaseEntity SelectedEntity { get; private set; }
		private bool isClicking = false;

		public UserTool_Selection(GameInputManager gameInputManager)
		{
			this.gameInputManager = gameInputManager;
			DoBeforeLeaving();
		}

		internal override void DoBeforeEntering()
		{
			ResetSelection();
		}

		internal override void DoBeforeLeaving()
		{
			ResetSelection();
			MouseCursorManager.Instance.SetCurrentCursor(MouseCursorManager.ECursorType.Default, MouseCursorManager.ECursorState.Normal);
		}

		internal override void Process()
		{
			HandleSelection();
		}

		public void SelectEntity(BaseEntity entity)
		{
			ResetSelection();
			Main.Instance.GameManager.GameGUIController.OnShowSelectedEntityInfo(entity);
			SelectedEntity = entity;
			SelectedEntity.SelectView();
		}

		void HandleSelection()
		{
			if (gameInputManager.Pointer1 != null)
			{
				BaseEntity entity = null;
				var ray = Main.Instance.GameManager.CameraController.Camera.ScreenPointToRay(gameInputManager.Pointer1.mousePos);
				RaycastHit hit;
				if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Entities")))
				{
					var view = hit.transform.root.GetComponent<EntityFeatureView>();
#if UNITY_EDITOR
					if (view == null)
						view = hit.transform.GetComponentInParent<EntityFeatureView>();
#endif
					if (view != null)
						entity = view.Feature.Entity;
				}
				if (entity == null || gameInputManager.IsMouseOverUI())
				{
					MouseCursorManager.Instance.SetCurrentCursor(MouseCursorManager.ECursorType.Default, MouseCursorManager.ECursorState.Normal);
				}
				else if (gameInputManager.Pointer1.isDragging)
				{
					MouseCursorManager.Instance.SetCurrentCursor(MouseCursorManager.ECursorType.Select, MouseCursorManager.ECursorState.Active);
				}
				else
				{
					MouseCursorManager.Instance.SetCurrentCursor(MouseCursorManager.ECursorType.Select, MouseCursorManager.ECursorState.Normal);
				}
				if (gameInputManager.Pointer1.releasedDrag)
				{
					if (entity != null && (SelectedEntity == null || SelectedEntity != entity))
					{
						SelectEntity(entity);
					}
				}
				if (gameInputManager.Pointer1 != null && gameInputManager.Pointer1.releasedDrag)
				{
					if (SelectedEntity != null && entity == null)
					{
						ResetSelection();
					}
				}
			}
			if (gameInputManager.Pointer2 != null && gameInputManager.Pointer2.releasedDrag)
			{
				if (SelectedEntity != null)
				{
					ResetSelection();
				}
			}
		}

		internal override void Stop()
		{
			ResetSelection();
		}

		public void ResetSelection()
		{
			if (SelectedEntity != null)
			{
				Main.Instance.GameManager.GameGUIController.OnCloseEntityInfo();
				SelectedEntity.DeselectView();
			}
			SelectedEntity = null;
		}
	}

}
