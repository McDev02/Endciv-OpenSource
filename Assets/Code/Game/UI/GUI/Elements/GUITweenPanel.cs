using UnityEngine;
using System.Collections;

namespace Endciv
{
	/// <summary>
	/// 
	/// </summary>
	[RequireComponent(typeof(RectTransform))]
	[RequireComponent(typeof(CanvasGroup))]
	[DisallowMultipleComponent]
	public class GUITweenPanel : GUICanvasGroup
	{
		RectTransform rect;
		[SerializeField] float animationSpeed = 10;
		[SerializeField] Vector2 closedPosition;
		[SerializeField] Vector2 openedPosition;
		float t;

		protected override void Awake()
		{
			//Put initialization logic here
			rect = (RectTransform)transform;

			base.Awake();
		}


		protected override void PlayAnimation(bool visible, bool skipAnim = false)
		{
			if (skipAnim)
			{
				t = visible ? 1 : 0;
			}
			if (!isAnimationRunning)
				StartCoroutine(TweenRoutine());
		}

		IEnumerator TweenRoutine()
		{
			bool kill = false;
			isAnimationRunning = true;
			while (!kill)
			{
				if (IsVisible)
				{
					t += animationSpeed * Time.unscaledDeltaTime;
					if (t >= 1) kill = true;
				}
				else
				{
					t -= animationSpeed * Time.unscaledDeltaTime;
					if (t <= 0) kill = true;
				}
				var pos = Vector2.Lerp(closedPosition, openedPosition, t);

				////Snap to 2 pixels due to UI scale
				//pos.x = (int)(pos.x * 0.5f + 0.5f) * 2;
				//pos.y = (int)(pos.y * 0.5f + 0.5f) * 2;

				rect.anchoredPosition = pos;
				if (kill)
					break;
				yield return true;
			}
			isAnimationRunning = false;
		}

		public void OnAnimationOutEnd()
		{
			if (isClosing)
			{
				if (OnWindowClosed != null)
					OnWindowClosed();
				gameObject.SetActive(false);
			}
			isClosing = false;
		}
		public void OnAnimationInEnd() { }
	}
}