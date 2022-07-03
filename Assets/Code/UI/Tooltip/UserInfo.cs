using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	public class UserInfo : GUIAnimatedPanel
	{
		[SerializeField] private Vector2 padding;
		[SerializeField] private int maxWidth = 300;
		[SerializeField] private int minHeight = 64;
		[SerializeField] Text label;

		RectTransform rectTransform;
		public CameraController cameraController;
		public GameInputManager inputManager;

		protected override void Awake()
		{
			base.Awake();
			rectTransform = (RectTransform)transform;
		}

		public void Run(CameraController cameraController, GameInputManager inputManager)
		{
			this.cameraController = cameraController;
			this.inputManager = inputManager;
		}

		public void SetPosition(Vector3 worldPos, float yOffset)
		{
			worldPos.y += yOffset;
			var uiPos = cameraController.Camera.WorldToScreenPoint(worldPos);
            rectTransform.anchoredPosition = uiPos * inputManager.UIScaleInv;
		}

		public void SetText(string text)
		{
			if (label.text == text)
				return;
			label.text = text;

			var settings = label.GetGenerationSettings(new Vector2(maxWidth, 0));
			settings.horizontalOverflow = HorizontalWrapMode.Wrap;
			settings.verticalOverflow = VerticalWrapMode.Overflow;
			settings.updateBounds = true;

			label.cachedTextGenerator.Populate(text, settings);

			var size = label.cachedTextGenerator.rectExtents.size;

			var pWidth = label.preferredWidth;

			if (size.x > pWidth)
			{
				size.x = pWidth;
			}

			size = size - label.rectTransform.sizeDelta + 2 * padding;
			size.y = Mathf.Max(size.y, minHeight);
			//Make sure size is a multiple of 4
			size.x = CivMath.GetNextMultipleOf(size.x, 4);
			size.y = CivMath.GetNextMultipleOf(size.y, 4);
			rectTransform.sizeDelta = size;
		}

	}
}