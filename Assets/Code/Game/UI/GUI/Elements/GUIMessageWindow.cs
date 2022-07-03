using UnityEngine;
using UnityEngine.UI;
namespace Endciv
{
	/// <summary>
	/// Window which can Show two buttons
	/// </summary>
	[RequireComponent(typeof(Animator))]
	[RequireComponent(typeof(CanvasGroup))]
	[DisallowMultipleComponent]
	public class GUIMessageWindow : GUIAnimatedPanel
	{
		[SerializeField] Text Message;
		[SerializeField] Button ConfirmButton;
		[SerializeField] Button DenyButton;
		[SerializeField] RectTransform progressIndicator;

		public ListenerCallInteger OnButtonPressed;

		protected override void Awake()
		{
			base.Awake();
			ConfirmButton.onClick.AddListener(() => OnButtonPressedCaller(0));
			DenyButton.onClick.AddListener(() => OnButtonPressedCaller(1));
		}

		public void OnShow(string message, bool buttonConfirm, bool buttonDeny, bool showLoading, ListenerCallInteger callback)
		{
			Setup(message, buttonConfirm, buttonDeny, showLoading, callback);
			OnShow();
		}
		public void OnOpen(string message, bool buttonConfirm, bool buttonDeny, bool showLoading, ListenerCallInteger callback)
		{
			Setup(message, buttonConfirm, buttonDeny, showLoading, callback);
			OnOpen();
		}

		public void Setup(string message, bool buttonConfirm, bool buttonDeny, bool showLoading, ListenerCallInteger callback)
		{
			Message.text = message;
			ConfirmButton.gameObject.SetActive(buttonConfirm);
			DenyButton.gameObject.SetActive(buttonDeny);
            progressIndicator.gameObject.SetActive(showLoading);
			OnButtonPressed = callback;
		}

		void OnButtonPressedCaller(int id)
		{
			if (OnButtonPressed != null)
				OnButtonPressed.Invoke(id);
			OnButtonPressed = null;
			//We always close once a button is pressed.
			OnClose();
		}

        private void Update()
        {
            if(progressIndicator != null && progressIndicator.gameObject.activeInHierarchy)
            {
                progressIndicator.localEulerAngles = new Vector3(0f, 0f, -Time.unscaledTime * 500f);
            }
                
        }
    }
}