using System;
using UnityEngine;

namespace Endciv
{
	public abstract class TooltipBase : MonoBehaviour
	{
		[NonSerialized] public TooltipBase m_MyPrefab;
		[NonSerialized] public RectTransform rectTransform;
		Vector2 safePadding = new Vector2(20, 20);

		GameInputManager inputManager;
		bool isRunning;

		private void Awake()
		{
			rectTransform = (RectTransform)transform;
		}

		public virtual void Setup(object context)
		{
			inputManager = Main.Instance.GameManager.gameInputManager;
			UpdatePosition();
			UpdateData();
			Show();
			isRunning = true;
		}

		public void Show()
		{
			gameObject.SetActive(true);
		}

		public void Hide()
		{
			gameObject.SetActive(false);
		}

		protected virtual void UpdateData()
		{
		}

		private void UpdatePosition()
		{
			// UI needs to be pixel perfect
			// mouse position is bottom left

			var mouseOffset = new Vector2(40, 0);

			Vector2 screen = inputManager.scaledScreenBounds;
			Vector2 pos = (Vector2)Input.mousePosition * inputManager.UIScaleInv + mouseOffset;
			Vector2 size = rectTransform.sizeDelta;
			Vector2 sizeBounds = rectTransform.sizeDelta + safePadding;

			if (pos.x + sizeBounds.x >= screen.x)
			{
				pos.x -= sizeBounds.x + mouseOffset.x + 5;
			}
			else if (pos.x - safePadding.x < 0)
			{
				pos.x = mouseOffset.x;
			}

			if (pos.y - sizeBounds.y <= 0)
			{
				pos.y += sizeBounds.y;
			}
			else if (pos.y + sizeBounds.y > screen.y)
			{
				pos.y -= mouseOffset.y + safePadding.y + 5;
			}

			pos.x = CivMath.GetClosestMultipleOf(pos.x, 2);
			pos.y = CivMath.GetClosestMultipleOf(pos.y, 2);

			rectTransform.anchoredPosition = pos;
		}

		private void LateUpdate()
		{
			if (!isRunning) return;
			UpdatePosition();
			UpdateData();
		}
	}
}