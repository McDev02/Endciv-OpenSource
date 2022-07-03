using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Endciv
{
	public class DraggableElement : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler
	{
		[SerializeField] DragDockController controller;
		public DockableElement dockableElement;
		public RectTransform rectBounds;
		public RectTransform parentObject;
		[NonSerialized] public Vector2 dragRelativePosition;
		[NonSerialized] public PointerEventData lastEventData;

		public bool enableDocking = true;
		public bool limitToScreenBounds = true;

		public void OnPointerDown(PointerEventData e)
		{
			lastEventData = e;
			if (controller != null)
				controller.BeginDrag(this, e);
		}

		public void OnPointerUp(PointerEventData e)
		{
			lastEventData = e;
			if (controller != null)
				controller.StopDrag(this);
			lastEventData = null;
		}

		private void Awake()
		{
			lastEventData = null;
			if (rectBounds == null) rectBounds = (RectTransform)transform;
			if (parentObject == null) parentObject = rectBounds;

			if (controller == null)
				controller = FindObjectOfType<DragDockController>();
			parentObject.SetParent(controller.transform, true);

			var guiAnimated = parentObject.GetComponent<GUICanvasGroup>();
			if (guiAnimated != null)
			{
				guiAnimated.OnWindowOpened -= UpdatePosition;
				guiAnimated.OnWindowOpened += UpdatePosition;
			}
		}

		void UpdatePosition()
		{
			controller.OnUpdatePosition(this, true);
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			OnDrag(eventData);
		}
		public void OnDrag(PointerEventData e)
		{
			lastEventData = e;
			//OnUpdatePosition(enableDocking);
		}

	}
}