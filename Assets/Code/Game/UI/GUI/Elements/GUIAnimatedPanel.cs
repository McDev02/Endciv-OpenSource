using UnityEngine;
namespace Endciv
{
	/// <summary>
	/// Base Panel which can be toggled on and off and animates fadeout
	/// </summary>
	[RequireComponent(typeof(Animator))]
	[RequireComponent(typeof(CanvasGroup))]
	[DisallowMultipleComponent]
	public class GUIAnimatedPanel : GUICanvasGroup
	{
		string showStateName;
		string hideStateName;

		Animator animator;

		protected override void Awake()
		{
			//Put initialization logic here
			animator = GetComponent<Animator>();
			showStateName = "PanelOnShow_Normal";
			hideStateName = "PanelOnHide_Normal";

			base.Awake();
		}
		
		protected override void PlayAnimation(bool visible, bool skipAnim = false)
		{
			if (animator != null && gameObject.activeInHierarchy)
			{
				animator.CrossFade(visible ? showStateName : hideStateName, 0.3f, 0, skipAnim ? 1f : 0f);
			}
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