using UnityEngine;

namespace Endciv
{
	[RequireComponent(typeof(RectTransform))]
	public class DockableElement : MonoBehaviour
	{
		DragDockController controller;
		public bool surviveSceneLoad;
		public RectTransform rect;
		public Vector2 Min { get { return rect.anchoredPosition; } }
		public Vector2 Max { get { return rect.anchoredPosition + rect.sizeDelta; } }

		private void Start()
		{
			if (controller == null)
			{
				controller = FindObjectOfType<DragDockController>();
			}
			Activate();

			rect = (RectTransform)transform;

			var panel = GetComponent<GUICanvasGroup>();
			if (panel != null)
			{
				panel.OnWindowOpened -= Activate;
				panel.OnWindowOpened += Activate;

				panel.OnWindowClosed -= Disable;
				panel.OnWindowClosed += Disable;
			}
		}

		private void OnDestroy()
		{
			Disable();
		}

		void Activate()
		{
			controller.RegisterDockableElement(this);
		}
		void Disable()
		{
			controller.DeregisterDockableElement(this);
		}
	}
}