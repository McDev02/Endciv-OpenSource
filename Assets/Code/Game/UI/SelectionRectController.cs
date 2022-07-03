using UnityEngine;
using UnityEngine.UI;
using System;

namespace Endciv
{
	public class SelectionRectController : MonoBehaviour
	{
		[SerializeField] Image selectionRectImage;
		RectTransform selectionRectObject;
		public enum ESelectionRectMode { Normal, Deletion }
		[SerializeField] Color normalColor;
		[SerializeField] Color deletionColor;
		[NonSerialized] public GameInputManager gameInputManager;

		private void Awake()
		{
			selectionRectObject = selectionRectImage.rectTransform;
		}

		public void Show()
		{
			selectionRectObject.gameObject.SetActive(true);
		}
		public void Hide()
		{
			selectionRectObject.gameObject.SetActive(false);
		}

		public Rect UpdateSelectionRect(Vector3 from, Vector3 to, ESelectionRectMode mode)
		{
			selectionRectObject.gameObject.SetActive(true);
			// Vector2 delta = new Vector2((to.x - from.x) / selectionRectObject.localScale.x, (from.y - to.y) / selectionRectObject.localScale.y);
			Vector2 delta = new Vector2((to.x - from.x), (from.y - to.y));
			Vector2 screenPos = Vector2.zero;
			screenPos.x = delta.x >= 0f ? from.x : to.x;
			screenPos.y = delta.y >= 0f ? from.y : to.y;
			delta.x = Mathf.Abs(delta.x);
			delta.y = Mathf.Abs(delta.y);

			switch (mode)
			{
				case ESelectionRectMode.Normal:
					selectionRectImage.color = normalColor;
					break;
				case ESelectionRectMode.Deletion:
					selectionRectImage.color = deletionColor;
					break;
			}

			selectionRectObject.position = screenPos;

			selectionRectObject.sizeDelta = new Vector2(delta.x * gameInputManager.UIScaleInv, delta.y * gameInputManager.UIScaleInv);
			screenPos.y -= delta.y;
			return new Rect(screenPos, delta);
		}
	}
}