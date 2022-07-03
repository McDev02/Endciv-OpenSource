using System;
using UnityEngine;
namespace Endciv
{
	[RequireComponent(typeof(CanvasGroup))]
	[DisallowMultipleComponent]
	public abstract class GUICanvasGroup : MonoBehaviour
	{
		public bool isAnimationRunning { get; protected set; }
		protected bool isClosing;
		public bool IsActive { get; private set; }
		[SerializeField]
		protected bool isVisible = true;
		public bool IsVisible
		{
			get { return isVisible; }
			set
			{
				if (isVisible != value)
				{
					isVisible = value;
					PlayAnimation(value, false);
				}
				IsActive = !isClosing;
			}
		}

		public Action OnWindowOpened;
		public Action OnWindowClosed;

		protected virtual void Awake()
		{
			isClosing = !isVisible;
			IsVisible = isVisible;
			PlayAnimation(isVisible, true);
		}
		void OnEnable()
		{
			PlayAnimation(isVisible, true);
		}

		public void OpenClose(bool v)
		{
			if (v) OnOpen();
			else OnClose();
		}

		public void OnToggleActive()
		{
			if (isVisible) OnClose();
			else OnOpen();
		}
		public void OnToggleVissibility()
		{
			if (isVisible) OnHide();
			else OnShow();
		}
		public virtual void OnShow()
		{
			isClosing = false;
			gameObject.SetActive(true);
			IsVisible = true;
			if (OnWindowOpened != null)
				OnWindowOpened.Invoke();
		}

		public virtual void OnHide()
		{
			isClosing = false;
			IsVisible = false;
		}

		public virtual void OnOpen()
		{
			OnShow();
		}

		public virtual void OnClose()
		{
			isClosing = true;
			IsVisible = false;
		}

		public virtual void OnCloseNow()
		{
			PlayAnimation(false, true);
			IsVisible = false;
			isClosing = false;
			OnWindowClosed?.Invoke();
		}

		public virtual void OnOpenNow()
		{
			PlayAnimation(true, true);
			IsVisible = true;
			isClosing = false;
			OnWindowOpened?.Invoke();
		}

		protected abstract void PlayAnimation(bool visible, bool skipAnim = false);

		void OnDestroy()
		{
			OnWindowOpened = null;
			OnWindowClosed = null;
		}
	}
}